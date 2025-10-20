using System.Numerics;

namespace DataStructures
{
    public struct Octant<T>
    {
        // data octantu.
        public int parent;
        public bool isLeaf;
        public int[] leafs; // tohle ne array ?! NEMUSÍ BÝT ARRAY, STAČÍ DRŽET IDEX K PRVNÍMU ! potom se můžu zbavit i isLeaf

        // držet int o tom v jakém levelu jsme?
        // nebo to zjistíme pomocí size?

        // prostorová data.
        public Vector3 position;
        public float size;

        // hodnota.
        public T value;

        public Octant(int parentID, Vector3 position, float size)
        {
            // set octree data
            this.parent = parentID;
            this.isLeaf = true;
            leafs = new int[8];
            // set spacial data
            this.position = position;
            this.size = size;
            // set default value
            value = default;
        }

        public bool ContainsPosition(Vector3 point)
        {
            Vector3 maxBounds = this.position + new Vector3(this.size, this.size, this.size);
            if (point.X >= this.position.X && point.Y >= this.position.Y && point.Z >= this.position.Z &&
            point.X < maxBounds.X && point.Y < maxBounds.Y && point.Z < maxBounds.Z) return true;
            return false;
        }
        
        public override string ToString()
        {
            string output = "Octant position: " + this.position.ToString() + "; size: " + this.size.ToString() + "; isLeaf?: " + this.isLeaf.ToString() + "; parent: " + this.parent.ToString();
            if (!isLeaf)
            {
                for (int i = 0; i < 8; i++)
                {
                    output += "; leaf" + i.ToString() + ": " + leafs[i].ToString();
                }
            }
            return output;
        }

    }
}