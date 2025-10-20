# SurfaceNets Infinite Terrain Demo
Using the Godot Engine v4.4.1 C#

This demo shows puts you in a world that is fully destructable and rendered in its entierity before your very eyes.
The world is made out of voxels that are converted into meshes.

<img style="float: right;" src="https://github.com/FiveN1/SurfaceNets-Infinite-Terrain-Demo/blob/main/assets/repo/screenshot1.png" alt="Nymphaea 3D screenshot" width="512"/>

> [!WARNING]
> This demo is still indev.
> Will lack some features

## Features
### SurfaceNets meshing ✅
Implemented.
Features the SurfaceNets algorithm wich creates a mesh from a unsigned byte field.
I chose the SurfaceNets method over MarchingCubes because it is faster (it can be calculated on the GPU), simplier and most importantly easier to stich with neighboring chunks of diffrent sizes.

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
