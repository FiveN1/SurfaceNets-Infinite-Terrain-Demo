using Godot;

namespace WorldSystem.Terrain
{
    public struct ChunkCollision
    {
        StaticBody3D staticBody;
        ConcavePolygonShape3D concavePolygonShape;
        CollisionShape3D collisionShape;

        public ChunkCollision(MeshInstance3D chunkMesh)
        {
            staticBody = new StaticBody3D();
            chunkMesh.AddChild(staticBody);
            collisionShape = new CollisionShape3D();
            staticBody.AddChild(collisionShape);

            concavePolygonShape = new ConcavePolygonShape3D();
            collisionShape.Shape = concavePolygonShape;
        }

        // JDE OPTIMALIZOVAT NA GPU !!
        public void Generate(Vector3 nodePosition, float nodeSize, SurfaceNet.MeshData meshData)
        {
            int arrayLength = meshData.indicesSize; //this.meshData.indices.Length;
            Vector3[] verticesForCollision = new Vector3[arrayLength]; // zbytečně dlouhý !!

            for (int i = 0; i < arrayLength; i++)
            {
                verticesForCollision[i] = meshData.vertexPositions[meshData.indices[i]];
            }

            concavePolygonShape.SetFaces(verticesForCollision);
        }

        public void Enabled(bool enabled)
        {
            collisionShape.Disabled = !enabled;
        }
    }
}