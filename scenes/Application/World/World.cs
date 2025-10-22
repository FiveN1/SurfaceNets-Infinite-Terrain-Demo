using Godot;

namespace WorldSystem
{
    // ## Svět
    //
    // obsahuje terrén
    // je rozdělen do chunků podle LOD. kde největší LOD je v místě kde se nachází hráč/kamera.
    public partial class World : Node
    {
        // hlavní systémy světa.
        // Terrain rendering & World Save
        private Terrain.WorldTerrain worldTerrain;
        private Save.WorldSave worldSave;

        // mesh node
        // tento bod bude ve scéně držet všechny meshe a kolizivní tvary chunků.
        [Export] public NodePath meshNodePath;
        Node3D meshNode;

        public bool LODUpdateDisabled = false;



        public override void _Ready()
        {
            // call base ready
            base._Ready();

            // použito jenom tady pro načtení světa
            string worldName = "world1";
            int worldSeed = 8;
            int worldScale = 3;

            // získáme referenci na bod který je v scéně
            meshNode = GetNode<Node3D>(meshNodePath);
            // construct systems
            // vytvoříme terrain system
            worldTerrain = new Terrain.WorldTerrain(meshNode, worldScale);
            // vytvoříme world save system
            worldSave = new Save.WorldSave(worldTerrain.worldPosition, worldTerrain.worldSize, worldName, worldSeed);
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

// ZMĚNIT DEBUG !!
// - aby se mřížka ukazovala pouze u jednoho chunku.
// - aby pro každý chunk nemusel být debug.


/* Změny
*
*
* ... úplně přepsáno.
* [17.09.2025] funkční save system. 
*
*
*/