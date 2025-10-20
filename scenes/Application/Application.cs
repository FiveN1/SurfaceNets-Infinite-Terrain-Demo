using Godot;
using System;

// Hra:
// - aspect ratio 4:3
// co ti říká?
// - tvorba...
// afdad5, 89602e

// shadow of the collossus

/*
*
* pixelované textury.
*   nemusí být vysokého rozlišení ktyž celá hra bude v horší kvalitě.
* mlha.
*   Představuje nejasnotu věcí.
*   Hlavně se objeví v té sekci kam půjde ten pán.
* low poly.
* Atypografie.
*
* budeš moct položit pouze v gridu.
* kolik ubereš talik přidáš - nemůžeš stvořit ani odebrat hmotu.
* můžeš transformovat jenom jeden kop za druhým.
*
* Partikly ve vzduchu
*
* Načítání levelu:
* - Podle Heightmap a dalších dat ohledně prop. (může být element randomnosti - random vykopávky, struktury)
* - Po načtení části do chunku se chunk uloží zvlášt.
*
* Pokud nebudeš dlouho nic ve světě tvořit začne se rozpadat. (!!)
*
* Všechno v té hře musí mít smysl, důvod, musí něco zdělovt.
*
* čím níž tím větší mlha
*
* Hořlavé západy slunce znamenají velké nebezpečí.
*
*/

/*
* Jak udělat hru?
*
* + nejdřív základní koncept - jedoduchý základ.
*       nedělat nikdy nic perfektní.
*       Nedělat vše najednou. Iterovat od pomalého základu.
*
* + Když tvoříš zbav se svého ega.
* 
* Dnes jsem byl uplně v prdeli, nevěděl jsem kam mířím měl jsem ve všem bordel.
*   pak jsem jel na kole tou nejhorší cestou kterou jsem mohl a vše najednou bylo super.
*   asi jsem si takhle nastavil hranici toho jak co může být špatné a teď mi připadá vše super.
*
* + Nějakej Tlak, a poté vypuštění. (toto se objevuje všude)
*
* + Hudba, Artstyl, UI a Gameplay musí říkat jeden příběh. (Synesthesia)
* 
* + Multiplayerové hry říkají něco o ostatních lidech takže je tam vždy něco nového.
*
* +
*/

/*
* Nápady pro optimalizaci...
*
* V světš bude určený počet collision bodů (pro 2x2 chunk).
*   tyto collision body se budou převádět z chunku do chunku.
*   NE! lepší je LOD, protože by to jinak znamenalo že objekty mimo blízkosti hráče nebudou kolidovat.
* Body kolize použe tam kde je jenom povrch.
*
*/

/*
* BOTTLENECK
*
* Kolizivní tvary. -> je jich tu moc. stačí mít jenom v okolí hráče. [VYŘEŠENO]
* Renderování malých chunků.
*   Lepší mít co největší chunky.
*   + Vzdálené chunky můžou být větší jelikož se nebudou updateovat. -> při přiblížení se rozdělí.
*
* Renderování prázdných chunků.
*   Chunky bez geometrie nemusí být renderovány.
*   + Určený počet meshů pro renderování?
*
* Stíny.
* ŘEŠENÍ:
* - per vertex stíny
*
* Chunk Save
*
*
*
*
*
*
*
*/

/* O čem bude?
*
* - inspirována noahovou archou.
* - člověk přežívá vlny.
*   (* vlny které tě zkouší, mohou tě zničit, velká potopa.)
*   (* vlny budou představovat nepřátelé kteří ti boří svět.)
* - budováním si tvoří toleranci. (mít něco v životě co tě podrží)
*   (* na tuhle část udělat kontrast -> ostatní postavy neumí tvořit a propadají vlnám)
*
* - ... vyšší level ...
*
*/

// zkusit takovou tu hru kde vidíš za sebe, nebo tu s těmi červy v aréně.

/*
* Co je styl?
* Co je charakter?
* Tvoř pravdivě.
* tvoř pro dobro ne pro peníze.
*
* https://www.youtube.com/watch?v=q_jR98O0h_o
*
*/

/*
* HRY CO SI ZAHRÁT:
*
* - Short hike
* - Ultrakill
* - That which gave chase
* - Red dead redemption 2
* - Subnautica
*
*/


public partial class Application : Node
{
    public Application()
    {
        RenderingServer.SetDebugGenerateWireframes(true);

        /*
        DataStructures.FragArray<int> fragArr1 = new DataStructures.FragArray<int>();
        int id1 = fragArr1.Add(165);
        GD.Print("added at: ", id1, " value: ", fragArr1.Get(id1));
        int id2 = fragArr1.Add(8);
        ref int dd = ref fragArr1.Get(id2);
        dd = 89;
        GD.Print("added at: ", id2, " value: ", fragArr1.Get(id2));
        fragArr1.Remove(id1);
        int id3 = fragArr1.Add(206);
        GD.Print("added at: ", id3, " value: ", fragArr1.Get(id3));
        */


        DataStructures.Octree<int> octree = new DataStructures.Octree<int>(new(0.0f, 0.0f, 0.0f), 2.0f);
        octree.SubdivideOctant(0);
        //octree.UnSubdivideOctant(0);
        //octree.SubdivideOctant(1);
        DataStructures.Octant<int> oct1 = octree.GetRoot();
        GD.Print("root: ", oct1);

        int neighborID = octree.FindOctantNeighbor(2, new System.Numerics.Vector3(-1, 0, 0));
        GD.Print("neighborID: ", neighborID);

        //GD.Print("1: ", octree.GetOctant(1));


    }
}


/*
* Plán
*
* Vygenerovat height mesh. (HOTOVO)
* Vygenerovat kolizi pro mesh. (HOTOVO)
* Pohyb hráče. (HOTOVO)
* Marching cubes generace. (HOTOVO)
* Marching cubes normály. (HOTOVO)
* Ray Triangle intersection.
* Per Vertex normaly.
* Marching cubes lineární interpolace. (HOTOVO)
* Marching cubes terrain modifikace.
* Marching cubes compute shader.
* Chunky.
* LOD.
* Načítání terénu.
* Textury.
*
*
*/
