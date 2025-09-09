using System;
using Godot;

namespace Octree
{
    public partial class Node
    {
        //
        // Section (tohle se používá pouze při tvorbě pole s vertexy, takže ne tady)
        //

        // jednoduše spočítá velikost roviny která vzniká dotykem dvou oktantů ve směru.
        // není potřeba ani zadávat octanty, pouze směr
        public int GetVertPlaneSize(Vector3I direction)
        {
            int vertFieldSize = Terrain22.Chunk.size;
            Vector3I invDirection = new Vector3I(1, 1, 1) - direction;
            // zjistíme velikost roviny, která je tvořena dotekem octantů.
            Vector3I vertPlaneSize = invDirection * vertFieldSize + direction;
            int vertCount = vertPlaneSize.X * vertPlaneSize.Y * vertPlaneSize.Z;
            return vertCount;
        }

        public int GetAllVertPlaneSizes(Vector3I direction, Octree.Node neighbor, int vertCount)
        {
            Vector3I invDirection = new Vector3I(1, 1, 1) - direction;
            Vector3I nodePlaneSize = invDirection * 2 + direction;
            // iterujem pro každý octant který se dotýká aktivního octantu
            for (int z = 0; z < nodePlaneSize.Z; z++)
            {
                for (int y = 0; y < nodePlaneSize.Y; y++)
                {
                    for (int x = 0; x < nodePlaneSize.X; x++)
                    {
                        int leafIndex = x + y * 2 + z * 4;
                        Octree.Node leaf = neighbor.leafs[leafIndex];
                        // pokud je pod-octant rozdělen, iterujem dál
                        // jinak získáme vert plane size pomocí směru.
                        if (!leaf.isLeaf)
                        {
                            vertCount += GetAllVertPlaneSizes(direction, leaf, vertCount);
                        }
                        else
                        {
                            vertCount += GetVertPlaneSize(direction);
                        }
                    }
                }
            }
            return vertCount;
        }

        // získání velikosti sekce pro daný směr
        public int GetSectionSize(Vector3I direction, Octree.Node neighbor)
        {
            if (!neighbor.isLeaf)
            {
                return GetAllVertPlaneSizes(direction, neighbor, 0);
            }
            return GetVertPlaneSize(direction);
        }

        public int GetAllSectionSize()
        {
            // velikst všech sekcí ...
            for (int i = 1; i < 2; i++)
            {
                Vector3I neighborDirection = new(i % 2, i / 2 % 2, i / 4);

            }
            return 0;
        }

        
    }
}