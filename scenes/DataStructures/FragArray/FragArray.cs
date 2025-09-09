
namespace DataStructures
{
    public class FragArray<T>
    {
        // POZNÁMKA: všechny blocky mají stejnou velikost
        public T[] dataBlock; // block držící data
        public bool[] booleanBlock; // block držící data o tom, jaké indexy jsou volné
        
        public int avalibleSize; // velikost používaných indexů
        public int bufferSize; // velikost bufferu

        public FragArray()
        {
            dataBlock = null;
            booleanBlock = null;
            bufferSize = 0;
            avalibleSize = 0;
        }

        public int Add(T data)
        {
            avalibleSize++; // změníme velikost používaných indexů
            if (avalibleSize > bufferSize) Resize(bufferSize + 8); // pokud je buffer plný tak ho rozšíříme o 16
            int avalibleIndex = FindAvalibleIndex(); // najdeme volný index
            if (avalibleIndex == bufferSize) return int.MaxValue; // invalid
            this.dataBlock[avalibleIndex] = data; // zapíšeme data
            this.booleanBlock[avalibleIndex] = true; // označíme index jako používaný
            return avalibleIndex;
        }

        public void Remove(int index)
        {
            //if (index >= this.bufferSize || index < 0) return;
            if (!this.booleanBlock[index]) return;
            this.booleanBlock[index] = false; // označíme index jako volný
            avalibleSize--; // změníme velikost používaných indexů
        }

        public ref T Get(int index)
        {
            if (index >= this.bufferSize || index < 0) throw new System.ArgumentException("Index out of range.", nameof(index));
            if (!this.booleanBlock[index]) throw new System.ArgumentException("Element at index empty.", nameof(index));
            return ref this.dataBlock[index];
        }

        public ref T[] GetDataBuffer()
        {
            return ref this.dataBlock;
        }

        public ref bool[] GetBoolBuffer()
        {
            return ref this.booleanBlock;
        }

        public void Resize(int newSize)
        {
            // změníme velikost blocků
            System.Array.Resize(ref this.dataBlock, newSize);
            System.Array.Resize(ref this.booleanBlock, newSize);
            // zapíšeme novou velikost
            this.bufferSize = newSize;
        }

        private int FindAvalibleIndex()
        {
            for (int i = 0; i < this.bufferSize; i++) if (!this.booleanBlock[i]) return i;
            return bufferSize;
        }

    }
}

/*
* Změny:
*
* [06.09.2025] vytvořeno, pro implementaci octree struktury. prakticky zkopírováno z nymphaea.
* [07.09.2025] přidány excepce do Get() funkce.
*
*/