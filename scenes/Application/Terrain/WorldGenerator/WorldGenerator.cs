using Godot;
using System;

public partial class WorldGenerator : Node
{

    [Export] Noise Noise1;
    [Export] float Noise1Intensity = 1.0f;
    [Export] Noise Noise2;
    [Export] float Noise2Intensity = 1.0f;
    [Export] Noise Noise3;
    [Export] float Noise3Intensity = 1.0f;

    [Export] float TerrainBaseHeight = 0.0f;

    public override void _Ready()
    {
        base._Ready();

        //Noise1 = new Noise(0,);
    }


    public byte GetValue(float x, float y, float z)
    {
        float Noise1Value = (Noise1.GetNoise2D(x, z) + 1.0f) * 0.5f;
        float Noise2Value = (Noise2.GetNoise2D(x, z) + 1.0f) * 0.5f;
        float Noise3Value = (Noise3.GetNoise2D(x, z) + 1.0f) * 0.5f;
        //float height = Noise1Value * Noise1Intensity + Noise2Value * Noise2Intensity + Noise3Value * Noise3Intensity + TerrainBaseHeight;
        float TerrainHeight = TerrainBaseHeight;
        TerrainHeight += Noise1Value * Noise1Intensity;
        TerrainHeight += Noise2Value * Noise2Intensity;
        TerrainHeight += Noise3Value * Noise3Intensity;


        // jakoby rozmaže hrany
        float BlendRegion = 1.0f;
        if (y < TerrainHeight)
        {
            if (y < TerrainHeight - BlendRegion)
            {
                return 255;
            }
            float value = (y - (TerrainHeight - BlendRegion)) / BlendRegion;
            return (byte)(255.0f - 255.0f * value);
        }
        // clamp
        return 0;
    }
}
