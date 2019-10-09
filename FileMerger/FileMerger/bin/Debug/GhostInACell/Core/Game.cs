using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class Game
    {
        public static int gameTime = 0;
        public static int bomb = 2;

        public static void InitializeFirstTurn()
        {
            Factory.CalculateShortestPaths();
        }

        public static void InitializeTurn()
        {
            Factory.ProcessFactoryStates();
            Factory.ProcessNeighbourCounts();
            Factory.CalculateAllArmyScore();
        }

        public static void MakeMove()
        {
            Strategy.AddMissions();

            while (Strategy.ExecuteMissions()) ;
        }

        public static void PrintActions()
        {
            Action.PrintActions();
        }

        public static void CleanUp()
        {
            Factory.CleanUp();
            Troop.CleanUp();
            Action.CleanUp();
            Bomb.CleanUp();
            Strategy.CleanUp();

            gameTime++;
        }
    }
}
