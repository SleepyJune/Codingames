using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
//URL: SHOWING PROBLEM GOING BACK AND FORTH DOING NOTHING (May 15,2017):https://www.codingame.com/replay/220469805 -SOLVED(May 15,2017, 3:30pm)
//URL: output = GG situation(May 15,2017):https://www.codingame.com/replay/220473658 -SOLVED(May 17,2017, 3:30pm)
//URL: output = stuck at diagnosis sending and taking from cloud (May 15,2017)://URL: output = GG situation(May 15,2017):https://www.codingame.com/replay/220476205 -SOLVED(May 17,2017, 3:30pm)
//URL: https://www.codingame.com/replay/223257212 - PROBLEM around turn 300, moving back and forth betwn molcules and diagnosis (May 17,2017)


/**
 * Bring data on patient samples from the diagnosis machine to the laboratory with enough molecules to produce medicine!
 **/
namespace Code4Life_Silver
{


    class Player
    {
        static void Main(string[] args)
        {
            Game.Get_Projects();

            // game loop
            while (true)
            {
                Game.Initialize();
                Game.Act();
                Game.Print_Output();
                Game.TurnEnd();
            }
        }
    }
}