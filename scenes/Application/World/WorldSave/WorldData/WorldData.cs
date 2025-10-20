using System;
using System.IO;
using Godot;

// mark for save...
// -> když worldsave dosáhne nějaké množství chunků které má uložit, zapíše je všechny najednou.

/*
* CO JE POMALÉ?:
* - časté psaní na disk (* lepší by bylo vše napsat v batch)
*
* - kopírování dat z chunků do bufferu. (* to by šlo vyřešit tím že by se to načetlo rovnou do chunku. pak yby se ale musel rozdělit buffer pro core a pro edge.)
*
*
*
*/

namespace WorldSystem.Save
{

    /*
    *
    * Sere mě jak nemůžu prostě všechno psát do jednoho bufferu, a vše to tam streamovat.
    * musím vše napsat do milionů různých souborů.
    *
    *
    */
    public class WorldData
    {




        //string worldDataDirectory;
        string worldDataFileName;
        public int savedChunksAmount = 0;


        private const int savedChunksAmountOrigin = 0; // = pozice kde je uložen savedChunksAmount
        private const int chunkBufferOrigin = sizeof(int);
        private int chunkBufferSize = Terrain.Chunk.fieldSize * Terrain.Chunk.fieldSize * Terrain.Chunk.fieldSize;

        //
        //
        //

        public WorldData(string worldDirectory)
        {

            worldDataFileName = Path.Combine(worldDirectory, "world_data.dat");


            LoadSavedChunksAmount();

        }


        public void SaveChunk(ref Terrain.Chunk chunk, ref int chunkIndex)
        {
            // vytvoříme nový chunk pokud neexistuje.
            if (chunkIndex == int.MaxValue)
            {
                chunkIndex = savedChunksAmount;
                savedChunksAmount++;
                WriteSavedChunksAmount();
            }
            // setup array
            // * zbavit se !! zbytečné allokace a kopírování (24.09.2025)
            byte[] chunkData = new byte[Terrain.Chunk.fieldSize * Terrain.Chunk.fieldSize * Terrain.Chunk.fieldSize];
            // získáme fieldData z chunku který chceme uložit.
            GetChunkFieldData(ref chunk, chunkData);
            // otevře soubor a zapíše do něj
            WriteChunk(chunkIndex, chunkData);

            //GD.Print("saved ", chunkName);
        }



        public byte[] LoadChunkData(int chunkIndex)
        {
            // * zbavit se !! zbytečné allokace a kopírování (24.09.2025)
            // * kopírovat přímo do chunku.
            byte[] chunkData = new byte[chunkBufferSize];


            ReadChunk(chunkIndex, chunkData);
            // vracíme, ikdyž může být prázdný. * check se provádí potom
            return chunkData;
        }

        //
        //
        //




        // zapíše data z chunku do bufferu.
        // zapíše také jeho index?
        // -> složité, v některých případech nabude ani fungovat?
        // -> to by se ale vyřešilo kdyby nějaké chunky byly načítány z tohoto bufferu co?
        // ...
        // Mít buffer (cache) ve kterém budou nevyužité chunky.
        // -> tento buffer se vynuluje když dojde k save.
        // -> postupně se bude plnit.
        /*
        public void MarkForSave()
        {

        }


        public void GetWorldDataFile()
        {

        }
        */

        private void GetChunkFieldData(ref Terrain.Chunk chunk, byte[] fieldData)
        {
            for (int z = 0; z < Terrain.Chunk.fieldSize; z++)
            {
                for (int y = 0; y < Terrain.Chunk.fieldSize; y++)
                {
                    for (int x = 0; x < Terrain.Chunk.fieldSize; x++)
                    {
                        int fileBufferIndex = x + y * Terrain.Chunk.fieldSize + z * Terrain.Chunk.fieldSize * Terrain.Chunk.fieldSize;
                        int chunkBufferIndex = x + y * Terrain.Chunk.realFieldSize + z * Terrain.Chunk.realFieldSize * Terrain.Chunk.realFieldSize;
                        fieldData[fileBufferIndex] = chunk.field[chunkBufferIndex];
                    }
                }
            }
        }
        private void WriteChunk(int chunkIndex, byte[] fieldData)
        {

            using (FileStream fileStream = new FileStream(worldDataFileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
            {
                fileStream.Seek(chunkBufferOrigin + chunkIndex * chunkBufferSize, SeekOrigin.Begin);
                fileStream.Write(fieldData, 0, fieldData.Length);
            }
            GD.Print("written chunk at: ", chunkIndex);
        }

        private void ReadChunk(int chunkIndex, byte[] fieldData)
        {
            // pokud soubor z nějkého důvodu neexistuje (což se nikdy nestane, jedině kdyby někdo odstranil soubor za běhu) tak se nic nenačte
            if (!File.Exists(worldDataFileName)) return;

            if (chunkIndex >= savedChunksAmount) return;

            using (FileStream fileStream = new FileStream(worldDataFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                fileStream.Seek(chunkBufferOrigin + chunkIndex * chunkBufferSize, SeekOrigin.Begin);
                fileStream.Read(fieldData, 0, fieldData.Length);
            }
        }



        //
        //
        //

        private void WriteSavedChunksAmount()
        {
            using (FileStream fileStream = new FileStream(worldDataFileName, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write))
            {
                // convert
                byte[] savedChunksAmountByteArray = BitConverter.GetBytes(savedChunksAmount);
                // write
                fileStream.Seek(savedChunksAmountOrigin, SeekOrigin.Begin);
                fileStream.Write(savedChunksAmountByteArray, 0, savedChunksAmountByteArray.Length);
            }
        }

        private void LoadSavedChunksAmount()
        {
            // pokud soubor neexistuje tak ho vytvoříme
            if (!File.Exists(worldDataFileName))
            {
                savedChunksAmount = 0;
                WriteSavedChunksAmount();
                return;
            }
            // načteme data
            using (FileStream fileStream = new FileStream(worldDataFileName, System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                // convert
                byte[] chunkBufferSizeByteArray = new byte[sizeof(int)];
                // write
                fileStream.Seek(savedChunksAmountOrigin, SeekOrigin.Begin);
                fileStream.Read(chunkBufferSizeByteArray, 0, chunkBufferSizeByteArray.Length);

                savedChunksAmount = BitConverter.ToInt32(chunkBufferSizeByteArray, 0);
            }
        }

        /*
        private void CreateFileIfNone()
        {
            if (File.Exists(worldDataFileName)) return;

            using (FileStream fileStream = new FileStream(worldDataFileName, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                // convert
                byte[] savedChunksAmountByteArray = BitConverter.GetBytes(savedChunksAmount);
                // write
                fileStream.Seek(savedChunksAmountOrigin, SeekOrigin.Begin);
                fileStream.Write(savedChunksAmountByteArray, 0, savedChunksAmountByteArray.Length);
            }

            GD.Print("CREATED NEW");
        }
        */




    }
}

// https://stackoverflow.com/questions/2398418/how-to-append-data-to-a-binary-file