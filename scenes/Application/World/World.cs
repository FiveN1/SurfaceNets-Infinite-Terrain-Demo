using Godot;

namespace WorldSystem
{
    // Svět
    //
    public partial class World : Node
    {

        Terrain.WorldTerrain worldTerrain; // private?
        Save.WorldSave worldSave;

        [Export] public NodePath meshNodePath;
        Node3D meshNode;

        public bool LODUpdateDisabled = false;

        public World()
        {

        }

        public override void _Ready()
        {
            base._Ready();

            // ZMĚNIT DEBUG !!
            // - aby se mřížka ukazovala pouze u jednoho chunku.
            // - aby pro každý chunk nemusel být debug.

            meshNode = GetNode<Node3D>(meshNodePath);

            worldTerrain = new Terrain.WorldTerrain(meshNode);
            // world save
            string worldName = "world1";
            int worldSeed = 8;
            worldSave = new Save.WorldSave(worldTerrain.worldPosition, worldTerrain.worldSize, worldName, worldSeed);

            UpdatePov(new(0.0f, 0.0f, 0.0f));


            int size = 8;
            for (int y = 0; y < size; y++)
            {
                string row = "";
                for (int x = 0; x < size; x++)
                {
                    row += Terrain.Chunk.GetFieldIndex2(x, y).ToString() + ", ";
                }
                GD.Print(row);
            }

        }

        // vygeneruje world octree strukturu podle toho kde se pov pozice nachází.
        public void UpdatePov(Vector3 povPosition)
        {
            if (LODUpdateDisabled) return;
            System.Numerics.Vector3 SysVector3 = new(povPosition.X, povPosition.Y, povPosition.Z);
            worldTerrain.UpdatePov(SysVector3, worldSave);
        }


        public void ModifyTerrain(Vector3 collisionPoint, bool extrude)
        {
            System.Numerics.Vector3 SysNumcollisionPoint = new(collisionPoint.X, collisionPoint.Y, collisionPoint.Z);
            worldTerrain.ModifyTerrain(SysNumcollisionPoint, extrude, worldSave);
        }


    }

}


// čistý kod!!
// když je někde duplikát funkce, většinou to lze zobecnit nebo udělat override.

/*
*
*
* ... úplně přepsáno.
* [17.09.2025] funkční save system. 
*
*
*/