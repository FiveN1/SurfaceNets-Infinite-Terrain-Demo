using System;
using Godot;

namespace Octree
{
    public partial class Node
    {

        // Pro každou rovinu (místo kde se chunky dotýkají) vytvoří stich.
        // je jedno zda se rovina skládá z více chunků.
        public int IterateOnSubdividedPlane(Vector3I direction, Octree.Node neighbor, int sectionIndex)
        {
            // pokud octant není rozdělen, iterujem na jeho rovině.
            if (neighbor.isLeaf)
            {
                return IterateOnPlane(direction, neighbor, sectionIndex);
            }
            // pokud ale je rozdělen iterujeme na každé rovině jeho listů.
            // velikost sekce je součtem všech sekcí listů
            int sectionSize = 0;
            // získáme listy které se dotýkají s aktivním octantem
            Vector3I invDirection = new Vector3I(1, 1, 1) - direction;
            Vector3I nodePlaneSize = invDirection * 2 + direction; // POZNÁMKA: 2 je tady proto že je to délka jedné strany krychle octantů.
            // iterujem pro každý octant který se dotýká aktivního octantu
            for (int z = 0; z < nodePlaneSize.Z; z++)
            {
                for (int y = 0; y < nodePlaneSize.Y; y++)
                {
                    for (int x = 0; x < nodePlaneSize.X; x++)
                    {
                        int leafIndex = x + y * 2 + z * 4;
                        Octree.Node leaf = neighbor.leafs[leafIndex];

                        sectionSize += IterateOnSubdividedPlane(direction, leaf, sectionIndex + sectionSize);

                    }
                }
            }
            // vrátíme součet všech sekcí
            return sectionSize;
        }

        // vytvoří stich pro rovinu s jedním chunkem.
        public int IterateOnPlane(Vector3I direction, Octree.Node neighbor, int sectionIndex)
        {
            GD.Print("--- iterating on: ", neighbor);

            int vertFieldSize = Terrain22.Chunk.size;

            // velikost roviny na které bude iterovat (rovina = část kde se body dotýkají)
            Vector3I invDirection = new Vector3I(1, 1, 1) - direction;
            Vector3I planeSize = invDirection * vertFieldSize + direction;
            int sectionSize = planeSize.X * planeSize.Y * planeSize.Z;

            // rozdíl octantů
            Vector3 posDiff = (neighbor.position - this.position) * invDirection;
            float sizeDiff = this.size / neighbor.size;
            GD.Print("posDiff: ", posDiff);
            GD.Print("sizeDiff: ", sizeDiff);

            // neighbor chunk
            if (neighbor.chunkIndex == -1) return 0;
            Terrain22.Chunk neighborChunk = chunkPool.GetChunk(neighbor.chunkIndex);
            if (this.chunkIndex == -1) return 0;
            Terrain22.Chunk activeChunk = chunkPool.GetChunk(this.chunkIndex);

            // JE TU PROBLÉM JAK HOVADO, KDYŽ JE NEIGHBOR VĚTŠÍ !!
            // PROTOŽE SE ZAPISUJE STEJNÝ VERTEX VÍCEKRÁT !!

            // iterate na rovině
            for (int z = 0; z < planeSize.Z; z++)
            {
                for (int y = 0; y < planeSize.Y; y++)
                {
                    for (int x = 0; x < planeSize.X; x++)
                    {
                        // pozice v sousedovi
                        Vector3 samplePos = new(x, y, z); // POZNÁMKA: samplePos není v aktivním chunku !
                        // sample pozice v každém chunku
                        // POZNÁMKA: je zaokrouhleno na floor
                        Vector3I neighborSamplePos;
                        Vector3I coreSamplePos;
                        // pokud je soused větší, musíme scalenout sample indexy. (protože iterujeme na menším)
                        if (neighbor.size > this.size)
                        {
                            neighborSamplePos = (Vector3I)((samplePos - posDiff) * sizeDiff);
                            coreSamplePos = (Vector3I)samplePos + direction * (vertFieldSize - 1);
                        }
                        else
                        {
                            neighborSamplePos = (Vector3I)samplePos;
                            coreSamplePos = (Vector3I)((samplePos + posDiff) / sizeDiff) + direction * (vertFieldSize - 1);
                        }
                        GD.Print("\tneighborSamplePos: ", neighborSamplePos);
                        GD.Print("\tcoreSamplePos: ", coreSamplePos);

                        // vert
                        int neighborVertIndex = neighborSamplePos.X + neighborSamplePos.Y * vertFieldSize + neighborSamplePos.Z * vertFieldSize * vertFieldSize;
                        Vector3 vertOffset = direction * vertFieldSize + posDiff / sizeDiff;
                        float vertScale = 1.0f / sizeDiff;
                        Vector3 neighborVert = neighborChunk.meshData.vertexPositions[neighborVertIndex] * vertScale + vertOffset;

                        // index v sekci
                        int inSectionIndex = x + y * planeSize.X + z * planeSize.X * planeSize.Y; // jde zjednodušit !! (pokud použijeme++)
                        int index = sectionIndex + inSectionIndex;

                        // write
                        activeChunk.meshData.vertexPositions[index] = neighborVert;

                        // stich
                        // ...
                        // tohle bude peklo


                        // získat všechny chunky které se dotýkají

                        // pro každý roh:
                        // -> zjistit do jaké sekce patří.
                        // -> najít index v sekci - nějaká funkce? - sample position relativní k sekci. - iteruje se pokud je dělená.
                        // -> zapsat

                        // TADY POKRAČOVAT ! vvv

                        // ? pokud je direction složený tak vrátit

                        // tohle asi odstranit...
                        if (direction != new Vector3I(1, 0, 0))
                        {
                            continue;
                        }
                        if (y >= planeSize.Y - 1 || z >= planeSize.Z - 1)
                        {
                            continue;
                        }

                        GetStichCubeSimple(direction, neighborSamplePos, coreSamplePos, sizeDiff, activeChunk, neighborChunk, sectionIndex, planeSize);

                        // stich mezi 2 chunky

                        // CÍL:
                        // - získat case code








                    }
                }
            }

            // tady pokračovat pro všechny strany v rovině.
            // ... spoj s vedlejšími chunky
            // ...

            GD.Print("returning sectionSize: ", sectionSize, "\n");
            return sectionSize;
        }

    }
}