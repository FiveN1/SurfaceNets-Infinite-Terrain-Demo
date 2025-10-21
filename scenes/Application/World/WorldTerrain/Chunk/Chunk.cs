using System;
using DataStructures;
using Godot;

namespace WorldSystem.Terrain
{
    public struct Chunk
    {
        public byte[] field;
        public static readonly int fieldSize = 8;
        public static readonly int realFieldSize = fieldSize + 1;

        // mesh
        private ChunkMesh mesh;
        // collision
        private ChunkCollision collision;
        // debug
        public readonly ChunkDebug debugVisual;

        // constructor
        public Chunk(ChunkContext context) // context
        {
            // field data
            this.field = new byte[realFieldSize * realFieldSize * realFieldSize];
            // mesh
            mesh = new ChunkMesh(fieldSize, context.meshNode);
            // collision
            collision = new ChunkCollision(mesh.surfaceNetMesh);
            // debug 
            debugVisual = new ChunkDebug(context.chunkDebugContext);

            //GD.Print("new chunk");
        }
        // Vytvoří chunk z dat fieldu.
        // Proot musí být předem načtený field (z WorldSave)
        public void Create(System.Numerics.Vector3 position, float size) // pass
        {
            // transform to Godot Vector3
            Vector3 GDposition = new Vector3(position.X, position.Y, position.Z);
            // generate mesh
            mesh.Generate(GDposition, size, ref field, fieldSize, realFieldSize);
            // generate collision
            collision.Generate(GDposition, size, mesh.meshData);
            // update debug
            debugVisual.Set(GDposition, size);
        }

        // ...
        public void Create2(Octant<int> octant, Octree<int> octree)
        {

        }

        public void Enabled(bool enabled)
        {
            mesh.Enabled(enabled);
            collision.Enabled(enabled);
            debugVisual.Enabled(enabled);

            //GD.Print("chun")
        }

        // Speciální coordinate systém, kde je jedno jak je buffer velký, indexy nezávisí na velikosti bufferu.
        // funguje ajko cibule. má vrstvy. (25.09.2025)
        //
        // (příklad 3x3 array)
        //
        // 8 7 5
        // 6 3 2
        // 4 1 0
        //
        // 8 7 6
        // 5 3 2
        // 4 1 0
        //   
        // = [0, 1, 2, 3, 4, 5, 6, 7, 8]
        //
        // 
        // 
        //
        //
        //
        // x + 1 = 
        //
        // žádné dvě souřadnice nesmí mít stejný index
        //
        // indexové systémy
        // (hledám pattern)
        //
        // (x + y)
        // 4 3 2
        // 3 2 1
        // 2 1 0
        //
        // (x * y)
        // 4 2 0
        // 2 1 0
        // 0 0 0
        //
        // y * (layer + 1) + x
        //          yx
        // 8 7 6    11 = 1 * 2 + 1 = 3
        // 5 3 2    21 = 2 * 3 + 1 = 7
        // 2 1 0    02 = 0 * 3 + 2 = 2 (* tohle chce spravit)  
        //
        // y * (layer + 1) + x * abs(x - y)
        //
        // 8 7 6
        // 5 2 2    
        // 2 1 0    
        //
        public static int GetFieldIndex_old(int x, int y)
        {
            // * zbavit se if !! (25.09.2025)
            // funguje správně
            int index;
            if (x > y)
            {
                index = x * (x - y) + y * (x + 1);
            }
            else
            {
                index = y * (y + 1) + x;
            }
            return index;
        }

        public static int GetFieldIndex(int x, int y, int z)
        {
            
            return x + y * realFieldSize + z * realFieldSize * realFieldSize;
        }

        public static int GetFieldIndex2(int x, int y)
        {
            // x * (x - y) + y * y;
            //return x * (x - y) + y * y;

            // x * y
            //return x * (x - y) + y + x * (y + 1);
            return y + x * (y + 1);
        }

        /*
        skoro dobře

        int layer = x > y ? x : y;

            int diff = int.Abs(x - y);
            if (diff == 0) diff = 1;

            return x * diff + y * (layer + 1);
        */

    }
}