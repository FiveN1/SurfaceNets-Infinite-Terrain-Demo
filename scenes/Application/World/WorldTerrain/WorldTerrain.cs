using DataStructures;
using Godot;
using System;

/*
* Napad pro optimalizaci:
*
* - při modifikaci několika chunků, poslat všechny chunky na gpu najednou.
*
*/


// Renderovaný terén z worldSave
// 
// Octree LOD system
// Dual-Contouring system
//
namespace WorldSystem.Terrain
{
    // spojuje chunky a octree dohromady
    public class WorldTerrain
    {
        //
        public int worldScale;
        public float renderDistanceScale = 1.2f; // LOD scale, podle toho se určuje vzdálenost kvalitních chunků. (1.0f, je default) (může jít od 0.1f, do +inf, víc než 2.0f není potřeba)
        
        // world dimenstions
        public readonly float worldSize;
        public readonly System.Numerics.Vector3 worldPosition;

        // chunk octree
        public DataStructures.Octree<int> octree;
        public DataStructures.Pool<Chunk> chunkPool;

        // chunk context
        public ChunkContext chunkContext;

        // constructor
        public WorldTerrain(Node3D meshNode, int worldScale)
        {
            this.worldScale = worldScale;
            // výpočet rozměrů světa podle worldScale
            this.worldSize = Mathf.Pow(2, worldScale) * Chunk.fieldSize;
            this.worldPosition = new System.Numerics.Vector3(-worldSize / 2.0f);

            // init struktur
            this.octree = new DataStructures.Octree<int>(worldPosition, worldSize);
            this.octree.GetOctant(octree.rootIndex).value = int.MaxValue; // set root default
            this.chunkPool = new DataStructures.Pool<Chunk>();

            // chunk context
            // ZBAVIT SE !!
            this.chunkContext = new ChunkContext(meshNode);
            this.chunkContext.DebugEnabled(true);
        }


        public void UpdatePov(System.Numerics.Vector3 povPosition, Save.WorldSave worldsave)
        {
            SubdivideIfClose(octree.rootIndex, povPosition, 0, worldScale, worldsave);
        }

        //
        // subdivide
        //

        // PROBLÉMY:
        // 1. tvorba zbytečných chunků.
        // - podle toho z jakého směru jde update se uvolnují a pak přidávají chunky.
        // - je možné že se některé chunky vytvoří dřív než se uvolní zbytek,
        // poté tedy vznikne nadbytek chunků které se nikdy nevyužijí.
        // ŘEŠENÍ:
        // - Odstranit nejdříve chunky, potom přidat.
        //
        // 2. není tu maximální limit chunků.
        // - bylo by dobré kdyby nejaké uplně vzdálené chunky nebyly tvořeny.
        // - poté by yblo možné mít opravdu nekonečný svět, bez toho aby počet chunků rostl.
        // ŘEŠENÍ:
        // - nejdříve se tvoří chunky nejblíže k pov point, poté více vzdálené.
        // počítal by se také počet chunků ve stromě, pokud by dosáhl limitu tak by se iterace zastavila.
        //
        private void SubdivideIfClose(int octantIndex, System.Numerics.Vector3 position, int iterationCount, int maxIterations, Save.WorldSave worldSave) // max iterations
        {
            ref DataStructures.Octant<int> octant = ref octree.GetOctant(octantIndex);
            //
            // Update počtu iterace
            //
            iterationCount++;
            // pokud byla dosažena maximální iterace tak pouze nastavíme jeho panel
            if (iterationCount > maxIterations)
            {
                //GD.Print("assign1");
                AssignChunk(octantIndex, worldSave);
                // add chunk pro octant
                return;
            }
            //
            // Distance funkce
            //
            // pomocí distance funkce zjistíme zda by měl být node rozdělen na větší detail
            bool isClose = CheckIfNodeClose(octant, position);
            //
            // Operace bodu
            //
            // pokud by měl být orzdělen ale není (je blízko), rozdělíme ho.
            if (isClose && octant.isLeaf)
            {
                //GD.Print("sub");
                octree.SubdivideOctant(octantIndex, int.MaxValue);
                UnAssignChunk(octantIndex, worldSave);
                // jelikož se možná změnila adresa tak ho musíme znovu získat
                octant = ref octree.GetOctant(octantIndex);
                // potom se bude iterovat nad jeho listy...
            }
            // pokud by neměl být rozdělen ale je (není blízko), přemeníme ho na list
            if (!isClose && !octant.isLeaf)
            {
                //GD.Print("unsub");
                UnsubdivideOctantAndRemoveChunk(octantIndex, worldSave);
                //activeNode.Unsubdivide();
            }
            //
            // Následné operace
            //
            // pokud je po operaci listem, přidáme mu panel.
            if (octant.isLeaf)
            {
                //GD.Print("assign2");
                AssignChunk(octantIndex, worldSave);
                return;
            }
            //GD.Print("cont");
            // pokud není po operaci listem, iterujem nad jeho listy
            // POZNÁMKA: iterujem od (+)konce aby bylo možné vytvořit spoje mezi chunky a nemuseli jsem iterovat znovu přes celý strom.
            for (int i = 7; i >= 0; i--)
            {
                SubdivideIfClose(octant.leafs[i], position, iterationCount, maxIterations, worldSave);
            }
        }

