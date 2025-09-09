using Godot;
using System;

public partial class Ui : Control
{

    [Export] NodePath fpsCounterPath;
    Label fpsCounter;

    public override void _Ready()
    {
        base._Ready();

        fpsCounter = GetNode<Label>(fpsCounterPath);
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        fpsCounter.Text = $"{Engine.GetFramesPerSecond()} fps";
    }


}
