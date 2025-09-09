using Godot;
using System;
using System.Collections.Generic;

public partial class QuadNode : Node
{
    // Základní parametry pro QuadTree
    public bool IsLeaf;
    public QuadNode[] Leafs;
    // Parametry používané pro tvorby 2D struktury
    public Vector2 Position;
    public float Size;
    // Reference na panel bodu
    Panel QuadNodeBox;

    public QuadNode(Vector2 NodePosition, float NodeSize)
    {
        Position = NodePosition;
        Size = NodeSize;

        IsLeaf = true;

        Leafs = new QuadNode[4];
        for (int i = 0; i < 4; i++)
        {
            Leafs[i] = null;

        }

        QuadNodeBox = null;
    }

    public void Subdivide(ref Node2D PanelParent)
    {
        if (!IsLeaf)
        {
            return;
        }
        for (int y = 0; y < 2; y++)
        {
            for (int x = 0; x < 2; x++)
            {
                Leafs[x + y * 2] = new QuadNode(new Vector2(x, y) * Size * 0.5f + Position, Size * 0.5f);
            }
        }

        IsLeaf = false;
        RemovePanel(ref PanelParent);
    }

    public void UnSubdivide(ref Node2D PanelParent)
    {
        if (IsLeaf)
        {
            return;
        }
        for (int i = 0; i < 4; i++)
        {
            Leafs[i].RemovePanel(ref PanelParent);
            Leafs[i].UnSubdivide(ref PanelParent);
            Leafs[i] = null;
        }
        IsLeaf = true;
    }

    //
    // Visual
    //

    public void AddPanel(ref Node2D PanelParent)
    {
        if (QuadNodeBox != null)
        {
            return;
        }
        QuadNodeBox = new Panel();
        QuadNodeBox.SetPosition(Position);
        QuadNodeBox.SetSize(new Vector2(Size, Size));
        PanelParent.AddChild(QuadNodeBox);
        StyleBoxFlat QuadNodePanelStyle = new StyleBoxFlat();
        QuadNodePanelStyle.SetBgColor(new Godot.Color((GD.Randi() % 100) * 0.01f, (GD.Randi() % 100) * 0.01f, (GD.Randi() % 100) * 0.01f, 1));
        QuadNodeBox.AddThemeStyleboxOverride("panel", QuadNodePanelStyle);
        //GD.Print("Added panel");
    }

    public void RemovePanel(ref Node2D PanelParent)
    {
        if (QuadNodeBox == null)
        {
            return;
        }
        PanelParent.RemoveChild(QuadNodeBox);
        QuadNodeBox = null;
        //GD.Print("Removed panel");
    }
}