        // přesunout do octree?
        private bool CheckIfNodeClose(DataStructures.Octant<int> octant, System.Numerics.Vector3 position)
        {
            System.Numerics.Vector3 CellCenterPos = octant.position + new System.Numerics.Vector3(octant.size) * 0.5f;
            float dist = (CellCenterPos - position).Length();
            if (dist < octant.size * renderDistanceScale) return true;
            return false;
        }

        //
        // chunk fetch
        //

        private int FetchChunk()
        {
            int chunkIndex = chunkPool.Fetch();
            ref Chunk chunk = ref chunkPool.Get(chunkIndex);
            // pokud je chunk fresh (nebyl předtím použit) tak ho inicializujem

            if (!chunkPool.CheckIfConstructed(chunkIndex))
            {
                chunk = new(chunkContext);
                chunkPool.SetAcConstructed(chunkIndex);
            }
            chunk.Enabled(true);
            return chunkIndex;
        }

        //
        // chunk
        //

        private void AssignChunk(int octantIndex, Save.WorldSave worldSave)
        {
            ref DataStructures.Octant<int> octant = ref octree.octants.Get(octantIndex);

            // pokud je chunk není prázdný tak se nic neděje.
            if (octant.value != int.MaxValue) return;
            //
            int chunkIndex = FetchChunk();
            if (chunkIndex == int.MaxValue)
            {
                GD.PrintErr("invalid chunk!");
                return;
            }
            ref Chunk chunk = ref chunkPool.Get(chunkIndex);
            // assign chunk to octant
            octant.value = chunkIndex;



            // načteme hodnoty chunku
            // * také získat neighbor hodnoty !
            worldSave.LoadChunk(ref chunk, octant.position, octant.size);
            //worldSave.worldGen.GenerateChunk(ref chunk, octant.position, octant.size);
            // vytvoříme mesh
            //GetChunkEdgeValues(octantIndex);
            chunk.Create(octant.position, octant.size);

            //GD.Print("assigned chunk: ", octant.value);
        }
        private void UnAssignChunk(int octantIndex, Save.WorldSave worldSave)
        {
            ref DataStructures.Octant<int> octant = ref octree.octants.Get(octantIndex);
            int chunkIndex = octant.value;
            if (chunkIndex == int.MaxValue) return;
            // get chunk
            ref Chunk chunk = ref chunkPool.Get(chunkIndex);
            // save
            worldSave.SaveChunk(ref chunk, octant.position, octant.size);
            // free
            chunk.Enabled(false);
            chunkPool.Free(chunkIndex);
            octant.value = int.MaxValue;
        }

        //
        // Octant subdivide
        //


        private void UnsubdivideOctantAndRemoveChunk(int octantIndex, Save.WorldSave worldSave)
        {
            //GD.Print("unsubdividing octant: ", octantIndex);
            ref DataStructures.Octant<int> octant = ref octree.octants.Get(octantIndex);
            // check zda nebyl subduvudován
            if (octant.isLeaf) return;
            // pro každý list -> unsubdivide dokud nenarazíme na dno
            // potom můžeme smazat když jsou všechny listy pryč
            for (int LeafIndex = 0; LeafIndex < 8; LeafIndex++)
            {
                UnsubdivideOctantAndRemoveChunk(octant.leafs[LeafIndex], worldSave);
                // pokud má chunk tak ho odeberem a vrátíme zpátky do poolu
                UnAssignChunk(octant.leafs[LeafIndex], worldSave);
                // pak ho odstraníme
                octree.octants.Remove(octant.leafs[LeafIndex]);
            }
            // teď je list
            octant.isLeaf = true;
        }


