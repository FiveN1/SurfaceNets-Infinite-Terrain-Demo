using Godot;
using WorldSaveSystem;



namespace WorldSystem
{

    public partial class World : Node
    {

        const int worldScale = 8;

        WorldSave worldSave;

        public World()
        {

            // world size
            // atd...


            worldSave = new WorldSave(new(0.0f, 0.0f, 0.0f), 1.0f);
        }

        public override void _Ready()
        {
            base._Ready();
        }



    }

}