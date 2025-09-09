using System;
using Godot;

// přesunout do surfacets

namespace SurfaceNet
{
    public class EdgeCube
    {
        public byte[] fieldValues;
        public int[] vertexIndices;

        public EdgeCube()
        {
            fieldValues = new byte[8];
            vertexIndices = new int[8];
        }

        // Vector3 activeBaseSamplePosition; -> základní sample pozice v aktivním oktantu, je v rohu. (0, 0, 0)
        // Vector3 neighborBaseSamplePosition; -> základní pozice v vedlejším oktantu. = activeBaseSamplePosition + neighborDirection = (1, 0, 0)
        // Vector3I neighborDirection; -> směr k sousedovi od aktivního oktantu.
        // float neighborScaleDiffrence; -> rozdíl velikostí aktivního a sousedního oktantu.
        //
        public void GetSample(Vector3 activeBaseSamplePosition, Vector3 neighborBaseSamplePosition, Vector3I neighborDirection, float neighborScaleDiffrence, ref Terrain22.Chunk activeChunk, ref Terrain22.Chunk neighborChunk, Octree.Node activeNode, Octree.Node neighborNode)
        {
            int vertFieldSize = Terrain22.Chunk.size;
            int fieldSize = Terrain22.Chunk.fieldSize;

            ref int vertWriteIndex = ref activeChunk.meshData.verticesSize;

            for (int i = 0; i < 8; i++)
            {
                Vector3I sampleOffset = new Vector3I(i % 2, i / 4, i / 2 % 2);


                if (sampleOffset.X == 1 && neighborDirection.X == 1 || sampleOffset.Y == 1 && neighborDirection.Y == 1 || sampleOffset.Z == 1 && neighborDirection.Z == 1) // SPRAVIT !!
                {
                    if (neighborDirection.X == 1)
                    {
                        sampleOffset.X = 0;
                    }
                    if (neighborDirection.Y == 1)
                    {
                        sampleOffset.Y = 0;
                    }
                    if (neighborDirection.Z == 1)
                    {
                        sampleOffset.Z = 0;
                    }
                    
                    //
                    Vector3 neighborSamplePos = neighborBaseSamplePosition + (Vector3)sampleOffset * neighborScaleDiffrence;
                    int neighborSampleIndex = (int)neighborSamplePos.X + (int)neighborSamplePos.Y * vertFieldSize + (int)neighborSamplePos.Z * vertFieldSize * vertFieldSize;
                    int neighborSampleVertIndex = (int)neighborSamplePos.X + (int)neighborSamplePos.Y * fieldSize + (int)neighborSamplePos.Z * fieldSize * fieldSize;
                    //
                    Vector3 vertOffset = (neighborNode.position - activeNode.position) / (activeNode.size / vertFieldSize);//neighborDirection * vertFieldSize;
                    Vector3 neighborVert = neighborChunk.meshData.vertexPositions[neighborSampleIndex] * (1.0f / neighborScaleDiffrence) + vertOffset;
                    activeChunk.meshData.vertexPositions[vertWriteIndex] = neighborVert;
                    //
                    vertexIndices[i] = vertWriteIndex;
                    fieldValues[i] = neighborChunk.field[neighborSampleVertIndex]; // není tenhle fieluuž v aktivním bodu? (je)
                    //
                    vertWriteIndex++;
                    continue;

                }
                //
                Vector3 activeSamplePos = activeBaseSamplePosition + sampleOffset;
                int activeVertIndex = (int)activeSamplePos.X + (int)activeSamplePos.Y * vertFieldSize + (int)activeSamplePos.Z * vertFieldSize * vertFieldSize;
                int activeFieldIndex = (int)activeSamplePos.X + (int)activeSamplePos.Y * fieldSize + (int)activeSamplePos.Z * fieldSize * fieldSize;
                //
                vertexIndices[i] = activeVertIndex;
                fieldValues[i] = activeChunk.field[activeFieldIndex];

            }

        }

        public byte GetCaseCode(byte isoLevel)
        {
            return (byte)((fieldValues[0] > isoLevel ? 0x01 : 0)
            | (fieldValues[1] > isoLevel ? 0x02 : 0)
            | (fieldValues[2] > isoLevel ? 0x04 : 0)
            | (fieldValues[3] > isoLevel ? 0x08 : 0)
            // horní část krychle
            | (fieldValues[4] > isoLevel ? 0x10 : 0)
            | (fieldValues[5] > isoLevel ? 0x20 : 0)
            | (fieldValues[6] > isoLevel ? 0x40 : 0)
            | (fieldValues[7] > isoLevel ? 0x80 : 0));
        }

        public override string ToString()
        {
            String output = base.ToString() + ":";
            for (int i = 0; i < 8; i++)
            {
                output += "\n\tindex: " + vertexIndices[i].ToString() + " field: " + fieldValues[i].ToString();
            }
            return output;
        }

    }
}