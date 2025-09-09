using Godot;
using System;
using System.IO;
// PLÁN NA ČISTÝ KOD!
// - nepoužívat static variable
// - nepoužívat inheritanci

public partial class Terrain : Node3D
{
    // Data třídy
    #region Data

    [Export] public NodePath MeshNodePath;
    Node3D MeshNode;
    StandardMaterial3D NodeMaterial;
    //OctreeNode RootNode;


    // TOHLE JE NAHOVNO POUŽÍVAT EVENT/SIGNAL!!
    [Export] public NodePath PlayerPath;
    CharacterBody3D Player;

    [Export] public NodePath WorldGeneratorPath;
    WorldGenerator WorldGen;

    int WorldScale;



    Octree.Node rootNode;


    #endregion Data

    // Funkce třídy
    #region Funkce
    public Terrain()
    {

    }

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

        WorldScale = 8;



        ChunkPool chunkPool = new ChunkPool(MeshNode);

        //GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;


        Octree.Node.SetMeshParentNode(MeshNode);
        Octree.Node.EnableDebugVisuals(false);
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


    #endregion Funkce

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
*/
