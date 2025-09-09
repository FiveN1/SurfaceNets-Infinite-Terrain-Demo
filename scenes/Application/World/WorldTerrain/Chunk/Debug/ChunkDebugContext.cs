using Godot;

namespace WorldSystem.Terrain
{
    public class ChunkDebugContext
    {
        public bool enabled;

        public readonly Node3D debugMeshNode;
        public readonly Shader debugMaterialShader;

        // constructor
        public ChunkDebugContext(Node3D meshNode)
        {
            enabled = false;

            this.debugMeshNode = meshNode;

            debugMaterialShader = GD.Load<Shader>("res://scenes/Application/World/WorldTerrain/Chunk/Shader/NodeDebugShader.gdshader");
        }
    }
}

// možná přidat nejakou lepší vizualizaci
// -> třeba minimapa v rohu nejbložších chunků?
// -> lajny místo volume