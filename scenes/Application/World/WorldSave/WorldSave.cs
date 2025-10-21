using DataStructures;
using Godot;
using System;
using System.IO;

// filestream?


namespace WorldSystem.Save
{
    // oddílný od terrain
    public partial class WorldSave : Node
    {

        const string appName = "SurfaceNets-Infinite-Terrain-Demo";
        string worldDirectory;

        Header header;
        WorldData worldData;
        public WorldGenration worldGen;

        //
        //
        //

        public WorldSave(System.Numerics.Vector3 WorldPosition, float worldSize, string worldName, int seed)
        {
            // získáme všechny cesty k souborům pro daný svět
            string appDataDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            worldDirectory = Path.Combine(appDataDirectory, appName, "worlds", worldName); // "C:/Users/tomas/AppData/Roaming/p1ga/worlds/world1"

            // hlavní directory
            CreateWorldDirectoryIfNone(worldDirectory);
            // header
            header = new Header(worldDirectory, WorldPosition, worldSize);
            // data
            worldData = new WorldData(worldDirectory);
            // world generation
            worldGen = new WorldGenration(seed);

        }

        //
        // save
        //

        // pass jenom octant
        public void SaveChunk(ref Terrain.Chunk chunk, System.Numerics.Vector3 position, float size) // uloží jeden chunk
        {
            // pokud chunk není max LOD tak se neukládá.
            // temp? 
            if (size != Terrain.Chunk.fieldSize) return;

            // pomocí pozice a velikosti najdeme chunk v octree saveu.
            int octantIndex = header.octree.SubdivideAtPoint(header.octree.rootIndex, position, size, int.MaxValue); // špatně.
            if (octantIndex == int.MaxValue) return; // stane se pouze pokud je chunk mimo svět

            // POZNÁMKA: je jedno jakou hodnotu má octant, worldData SaveChunk() si s tím poradí
            ref int chunkIndex = ref header.octree.GetOctant(octantIndex).value;

            // uloží se data chunku
            worldData.SaveChunk(ref chunk, ref chunkIndex); // velmi pomalé (23.09.2025)
            // společně s chunkem se musí uložit i header
            header.Save();
        }

        //
        // load
        //

        // Načteme field chunku na jakémkoliv LOD. (od nejmenšího klidně až po nejvyšší)
        // Data načteme pomocí velikosti a pozice octantu. kde nejdříve v worldSave header struktuře najdeme stejný octant
        // a poté načteme všechny chunky které obsahuje.
        // pokud je chunk moc velký (tak velký že políčka ve fieldu jsou větší než samotný chunk na max LOD) tak zapisujem pouze chunky které spadají do fieldu na pozici.
        // (* to no co je nahoře ještě implementovat!) (23.09.2025)
        // #### Parametry:
        // - ref Terrain.Chunk chunk -> chunk do kterého chceme načíst data.
        // - System.Numerics.Vector3 octantPosition -> pozice octantu ke kterému náleží chunk.
        // - float octantSize -> velikost octantu ke kterému náleží chunk.
        public void LoadChunk(ref Terrain.Chunk chunk, System.Numerics.Vector3 octantPosition, float octantSize)
        {
            // zkusíme najdeme octant v headeru
            int sameOctantIndex = header.FindSameOctant(octantPosition, octantSize);
            // pokud octant ještě nebyl vygenerován (tedy neexistuje ve stromě ani jako root) tak vlastně neobsahuje žádné uložené chunky které by mohl obsahovat.
            // * (protože není listem, a ani nemá listy)
            if (sameOctantIndex == int.MaxValue)
            {
                FillChunkPieceProcedural(ref chunk, new System.Numerics.Vector3(0.0f), Terrain.Chunk.fieldSize, octantPosition, octantSize);
                return;
            }
            // pokud octant existuje kdekoliv ve stromě tak z něj vyčteme chunkdata.
            ref Octant<int> sameOctant = ref header.GetOctant(sameOctantIndex);
            LoadChunkData(ref chunk, ref sameOctant, octantPosition, octantSize); // něco špatně (23.09.2025) (už dobrý 23.09.2025)
        }

        // zaplní celou část chunku kterou zabírá zadaný octant k poměru k chunkPoisiton a chunkSize.
        private void LoadChunkData(ref Terrain.Chunk chunk, ref Octant<int> octant, System.Numerics.Vector3 chunkPosition, float chunkSize) // octant nemusí pokrývat celý chunk
        {
            // *POZNÁMKA: octant v save headeru je identický s octantem v rendereru.

            // pokud octant neobsahuje žádné podoctanty, které by mohly obsahovat uložené chunky tak ho načtem. (z dat octantu)
            if (octant.isLeaf)
            {
                FillChunkPiece(ref chunk, ref octant, chunkPosition, chunkSize);
                return;
            }

            // pokud se hledaný octant skládá z dalších octantů, iterujem dál
            for (int leafIndex = 0; leafIndex < 8; leafIndex++)
            {
                int octantLeafIndex = octant.leafs[leafIndex];
                ref Octant<int> octantLeaf = ref header.octree.GetOctant(octantLeafIndex);
                // iterujem dál
                LoadChunkData(ref chunk, ref octantLeaf, chunkPosition, chunkSize);
            }
        }

