using Godot;
using System;
using System.Collections.Generic;

public partial class QuadTreeDemo : Node2D
{
    QuadNode RootNode;
    const int TreeDepth = 6;

    [Export] NodePath PanelNodePath;
    Node2D PanelNode;

    [Export] NodePath PlayerPath;

    CharacterBody3D Player;
    Panel PovPointPanel;

    public QuadTreeDemo()
    {

    }

    public override void _Ready()
    {
        base._Ready();
        // Nastavíme hlavní kořen stromu.
        // bude to bod o velikosti celé mapy, (může být skoto nekonečná)
        // nemusíme ho přidávat do scény.
        RootNode = new QuadNode(new(0, 0), 512);
        // určíme bod na který se budou vázat panely stromu
        PanelNode = GetNode<Node2D>(PanelNodePath);
        // určíme hráče od kterého budem brát pozici
        Player = GetNode<CharacterBody3D>(PlayerPath);
        // vytvoříme viditelný bod pozice hráče
        PovPointPanel = new Panel();
        PovPointPanel.SetSize(new Vector2(4, 4));
        AddChild(PovPointPanel);
        StyleBoxFlat newStyle2 = new StyleBoxFlat();
        newStyle2.SetBgColor(new Godot.Color(1, 1, 1, 1));
        PovPointPanel.AddThemeStyleboxOverride("panel", newStyle2);
        PovPointPanel.TopLevel = true;
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        // získáme pozici 2D hráče
        Vector2 PovPoint = new Vector2(Player.Position.X, Player.Position.Z) * 8.0f;;
        //
        PovPointPanel.SetPosition(PovPoint + Position);
        // traversnem celý strom a při tom updatenem jeho větve pro nový pov point.
        // rychlost O(N), kde N představuje počet větví.
        // velmi effektivní stačí celý strom traversnout pouze jednou a vše se updatne.
        SubdivideIfClose(ref RootNode, PovPoint, 0, TreeDepth);
    }
    //
    // Core funkce
    //

    bool CheckIfNodeClose(ref QuadNode ActiveNode, Vector2 Point)
    {
        // 
        float RenderDistanceScale = 1.5f;

        Vector2 CellCenterPos = ActiveNode.Position + new Vector2(ActiveNode.Size, ActiveNode.Size) * 0.5f;
        float dist = (CellCenterPos - Point).Length();
        if (dist < ActiveNode.Size * RenderDistanceScale)
        {
            return true;
        }
        return false;
    }

    void SubdivideIfClose(ref QuadNode ActiveNode, Vector2 Point, int IterationCount, int MaxIterations) // max iterations
    {
        //
        // Update počtu iterace
        //
        IterationCount++;
        // pokud byla dosažena maximální iterace tak pouze nastavíme jeho panel
        if (IterationCount > MaxIterations)
        {
            ActiveNode.AddPanel(ref PanelNode);
            return;
        }
        //
        // Distance funkce
        //
        // pomocí distance funkce zjistíme zda by měl být node rozdělen na větší detail
        bool IsClose = CheckIfNodeClose(ref ActiveNode, Point);
        //
        // Operace bodu
        //
        // pokud by měl být orzdělen ale není (je blízko), rozdělíme ho.
        if (IsClose && ActiveNode.IsLeaf)
        {
            ActiveNode.Subdivide(ref PanelNode);
        }
        // pokud by neměl být rozdělen ale je (není blízko), přemeníme ho na list
        if (!IsClose && !ActiveNode.IsLeaf)
        {
            ActiveNode.UnSubdivide(ref PanelNode);
        }
        //
        // Následné operace
        //
        // pokud je po operaci listem, přidáme mu panel.
        if (ActiveNode.IsLeaf)
        {
            ActiveNode.AddPanel(ref PanelNode);
            return;
        }
        // pokud není po operaci listem, iterujem nad jeho listy
        for (int i = 0; i < 4; i++)
        {
            SubdivideIfClose(ref ActiveNode.Leafs[i], Point, IterationCount, MaxIterations);
        }
    }
}
