using System;
using System.Net;
using Godot;

namespace Octree
{
    public partial class Node
    {

        // získáme stich cube skládájící se z 2 chunků
        public void GetStichCubeSimple(Vector3I direction, Vector3I neighborBaseSamplePos, Vector3I coreBaseSamplePos, float sizeDiff, Terrain22.Chunk activeChunk, Terrain22.Chunk neighborChunk, int neighborSection, Vector3I planeSize)
        {
            GD.Print("\t--- GetStichCubeSimple");

            int valueFieldSize = Terrain22.Chunk.fieldSize;
            int vertFieldSize = Terrain22.Chunk.size;

            byte[] fieldValues = new byte[8];
            int[] vertexIndices = new int[8];

            // získání všech samplů
            for (int i = 0; i < 8; i++)
            {
                Vector3 sampleOffset = new(i % 2, i / 4, i / 2 % 2);

                //
                // field values
                //

                float neighborOffsetScale = 1.0f;
                float coreOffsetScale = 1.0f;

                if (sizeDiff < 1.0f) // pokud je soused větší, BORDEL
                {
                    neighborOffsetScale = sizeDiff;
                }
                else
                {
                    coreOffsetScale = sizeDiff;
                }

                // sample neighbor
                // pokud sampleOffset je ve směru souseda
                if (sampleOffset * direction == direction) // POZNÁMKA: MŮŽEME SE ZBAVIT IF CHECKU !! (pomocí několik for loopů)
                {
                    Vector3I neighborSamplePos = neighborBaseSamplePos + (Vector3I)((sampleOffset - direction) * neighborOffsetScale);
                    int neighborFieldIndex = neighborSamplePos.X + neighborSamplePos.Y * valueFieldSize + neighborSamplePos.Z * valueFieldSize * valueFieldSize;
                    GD.Print("\t\tin neighbor: ", neighborSamplePos);

                    // write
                    fieldValues[i] = neighborChunk.field[neighborFieldIndex];

                    // index
                    // ŠPATNĚ !!
                    // protože tam je plane !!
                    // převést na plane !!

                    // ŠPATNĚ !!
                    // neighbor planeSize místo vert field size?
                    int inSectionIndex = neighborSamplePos.X + neighborSamplePos.Y * planeSize.X + neighborSamplePos.Z * planeSize.X * planeSize.Y;

                    // ...



                    int neighborVertIndex = neighborSection + inSectionIndex;
                    vertexIndices[i] = neighborVertIndex;
                    GD.Print("\t\tvert index: ", neighborVertIndex, ", offset: ", neighborSamplePos.X + neighborSamplePos.Y * vertFieldSize + neighborSamplePos.Z * vertFieldSize * vertFieldSize);

                }
                else
                {
                    // sample core
                    Vector3I coreSamplePos = coreBaseSamplePos + (Vector3I)(sampleOffset / coreOffsetScale);
                    int coreFieldIndex = coreSamplePos.X + coreSamplePos.Y * valueFieldSize + coreSamplePos.Z * valueFieldSize * valueFieldSize;
                    GD.Print("\t\tin core: ", coreSamplePos);
                    // write
                    fieldValues[i] = activeChunk.field[coreFieldIndex];

                    // index
                    int coreVertIndex = coreSamplePos.X + coreSamplePos.Y * vertFieldSize + coreSamplePos.Z * vertFieldSize * vertFieldSize;
                    vertexIndices[i] = coreVertIndex;

                    GD.Print("\t\tvert index: ", coreVertIndex);

                }
                GD.Print("\t\ti: ", i, ", value: ", fieldValues[i], ", vert index: ", vertexIndices[i]);

            }

            // casecode
            byte caseCode = GetCaseCode(fieldValues, 128);
            GD.Print("caseCode: ", caseCode);
            if (caseCode == 0 || caseCode == 255) return;

            // stich
            GD.Print("initial size: ", activeChunk.meshData.indicesSize);
            for (int i = 0; i < 18; i++)
            {
                int index = SurfaceNet.Tables.indexTable3D[caseCode, i];
                if (index == -1) break;
                activeChunk.meshData.indices[activeChunk.meshData.indicesSize] = vertexIndices[index];
                activeChunk.meshData.indicesSize++;
            }
            GD.Print("final size: ", activeChunk.meshData.indicesSize);


        }

        // získáme stich cube z více než 2 chunků
        // ...
        
        public byte GetCaseCode(byte[] fieldValues, byte isoLevel)
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
    }
}