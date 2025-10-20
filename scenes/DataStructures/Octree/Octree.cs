using System;
using System.Numerics;

namespace DataStructures
{
    // Octree
    // strom dělený po 8.
    // každá větev a každý kořen může držet jakoukoli hodnotu typu T.
    public class Octree<T>
    {
        // 1D pole všech octantů v stromě.
        // tímto se nefragmentuje paměť.
        // octanty mezi sebou drží reference pomocí indexů.
        public FragArray<Octant<T>> octants;

        public readonly int rootIndex; // root octant bude vždy na indexu 0

        //
        // Constructor / Destructor
        //

        public Octree()
        {
            // init pole
            octants = new FragArray<Octant<T>>();
            // neobsahuje nic
            rootIndex = 0;
        }

        public Octree(Vector3 position, float size)
        {
            // init pole
            octants = new FragArray<Octant<T>>();
            // root octant
            rootIndex = octants.Add(new Octant<T>(0, position, size)); // je vždy 0
        }

        public Octree(Vector3 position, float size, T defaultValue)
        {
            // init pole
            octants = new FragArray<Octant<T>>();
            // root octant
            rootIndex = octants.Add(new Octant<T>(0, position, size)); // je vždy 0
        }

        ~Octree()
        {
            // empty
        }

        //
        // Get
        //

        public ref Octant<T> GetOctant(int OctantIndex)
        {
            return ref octants.Get(OctantIndex);
        }

        public ref Octant<T> GetRoot()
        {
            return ref octants.Get(rootIndex); // root je vždy 0, protože je první v poli
        }

        public FragArray<Octant<T>> GetOctantArray()
        {
            return octants;
        }

        //
        // Subdivide
        //

