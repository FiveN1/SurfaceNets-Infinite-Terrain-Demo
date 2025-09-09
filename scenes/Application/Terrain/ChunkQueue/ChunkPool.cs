using Godot;
using System;
using System.Collections.Generic;

public class ChunkPool
{

    List<Terrain22.Chunk> queue; // nedrží se indexy !!
    Node3D meshParentNode;

    public ChunkPool(Node3D meshParentNode)
    {
        queue = new List<Terrain22.Chunk>();
        this.meshParentNode = meshParentNode;
    }

    public int GetAvalibeChunk()
    {
        Terrain22.Chunk chunk;
        // najdeme volný chunk v poli
        for (int chunkIndex = 0; chunkIndex < queue.Count; chunkIndex++)
        {
            chunk = queue[chunkIndex];
            if (!chunk.usedbyNode)
            {
                chunk.Enable();
                return chunkIndex;
            }
        }
        // pokud není volný tak vytvoříme nový
        chunk = new Terrain22.Chunk(meshParentNode);
        queue.Add(chunk);

        
        chunk.Enable();
        return queue.Count - 1;
    }

    public void MakeChunkavalible(int chunkIndex)
    {
        //if (chunkIndex < 0) return;
        //if (chunkIndex >= queue.Count) return;

        Terrain22.Chunk chunk = queue[chunkIndex];

        chunk.Disable();
    }

    public Terrain22.Chunk GetChunk(int chunkIndex)
    {
        return queue[chunkIndex];
    }

    public void ClearQueue()
    {

    }

}