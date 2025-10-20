using Godot;
using System;

namespace WorldSystem.Terrain
{

    public class TerrainSubdivision
    {
        /*
        public void Subdivide(int octantIndex, ref WorldTerrain terrain)
        {
            // POZNÁMKA: jelikož se tady přidávají data, je možné že se změní adresy elementů v array (kvůli reallokaci)

            // check zda už byl subdividován
            if (!terrain.octree.octants.Get(octantIndex).isLeaf) return;
            // předem zapíšeme data o octantu
            System.Numerics.Vector3 octantPosition = terrain.octree.octants.Get(octantIndex).position;
            float octantSize = terrain.octree.octants.Get(octantIndex).size;
            // vytvoříme 8 listů
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        int leafOctantIndex = terrain.octree.octants.Add(new DataStructures.Octant<int>(octantIndex, new System.Numerics.Vector3(x, y, z) * octantSize * 0.5f + octantPosition, octantSize * 0.5f));
                        terrain.octree.octants.Get(octantIndex).leafs[x + y * 2 + z * 4] = leafOctantIndex;
                        terrain.octree.octants.Get(leafOctantIndex).value = int.MaxValue;
                    }
                }
            }
            // teď už není listem
            terrain.octree.octants.Get(octantIndex).isLeaf = false;
        }
        */
    }
}