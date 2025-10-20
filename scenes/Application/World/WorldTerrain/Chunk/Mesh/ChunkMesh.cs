using Godot;

namespace WorldSystem.Terrain
{
    public struct ChunkMesh
    {
        public MeshInstance3D surfaceNetMesh;
        public SurfaceNet.MeshData meshData;

        // constructor
        public ChunkMesh(int fieldSize, Node3D meshNode)
        {
            surfaceNetMesh = new MeshInstance3D();
            meshNode.AddChild(surfaceNetMesh);
            meshData = new SurfaceNet.MeshData(fieldSize);
        }

        public void Generate(Vector3 nodePosition, float nodeSize, ref byte[] field, int fieldSize, int realFieldSize)
        {
            // získáme krajní field hodnoty z vedlejších chunků
            //node.GetEdgeFieldValues(); // POMALÉ! (lze snadno optimalizovat)
            // vygenerovat core mesh
            SurfaceNet.Algorithm.GenerateMeshData2(ref field, new(realFieldSize, realFieldSize, realFieldSize), 128, ref meshData, new(realFieldSize - 1, realFieldSize - 1, realFieldSize - 1)); // POMALÉ! (lze optimalizovat pomocí GPU)
            // spojit mesh s vedlejším meshem
            //node.StichChunks(ref this.meshData);
            //node.Stich2();
            // Vygenerovat normály
            SurfaceNet.Algorithm.GenerateNormals(ref this.meshData); // POMALÉ! (lze optimalizovat pomocí GPU)

            // Updatenout vedlejší chunky !
            //node.UpdateNeighborChunkStiches();

            // zapíšeme do meshe
            Godot.ArrayMesh arrayMesh = new Godot.ArrayMesh();
            Godot.Collections.Array arrays = [];
            arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = this.meshData.vertexPositions;
            arrays[(int)Mesh.ArrayType.Normal] = this.meshData.vertexNormals;
            arrays[(int)Mesh.ArrayType.Index] = this.meshData.indices;


            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays); // POMALE! (z nějakého důvodu velmi pomalé)
            surfaceNetMesh.Mesh = arrayMesh;

            // toho se můžu zbavit ?
            surfaceNetMesh.Position = nodePosition;
            surfaceNetMesh.Scale = new Vector3(nodeSize, nodeSize, nodeSize) * (1.0f / (float)fieldSize); // ((float)realFieldSize - 2.0f)
        }

        public void Enabled(bool enabled)
        {
            surfaceNetMesh.Visible = enabled;
        }
        
    }
}
