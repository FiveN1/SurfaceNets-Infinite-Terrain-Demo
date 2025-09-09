using System;
using Godot;

namespace Octree
{
    public partial class Node
    {
        public struct FieldCube
        {
            public Vector3[] vertexPositions;
            public Vector3[] vertexNormals;
            public int[] indices;

            public FieldCube()
            {
                vertexPositions = new Vector3[8];
                vertexNormals = new Vector3[8];
                indices = new int[8];
            }
        }

        public void StichChunks(ref SurfaceNet.MeshData meshData)
        {

            if (this.position != new Vector3(0, 0, 0))
            {
                return;
            }

            
            /*
            int vertFieldSize = Terrain22.Chunk.size;

            int vertWriteIndex = vertFieldSize * vertFieldSize * vertFieldSize;
            int indexWriteIndex = vertFieldSize * vertFieldSize * vertFieldSize * 18;

            Terrain22.Chunk thisChunk = chunkPool.GetChunk(this.chunkIndex);

            for (int i = 1; i < 8; i++) // 7 různých směrů kde je potřeba stich
            {
                Vector3I neighborDirection = new(i % 2, i / 2 % 2, i / 4);
                StichAtDirection(thisChunk, ref vertWriteIndex, ref indexWriteIndex, neighborDirection);
            }
            */


            int vertFieldSize = Terrain22.Chunk.size;
            int vertWriteIndex = vertFieldSize * vertFieldSize * vertFieldSize; // změnit !!


            // problém v sample cube a sample base point!!
            StichAtDirection2X(new(0, 0, 1), ref vertWriteIndex, ref meshData);

        }

        void StichAtDirection(Terrain22.Chunk activeChunk, ref int vertWriteIndex, ref int indexWriteIndex, Vector3I direction)
        {

            int vertFieldSize = Terrain22.Chunk.size;
            int chunkSize = Terrain22.Chunk.size;

            Octree.Node rootNode = this.GetRootNode();

            Vector3 planeBetweenChunks = new Vector3(vertFieldSize - 2, vertFieldSize - 2, vertFieldSize - 2) * (new Vector3(1, 1, 1) - direction) + new Vector3(1, 1, 1);

            for (int z = 0; z < planeBetweenChunks.Z; z++)
            {
                for (int y = 0; y < planeBetweenChunks.Y; y++)
                {
                    for (int x = 0; x < planeBetweenChunks.X; x++)
                    {
                        float scale = this.size / chunkSize;
                        Vector3I chunkEdgePlanePosition = direction * (vertFieldSize - 1);
                        Vector3I centerPosRelative = new Vector3I(x + 1, y + 1, z + 1) + chunkEdgePlanePosition;
                        Vector3 centerPosition = (Vector3)centerPosRelative * scale + this.position;
                        StichVertCube(rootNode, centerPosition, ref vertWriteIndex, ref indexWriteIndex, activeChunk);
                    }
                }
            }
        }





        // jak to udělat aby nedocházelo k duplikaci vertexů?
        // -> použít předchozí cube? (NE)
        // -> vytvořit indexový systém (zatím nejlepší možnost)
        //    ale to bych musel vytvořit nějakou prostorovou nadstrukturu. a taky by to musel být rozděleno do 2d quadů a museli by se spojovat i chunky na edge...

