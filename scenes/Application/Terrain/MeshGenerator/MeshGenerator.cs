using Godot;
using System;

public partial class MeshGenerator : Node
{

    RenderingDevice LocalRenderingDevice;

    public MeshGenerator()
    {
        // vytvoříme locální rendering device
        LocalRenderingDevice = RenderingServer.CreateLocalRenderingDevice();
        if (LocalRenderingDevice == null)
        {
            GD.PrintErr("MeshGenerator Error: null LocalRenderingDevice");
            return;
        }
        // vytvoříme MC Compute Shader
        RDShaderFile ComputeShaderFile = GD.Load<RDShaderFile>("res://scenes/Application/Terrain/MeshGenerator/MCAlgo.glsl");
        RDShaderSpirV ComputeShaderBytecode = ComputeShaderFile.GetSpirV();
        Rid ComputeShader = LocalRenderingDevice.ShaderCreateFromSpirV(ComputeShaderBytecode);





        float[] input = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
        var inputBytes = new byte[input.Length * sizeof(float)];
        Buffer.BlockCopy(input, 0, inputBytes, 0, inputBytes.Length);

        // Create a storage buffer that can hold our float values.
        // Each float has 4 bytes (32 bit) so 10 x 4 = 40 bytes
        var buffer = LocalRenderingDevice.StorageBufferCreate((uint)inputBytes.Length, inputBytes);


        // Create a uniform to assign the buffer to the rendering device
        var uniform = new RDUniform
        {
            UniformType = RenderingDevice.UniformType.StorageBuffer,
            Binding = 0
        };
        uniform.AddId(buffer);
        var uniformSet = LocalRenderingDevice.UniformSetCreate([uniform], ComputeShader, 0);


        // Create a compute pipeline
        var pipeline = LocalRenderingDevice.ComputePipelineCreate(ComputeShader);
        var computeList = LocalRenderingDevice.ComputeListBegin();
        LocalRenderingDevice.ComputeListBindComputePipeline(computeList, pipeline);
        LocalRenderingDevice.ComputeListBindUniformSet(computeList, uniformSet, 0);
        LocalRenderingDevice.ComputeListDispatch(computeList, xGroups: 5, yGroups: 1, zGroups: 1);
        LocalRenderingDevice.ComputeListEnd();


        // Submit to GPU and wait for sync
        LocalRenderingDevice.Submit();
        LocalRenderingDevice.Sync();

        // Read back the data from the buffers
        var outputBytes = LocalRenderingDevice.BufferGetData(buffer);
        var output = new float[input.Length];
        Buffer.BlockCopy(outputBytes, 0, output, 0, outputBytes.Length);
        GD.Print("Input: ", string.Join(", ", input));
        GD.Print("Output: ", string.Join(", ", output));

        GD.Print("Created Mesh Generator");
        //ConcavePolygonShape3D
    }

    ~MeshGenerator()
    {
        GD.Print("Deleted Mesh Generator");
    }

    /*
    void GenerateMCMesh(OctreeNode ActiveNode, byte IsoLevel)
    {
        
    }
    */
}
