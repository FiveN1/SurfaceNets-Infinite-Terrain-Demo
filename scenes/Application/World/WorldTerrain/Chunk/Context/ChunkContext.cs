using Godot;

namespace WorldSystem.Terrain
{
    public class ChunkContext
    {
        public readonly Node3D meshNode;
        public readonly ChunkDebugContext chunkDebugContext;

        public ChunkContext(Node3D meshNode)
        {
            this.meshNode = meshNode;
            chunkDebugContext = new ChunkDebugContext(this.meshNode);
        }

        public void DebugEnabled(bool enabled)
        {
            chunkDebugContext.enabled = enabled;
        }
    }
}