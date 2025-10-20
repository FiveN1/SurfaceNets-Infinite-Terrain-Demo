using Godot;
using System;

namespace SurfaceNet
{

    public static class Algorithm
    {

        public static byte GetCaseCode(ref byte[] field, Vector3I fieldSize, byte isoLevel, int cellIndex)
        {
            // spodní část krychle
            return (byte)((field[cellIndex] > isoLevel ? 0x01 : 0)
            | (field[cellIndex + 1] > isoLevel ? 0x02 : 0)
            | (field[cellIndex + fieldSize.X * fieldSize.Y] > isoLevel ? 0x04 : 0)
            | (field[cellIndex + 1 + fieldSize.X * fieldSize.Y] > isoLevel ? 0x08 : 0)
            // horní část krychle
            | (field[cellIndex + fieldSize.X] > isoLevel ? 0x10 : 0)
            | (field[cellIndex + 1 + fieldSize.X] > isoLevel ? 0x20 : 0)
            | (field[cellIndex + fieldSize.X * fieldSize.Y + fieldSize.X] > isoLevel ? 0x40 : 0)
            | (field[cellIndex + 1 + fieldSize.X * fieldSize.Y + fieldSize.X] > isoLevel ? 0x80 : 0));
        }

        public static Vector3 GetVertexPosition(ref byte[] field3D, Vector3I field3DSize, byte isoLevel, int cellIndex, byte caseCode)
        {
            Vector3 vertexPosition = new(0, 0, 0);
            int edgeCount = 0;
            // pro všechny body na hranách získáme jejich pozice a přičteme k vertexPosition
            for (int edgeCodeIndex = 0; edgeCodeIndex < 4; edgeCodeIndex++)
            {
                // získáme kod
                byte edgeCode = Tables.edgeCodeTable[caseCode, edgeCodeIndex];
                // check zda nejsme na konci
                if (edgeCode == 0x00) break;
                // získáme indexy z kodu.
                byte cornerId0 = (byte)((edgeCode >> 4) & 0x0F); // první číslo
                byte cornerId1 = (byte)((edgeCode) & 0x0F); // druhé číslo
                // získáme 2D pozici z indexů
                Vector3I cornerV0 = new(cornerId0 % 2, cornerId0 / 4, cornerId0 / 2 % 2);
                Vector3I cornerV1 = new(cornerId1 % 2, cornerId1 / 4, cornerId1 / 2 % 2);
                // vypočítáme interpolaci
                byte value0 = field3D[cellIndex + cornerV0.X + cornerV0.Y * field3DSize.X + cornerV0.Z * field3DSize.X * field3DSize.Y];
                byte value1 = field3D[cellIndex + cornerV1.X + cornerV1.Y * field3DSize.X + cornerV1.Z * field3DSize.X * field3DSize.Y];
                float interoplatedScale = Mathf.Abs((float)isoLevel - (float)value0) / Mathf.Abs((float)value0 - (float)value1);
                // získáme pozici na hraně
                Vector3 edgePosition = (Vector3)(cornerV1 - cornerV0) * interoplatedScale + cornerV0;
                // přičteme pro získání průměru
                vertexPosition += edgePosition;
                edgeCount++;
            }
            // získáme průměr
            vertexPosition /= edgeCount;
            return vertexPosition;
        }

        public static void GetIndices(byte caseCode, int vertexIndex, ref int[] indices, ref int writeIndex, Vector3I fieldSize)
        {
            for (int i = 0; i < 18; i++)
            {
                int index = Tables.indexTable3D[caseCode, i];
                if (index == -1) break;
                Vector3I indexV = new(index % 2, index / 4, (index / 2) % 2);
                indices[writeIndex] = vertexIndex + indexV.X + indexV.Y * (fieldSize.X - 1) + indexV.Z * (fieldSize.X - 1) * (fieldSize.Y - 1);
                writeIndex++;
            }
        }