        public void SubdivideOctant(int OctantIndex)
        {
            // POZNÁMKA: jelikož se tady přidávají data, je možné že se změní adresy elementů v array (kvůli reallokaci)

            // check zda už byl subdividován
            if (!octants.Get(OctantIndex).isLeaf) return;
            // předem zapíšeme data o octantu
            Vector3 octantPosition = octants.Get(OctantIndex).position;
            float octantSize = octants.Get(OctantIndex).size;
            // vytvoříme 8 listů
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        int leafOctantIndex = octants.Add(new Octant<T>(OctantIndex, new Vector3(x, y, z) * octantSize * 0.5f + octantPosition, octantSize * 0.5f));
                        octants.Get(OctantIndex).leafs[x + y * 2 + z * 4] = leafOctantIndex;
                    }
                }
            }
            // teď už není listem
            octants.Get(OctantIndex).isLeaf = false;
        }

        // rozdělí leaf do 8 octantů
        // POZNÁMKA: toto je jediná funkce kde se tvoří nové octanty. 
        public void SubdivideOctant(int octantIndex, T copyValue)
        {
            // POZNÁMKA: jelikož se tady přidávají data, je možné že se změní adresy elementů v array (kvůli reallokaci)

            // check zda už byl subdividován
            if (!octants.Get(octantIndex).isLeaf) return;
            // předem zapíšeme data o octantu
            Vector3 octantPosition = octants.Get(octantIndex).position;
            float octantSize = octants.Get(octantIndex).size;
            // vytvoříme 8 listů
            for (int z = 0; z < 2; z++)
            {
                for (int y = 0; y < 2; y++)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        int leafOctantIndex = octants.Add(new Octant<T>(octantIndex, new Vector3(x, y, z) * octantSize * 0.5f + octantPosition, octantSize * 0.5f));
                        octants.Get(octantIndex).leafs[x + y * 2 + z * 4] = leafOctantIndex;
                        // set copy value
                        octants.Get(leafOctantIndex).value = copyValue;

                    }
                }
            }
            // teď už není listem
            octants.Get(octantIndex).isLeaf = false;
        }

        // z rozděleného octantu (kořene) vytvoří jeden (leaf)
        // POZNÁMKA: toto je jediná funkce kde se odstranují octanty. 
        public void UnSubdivideOctant(int OctantIndex)
        {
            ref Octant<T> octant = ref octants.Get(OctantIndex);
            // check zda nebyl subduvudován
            if (octant.isLeaf) return;
            // pro každý list -> unsubdivide dokud nenarazíme na dno
            // potom můžeme smazat když jsou všechny listy pryč
            for (int LeafIndex = 0; LeafIndex < 8; LeafIndex++)
            {
                UnSubdivideOctant(octant.leafs[LeafIndex]);
                octants.Remove(octant.leafs[LeafIndex]);
            }
            // teď je list
            octant.isLeaf = true;
        }

        //
        // Find
        //

        // najde nejmenší octant obsahující pozici.
        // #### Parametry:
        // - Vector3 position -> pozice hterou hledáme
        // - int beginOctantIndex -> octant od kterého začínáme iterovat.
        // NÁPAD: pomocí floor triků by šlo optimalizovat??
        public int FindOctantWithPosition(int beginOctantIndex, Vector3 position)
        {
            ref Octant<T> octant = ref octants.Get(beginOctantIndex);
            // pokud neobsahuje pozici tak se vrací null
            if (!octant.ContainsPosition(position)) return int.MaxValue;
            // pokud není listem, najdem list který obsahuje pozici a iterujem
            if (octant.isLeaf) return beginOctantIndex;
            // nejdeme list s pozicí
            for (int leafIndex = 0; leafIndex < 8; leafIndex++)
            {
                // získáme list a jeho index
                int leafOctantIndex = octant.leafs[leafIndex];
                ref Octant<T> leafOctant = ref octants.Get(octant.leafs[leafIndex]);
                // pokud obsahuje pozici tak iterujem na listu
                if (leafOctant.ContainsPosition(position)) return FindOctantWithPosition(leafOctantIndex, position);
            }
            // sem by se nikdo nikdy neměl dostat...
            return int.MaxValue;
        }

        // získáme octant s pozicí o nejmenší velikosti.
        public int FindOctantWithPosition(int beginOctantIndex, Vector3 position, float minSize)
        {
            ref Octant<T> octant = ref octants.Get(beginOctantIndex);
            // pokud neobsahuje pozici tak se vrací null
            if (!octant.ContainsPosition(position)) return int.MaxValue;
            // pokud není listem, najdem list který obsahuje pozici a iterujem
            if (octant.isLeaf || octant.size <= minSize) return beginOctantIndex;
            // nejdeme list s pozicí
            for (int leafIndex = 0; leafIndex < 8; leafIndex++)
            {
                // získáme list a jeho index
                int leafOctantIndex = octant.leafs[leafIndex];
                ref Octant<T> leafOctant = ref octants.Get(octant.leafs[leafIndex]);
                // pokud obsahuje pozici tak iterujem na listu
                if (leafOctant.ContainsPosition(position)) return FindOctantWithPosition(leafOctantIndex, position, minSize);
            }
            // sem by se nikdo nikdy neměl dostat...
            return int.MaxValue;
        }

        // sečíst indexy?

        //
        // 0 0 
        // 0 0
        //
        //
        public int FindOctantNeighborSibling(int octantIndex, Vector3 direction) // něco špatně...
        {
            // získáme octant
            ref Octant<T> octant = ref octants.Get(octantIndex);

            // pokud se jedná o kořen tak nemá sousedy.
            // sem se třeba dojde pokud hledáme souseda mimo bounds
            if (octant.parent == octantIndex) return int.MaxValue;
            ref Octant<T> octantParent = ref octants.Get(octant.parent);

            Vector3 positionInParent = (octant.position - octantParent.position) / octant.size;
            Vector3 neighborPosInParent = positionInParent + direction;
            //Godot.GD.Print("positionInParent: ", positionInParent, ", neighborPosInParent: ", neighborPosInParent);

            if (neighborPosInParent.X > 1 || neighborPosInParent.X < 0 || neighborPosInParent.Y > 1 || neighborPosInParent.Y < 0 || neighborPosInParent.Z > 1 || neighborPosInParent.Z < 0)
            {
                // transformujem pozici aby místo (0, 1) byla (-1, 1)
                Vector3 positionInParentScaled = positionInParent * 2 - new Vector3(1);
                // tímto jednoduchým trikem spravíme směr. protože pokud by směr nebyl na hraně tak by byl špatný.
                // POZNÁMKA: jelikož se tu nachází i negativní hodnoty tak se podle toho musí určit round.
                direction = (direction + positionInParentScaled) / 2;
                direction = new(
                    direction.X > 0.0f ? Single.Floor(direction.X) : Single.Ceiling(direction.X),
                    direction.Y > 0.0f ? Single.Floor(direction.Y) : Single.Ceiling(direction.Y),
                    direction.Z > 0.0f ? Single.Floor(direction.Z) : Single.Ceiling(direction.Z));
                // iterujem na rodiči
                return FindOctantNeighborSibling(octant.parent, direction);
            }
            int indexInParent = (int)neighborPosInParent.X + (int)neighborPosInParent.Y * 2 + (int)neighborPosInParent.Z * 4;
            // pokud je vše vpohodě, vrátíme souseda, který je také sourozenec.
            return octantParent.leafs[indexInParent];

        }

        // 1, -1, 1
        // 2, -1, 1

        public int FindOctantNeighbor(int octantIndex, Vector3 direction)
        {

            ref Octant<T> octant = ref octants.Get(octantIndex);

            int neighborIndex = FindOctantNeighborSibling(octantIndex, direction);
            if (neighborIndex == int.MaxValue) return int.MaxValue;
            // iterovat do nejmenší velikosti která zabírá celou stranu direction
            // dokud bod nebudude obsahovat body které se nedotýkají bodu.
            Vector3 samplePosition = octant.position + direction * octant.size;
            // získáme octant který je stejné nebo větší velikosti než aktivní octant
            neighborIndex = FindOctantWithPosition(neighborIndex, samplePosition, octant.size);
            return neighborIndex;
        }

        // Rozdělí octree tak aby octant který drží bod byl o velikosti minSize.
        // pokud je strom už rozdělen, neděje se nic.
        public int SubdivideAtPoint(int beginOctantIndex, System.Numerics.Vector3 position, float minSize)
        {
            // najdem octant obsahující pozici
            int octIndex = FindOctantWithPosition(beginOctantIndex, position, minSize);
            if (octIndex == int.MaxValue) return int.MaxValue; // pokud je invalidní pozice
            Octant<T> octant = GetOctant(octIndex);
            // pokud je octant větší než požadovaná velikost tak pokračujem
            if (octant.size > minSize)
            {
                SubdivideOctant(octIndex);
                SubdivideAtPoint(octIndex, position, minSize);
            }
            // jinak vracíme aktualní 
            return octIndex;
        }

        // Postupně rozdělujem octree tak abychom se dostali na pozici o určité velikosti.
        public int SubdivideAtPoint(int beginOctantIndex, System.Numerics.Vector3 position, float minSize, T copyValue)
        {
            // najdem octant obsahující pozici
            int octIndex = FindOctantWithPosition(beginOctantIndex, position, minSize);
            if (octIndex == int.MaxValue) return int.MaxValue; // pokud je invalidní pozice
            Octant<T> octant = GetOctant(octIndex);
            // pokud je octant větší než požadovaná velikost tak pokračujem
            if (octant.size > minSize)
            {
                SubdivideOctant(octIndex, copyValue);
                return SubdivideAtPoint(octIndex, position, minSize, copyValue);
            }
            // jinak vracíme aktualní 
            return octIndex;
        }

    }
}

/*
* Změny:
* 
* [06.09.2025] vytvořeno, protože tuto strukturu budu používat jak v terrain tak i v world save. 
* potom možná i v dalších, tak jsem chtěl vytvořit jednoduchou instanci kterou lze používat na různých místech. 
* snad takhle taky vyčistím všechen ten bordel co je teď v implementaci terénu. (všechny ne-octree data budou v chunku)
* [07.09.2025] implementace základních funkcí (stejné jako předtím).
* všiml jsem si ale jedné optimalizace u FindNeighbor, kde lze místo vektorů použít leafIndexy pro získání souseda.
*
*/


// music scale je unikátní charakter hlasu.