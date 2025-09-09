using Godot;
using System;

// Renderovaný terén z worldSave
// 
// Octree LOD system
// Dual-Contouring system
//
namespace WorldSystem.Terrain
{
    public class WorldTerrain
    {
        //
        public const int worldScale = 2;
        public readonly float worldSize;
        public readonly System.Numerics.Vector3 worldPosition;

        // chunk octree
        private DataStructures.Octree<int> octree;
        private DataStructures.FragArray<Chunk> chunks;

        // chunk context
        private ChunkContext chunkContext;

        // constructor
        public WorldTerrain(Node3D meshNode)
        {
            // výpočet rozměrů světa podle worldScale
            worldSize = Mathf.Pow(2, worldScale) * Terrain22.Chunk.size;
            worldPosition = new System.Numerics.Vector3(-worldSize / 2.0f);

            // init struktur
            octree = new DataStructures.Octree<int>(worldPosition, worldSize);
            chunks = new DataStructures.FragArray<Chunk>();

            // chunk context
            chunkContext = new ChunkContext(meshNode);
            chunkContext.DebugEnabled(true);
        }


        public void Update(System.Numerics.Vector3 povPosition, WorldSave worldsave)
        {
            SubdivideIfClose(octree.rootIndex, povPosition, 0, worldScale, worldsave);
        }

        //
        //
        //

        private void SubdivideIfClose(int octantIndex, System.Numerics.Vector3 position, int iterationCount, int maxIterations, WorldSave worldSave) // max iterations
        {
            ref DataStructures.Octant<int> octant = ref octree.GetOctant(octantIndex);
            //
            // Update počtu iterace
            //
            iterationCount++;
            // pokud byla dosažena maximální iterace tak pouze nastavíme jeho panel
            if (iterationCount > maxIterations)
            {
                AssignChunk(ref octant, worldSave);
                // add chunk pro octant
                return;
            }
            //
            // Distance funkce
            //
            // pomocí distance funkce zjistíme zda by měl být node rozdělen na větší detail
            bool isClose = CheckIfNodeClose(octant, position);
            //
            // Operace bodu
            //
            // pokud by měl být orzdělen ale není (je blízko), rozdělíme ho.
            if (isClose && octant.isLeaf)
            {
                octree.SubdivideOctant(octantIndex);
                // jelikož se možná změnila adresa tak ho musíme znovu získat
                octant = ref octree.GetOctant(octantIndex);
                // potom se bude iterovat nad jeho listy...
                //activeNode.Subdivide();
            }
            // pokud by neměl být rozdělen ale je (není blízko), přemeníme ho na list
            if (!isClose && !octant.isLeaf)
            {
                octree.UnSubdivideOctant(octantIndex);
                //activeNode.Unsubdivide();
            }
            //
            // Následné operace
            //
            // pokud je po operaci listem, přidáme mu panel.
            if (octant.isLeaf)
            {
                AssignChunk(ref octant, worldSave);
                return;
            }
            // pokud není po operaci listem, iterujem nad jeho listy
            // POZNÁMKA: iterujem od (+)konce aby bylo možné vytvořit spoje mezi chunky a nemuseli jsem iterovat znovu přes celý strom.
            for (int i = 7; i >= 0; i--)
            {
                SubdivideIfClose(octant.leafs[i], position, iterationCount, maxIterations, worldSave);
            }
        }

        private bool CheckIfNodeClose(DataStructures.Octant<int> octant, System.Numerics.Vector3 position)
        {
            float renderDistanceScale = 1.0f;

            System.Numerics.Vector3 CellCenterPos = octant.position + new System.Numerics.Vector3(octant.size) * 0.5f;
            float dist = (CellCenterPos - position).Length();
            if (dist < octant.size * renderDistanceScale) return true;
            return false;
        }

        private int FetchChunk()
        {
            int chunkIndex = chunks.AddEmpty();
            ref Chunk chunk = ref chunks.Get(chunkIndex);
            // pokud je chunk fresh (nebyl předtím použit) tak ho inicializujem
            if (chunk.Equals(default(WorldSystem.Terrain.Chunk)))
            {
                chunk = new(chunkContext);
            }
            return chunkIndex;
        }

        private void AssignChunk(ref DataStructures.Octant<int> octant, WorldSave worldSave)
        {
            int chunkIndex = FetchChunk();
            ref Chunk chunk = ref chunks.Get(chunkIndex);
            // načteme hodnoty chunku
            // * také získat neighbor hodnoty !
            worldSave.LoadChunk(ref chunk, octant.position, octant.size);
            // vytvoříme mesh
            chunk.Create(octant.position, octant.size);
            // assign chunk to octant
            octant.value = chunkIndex;
        }























