using Godot;
using System;

// čistší implementace octree systému
// JE TADY BORDEL!!
// NEVÍM JAK UDĚLAT DOBREJ CHUNK SYSTÉM!!!
// -> proč chci udělat dobrej chunk systém?
// abych potom mohl snadno implementovat save system atd...
// -> proč nejdřív nepřidám ty funkce a potom chunk systém?
// tak jsem jenom kokot, jsem zjistil.
namespace Octree
{
    // Node / Chunk
    // ZAČISTIT !!
    // ROZDĚLIT DO NĚKOLIKA SOUBORŮ !
    public partial class Node // Partial class? rozdělit různé funkce do souborů.
    {
        //
        // Data
        //
        public bool isLeaf;
        Octree.Node[] leafs;
        Octree.Node Parent;

        //
        // Spacial data
        //

        public readonly float size;
        public readonly Vector3 position;

        //
        // Constructor / Destructor
        //

        static Node()
        {
            InitDebugStatic();
        }

        public Node(Vector3 NodePosition, float NodeSize, Octree.Node ParentNode)
        {
            this.isLeaf = true;
            this.leafs = new Octree.Node[8];
            this.Parent = ParentNode;

            // tohle do chunku? (ne?)
            this.size = NodeSize;
            this.position = NodePosition;

            InitDebug();
            InitChunk();

        }

        ~Node()
        {
            GD.Print("OctreeNode Deleted!");
        }

        //
        // Subdivide funkce
        //

