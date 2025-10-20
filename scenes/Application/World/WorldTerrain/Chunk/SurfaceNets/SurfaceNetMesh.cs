using Godot;
using System;

namespace SurfaceNet
{


    public struct NeighborSection
    {
        // array s indexy pro chunky
        // -> nebo by to šlo líp?

        // velikost v 3d prostoru ? (nebo spíš ve 2d)

        
        
    }

    public struct MeshData
    {
        public Vector3[] vertexPositions;
        public Vector3[] vertexNormals;
        public int[] indices;

        public int indicesSize;

        public int verticesSize; // funguje jako write index pro stich



        int[] neighborSections;


        public MeshData(int fieldSize)
        {
            int maxCoreVerticesCount = fieldSize * fieldSize * fieldSize;
            //int maxNeighborVerticesCount = 3 * fieldSize * fieldSize + 3 * fieldSize + 1;
            int maxNeighborVerticesCount = fieldSize * fieldSize * fieldSize * 7;
            int maxVerticesCount = maxCoreVerticesCount + maxNeighborVerticesCount;
            // Každá buňka má jeden vertex, takže velikost bude fieldsize - 1
            this.vertexPositions = new Vector3[maxVerticesCount];
            this.vertexNormals = new Vector3[maxVerticesCount];
            // pro jednu buňku je maximální počet quadů 3, takže 6 trojuhelníku což je 18 indexů.
            this.indices = new int[maxVerticesCount * 18]; // může teoreticky být míň než 18
                                                           // aktuální velikost indexů získaná pomocí writeindex
            this.indicesSize = 0;
            this.verticesSize = 0;


            // tady implementovat 7 useků které budou držet vertexy sousedů (ze 7 stran)
            // každý usek bude držet:
            // -> začáteční index
            // -> chunky? (může obsahovat různé velikosti) nějaká struktura
            // -> indexy chunků s velikostmi.
            // 
            // co je problém?
            // -> data jsou problém.
            //
            //
            //

            neighborSections = new int[7]; // 7 stran


        }
    }


}