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
        }

        public void Create(System.Numerics.Vector3 position, float size)
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

    }
}