        /*
        public override void _Ready()
        {
            base._Ready();
            GD.Print("Happy World Daddy");

            NodeMaterial = GD.Load<StandardMaterial3D>("res://scenes/Application/Terrain/StandardMaterial1.tres"); // res://scenes/Application/Terrain/StandardTransparentMaterial.tres

            MeshNode = GetNode<Node3D>(MeshNodePath);
            GD.Print("node: ", MeshNode, " path: ", MeshNodePath);

            Player = GetNode<CharacterBody3D>(PlayerPath);

            WorldGen = GetNode<WorldGenerator>(WorldGeneratorPath);

            Octree.Tree.worldGen = GetNode<WorldGenerator>(WorldGeneratorPath);



            ChunkPool chunkPool = new ChunkPool(MeshNode);

            //GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;


            Octree.Node.SetMeshParentNode(MeshNode);
            Octree.Node.EnableDebugVisuals(true);
            Octree.Node.SetChunkQueue(chunkPool);

            float worldSize = Mathf.Pow(2, WorldScale) * Terrain22.Chunk.size;
            float worldPosition = -worldSize / 2;
            rootNode = new Octree.Node(new(worldPosition, worldPosition, worldPosition), worldSize, null);
            Octree.Tree.SubdivideIfClose(rootNode, new(0, 0, 0), 0, WorldScale);
            GD.Print("world created of size: ", worldSize, " units");
        }

        public override void _PhysicsProcess(double delta)
        {
            base._PhysicsProcess(delta);

            Vector3 PovPoint = Player.Position - Position;

            Octree.Tree.SubdivideIfClose(rootNode, PovPoint, 0, WorldScale);
        }

        public void ModifyTerrain(Vector3 collisionPoint)
        {
            Vector3 localCollisionPoint = collisionPoint - this.Position;

            int fieldSize = Terrain22.Chunk.fieldSize;

            GD.Print("--- modifying");


            for (int z = (int)localCollisionPoint.Z - 1; z <= (int)localCollisionPoint.Z + 1; z++)
            {
                for (int y = (int)localCollisionPoint.Y - 1; y <= (int)localCollisionPoint.Y + 1; y++)
                {
                    for (int x = (int)localCollisionPoint.X - 1; x <= (int)localCollisionPoint.X + 1; x++)
                    {
                        Vector3I modifyerCollisionPoint = new(x, y, z);
                        GD.Print("point: ", modifyerCollisionPoint);

                        Octree.Node collidingNode = rootNode.GetNodeWithPosition(modifyerCollisionPoint);
                        Vector3 localChunkCollisionPoint = modifyerCollisionPoint - collidingNode.position;

                        int fieldIndex = (int)localChunkCollisionPoint.X + (int)localChunkCollisionPoint.Y * fieldSize + (int)localChunkCollisionPoint.Z * fieldSize * fieldSize;

                        Terrain22.Chunk chunk = Octree.Node.chunkPool.GetChunk(collidingNode.chunkIndex);

                        if (chunk.field[fieldIndex] < 255 - 4)
                        {
                            chunk.field[fieldIndex] += 4;
                        }
                        //collidingNode.GenerateChunkMesh(chunk);
                    }
                }
            }

            Octree.Node collidingNode1 = rootNode.GetNodeWithPosition(localCollisionPoint);
            Terrain22.Chunk chunk1 = Octree.Node.chunkPool.GetChunk(collidingNode1.chunkIndex);
            collidingNode1.GenerateChunkMesh(chunk1);

        }
        */


    }
}



/*
*
*                [Root]
*       [Chunk1], ... , [Chunk8]
* [Chunk1] ...
*
* Pro každý chunk může být 8 dalších chunků
*
*/

// PROBLÉM:
// objekty se neuvolnujou!!

/*
* Zdroje:
*
* DUAL CONTOURING:
* https://www.cs.rice.edu/~jwarren/papers/dualcontour.pdf
* https://www.mattkeeter.com/projects/contours/
* https://www.reddit.com/r/Unity3D/comments/ieenax/highaccuracy_dual_contouring_on_the_gpu_tech/
* https://voxel-tools.readthedocs.io/en/latest/smooth_terrain/
* https://www.graphics.rwth-aachen.de/publication/131/feature1.pdf
*
* TEXTURY:
* https://gamedev.stackexchange.com/questions/53272/how-to-texture-a-surface-generated-by-marching-cubes-algorithm
*
* CHUNK BLEND:
* https://ngildea.blogspot.com/2014/09/dual-contouring-chunked-terrain.html
* https://www.evl.uic.edu/vchand2/thesis/papers/Marching%20Cubes.pdf
* https://transvoxel.org//
* TransVoxel Algorithm
* https://www.youtube.com/watch?v=U6WhkL3IVYg&t=85s
*/

/*
* Plán:
*
* PRIORITA | PLÁN
*   (1)     [začistit kod]
*   (3)     [Vytvořit mesh z fieldu pomocí dual contouring]
*   (x)     [Vytvořit Octree terén]                         [HOTOVO]
*   (2)     [Implementovat ukládání octree]
*   (4)     [Různé materiály / textury]
*   (5)     [GPU Akcelerace]
*   (1)     [Chunk Blending]
*   (3)     [Isolevel zaležící na velikosti buněk!]
*   (5)     [Vzdálené chunky se nebudou renderovat?]
*   (1)     [Debug Mesh]
*/

// Každý chunk bude mít vlastní field, a mesh na určené velikosti. 
// místo add mesh funkce bude generate, kde field bude inicializován a nasteven pomocí noise funkce

/*
* Změny
*
* [16.07.2025] Vytvořeno, teď organizuji své scény a skripty dohromady.
* Tento nový terén bude založen na Octree systému a bude tvořen pomocí Dual Contouring metody nebo EMC metody ještě uvidím.
* Taky se tady budu snažit držet kod co nejlepší pro snadnou upravu v budoucnu.
* [17.07.2025] Přidán Octree systém
* [18.07.2025] Implementován jednoduchý dynamický Octree systém.
* [19.07.2025] implementace funkce hledání sousedů v octree.
* [20.07.2025] snaha přidat funkční chunk blending.
* [09.09.2025] teď je vše nádherné. (všechno rozděleno do svých tříd, bez použití static)
*/
