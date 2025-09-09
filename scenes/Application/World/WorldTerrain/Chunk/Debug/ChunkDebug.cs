using Godot;

namespace WorldSystem.Terrain
{
    //
    public struct ChunkDebug
    {
        // shader debug context
        ChunkDebugContext context;

        // debug visuals
        MeshInstance3D debugBox;
        ShaderMaterial debugMaterial;
        Color debugColor;

        // constructor
        public ChunkDebug(ChunkDebugContext chunkDebugContext)
        {
            // context set
            this.context = chunkDebugContext;
            // mesh
            this.debugBox = new MeshInstance3D();
            this.context.debugMeshNode.AddChild(debugBox);
            this.debugBox.Visible = context.enabled;
            // mesh shader
            this.debugColor = new Color(1.0f, 1.0f, 1.0f);
            this.debugMaterial = new ShaderMaterial();
            debugMaterial.Shader = this.context.debugMaterialShader;
            debugMaterial.SetShaderParameter("color", this.debugColor);
        }

        public void Set(Vector3 position, float size)
        {
            if (!context.enabled) return;
            this.debugBox.Visible = context.enabled;
            // mesh
            this.debugBox.Mesh = GenerateAABBCube();
            this.debugBox.Position = position;
            this.debugBox.Scale = new Vector3(size, size, size);
            // material
            this.debugBox.SetSurfaceOverrideMaterial(0, debugMaterial);
        }

        public void SetColor(Color color)
        {
            this.debugColor = color;
            debugMaterial.SetShaderParameter("color", this.debugColor);
        }

        //
        //
        //

        private Godot.ArrayMesh GenerateAABBCube()
        {
            Vector3[] vertexPositions = new Vector3[8];
            int[] indices = new int[24];

            int writeIndex = 0;
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        int i = x + y * 2 + z * 4;
                        Vector3 vertexPosition = new(x, y, z);
                        vertexPositions[i] = vertexPosition;

                        if (x > 0)
                        {
                            indices[writeIndex] = i;
                            writeIndex++;
                            int i2 = 0 + y * 2 + z * 4;
                            indices[writeIndex] = i2;
                            writeIndex++;
                        }
                        if (y > 0)
                        {
                            indices[writeIndex] = i;
                            writeIndex++;
                            int i2 = x + 0 * 2 + z * 4;
                            indices[writeIndex] = i2;
                            writeIndex++;
                        }
                        if (z > 0)
                        {
                            indices[writeIndex] = i;
                            writeIndex++;
                            int i2 = x + y * 2 + 0 * 4;
                            indices[writeIndex] = i2;
                            writeIndex++;
                        }
                    }
                }
            }

            Godot.ArrayMesh arrayMesh = new Godot.ArrayMesh();
            Godot.Collections.Array arrays = [];
            arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = vertexPositions;
            arrays[(int)Mesh.ArrayType.Index] = indices;

            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Lines, arrays); // POMALE! (z nějakého důvodu velmi pomalé)
            return arrayMesh;
        }

    }
}