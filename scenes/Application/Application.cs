using Godot;
using System;

public partial class Application : Node
{
    public Application()
    {
        // povolíme wireframe
        RenderingServer.SetDebugGenerateWireframes(true);

        int size = 10;
        for (int y = 0; y < size; y++)
        {
            string i = "";
            for (int x = 0; x < size; x++)
            {
                //i += WorldSystem.Terrain.Chunk.GetFieldIndex(x, y).ToString() + ", ";
            }
            //GD.Print(i);
        }

    }

}
