

namespace DataStructures
{
    public class Pool<T>
    {
        T[] dataBuffer;
        bool[] boolBuffer;

        int activeSize;
        int bufferSize;

        public Pool()
        {
            dataBuffer = null;
            boolBuffer = null;
            activeSize = 0;
            bufferSize = 0;
        }

        public int Fetch()
        {
            activeSize++;
            if (activeSize > bufferSize) Resize(bufferSize + 8); // pokud je buffer plný tak ho rozšíříme o 16
            int avalibleIndex = FindAvalibleIndex(); // najdeme volný index
            if (avalibleIndex == bufferSize) return int.MaxValue; // invalid
            //this.dataBuffer[avalibleIndex] = new T(); // zapíšeme data
            this.boolBuffer[avalibleIndex] = true; // označíme index jako používaný
            return avalibleIndex;
        }

        public void Drop(int index)
        {
            activeSize--;

        }

        public ref T Get(int index)
        {
            return ref dataBuffer[index];
        }


        public void Resize(int newSize)
        {
            // změníme velikost blocků
            System.Array.Resize(ref this.dataBuffer, newSize);
            System.Array.Resize(ref this.boolBuffer, newSize);
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