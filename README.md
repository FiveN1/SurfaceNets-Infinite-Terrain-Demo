# SurfaceNets infinite terrain demo
Using Godot 4.4.1 C#

> [!WARNING]
> This demo is still indev.
> Will lack some features

## Features
### SurfaceNets meshing ✅
Implemented.
Features the SurfaceNets algorithm wich creates a mesh from a unsigned byte field.
I chose the SurfaceNets method over MarchingCubes because it is faster, simplier and most importantly easier to stich with neighboring chunks of diffrent sizes.

[Check it out here!](scenes/Application/World/WorldTerrain/Chunk/SurfaceNets)

### Octree LOD system ✅
Implemented

### Chunk save system ✅
Implemented.
A pretty complex system wich stores every modified chunk of the world in one file and keeps track of them trough an octree data structure stored in the header file.

> [!WARNING]
> This system is not optimized!

[Check it out here!](scenes/Application/World/WorldSave)

### Chunk Stich system❌
*Not yet implemented*

## Origin
This was originally supposed to be a game but i got overwhelmed by its scope.
So i will at least share my current progress here with anyone who wants to implement this cool terrain rendering method.
Maybe i will return to it in the future when I'll have some more time.
