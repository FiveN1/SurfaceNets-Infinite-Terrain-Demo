

namespace DataStructures
{
    // velmi podobné jako frag array
    public class Pool<T>
    {
        // POZNÁMKA: všechny blocky mají stejnou velikost
        public T[] dataBuffer; // block držící data
        public bool[] boolBuffer; // block držící data o tom, jaké indexy jsou volné
        public bool[] hasBeenGeneratedBuffer;

        public int avalibleSize; // velikost používaných indexů
        public int bufferSize; // velikost bufferu

        public Pool()
        {
            dataBuffer = null;
            boolBuffer = null;
            hasBeenGeneratedBuffer = null;
            bufferSize = 0;
            avalibleSize = 0;
        }

        public int Fetch()
        {
            avalibleSize++; // změníme velikost používaných indexů
            if (avalibleSize > bufferSize) Resize(bufferSize + 8); // pokud je buffer plný tak ho rozšíříme o 16
            int avalibleIndex = FindAvalibleIndex(); // najdeme volný index
            if (avalibleIndex == bufferSize) return int.MaxValue; // invalid
            
            this.boolBuffer[avalibleIndex] = true; // označíme index jako používaný
            return avalibleIndex;
        }

        public void Free(int index)
        {
            //if (index >= this.bufferSize || index < 0) return;
            if (!this.boolBuffer[index]) return;
            this.boolBuffer[index] = false; // označíme index jako volný
            avalibleSize--; // změníme velikost používaných indexů
        }

        public ref T Get(int index)
        {
            if (index >= this.bufferSize || index < 0) throw new System.ArgumentException("Index out of range.", nameof(index));
            if (!this.boolBuffer[index]) throw new System.ArgumentException("Element at index empty.", nameof(index));
            return ref this.dataBuffer[index];
        }

        public bool CheckIfConstructed(int index)
        {
            if (index >= this.bufferSize || index < 0) throw new System.ArgumentException("Index out of range.", nameof(index));
            return this.hasBeenGeneratedBuffer[index];
        }
        public void SetAcConstructed(int index)
        {
            if (index >= this.bufferSize || index < 0) throw new System.ArgumentException("Index out of range.", nameof(index));
            this.hasBeenGeneratedBuffer[index] = true;
        }

        public ref T[] GetDataBuffer()
        {
            return ref this.dataBuffer;
        }

        public ref bool[] GetBoolBuffer()
        {
            return ref this.boolBuffer;
        }

        public void Resize(int newSize)
        {
            // změníme velikost blocků
            System.Array.Resize(ref this.dataBuffer, newSize);
            System.Array.Resize(ref this.boolBuffer, newSize);
            System.Array.Resize(ref this.hasBeenGeneratedBuffer, newSize);
            // zapíšeme novou velikost
            this.bufferSize = newSize;
        }

        private int FindAvalibleIndex()
        {
            for (int i = 0; i < this.bufferSize; i++) if (!this.boolBuffer[i]) return i;
            return bufferSize;
        }

    }
}