        void StichAtDirection2X(Vector3I direction, ref int vertWriteIndex, ref SurfaceNet.MeshData meshData) // = new(1, 0, 0)
        {

            int vertFieldSize = Terrain22.Chunk.size;
            int fieldSize = Terrain22.Chunk.fieldSize;


            Octree.Node neighbor = this.GetNeighborReal(direction);
            if (neighbor == null) return;


            if (this.chunkIndex == -1) return;
            Terrain22.Chunk activeChunk = chunkPool.GetChunk(this.chunkIndex);
            if (neighbor.chunkIndex == -1) return;
            Terrain22.Chunk neighborChunk = chunkPool.GetChunk(neighbor.chunkIndex);

            if (!neighbor.isLeaf)
            {
                // zpracovat...
                // -> pro každý pod node v node:

                GD.Print("soused je menší!");

                for (int leafIndex = 0; leafIndex < 8; leafIndex++)
                {
                    // check zda je bod na straně...
                    // -> otočit direction (* -1)
                    // -> převést body do (-1 1)


                    Vector3I leafPosition = new(leafIndex % 2, leafIndex / 2 % 2, leafIndex / 4);
                    leafPosition = leafPosition * 2 - new Vector3I(1, 1, 1); // přeměněno aby směřovalo od centra rodiče

                    if (true)
                    {

                    }

                }

                return;
            }


            Vector3 planeSize = new Vector3(vertFieldSize - 2, vertFieldSize - 2, vertFieldSize - 2) * (new Vector3(1, 1, 1) - direction) + new Vector3(1, 1, 1);

            IterateOnNeighbor(planeSize, direction, 1.0f, ref neighbor);

        }



        public void IterateOnNeighbor(Vector3 planeSize, Vector3I neighborDirection, float stepSize, ref Octree.Node neighborNode)
        {

            int vertFieldSize = Terrain22.Chunk.size;
            int fieldSize = Terrain22.Chunk.fieldSize;

            // plane size
            // step ...

            // získat chunk tady !?
            // -> možná kromě aktivního ?

            if (this.chunkIndex == -1) return;
            Terrain22.Chunk activeChunk = chunkPool.GetChunk(this.chunkIndex);
            if (neighborNode.chunkIndex == -1) return;
            Terrain22.Chunk neighborChunk = chunkPool.GetChunk(neighborNode.chunkIndex);


            for (float z = 0; z < planeSize.Z; z += stepSize)
            {
                for (float y = 0; y < planeSize.Y; y += stepSize)
                {
                    for (float x = 0; x < planeSize.X; x += stepSize)
                    {
                        //
                        // sample position
                        //

                        // první pozice tady
                        // špatně?? POŘÁD ŠPATNĚ !!
                        Vector3 activeBasePos = new Vector3(x, y, z) * (new Vector3I(1, 1, 1) - neighborDirection) + neighborDirection * (vertFieldSize - 1); // DODĚLAT !!

                        // první pozice v sousedovi
                        Vector3 neighborOffset = (this.position - neighborNode.position) * (new Vector3I(1, 1, 1) - neighborDirection) / neighborNode.size * vertFieldSize; // ??
                        float neighborScaleDiffrence = this.size / neighborNode.size;

                        Vector3 neighborBasePos = new Vector3(x, y, z) * (new Vector3I(1, 1, 1) - neighborDirection) * neighborScaleDiffrence + neighborOffset;

                        GD.Print("získány base pozice pro sample: activeBasePos: ", activeBasePos, " neighborBasePos:", neighborBasePos);

                        //
                        // get sample cube values
                        //

                        SurfaceNet.EdgeCube edgecube = new SurfaceNet.EdgeCube();

                        edgecube.GetSample(activeBasePos, neighborBasePos, neighborDirection, neighborScaleDiffrence, ref activeChunk, ref neighborChunk, this, neighborNode);

                        GD.Print(edgecube);

                        byte caseCode = edgecube.GetCaseCode(128);

                        for (int i = 0; i < 18; i++)
                        {
                            int index = SurfaceNet.Tables.indexTable3D[caseCode, i];
                            if (index == -1) break;
                            activeChunk.meshData.indices[activeChunk.meshData.indicesSize] = edgecube.vertexIndices[index];
                            activeChunk.meshData.indicesSize++;
                        }
                    }
                }
            }
        }

        //
        // Algo
        //

