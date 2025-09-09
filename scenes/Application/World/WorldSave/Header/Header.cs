

using System;
using System.IO;
using System.Runtime.InteropServices;
using DataStructures;
using Godot;
using Godot.Collections;

namespace WorldSaveSystem
{

    /* WorldSaveSystem - Header
    *
    * Je to octree struktura, kde každý octant odkazuje na chunk uložený na disku.
    * Header je uložen ve svém vlastním souboru.
    *
    * celej header načtenej najednou?
    *
    */
    public class Header
    {
        // pool
        public DataStructures.Octree<int> octree;

        public Header(System.Numerics.Vector3 worldPosition, float worldSize)
        {
            octree = new DataStructures.Octree<int>(worldPosition, worldSize);
        }

        public void LoadFromFile(string filename)
        {
            // načteme data jako byte array
            byte[] fileData = File.ReadAllBytes(filename);
            // zjistíme velikost octant bufferu
            int octantBufferSize = BitConverter.ToInt32(fileData, 0);
            int octantByteSize = GetOctantIntByteSize();
            // nastavíme velikost octant poolu na tu získanou ze souboru
            octree.octants.Resize(octantBufferSize);
            // zjistíme offsety na kterých se nachází buffery
            int octantDataBufferOffset = 4;
            int octantBoolBufferOffset = 4 + octantBufferSize * octantByteSize;
            // získání dat z byte array
            for (int i = 0; i < octantBufferSize; i++)
            {
                bool octantAtIndexExists = BitConverter.ToBoolean(fileData, octantBoolBufferOffset + i);
                octree.octants.booleanBlock[i] = octantAtIndexExists;
                if (octantAtIndexExists)
                {
                    octree.octants.dataBlock[i] = ReadOctantIntFromBytes(fileData, octantDataBufferOffset + i * octantByteSize);
                    GD.Print("added octant: ", octree.octants.dataBlock[i]);
                }
            }
            GD.Print("--- Loaded Header");
        }

        public void SaveToFile(string filename)
        {
            /* File Struktura:
            *
            * int octantBufferSize = 4 bytes
            * int[] octantDataBuffer = 4 * octantBufferSize bytes
            * bool[] octantBooleanBuffer = octantBufferSize bytes
            *
            *
            * celkově 4 + 5 * octantBufferSize bytes
            */

            int octantBufferSizeBytesLength = sizeof(int);
            int octantByteSize = GetOctantIntByteSize();
            int octantDataBufferBytesLength = octree.octants.GetDataBuffer().Length * octantByteSize;
            int octantBooleanBufferBytesLength = octree.octants.GetBoolBuffer().Length * sizeof(bool);

            // final buffer
            int fileSize = octantBufferSizeBytesLength + octantDataBufferBytesLength + octantBooleanBufferBytesLength;
            byte[] fileData = new byte[fileSize];
            int writeIndex = 0;

            // copy buffer size
            Buffer.BlockCopy(BitConverter.GetBytes(octree.octants.bufferSize), 0, fileData, writeIndex, octantBufferSizeBytesLength);
            writeIndex += octantBufferSizeBytesLength;
            // copy octant data
            for (int i = 0; i < octree.octants.GetDataBuffer().Length; i++)
            {
                if (octree.octants.booleanBlock[i])
                {
                    WriteOctantIntBytes(octree.octants.dataBlock[i], fileData, writeIndex);
                }
                writeIndex += octantByteSize;
            }
            // copy boolean buffer
            Buffer.BlockCopy(octree.octants.GetBoolBuffer(), 0, fileData, writeIndex, octantBooleanBufferBytesLength);
            writeIndex += octantBooleanBufferBytesLength;

            // write
            File.WriteAllBytes(filename, fileData);

            GD.Print("--- Saved Header");
        }

        public int FindChunk(System.Numerics.Vector3 position, float size)
        {
            return octree.FindOctantWithPositionOfSize(octree.rootIndex, position, size);
        }