        public static void GenerateNormals(ref MeshData meshData)
        {
            for (int i = 0; i < meshData.indices.Length / 3; i += 3)
            {
                int index0 = meshData.indices[i];
                int index1 = meshData.indices[i + 1];
                int index2 = meshData.indices[i + 2];

                Vector3 vert1 = meshData.vertexPositions[index0];
                Vector3 vert2 = meshData.vertexPositions[index1];
                Vector3 vert3 = meshData.vertexPositions[index2];

                Vector3 normal = (vert3 - vert1).Cross(vert2 - vert1).Normalized();
                // set indices!!
                meshData.vertexNormals[index0] += normal;
                meshData.vertexNormals[index1] += normal;
                meshData.vertexNormals[index2] += normal;

                // dík: https://computergraphics.stackexchange.com/questions/4031/programmatically-generating-vertex-normals
            }
            for (int i = 0; i < meshData.vertexNormals.Length; i++)
            {
                meshData.vertexNormals[i] = meshData.vertexNormals[i].Normalized();
            }

        }
        // PROBOLÉMY:
        // - mezery mezi vertexy. (ukládají se nepoužívané vertexy, vertexy(0, 0, 0))
        // - používá se pouze vertex shading. lepší by byla kombinace vertex a flat shading. ?
        // octree node pro získání sousedů
        public static MeshData GenerateMeshData(ref byte[] field, Vector3I fieldSize, byte isoLevel)
        {
            // Optimální process s podporou GPU:
            // skládá se z 3 cyklů kde jeden cykl projde přes celé pole.
            // (1) zjistit pole s indexy vertexů, tohle také vypočítá celkový počet vertexů. (CPU)
            // (2) vytvořit vertexy a indexy (GPU)
            // (3) vytvořit vertexy a indexy mezi chunky (CPU) (GPU?)
            // (4) vytvořit normály (GPU)

            // Mesh data
            // velikost pole buňek/vertexů
            Vector3I vertexFieldSize = fieldSize - new Vector3I(1, 1, 1);
            // data meshe
            MeshData meshData = new MeshData(vertexFieldSize.X);

            int writeIndex = 0; // předem spočítat?
            // pro každou buňku v poli zjistíme vertex.
            for (int z = 0; z < vertexFieldSize.Z; z++)
            {
                for (int y = 0; y < vertexFieldSize.Y; y++)
                {
                    for (int x = 0; x < vertexFieldSize.X; x++)
                    {
                        // tohle všechno dát do funkce?
                        // index buňky v 3D poli
                        int cellIndex = x + y * fieldSize.X + z * fieldSize.X * fieldSize.Y;
                        // zjistíme o jakou konfiguraci se jedná.
                        byte caseCode = GetCaseCode(ref field, fieldSize, isoLevel, cellIndex);
                        // pokud se jedná o prázdnou konfiguraci tak pokračujem dál na další buňku.
                        if (caseCode == 0 || caseCode == 255) continue;
                        // pozice vertexu v poli vertexů
                        int vertexIndex = x + y * vertexFieldSize.X + z * vertexFieldSize.X * vertexFieldSize.Y;
                        // zjistníme pozici vertexu v buňce pomocí čísla konfigurace které vložíme do tabulky. (velmi rychlé)
                        meshData.vertexPositions[vertexIndex] = GetVertexPosition(ref field, fieldSize, isoLevel, cellIndex, caseCode) + new Vector3(x, y, z);



                        if (x >= vertexFieldSize.X - 1 || y >= vertexFieldSize.Y - 1 || z >= vertexFieldSize.Z - 1) continue;
                        // přidat indices do pole podle caseCode
                        GetIndices(caseCode, vertexIndex, ref meshData.indices, ref writeIndex, fieldSize);
                    }
                }
            }
            // vytvoříme normály
            //GenerateNormals(ref meshData);
            // vrátíme vytvořená data
            return meshData;
        }


        // MUSÍ JÍT OD ZÁDU DO PŘEDU !!
        public static void GenerateMeshData2(ref byte[] field, Vector3I fieldSize, byte isoLevel, ref MeshData meshData, Vector3I vertexFieldSize)
        {
            // Optimální process s podporou GPU:
            // skládá se z 3 cyklů kde jeden cykl projde přes celé pole.
            // (1) zjistit pole s indexy vertexů, tohle také vypočítá celkový počet vertexů. (CPU)
            // (2) vytvořit vertexy a indexy (GPU)
            // (3) vytvořit vertexy a indexy mezi chunky (CPU) (GPU?)
            // (4) vytvořit normály (GPU)

            // Mesh data
            // velikost pole buňek/vertexů
            //Vector3I vertexFieldSize = fieldSize - new Vector3I(1, 1, 1);
            // data meshe
            //MeshData meshData = new MeshData(vertexFieldSize.X, vertexFieldSize.Y, vertexFieldSize.Z);


            meshData.indicesSize = 0;
            // clear indices buffer
            for (int i = 0; i < meshData.indices.Length; i++)
            {
                meshData.indices[i] = 0;
            }

            int writeIndex = 0; // předem spočítat?
            // pro každou buňku v poli zjistíme vertex.
            for (int z = 0; z < vertexFieldSize.Z; z++)
            {
                for (int y = 0; y < vertexFieldSize.Y; y++)
                {
                    for (int x = 0; x < vertexFieldSize.X; x++)
                    {
                        // tohle všechno dát do funkce?
                        // index buňky v 3D poli
                        int cellIndex = x + y * fieldSize.X + z * fieldSize.X * fieldSize.Y;
                        // zjistíme o jakou konfiguraci se jedná.
                        byte caseCode = GetCaseCode(ref field, fieldSize, isoLevel, cellIndex);
                        // pokud se jedná o prázdnou konfiguraci tak pokračujem dál na další buňku.
                        if (caseCode == 0 || caseCode == 255) continue;
                        // pozice vertexu v poli vertexů
                        int vertexIndex = x + y * vertexFieldSize.X + z * vertexFieldSize.X * vertexFieldSize.Y;
                        // zjistníme pozici vertexu v buňce pomocí čísla konfigurace které vložíme do tabulky. (velmi rychlé)
                        meshData.vertexPositions[vertexIndex] = GetVertexPosition(ref field, fieldSize, isoLevel, cellIndex, caseCode) + new Vector3(x, y, z);



                        if (x >= vertexFieldSize.X - 1 || y >= vertexFieldSize.Y - 1 || z >= vertexFieldSize.Z - 1) continue;
                        // přidat indices do pole podle caseCode
                        GetIndices(caseCode, vertexIndex, ref meshData.indices, ref writeIndex, fieldSize);
                    }
                }
            }
            meshData.indicesSize = writeIndex;
            meshData.verticesSize = vertexFieldSize.X * vertexFieldSize.Y * vertexFieldSize.Z;
        }