        public void StichVertCube(Octree.Node rootNode, Vector3 centerPosition, ref int vertWriteIndex, ref int indexWriteIndex, Terrain22.Chunk activeChunk)
        {
            Vector3[] vertices = new Vector3[8];
            int[] indices = new int[8];

            // Získáme 8 vertexů a jejich indexy pokud jsou v tomto bodu
            GetVertCube(ref vertices, ref indices, rootNode, centerPosition);

            // zapíšeme vertexy které jsou mimo chunk, přitom také získáme jejich indexy
            for (int i = 0; i < 8; i++)
            {
                if (indices[i] == -1) // pokud mimo chunk, uložíme a získáme index
                {
                    activeChunk.meshData.vertexPositions[vertWriteIndex] = vertices[i];
                    indices[i] = vertWriteIndex;
                    vertWriteIndex++;
                }
            }

            //GD.Print("vertCube: ", centerPosition);
            for (int i = 0; i < 8; i++)
            {
                //GD.Print("\tvertex: ", vertices[i], ", index: ", indices[i], ", normal: ", normals[i]);
            }

            byte caseCode = GetCaseCode(centerPosition, rootNode, 128);
            //GD.Print("\tCaseCode: ", caseCode);


            //GD.Print("\tIndices:");
            for (int i = 0; i < 18; i++)
            {
                int index = SurfaceNet.Tables.indexTable3D[caseCode, i];
                if (index == -1) break;
                //GD.Print("\t", index, ", ", indices[index]);

                activeChunk.meshData.indices[indexWriteIndex] = indices[index];

                indexWriteIndex++;
            }




            // něco s floor?


        }

        // rozdělit na 2 funkce
        // -> getThisCubePart()
        // -> getNeighborCubePart()
        public void GetVertCube(ref Vector3[] vertCubePositions, ref int[] vertCubeIndices, Octree.Node rootNode, Vector3 centerPosition)
        {
            // loop
            for (int i = 0; i < 8; i++)
            {
                // získání sample pozice
                Vector3 centerOffset = (new Vector3(i % 2, i / 4, i / 2 % 2) * 2.0f - new Vector3(1.0f, 1.0f, 1.0f)) * 0.1f; // offset pozice od centra krychle
                Vector3 samplePosition = centerPosition + centerOffset;
                // získání bodu
                Octree.Node nodeWithPos = rootNode.GetNodeWithPosition(samplePosition); // je zaručené že bude listem
                // kontrola bodu
                if (nodeWithPos == null) continue;
                if (nodeWithPos.chunkIndex == -1) continue;
                // získání vertexu
                int sampleVertIndex = GetVertIndex(nodeWithPos, samplePosition);
                Vector3 sampleVertPosition = GetVertRelativePosition(nodeWithPos, sampleVertIndex);
                // zapsání vertexu
                vertCubePositions[i] = sampleVertPosition;
                // zapsaní indexu
                if (nodeWithPos == this) // pokud je v aktivním chunku
                {
                    vertCubeIndices[i] = sampleVertIndex;
                }
                else // -1 pokud index je mimo aktivní chunk
                {
                    vertCubeIndices[i] = -1;
                }
            }

            // pokud je vedlejší chunk menší tak iterovat od něj?
        }







        public int GetVertIndex(Octree.Node nodeWithPos, Vector3 samplePosition)
        {
            // Získání pozice v Octantu
            Vector3 posInNode = samplePosition - nodeWithPos.position;
            // Získání indexu v chunku
            int chunkSize = Terrain22.Chunk.size;
            int vertFieldSize = Terrain22.Chunk.size;
            Vector3I posInChunk = (Vector3I)(posInNode / (nodeWithPos.size / chunkSize)); // MOŽNÁ ŠPATNĚ
            int vertIndex = posInChunk.X + posInChunk.Y * vertFieldSize + posInChunk.Z * vertFieldSize * vertFieldSize;
            return vertIndex;
        }

        public Vector3 GetVertRelativePosition(Octree.Node nodeWithPos, int sampleVertIndex)
        {
            // získání chunku
            Terrain22.Chunk sampleChunk = chunkPool.GetChunk(nodeWithPos.chunkIndex);
            // získání vertexu
            int chunkSize = Terrain22.Chunk.size;
            float scaleDiffrence = nodeWithPos.size / this.size;
            Vector3 sampleVertPosition = sampleChunk.meshData.vertexPositions[sampleVertIndex] * scaleDiffrence;
            // nastavíme relativní pozici pokud se jedná o jinej chunk
            if (nodeWithPos != this && sampleVertPosition != new Vector3(0, 0, 0))
            {
                Vector3 offset = (nodeWithPos.position - this.position) / (this.size / chunkSize); // vždy bude stejné, takže tohle nemusíme ani počítat?
                sampleVertPosition += offset;
            }
            return sampleVertPosition;
        }

