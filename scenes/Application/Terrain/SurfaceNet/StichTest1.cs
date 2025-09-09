using System;
using Godot;


/*
*
* novější design V5
*
* CONCEPT:
*
* vytvoříme spoje pro každou plochu.
* plochou se popčítá každý dotek mezi aktivním octanetem a sousedícím octantem. (mohou být různých velikostí)
* (ale pro to aby fungovaly musí být také v nějakých případech 1D, nebo 0D)
* bude třeba ale také udělat:
* - akždou plochu (array vertexů) zařadit aby bylo možné ji získat pro spojení s vertexy.
*   nejlépe tak aby strukturou id byly samotné octanty sousedící s aktivním octantem.
* - pro tohle bude jedna funkce, nejlépe. (pro čitelnost, je nejspíš možné)
* 
* hrany budou složitější protože se jedná pouze o spoje s existujícími vertexy.
* pro každou hranu zjistit:
* - všechny octanty na každé straně. (není jisté)
*
* PRINCIP:
*
* (0) pro každý směr:
* - získat plochy
*
* (1) Pro každou plochu:
* - uložit vertexy, a rovnou spojit vertexy s vertexy v ploše.
* 
* (2) uložit vertexy na hranách:
* - nespojovat, protože není s čím spojit. 
*
* (3) spojit hrany:
* - pomocí získaných vertexů iterovat pouze na hranách ploch.
* - iteruje se na menší ploše.
* - nejspíš se rozlišuje podle toho kolik se spojuje ploch.
*   takže jedna iterace pro pouze 3 ocotanty (2 octanty se spojily v prvním kroku), jinak pro 4 octanty atd...
* (4) hotovo.
*
* PROBLÉMY:
* - jak zařadit vertexy které jsou na hraně? (vertexy z cotantu +XY atd..)
*   u nich se nejedná o 2D plocu ale třeba o 1D nebo 0D.
* - jak uložit vertexy tak aby je bylo možné získat pro spojení hran.
* 
*
*
*/
/*
*
* design V6
*
* Pro každou stranu vytvořit mesh. (rovnou)
*
*
*
*/




namespace SurfaceNet
{
    // sekce může být strom

    public struct Section
    {
        // sekce obsahuje:
        // body.
        // -> velikost. (3d vektor) (aby bylo možné tvořit array až do 3d)
        // -> pozice.
        // -> ...

        Vector3 size;
        Vector3 position;

        bool isLeaf;


        public Section()
        {
            isLeaf = true;
        }
    }


    public class ChunkEdgeData
    {
        // každá sekce se skládá z dalších pod sekcí.

        // section pool.

        int[] sections;

        public ChunkEdgeData()
        {
            // pro každý edge získat rovnou index !
            //

        }



    }







}