        // Rozdělí tento list na 8 listů, každý o polovinu menší než tento.
        // Bod nesmí být předem rozdělen
        public void Subdivide()
        {
            // check zda už byl subdividován
            if (!this.isLeaf) return;
            // vytvoříme 8 listů
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        int LeafIndex = x + y * 2 + z * 4;
                        Octree.Node leaf = new Octree.Node(new Vector3(x, y, z) * size * 0.5f + position, size * 0.5f, this);
                        this.leafs[LeafIndex] = leaf;
                    }
                }
            }
            this.RemoveVisual();
            // teď už není listem
            this.isLeaf = false;
        }

        // Odstraní všechny listy tohoto bodu, tímto se stane listem.
        public void Unsubdivide()
        {
            // check zda nebyl subduvudován
            if (this.isLeaf) return;
            // pro každý list -> unsubdivide dokud nenarazíme na dno
            // potom můžeme smazat když jsou všechny listy pryč
            for (int LeafIndex = 0; LeafIndex < 8; LeafIndex++)
            {
                this.leafs[LeafIndex].RemoveVisual();
                this.leafs[LeafIndex].Unsubdivide();
                this.leafs[LeafIndex] = null;
            }
            // teď je list
            this.isLeaf = true;
        }

        //
        //
        //

        public void SetVisual()
        {
            CreateDebugBox();
            CreateChunk();
        }

        public void RemoveVisual()
        {
            RemoveDebugBox();
            RemoveChunk();
        }

        //
        // Get funkce
        //

        // Vrátí bod pod tímto bodem na indexu 0 až 7
        // ## Parametry:
        // - int leafIndex -> index pod bodu, (0 až 7)
        // ## Return:
        // - Octree.Node Node -> bod pod tímto bodem na indexu.
        public Octree.Node GetLeaf(int leafIndex)
        {
            if (this.isLeaf) return null;
            if (leafIndex < 0 || leafIndex >= 8) return null;
            return this.leafs[leafIndex];
        }

        public Octree.Node GetParent()
        {
            return this.Parent;
        }

        /*
        public Octree.Node GetNeighbor(Vector3I Direction)
        {
            // pokud se jedná o kořen tak nemá sousedy.
            if (Parent == null) return null;
            // Získáme index v rodičovi.
            Vector3I positionInParent = (Vector3I)((this.position - Parent.position) / this.size) + Direction;
            //GD.Print("position in parent: ", positionInParent);

            int indexInParent = positionInParent.X + positionInParent.Y * 2 + positionInParent.Z * 4;
            // pokud je index mimo tak iterujem dál.
            if (indexInParent < 0 || indexInParent > 7 || positionInParent.X > 1 || positionInParent.Y > 1 || positionInParent.Z > 1 || positionInParent.X < 0 || positionInParent.Y < 0 || positionInParent.Z < 0) return Parent.GetNeighbor(Direction);
            // pokud je vše vpohodě, vrátíme souseda, který je také sourozenec.
            return Parent.GetLeaf(indexInParent);
        }
        */

        public bool ContainsPosition(Vector3 point)
        {
            Vector3I maxBounds = (Vector3I)this.position + new Vector3I((int)this.size, (int)this.size, (int)this.size);

            if (point.X >= this.position.X && point.Y >= this.position.Y && point.Z >= this.position.Z &&
            point.X < maxBounds.X && point.Y < maxBounds.Y && point.Z < maxBounds.Z)
            {
                return true;
            }
            return false;
        }

        Octree.Node GetRootNode()
        {
            if (this.Parent == null)
            {
                return this;
            }
            return this.Parent.GetRootNode();
        }

        public Octree.Node GetNodeWithPosition(Vector3 point)
        {
            // pokud neobsahuje pozici tak se vrací null
            if (!this.ContainsPosition(point))
            {
                return null;
            }
            // pokud není listem, najdem list který obsahuje pozici a iterujem
            if (!this.isLeaf)
            {
                for (int leafIndex = 0; leafIndex < 8; leafIndex++)
                {
                    Octree.Node leaf = this.leafs[leafIndex];
                    if (leaf.ContainsPosition(point))
                    {
                        return leaf.GetNodeWithPosition(point);
                    }
                }
            }
            // jinak vracíme tohle
            return this;
        }

        public Octree.Node GetNeighbor(Vector3I direction)
        {
            // pokud se jedná o kořen tak nemá sousedy.
            // sem se třeba dojde pokud hledáme souseda mimo bounds
            if (this.Parent == null) return null;
            // Získáme index v rodičovi.
            Vector3I thisPosInParent = (Vector3I)((this.position - Parent.position) / this.size);
            Vector3I neighborPosInParent = thisPosInParent + direction;
            // check zda soused je bratrem
            if (neighborPosInParent.X > 1 || neighborPosInParent.X < 0 || neighborPosInParent.Y > 1 || neighborPosInParent.Y < 0 || neighborPosInParent.Z > 1 || neighborPosInParent.Z < 0)
            {
                // transformujem pozici aby místo (0, 1) byla (-1, 1)
                Vector3I transThisPosInParent = thisPosInParent * 2 - new Vector3I(1, 1, 1);
                // tímto jednoduchým trikem spravíme směr. protože pokud by směr nebyl na hraně tak by byl špatný.
                direction = (direction + transThisPosInParent) / 2;
                // iterujem na rodiči
                return this.Parent.GetNeighbor(direction);
            }
            int indexInParent = neighborPosInParent.X + neighborPosInParent.Y * 2 + neighborPosInParent.Z * 4;
            // pokud je vše vpohodě, vrátíme souseda, který je také sourozenec.
            return this.Parent.GetLeaf(indexInParent);
            

            // tady iterovat dokud soused nebude stejné velikosti ?

        }

        public Octree.Node GetNodeOfSizeAtPoint(Vector3 point, float minSize)
        {
            if (!this.ContainsPosition(point))
            {
                return null;
            }
            // pokud není listem, najdem list který obsahuje pozici a iterujem
            if (!this.isLeaf && this.size > minSize)
            {
                for (int leafIndex = 0; leafIndex < 8; leafIndex++)
                {
                    Octree.Node leaf = this.leafs[leafIndex];
                    if (leaf.ContainsPosition(point))
                    {
                        return leaf.GetNodeWithPosition(point);
                    }
                }
            }
            // jinak vracíme tohle
            return this;
        }

        public Octree.Node GetNeighborReal(Vector3I direction)
        {
            Octree.Node neighborNode = GetNeighbor(direction);
            if (neighborNode == null) return null;
            // iterovat do nejmenší velikosti která zabírá celou stranu direction
            // dokud bod nebudude obsahovat body které se nedotýkají bodu.
            Vector3 samplePosition = this.position + (Vector3)direction * this.size;
            // získáme octant který je stejné nebo větší velikosti než aktivní octant
            neighborNode = neighborNode.GetNodeOfSizeAtPoint(samplePosition, this.size);
            return neighborNode;
        }


        //
        // Other
        //

        public override string ToString()
        {
            // Print celý strom dolů od toho bodu.
            string nodeString = "Octree Node xyz: " + this.position.ToString() + " size: " + this.size.ToString(); // získat index v celém stromě.
            return nodeString;
        }
    }
}

/*
* Změny:
*
* [19.07.2025] implementace funkce hledání sousedů v octree.
* [23.07.2025] vytvořeno, stejný kod jako v OctreeNode.cs který jsem používal poslední týden akorát čistší.
* [24.07.2025] přidány nějaké základní funkce.
* [...] píčoviny
* [10.08.2025] GetNeighbor funkce funguje správně.
*
*
*/