        // dodělat chunk update.
        // PROBLÉM:
        // - nelze modifikovat chunk který má nižší lod
        public void ModifyTerrain(System.Numerics.Vector3 collisionPoint, bool extrude, Save.WorldSave worldSave)
        {

            int brushSize = 3;
            byte brushAmount = 4;

            // pole s všemi octanty ve kterých se změnili hodnoty chunku
            // * je nahovno že se tento array pořád allokuje (25.09.2025)
            System.Collections.Generic.List<int> modifiedOctants = new System.Collections.Generic.List<int>();

            // loop přez všechny hodnoty fieldů chunků
            for (int z = 0; z <= brushSize; z++)
            {
                for (int y = 0; y <= brushSize; y++)
                {
                    for (int x = 0; x <= brushSize; x++)
                    {
                        // * tady je možná chyba
                        System.Numerics.Vector3 modifyPoint = collisionPoint + new System.Numerics.Vector3(x, y, z) - new System.Numerics.Vector3(brushSize - 1.0f) * 0.5f;

                        // get octant
                        int octantIndex = octree.FindOctantWithPosition(octree.rootIndex, modifyPoint);
                        if (octantIndex == int.MaxValue) continue; // pokud invalidní octant
                        ref Octant<int> octant = ref octree.GetOctant(octantIndex);

                        // get chunk
                        int chunkIndex = octant.value;
                        if (chunkIndex == int.MaxValue) continue; // pokud invalidní chunk
                        ref Terrain.Chunk chunk = ref chunkPool.Get(chunkIndex);

                        if (!modifiedOctants.Contains(octantIndex)) modifiedOctants.Add(octantIndex);

                        // pozice ve fieldu
                        System.Numerics.Vector3 localChunkModifyPoint = modifyPoint - octant.position;
                        int fieldIndex = (int)localChunkModifyPoint.X + (int)localChunkModifyPoint.Y * Terrain.Chunk.realFieldSize + (int)localChunkModifyPoint.Z * Terrain.Chunk.realFieldSize * Terrain.Chunk.realFieldSize;

                        // modify field
                        if (extrude)
                        {
                            if (chunk.field[fieldIndex] < byte.MaxValue - brushAmount) chunk.field[fieldIndex] += brushAmount;
                        }
                        else
                        {
                            if (chunk.field[fieldIndex] >= brushAmount) chunk.field[fieldIndex] -= brushAmount;
                        }
                    }
                }
            }

            // update všech modifikovaných chunků
            foreach (int octantIndex in modifiedOctants)
            {
                //GD.Print("\t", octantIndex);

                ref Octant<int> octant = ref octree.GetOctant(octantIndex);
                ref Terrain.Chunk chunk = ref chunkPool.Get(octant.value);
                // update & save
                chunk.Create(octant.position, octant.size);
                worldSave.SaveChunk(ref chunk, octant.position, octant.size); // (23.09.2025)
            }

        }

        //
        // set chunk edge values
        //

        public void GetChunkEdgeValues(int octantIndex)
        {
            Octant<int> octant = octree.GetOctant(octantIndex);
            int chunkIndex = octant.value;
            if (chunkIndex == int.MaxValue) return;
            ref Terrain.Chunk chunk = ref chunkPool.Get(chunkIndex);

            for (int i = 1; i < 8; i++)
            {
                //
                System.Numerics.Vector3 neighborDirection = new(i % 2, i / 2 % 2, i / 4);

                // get neighbor octant
                int neighborOctantIndex = octree.FindOctantNeighbor(octantIndex, neighborDirection);
                if (neighborOctantIndex == int.MaxValue) continue;
                Octant<int> neighborOctant = octree.GetOctant(neighborOctantIndex);

                // plane
                System.Numerics.Vector3 invDirection = new System.Numerics.Vector3(1, 1, 1) - neighborDirection;
                System.Numerics.Vector3 planeSize = invDirection * Terrain.Chunk.fieldSize + neighborDirection;

                // diffrence
                float scaleDiffrence = octant.size / neighborOctant.size;
                System.Numerics.Vector3 positionDiffrence = (octant.position - neighborOctant.position) * invDirection;

                //
                for (int z = 0; z < planeSize.Z; z++)
                {
                    for (int y = 0; y < planeSize.Y; y++)
                    {
                        for (int x = 0; x < planeSize.X; x++)
                        {
                            // pos
                            System.Numerics.Vector3 pos = new(x, y, z);

                            // field position
                            System.Numerics.Vector3 fieldPosition = pos + neighborDirection * Terrain.Chunk.fieldSize;
                            int fieldIndex = (int)fieldPosition.X + (int)fieldPosition.Y * Terrain.Chunk.realFieldSize + (int)fieldPosition.Z * Terrain.Chunk.realFieldSize * Terrain.Chunk.realFieldSize;

                            // získání neighbor chunk
                            // * vytvořeno takhle protože je možné že sousedící octantu bude složen z menších chunků
                            // -> tady zjistíme zda je složen z dalších chunků, pokud ano tak hledáme hodnotu přez několik octantů.
                            int neighborChunkIndex;
                            if (!neighborOctant.isLeaf)
                            {
                                // global pos
                                System.Numerics.Vector3 globalPosition = fieldPosition * octant.size / Terrain.Chunk.fieldSize + octant.position;
                                int subNeighborOctantIndex = octree.FindOctantWithPosition(neighborOctantIndex, globalPosition);
                                if (subNeighborOctantIndex == int.MaxValue) continue;
                                Octant<int> subNeighborOctant = octree.GetOctant(subNeighborOctantIndex);
                                neighborChunkIndex = subNeighborOctant.value;
                            }
                            else
                            {
                                neighborChunkIndex = neighborOctant.value;
                            }
                            if (neighborChunkIndex == int.MaxValue) continue;
                            ref Terrain.Chunk neighborChunk = ref chunkPool.Get(neighborChunkIndex);

                            // neighbor field position
                            System.Numerics.Vector3 neighborFieldPosition = (pos + positionDiffrence) * scaleDiffrence;
                            int neighborFieldIndex = (int)neighborFieldPosition.X + (int)neighborFieldPosition.Y * Terrain.Chunk.realFieldSize + (int)neighborFieldPosition.Z * Terrain.Chunk.realFieldSize * Terrain.Chunk.realFieldSize;
                            byte neighborFieldValue = neighborChunk.field[neighborFieldIndex];

                            // set
                            chunk.field[fieldIndex] = neighborFieldValue;

                        }
                    }
                }



            }
        }
    }
}