        public byte GetFieldValue(Octree.Node rootNode, Vector3 samplePosition)
        {
            Octree.Node nodeWithPos = rootNode.GetNodeWithPosition(samplePosition); // pomalé !!

            if (nodeWithPos == null) return 0;
            if (nodeWithPos.chunkIndex == -1) return 0;

            Terrain22.Chunk sampleChunk = chunkPool.GetChunk(nodeWithPos.chunkIndex);

            Vector3 posInNode = samplePosition - nodeWithPos.position;

            int chunkSize = Terrain22.Chunk.size;
            int fieldSize = Terrain22.Chunk.fieldSize;
            Vector3I posInChunk = (Vector3I)(posInNode / (nodeWithPos.size / chunkSize)); // MOŽNÁ ŠPATNĚ
            int fieldIndex = posInChunk.X + posInChunk.Y * fieldSize + posInChunk.Z * fieldSize * fieldSize;

            byte fieldValue = sampleChunk.field[fieldIndex];
            //
            return fieldValue;
        }

        public byte GetCaseCode(Vector3 centerPosition, Octree.Node rootNode, byte isoLevel) // neskutečně pomalé
        {
            return (byte)((GetFieldValue(rootNode, centerPosition + new Vector3(-0.1f, -0.1f, -0.1f)) > isoLevel ? 0x01 : 0)
            | (GetFieldValue(rootNode, centerPosition + new Vector3(0.1f, -0.1f, -0.1f)) > isoLevel ? 0x02 : 0)
            | (GetFieldValue(rootNode, centerPosition + new Vector3(-0.1f, -0.1f, 0.1f)) > isoLevel ? 0x04 : 0)
            | (GetFieldValue(rootNode, centerPosition + new Vector3(0.1f, -0.1f, 0.1f)) > isoLevel ? 0x08 : 0)
            // horní část krychle
            | (GetFieldValue(rootNode, centerPosition + new Vector3(-0.1f, 0.1f, -0.1f)) > isoLevel ? 0x10 : 0)
            | (GetFieldValue(rootNode, centerPosition + new Vector3(0.1f, 0.1f, -0.1f)) > isoLevel ? 0x20 : 0)
            | (GetFieldValue(rootNode, centerPosition + new Vector3(-0.1f, 0.1f, 0.1f)) > isoLevel ? 0x40 : 0)
            | (GetFieldValue(rootNode, centerPosition + new Vector3(0.1f, 0.1f, 0.1f)) > isoLevel ? 0x80 : 0));
        }


        public void GenerateEdgeNormals()
        {
            // jelikož vertexy na hraně nejsou používány v core meshi je třeba pro ně vytvořit normály.

        }

    }
}

// jak jsem dokázal tohle udělat?
// - dělit kod do funkcí aby byl dobře čitelný
//

// Priorita:
// - (1) update vedlejších chunků, -> správný get neighbor algorithm.
// - (2) stich s menšími chunky než aktivní chunk.
// - (3) neduplikovat vertexy
// - [HOTOVO] Stich pro několik stran (kvůli kontrole normálů jak vypadají, protože dojde k duplikaci)

// Lepší způsob, pro chunk pro který je třeba vytvořit stich:
// 1. najít všechny sousedy.
// 2. pro každého souseda, iterovat cube pro menší hranu. (menší hrana bude mít větší detail)

// [07.08.2025] vytvořeno protože všechno v NodeVisual.CS je bordel
// [08.08.2025] skoro plně funknční, pomalé neoptimalizované (to vůbec nevadí hlavně že to konečně nějak funguje)