using Godot;

namespace WorldSystem
{
    // Svět
    //
    public partial class World : Node
    {

        Terrain.WorldTerrain worldTerrain;
        WorldSave worldSave;
        WorldGenration worldGeneration;

        [Export] public NodePath meshNodePath;
        Node3D meshNode;

        public World()
        {

        }

        public override void _Ready()
        {
            base._Ready();

            meshNode = GetNode<Node3D>(meshNodePath);

            worldTerrain = new Terrain.WorldTerrain(meshNode);
            worldSave = new WorldSave(worldTerrain.worldPosition, worldTerrain.worldSize);
            worldGeneration = new WorldGenration();

            worldTerrain.Update(new(-16.0f, 0.0f, 0.0f), worldSave);
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            // worldTerrain.Update(new(0.0f, 0.0f, 0.0f));
        }



    }

}