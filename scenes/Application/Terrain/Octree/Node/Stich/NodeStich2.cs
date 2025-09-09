using System;
using Godot;

namespace Octree
{
    public partial class Node
    {

        // Spojí mesh chunku s všemi vedlejšími chunky
        public void Stich2()
        {
            
            if (this.position != new Vector3(2, 0, 0))
            {
                return;
            }
            SetDebugColor(new Color(0.0f, 1.0f, 0.0f));

            GD.Print("--- Stiching");

            int vertFieldSize = Terrain22.Chunk.size;
            int coreSectionSize = vertFieldSize * vertFieldSize * vertFieldSize;
            int sectionindex = coreSectionSize;

            for (int i = 1; i < 2; i++)
            {
                Vector3I neighborDirection = new(i % 2, i / 2 % 2, i / 4);
                sectionindex += StichDirection(neighborDirection, sectionindex);
            }

        }

        /*
        * Pro StichDirection():
        * -> vytvořit sekci o velikosti
        * -> zapisovat vertexy
        * -> pokud casecube nejde out of bounds, spojit a vytvořit indexy.
        *
        * Takhle by se nemělo nic opakovat a vše by mělo proběhnout v jednom loopu
        *
        *
        */
        public int StichDirection(Vector3I direction, int sectionindex)
        {

            // získáme souseda stejné nebo větší velikosti
            Octree.Node neighbor = this.GetNeighborReal(direction);
            if (neighbor == null) return 0;
            GD.Print("active node:", this);
            GD.Print("neighbor node:", neighbor);

            // získáme všechny vertexy
            int sectionSize = IterateOnSubdividedPlane(direction, neighbor, sectionindex);
            GD.Print("final section size: ", sectionSize);

            // vrátíme nová index
            return sectionSize;
        }


    }
}