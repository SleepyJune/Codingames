using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
    class Game
    {
        public static int timeLeft = 0;
        public static int gameTurn = 0;

        public static void InitializeFirstTurn()
        {
            float loopStartTime = Timer.TickCount;

            Pathfinding.InitializeShortestPaths();

            float loopTime = Timer.TickCount - loopStartTime;
            Console.Error.WriteLine("Initialization Time: " + loopTime);
        }

        public static void InitializeTurn()
        {
            timeLeft = gameTurn == 0 ? 1000 : 50;
            Cannon.GetExplodingMines();
            Ship.GetAllyShipPaths();
        }       

        public static void MakeMove()
        {            
            Strategy.MakeMove();
        }

        public static void PrintActions()
        {
            Action.PrintActions();
        }

        public static void CleanUp()
        {
            Ship.CleanUp();
            Barrel.CleanUp();
            Action.CleanUp();
            Cannon.CleanUp();
            Mine.CleanUp();

            gameTurn++;
        }
    }
}
