using Godot;
using Godot.NativeInterop;
using System;

// čistší implementace octree systému

// ZMENŠIT CHUNK NA  3X3 NEBO 2X2

namespace Octree
{
    public partial class Node
    {

        public static ChunkPool chunkPool;

        public int chunkIndex;

        public void InitChunk()
        {
            chunkIndex = -1;
        }

        public void CreateChunk()
        {

            if (chunkIndex != -1) return;

            

            //
            // Get chunk
            //
            chunkIndex = chunkPool.GetAvalibeChunk();
            //
            Terrain22.Chunk chunk = chunkPool.GetChunk(chunkIndex);

            //chunk.Enable();


            for (int i = 0; i < chunk.field.Length; i++)
            {
                chunk.field[i] = 0;
            }
            chunk.LoadField(Octree.Tree.worldGen, this.size, this.position);

            GenerateChunkMesh(chunk);

        }


        public void RemoveChunk()
        {

            if (chunkIndex == -1) return;
            

            chunkPool.MakeChunkavalible(chunkIndex);
            chunkIndex = -1;

        }

        public static void SetChunkQueue(ChunkPool chunkPoolInstance)
        {
            chunkPool = chunkPoolInstance;
        }


        //
        // CHUNK EDGE CONNECT
        //

        public void GenerateChunkMesh(Terrain22.Chunk chunk)
        {

            chunk.GenerateMesh(this.position, this.size, this);

        }











        public void GetEdgeFieldValues() // JDE OPTIMALIZOVAT !!
        {
            Octree.Node rootNode = this.GetRootNode();
            Terrain22.Chunk thisChunk = chunkPool.GetChunk(this.chunkIndex);

            for (int i = 1; i < 8; i++)
            {
                Vector3I neighborDirection = new(i % 2, i / 2 % 2, i / 4);
                GetEdgeFieldValuesAtDirection(thisChunk, neighborDirection, rootNode);
            }
        }

        public void GetEdgeFieldValuesAtDirection(Terrain22.Chunk activeChunk, Vector3I neighborDirection, Octree.Node rootNode)
        {
            int fieldSize = Terrain22.Chunk.fieldSize;
            int chunkSize = Terrain22.Chunk.size;
            Vector3 planeBetweenChunks = new Vector3(fieldSize - 1, fieldSize - 1, fieldSize - 1) * (new Vector3(1, 1, 1) - neighborDirection) + new Vector3(1, 1, 1);

            for (int z = 0; z < planeBetweenChunks.Z; z++)
            {
                for (int y = 0; y < planeBetweenChunks.Y; y++)
                {
                    for (int x = 0; x < planeBetweenChunks.X; x++)
                    {
                        // pozice relativní k chunku (k chunk fieldu)
                        Vector3 relativeEdgePosition = new Vector3I(x + (fieldSize - 1) * neighborDirection.X, y + (fieldSize - 1) * neighborDirection.Y, z + (fieldSize - 1) * neighborDirection.Z);
                        // transformujem pozici tak aby byla v globálním (octree) světě
                        float scale = this.size / chunkSize;
                        Vector3 samplePosition = (Vector3I)(relativeEdgePosition * scale + this.position);
                        // získáme hodnotu z fieldu
                        byte fieldValue = GetFieldValue(rootNode, samplePosition);
                        // zapíšeme
                        int thisIndex = (x + (fieldSize - 1) * neighborDirection.X) + (y + (fieldSize - 1) * neighborDirection.Y) * fieldSize + (z + (fieldSize - 1) * neighborDirection.Z) * fieldSize * fieldSize;
                        activeChunk.field[thisIndex] = fieldValue;
                    }
                }
            }

        }



        /*
        public void UpdateNeighborChunkStiches()
        {
            // Získat chunky které jsou za tímto chunkem a updatenout (-x, -y, -z, -xy, ...)
            // určitě tu je nějaká optimalizace aby se nemusely generovat všechny chunky...


            // vymyslet lepší get neighbor algorithm !!
            GD.Print("active node: ", this);
            for (int i = 1; i < 8; i++)
            {
                Vector3I direction = new Vector3I(i % 2, i / 2 % 2, i / 4) * -1;
                Octree.Node neighborNode = this.GetNeighborReal(direction);
                if (neighborNode == null) continue;
                if (neighborNode.chunkIndex == -1) continue;
                //neoghborNode.SetDebugColor(new(0.0f, 0.0f, 1.0f));
                Terrain22.Chunk chunk = chunkPool.GetChunk(neighborNode.chunkIndex);

                neighborNode.StichChunks(chunk.meshData);
                
                SurfaceNet.Algorithm.GenerateNormals(ref chunk.meshData);

                Godot.ArrayMesh arrayMesh = new Godot.ArrayMesh();
                Godot.Collections.Array arrays = [];
                arrays.Resize((int)Mesh.ArrayType.Max);
                arrays[(int)Mesh.ArrayType.Vertex] = chunk.meshData.vertexPositions;
                arrays[(int)Mesh.ArrayType.Normal] = chunk.meshData.vertexNormals;
                arrays[(int)Mesh.ArrayType.Index] = chunk.meshData.indices;

                arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
                chunk.surfaceNetMesh.Mesh = arrayMesh;

                GD.Print("updateing: node: ", neighborNode);
            }

        }
        */



        /*
        public void StichEdge(Terrain22.Chunk activeChunk, Terrain22.Chunk neighborChunk)
        {
            
            //musí se vědět na jaké straně iterovat, jakej je resolution a začátek a konec.


        }
        */

    }
}


// https://github.com/josebasierra/voxel-planets/tree/master?tab=readme-ov-file
// https://josebasierra.gitlab.io/VoxelPlanets#seams