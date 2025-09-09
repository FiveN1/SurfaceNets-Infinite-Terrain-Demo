using Godot;
using System;
using System.IO;



namespace WorldSaveSystem
{
    // oddílný od terrain
    public partial class WorldSave : Node
    {
        // stará se o save

        // každý world má svou složku

        const string appName = "p1ga";
        string worldDirectory;
        string headerDirectory;
        string chunkDataDirectory;

        Header header;

        //
        //
        //

        public WorldSave(System.Numerics.Vector3 WorldPosition, float worldSize)
        {
            string appDataDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);

            worldDirectory = Path.Combine(appDataDirectory, appName, "worlds", "world1"); // "C:/Users/tomas/AppData/Roaming/p1ga/worlds/world1"
            //headerDirectory = Path.Combine(worldDirectory, "header.dat");
            //chunkDataDirectory = Path.Combine(worldDirectory, "chunk_data");

            header = new Header(WorldPosition, worldSize);

            Load(worldDirectory);
        }

        // načte svět ze složky
        // pokud složka neexisuje vytvoří novou
        public void Load(string directory)
        {

            // hlavní directory
            if (!Directory.Exists(directory))
            {
                GD.PrintErr("World directory empty: ", directory);
                Directory.CreateDirectory(directory);
                return;
            }
            GD.Print("World Exists");

            // header
            headerDirectory = Path.Combine(directory, "header.dat");
            if (!File.Exists(headerDirectory))
            {
                GD.PrintErr("World does not have a header: ", headerDirectory);
                header.SaveToFile(headerDirectory); // vytvoříme nový empty file, pokud není
            }
            header.SaveToFile(headerDirectory);
            header.LoadFromFile(headerDirectory);

            // data
            chunkDataDirectory = Path.Combine(directory, "chunk_data");
            if (!File.Exists(chunkDataDirectory))
            {
                GD.PrintErr("World does not contain any chunk data: ", chunkDataDirectory);
                return;
            }


        }
        // uloží svět
        public void Save()
        {

        }

        public void LoadChunk(Terrain22.Chunk chunk, System.Numerics.Vector3 position, float size)
        {
            // ...

            // pokud chunk není největší kvality, tak neukládat.


            if (size != Terrain22.Chunk.size)
            {
                // načíst procedural
                return;
            }

            // najít chunk v headeru
            int chunkIndex = header.FindChunk(position, size);
            if (chunkIndex == int.MaxValue)
            {
                // neexistuje
                // procedural
                return;
            }


            // pokud existuje tak načíst

            // pokud ne tak vygenerovat procedural
            
            // traversnout strom dokud nenarazíme na požadovaný chunk



        }

        public void SaveChunk(Terrain22.Chunk chunk, System.Numerics.Vector3 position, float size) // uloží jeden chunk
        {
            // pokud chunk není na max lod tak neukladáme
            // chunk musí být max lod
            if (size != Terrain22.Chunk.size) return;
            // získáme název souboru chunku (pomocí id)
            int chunkID = header.GetOctantValue(position);
            string chunkName = "chunk" + chunkID.ToString() + ".dat";
            string chunkFilename = Path.Combine(chunkDataDirectory, chunkName);
            // zapsat chunk
            // ...
            byte[] arr = new byte[1];
            File.WriteAllBytes(chunkFilename, arr);
        }


    }
}

/*
*
* [06.09.2025] vytvořeno
*
*/