/*
*
*                [Root]
*       [Chunk1], ... , [Chunk8]
* [Chunk1] ...
*
* Pro každý chunk může být 8 dalších chunků
*
*/

// PROBLÉM:
// objekty se neuvolnujou!!

/*
* Zdroje:
*
* DUAL CONTOURING:
* https://www.cs.rice.edu/~jwarren/papers/dualcontour.pdf
* https://www.mattkeeter.com/projects/contours/
* https://www.reddit.com/r/Unity3D/comments/ieenax/highaccuracy_dual_contouring_on_the_gpu_tech/
* https://voxel-tools.readthedocs.io/en/latest/smooth_terrain/
* https://www.graphics.rwth-aachen.de/publication/131/feature1.pdf
*
* TEXTURY:
* https://gamedev.stackexchange.com/questions/53272/how-to-texture-a-surface-generated-by-marching-cubes-algorithm
*
* CHUNK BLEND:
* https://ngildea.blogspot.com/2014/09/dual-contouring-chunked-terrain.html
* https://www.evl.uic.edu/vchand2/thesis/papers/Marching%20Cubes.pdf
* https://transvoxel.org//
* TransVoxel Algorithm
* https://www.youtube.com/watch?v=U6WhkL3IVYg&t=85s
*/

/*
* Plán:
*
* PRIORITA | PLÁN
*   (1)     [začistit kod]
*   (3)     [Vytvořit mesh z fieldu pomocí dual contouring]
*   (x)     [Vytvořit Octree terén]                         [HOTOVO]
*   (2)     [Implementovat ukládání octree]
*   (4)     [Různé materiály / textury]
*   (5)     [GPU Akcelerace]
*   (1)     [Chunk Blending]
*   (3)     [Isolevel zaležící na velikosti buněk!]
*   (5)     [Vzdálené chunky se nebudou renderovat?]
*   (1)     [Debug Mesh]
*/

// Každý chunk bude mít vlastní field, a mesh na určené velikosti. 
// místo add mesh funkce bude generate, kde field bude inicializován a nasteven pomocí noise funkce

/*
* Změny
*
* [16.07.2025] Vytvořeno, teď organizuji své scény a skripty dohromady.
* Tento nový terén bude založen na Octree systému a bude tvořen pomocí Dual Contouring metody nebo EMC metody ještě uvidím.
* Taky se tady budu snažit držet kod co nejlepší pro snadnou upravu v budoucnu.
* [17.07.2025] Přidán Octree systém
* [18.07.2025] Implementován jednoduchý dynamický Octree systém.
* [19.07.2025] implementace funkce hledání sousedů v octree.
* [20.07.2025] snaha přidat funkční chunk blending.
* [09.09.2025] teď je vše nádherné. (všechno rozděleno do svých tříd, bez použití static)
* [17.09.2025] funkční save chunků/ světa, skoro funkční modifikace terénu.
* [22.10.2025] začištění
*
*/