        // octant o min velikosti
        public int GetOctantValue(System.Numerics.Vector3 octantPosition)
        {
            // najdem octant obsahující pozici
            // octant bude o nejmeněí velikosti
            int octIndex = octree.SubdivideAtPoint(octree.rootIndex, octantPosition, Terrain22.Chunk.size);
            if (octIndex == int.MaxValue) return int.MaxValue;
            Octant<int> octant = octree.GetOctant(octIndex);
            // pokud je octant empty (není vázán na žádný uložený chunk)
            if (octant.value == int.MaxValue) // znamená že není vázán na žádný chunk
            {
                // vytvoříme chunk
                // jak zjistit index? 
                // pomocí počtů elementů v octree...? -> to se ale neukládá takže to musíme spravit
                octant.value = 0;
            }
            return octant.value;
        }

        //
        // octant byte save
        //


        private int GetOctantIntByteSize()
        {
            int binarySize = 0;
            binarySize += sizeof(int); // parent
            binarySize += sizeof(bool); // isLeaf
            binarySize += sizeof(int) * 8; // leafs
            binarySize += sizeof(float); // position x
            binarySize += sizeof(float); // position y
            binarySize += sizeof(float); // position z
            binarySize += sizeof(float); // size
            binarySize += sizeof(int); // int value
            return binarySize;
        }
        private void WriteOctantIntBytes(Octant<int> octant, System.Array destination, int offset)
        {
            int writeOffset = offset;
            Buffer.BlockCopy(BitConverter.GetBytes(octant.parent), 0, destination, writeOffset, sizeof(int)); // 0
            writeOffset += sizeof(int);
            Buffer.BlockCopy(BitConverter.GetBytes(octant.isLeaf), 0, destination, writeOffset, sizeof(bool)); // 4
            writeOffset += sizeof(bool);
            // leafs
            for (int i = 0; i < 8; i++)
            {
                Buffer.BlockCopy(BitConverter.GetBytes(octant.leafs[i]), 0, destination, writeOffset, sizeof(int)); // 5
                writeOffset += sizeof(int);
            }
            // position
            Buffer.BlockCopy(BitConverter.GetBytes(octant.position.X), 0, destination, writeOffset, sizeof(float)); // 37
            writeOffset += sizeof(float);
            Buffer.BlockCopy(BitConverter.GetBytes(octant.position.Y), 0, destination, writeOffset, sizeof(float)); // 41
            writeOffset += sizeof(float);
            Buffer.BlockCopy(BitConverter.GetBytes(octant.position.Z), 0, destination, writeOffset, sizeof(float)); // 45
            writeOffset += sizeof(float);
            // size
            Buffer.BlockCopy(BitConverter.GetBytes(octant.size), 0, destination, writeOffset, sizeof(float)); // 49
            writeOffset += sizeof(float);
            // int value
            Buffer.BlockCopy(BitConverter.GetBytes(octant.value), 0, destination, writeOffset, sizeof(int)); // 53
            writeOffset += sizeof(int);
            // 57


            GD.Print("written octant to byte array: ", writeOffset - offset, "bytes");

        }

        private Octant<int> ReadOctantIntFromBytes(byte[] source, int offset)
        {
            int parent = BitConverter.ToInt32(source, offset);
            bool isLeaf = BitConverter.ToBoolean(source, offset + 4);
            
            System.Numerics.Vector3 position = new(BitConverter.ToSingle(source, offset + 37), BitConverter.ToSingle(source, offset + 41), BitConverter.ToSingle(source, offset + 45));
            float size = BitConverter.ToSingle(source, offset + 49);
            int value = BitConverter.ToInt32(source, offset + 53);

            Octant<int> octant = new Octant<int>(parent, position, size);
            octant.isLeaf = isLeaf;
            octant.value = value;

            int leafsOffset = offset + 5;
            for (int i = 0; i < 8; i++)
            {
                octant.leafs[i] = BitConverter.ToInt32(source, leafsOffset + i * 4);
            }

            return octant;
        }

    }





}