        // pokud zadaný octant obsahuje uložený chunk (je leaf na max LOD) tak zaplníme část chunku kterou zabírá.
        private void FillChunkPiece(ref Terrain.Chunk chunk, ref Octant<int> octant, System.Numerics.Vector3 chunkPosition, float chunkSize)
        {
            // pokud je listem tak zapíšeme do chunku tu část kterou zabírá.
            // * celkem effektivní, ale pro obří chunky (kde je field tile větší než nejmenší chunk) to není tak dobré.

            // pozice a velikost bloku který zabýrá podchunk v chunku.
            System.Numerics.Vector3 positionInChunk = (octant.position - chunkPosition) / chunkSize * Terrain.Chunk.fieldSize;
            float sizeInChunk = octant.size / chunkSize * Terrain.Chunk.fieldSize;

            // pokud podchunk neexistuje tak zaplníme jeho místo novým terénem.
            if (octant.value == int.MaxValue)
            {
                FillChunkPieceProcedural(ref chunk, positionInChunk, sizeInChunk, chunkPosition, chunkSize);
                return;
            }

            // pokud k octantu patří uložený chunk, zkusíme načíst jeho data.
            // * POZNÁMKA: pokud se nepodaří načíst data chunku z této funkce, tak se vrátí prázdný array ale pořád o velikosti.
            byte[] chunkData = worldData.LoadChunkData(octant.value);



            // pokud se vše povede (povede se načíst data chunku) tak zaplníme část kterou zabírá v požadovaném chunku.
            FillChunkPieceLoad(ref chunk, positionInChunk, sizeInChunk, chunkData);
        }

        private void FillChunkPieceProcedural(ref Terrain.Chunk chunk, System.Numerics.Vector3 positionInChunk, float sizeInChunk, System.Numerics.Vector3 chunkPosition, float chunkSize)
        {
            for (int z = 0; z < sizeInChunk; z++)
            {
                for (int y = 0; y < sizeInChunk; y++)
                {
                    for (int x = 0; x < sizeInChunk; x++)
                    {
                        System.Numerics.Vector3 fieldPosition = new System.Numerics.Vector3(x, y, z) + positionInChunk;
                        int fieldIndex = (int)fieldPosition.X + (int)fieldPosition.Y * Terrain.Chunk.realFieldSize + (int)fieldPosition.Z * Terrain.Chunk.realFieldSize * Terrain.Chunk.realFieldSize;

                        System.Numerics.Vector3 globalPosition = fieldPosition * (chunkSize / Terrain.Chunk.fieldSize) + chunkPosition;
                        chunk.field[fieldIndex] = worldGen.GetValue(globalPosition);
                    }
                }
            }
        }

        private void FillChunkPieceLoad(ref Terrain.Chunk chunk, System.Numerics.Vector3 positionInChunk, float sizeInChunk, byte[] chunkData)
        {
            for (int z = 0; z < sizeInChunk; z++)
            {
                for (int y = 0; y < sizeInChunk; y++)
                {
                    for (int x = 0; x < sizeInChunk; x++)
                    {
                        System.Numerics.Vector3 fieldPosition = new System.Numerics.Vector3(x, y, z) + positionInChunk;
                        int fieldIndex = (int)fieldPosition.X + (int)fieldPosition.Y * Terrain.Chunk.realFieldSize + (int)fieldPosition.Z * Terrain.Chunk.realFieldSize * Terrain.Chunk.realFieldSize;

                        System.Numerics.Vector3 savedFieldPosition = new System.Numerics.Vector3(x, y, z) * (Terrain.Chunk.fieldSize / sizeInChunk);
                        int chunkDataIndex = (int)savedFieldPosition.X + (int)savedFieldPosition.Y * Terrain.Chunk.fieldSize + (int)savedFieldPosition.Z * Terrain.Chunk.fieldSize * Terrain.Chunk.fieldSize;

                        chunk.field[fieldIndex] = chunkData[chunkDataIndex];
                    }
                }
            }
        }

        private void CreateWorldDirectoryIfNone(string worldDirectory)
        {
            if (!Directory.Exists(worldDirectory))
            {
                GD.PrintErr("World directory empty: ", worldDirectory);
                Directory.CreateDirectory(worldDirectory);
            }
            GD.Print("World Exists");
        }


    }
}

/*
*
* [06.09.2025] vytvořeno
* [23.09.2025] teď konečně funkční save a load při nižším lod. ale je to pomalé...
*/