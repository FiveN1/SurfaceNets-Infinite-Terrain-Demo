

using Godot;

namespace WorldSystem
{
    public class WorldGenration
    {
        public readonly int seed;
        const float baseHeight = 550.0f; //3.0f

        FastNoiseLite noise1;
        const float noise1Height = 1000.0f;

        FastNoiseLite noise2;
        const float noise2Height = 100.0f;

        public WorldGenration(int seed)
        {
            this.seed = seed;
            //
            noise1 = new FastNoiseLite();
            noise1.NoiseType = FastNoiseLite.NoiseTypeEnum.Cellular;
            noise1.Seed = this.seed;
            noise1.Frequency = 0.001f;
            //
            noise2 = new FastNoiseLite();
            noise2.NoiseType = FastNoiseLite.NoiseTypeEnum.SimplexSmooth;
            noise2.Seed = this.seed;
            noise2.Frequency = 0.004f;
        }

        /*
        public void GenerateChunk(ref Terrain.Chunk chunk, System.Numerics.Vector3 chunkPosition, float chunkSize)
        {
            for (int z = 0; z < Terrain.Chunk.fieldSize; z++)
            {
                for (int y = 0; y < Terrain.Chunk.fieldSize; y++)
                {
                    for (int x = 0; x < Terrain.Chunk.fieldSize; x++)
                    {
                        System.Numerics.Vector3 voxelPosition = chunkPosition + new System.Numerics.Vector3(x, y, z) * (chunkSize / Terrain.Chunk.realFieldSize);
                        int chunkBufferIndex = x + y * Terrain.Chunk.realFieldSize + z * Terrain.Chunk.realFieldSize * Terrain.Chunk.realFieldSize;

                        chunk.field[chunkBufferIndex] = GetValue(voxelPosition);
                    }
                }
            }
            
        }
        */

        public byte GetValue(System.Numerics.Vector3 position)
        {
            // POZNÁMKA: value jde od -1.0f do 1.0f;
            float noise1Value = noise1.GetNoise2D(position.X, position.Z) * noise1Height;
            float noise2Value = noise2.GetNoise2D(position.X, position.Z) * noise2Height;

            float terrainHeight = baseHeight + noise1Value + noise2Value;

            // jakoby rozmaže hrany
            float BlendRegion = 1.0f;
            if (position.Y < terrainHeight)
            {
                if (position.Y < terrainHeight - BlendRegion)
                {
                    return 255;
                }
                float value = (position.Y - (terrainHeight - BlendRegion)) / BlendRegion;
                return (byte)(255.0f - 255.0f * value);
            }
            // clamp
            return 0;
        }
    }
}