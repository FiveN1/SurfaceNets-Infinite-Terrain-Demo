# World Save System

This system takes care of the chunk storage.

It is simple as calling: `SaveChunk()`, when you want to save a chunk at position, and calling `LoadChunk()` when you want to get the data of a desired chunk.
It does not matter if a chunk was already saved when calling `LoadChunk()`, it will load the procedural data if not saved.

Everything else is handeled by this system.

## How is the world saved?
If you look into the world save folder, you will see theese two files: `header.dat` and `world_data.dat`.

### Header
The `header.dat` file contains an octree datastructure wich holds the indexes of saved chunks in `world_data.dat`.
You can imagine this like big 3D octree structure in wich the smallest cells that have an valid ID, contain a saved chunk.

### Body
The `world_data.dat` file contains the actual chunk data stored in one big array.
Note that a special kind of anrray is used here wich can have holes between elements, wich keeps the elements at the same position in array, even if some elements are removed or added. This kind of array is called an Fragmented array, or [FragArray](../../../DataStructures/FragArray) for short.

> [!NOTE]
> Each file has its own class

## How do the save/load functions work?
...


! přidat obrázky !




