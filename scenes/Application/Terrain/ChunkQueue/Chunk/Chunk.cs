using Godot;
using System;
using System.IO;

namespace Terrain22
{
    public partial class Chunk
    {
        //
        // Data
        //

        public byte[] field;
        public static readonly int size;
        public static readonly int fieldSize;

        //
        // Visual
        //

        //static Node3D meshParentNode;
        public MeshInstance3D surfaceNetMesh;
        //Vector3[] vertexPositions;

        public SurfaceNet.MeshData meshData;

        public bool usedbyNode;

        // 
        // COLLISION
        // 

        StaticBody3D staticBody;

        ConcavePolygonShape3D concavePolygonShape;
        CollisionShape3D collisionShape;

        //
        // Constructor
        //

        static Chunk()
        {
            size = 8;
            fieldSize = size + 1;
        }
        public Chunk(Node3D meshParentNode)
        {
            // field
            this.field = new byte[fieldSize * fieldSize * fieldSize];
            for (int i = 0; i < fieldSize * fieldSize * fieldSize; i++)
            {
                this.field[i] = 0;
            }
            // mesh
            this.surfaceNetMesh = new MeshInstance3D();
            meshParentNode.AddChild(surfaceNetMesh);
            meshData = new SurfaceNet.MeshData(size);

            // collision
            staticBody = new StaticBody3D();
            surfaceNetMesh.AddChild(staticBody);
            collisionShape = new CollisionShape3D();
            staticBody.AddChild(collisionShape);

            concavePolygonShape = new ConcavePolygonShape3D();

            collisionShape.Shape = concavePolygonShape;

            // hotovo
            GD.Print("new Chunk Generated");
        }

        ~Chunk()
        {

        }

        //
        //
        //

        public void LoadField(WorldGenerator worldGen, float nodeSize, Vector3 nodePosition)
        {

            // new nebo ze save
            // načíst také okolní hodnoty?
            
            // nažíst z worldsave


            GenerateNewField(worldGen, nodeSize, nodePosition);
            //GD.Print("chunk from: ", filePath, " not found, generated new");
        }


        void GenerateNewField(WorldGenerator worldGen, float nodeSize, Vector3 nodePosition)
        {
            for (int z = 0; z < size; z++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        int FieldIndex = x + y * fieldSize + z * fieldSize * fieldSize;
                        Vector3 Coords = new Vector3(x, y, z) * (nodeSize / (float)size) + nodePosition; // (FieldSize - 2.0f)
                        //Field[x + z * FieldSize * FieldSize] = 0;
                        this.field[FieldIndex] = worldGen.GetValue(Coords.X, Coords.Y, Coords.Z);
                    }
                }
            }
        }

        public void GenerateMesh(Vector3 nodePosition, float nodeSize, Octree.Node node)
        {
            // získáme krajní field hodnoty z vedlejších chunků
            node.GetEdgeFieldValues(); // POMALÉ! (lze snadno optimalizovat)
            // vygenerovat core mesh
            SurfaceNet.Algorithm.GenerateMeshData2(ref this.field, new(fieldSize, fieldSize, fieldSize), 128, ref this.meshData, new(fieldSize - 1, fieldSize - 1, fieldSize - 1)); // POMALÉ! (lze optimalizovat pomocí GPU)
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
            surfaceNetMesh.Position = nodePosition;
            surfaceNetMesh.Scale = new Vector3(nodeSize, nodeSize, nodeSize) * (1.0f / (float)size); // ((float)FieldSize - 2.0f)


            GenerateCollision(nodePosition, nodeSize);
        }


        // JDE OPTIMALIZOVAT NA GPU !
        //
        public void GenerateCollision(Vector3 nodePosition, float nodeSize)
        {
            int arrayLength = this.meshData.indicesSize; //this.meshData.indices.Length;
            Vector3[] verticesForCollision = new Vector3[arrayLength]; // zbytečně dlouhý !!

            for (int i = 0; i < arrayLength; i++)
            {
                verticesForCollision[i] = meshData.vertexPositions[meshData.indices[i]];
            }

            concavePolygonShape.SetFaces(verticesForCollision);
        }



        public void SaveInFile(Vector3 nodePosition)
        {
            string appDataDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            appDataDirectory = Path.Combine(appDataDirectory, "p1ga", "chunks");

            System.IO.Directory.CreateDirectory(appDataDirectory);



            string filename = "chunk." + nodePosition.X.ToString() + "." + nodePosition.Y.ToString() + "." + nodePosition.Z.ToString() + ".dat";
            using (FileStream fs = File.Create(Path.Combine(appDataDirectory, filename)))
            {
                // write data   
                fs.Write(this.field, 0, fieldSize * fieldSize * fieldSize);
            }

            //GD.Print("saved chunk: ", filename);

            // název bude pozice chunku.
            // chunk musí být nejmenší. (nesmí být nižší kvality)
        }

        public void Enable()
        {
            this.usedbyNode = true;
            this.surfaceNetMesh.Visible = true;
            this.collisionShape.Disabled = false;

        }

        public void Disable()
        {
            this.usedbyNode = false;
            this.surfaceNetMesh.Visible = false;
            this.collisionShape.Disabled = true;
        }
    }
}