using Godot;
using System;

namespace Octree
{
    public class Tree
    {
        Octree.Node rootNode;

        public static float renderDistanceScale = 1.0f;

        public static WorldGenerator worldGen;



        public Tree()
        {
            rootNode = new Octree.Node(new(0.0f, 0.0f, 0.0f), 1.0f, null);
        }



        public static bool CheckIfNodeClose(Octree.Node activeNode, Vector3 point)
        {
            Vector3 CellCenterPos = activeNode.position + new Vector3(activeNode.size, activeNode.size, activeNode.size) * 0.5f;
            float dist = (CellCenterPos - point).Length();
            if (dist < activeNode.size * renderDistanceScale) return true;
            return false;
        }

        public static void SubdivideIfClose(Octree.Node activeNode, Vector3 point, int iterationCount, int maxIterations) // max iterations
        {
            //
            // Update počtu iterace
            //
            iterationCount++;
            // pokud byla dosažena maximální iterace tak pouze nastavíme jeho panel
            if (iterationCount > maxIterations)
            {
                activeNode.SetVisual();
                return;
            }
            //
            // Distance funkce
            //
            // pomocí distance funkce zjistíme zda by měl být node rozdělen na větší detail
            bool isClose = CheckIfNodeClose(activeNode, point);
            //
            // Operace bodu
            //
            // pokud by měl být orzdělen ale není (je blízko), rozdělíme ho.
            if (isClose && activeNode.isLeaf)
            {
                activeNode.Subdivide();
            }
            // pokud by neměl být rozdělen ale je (není blízko), přemeníme ho na list
            if (!isClose && !activeNode.isLeaf)
            {
                activeNode.Unsubdivide();
            }
            //
            // Následné operace
            //
            // pokud je po operaci listem, přidáme mu panel.
            if (activeNode.isLeaf)
            {
                activeNode.SetVisual();
                return;
            }
            // pokud není po operaci listem, iterujem nad jeho listy
            // POZNÁMKA: iterujem od (+)konce aby bylo možné vytvořit spoje mezi chunky a nemuseli jsem iterovat znovu přes celý strom.
            for (int i = 7; i >= 0; i--)
            {
                SubdivideIfClose(activeNode.GetLeaf(i), point, iterationCount, maxIterations);
            }
        }
    }
}