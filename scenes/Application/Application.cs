using Godot;
using System;

public partial class Application : Node
{
    public Application()
    {
        // povolíme wireframe temp.
        RenderingServer.SetDebugGenerateWireframes(true);
    }
}