        //
        // Nový plán
        //
        // (1) získat vertexy a uložit je v poli
        // -> pole musí podporovat ukládání vertexů různých velikostí. (? octree)
        // (2) spojit je 

        public static void GenerateVertices(ref byte[] field, Vector3I fieldSize, byte isoLevel, ref MeshData meshData, Vector3I vertexFieldSize)
        {
            // pro každou buňku v poli zjistíme vertex.
            for (int z = 0; z < vertexFieldSize.Z; z++)
            {
                for (int y = 0; y < vertexFieldSize.Y; y++)
                {
                    for (int x = 0; x < vertexFieldSize.X; x++)
                    {
                        // tohle všechno dát do funkce?
                        // index buňky v 3D poli
                        int cellIndex = x + y * fieldSize.X + z * fieldSize.X * fieldSize.Y;
                        // zjistíme o jakou konfiguraci se jedná.
                        byte caseCode = GetCaseCode(ref field, fieldSize, isoLevel, cellIndex);
                        // pokud se jedná o prázdnou konfiguraci tak pokračujem dál na další buňku.
                        if (caseCode == 0 || caseCode == 255) continue;
                        // pozice vertexu v poli vertexů
                        int vertexIndex = x + y * vertexFieldSize.X + z * vertexFieldSize.X * vertexFieldSize.Y;
                        // zjistníme pozici vertexu v buňce pomocí čísla konfigurace které vložíme do tabulky. (velmi rychlé)
                        meshData.vertexPositions[vertexIndex] = GetVertexPosition(ref field, fieldSize, isoLevel, cellIndex, caseCode) + new Vector3(x, y, z);
                    }
                }
            }
        }
        
        public static void GenerateIndices(ref byte[] field, Vector3I fieldSize, byte isoLevel, ref MeshData meshData, Vector3I vertexFieldSize)
        {
            // pro každou buňku v poli zjistíme vertex.
            for (int z = 0; z < vertexFieldSize.Z - 1; z++)
            {
                for (int y = 0; y < vertexFieldSize.Y - 1; y++)
                {
                    for (int x = 0; x < vertexFieldSize.X - 1; x++)
                    {
                        
                    }
                }
            }
        }

        /*
        public static Godot.ArrayMesh GenerateOctreeMesh3D(OctreeNode octreeNode)
        {
            MeshData meshData = GenerateMeshData(ref octreeNode.Field, new(OctreeNode.FieldSize, OctreeNode.FieldSize, OctreeNode.FieldSize), 128);

            // získat souseda


            Godot.ArrayMesh arrayMesh = new ArrayMesh();
            Godot.Collections.Array arrays = [];
            arrays.Resize((int)Mesh.ArrayType.Max);
            arrays[(int)Mesh.ArrayType.Vertex] = meshData.vertexPositions;
            arrays[(int)Mesh.ArrayType.Normal] = meshData.vertexNormals;
            arrays[(int)Mesh.ArrayType.Index] = meshData.indices;
            arrayMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, arrays);
            return arrayMesh;
        }
        */
    }
}

/*
* Zdroje:
* https://www.youtube.com/@thecodejar
* https://bonsairobo.medium.com/smooth-voxel-mapping-a-technical-deep-dive-on-real-time-surface-nets-and-texturing-ef06d0f8ca14
* https://jcgt.org/published/0011/01/03/paper.pdf
* https://scholar.harvard.edu/files/sfrisken/files/constrained_elastic_surfacenets.pdf
*
* https://www.mattkeeter.com/projects/contours/
*
* Chunk Edge:
* https://ngildea.blogspot.com/2014/09/dual-contouring-chunked-terrain.html
*
*/

/*
* Změny:
*
* [22.07.2025] vytvořeno, funkční Surface Net algorithm (Dual contouring)
* [23.07.2025] začištěno
*
*/