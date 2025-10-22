# World
The world is split into two systems.
The rendering system (WorldTerrain) and save system (WorldSave)

Each one has its own octree structure, used for diffrent purposes.

In WorldTerrain it is used for dynamic LOD, and in WorldSave it is used in the header for mapping chunks from buffer to 3D world.
