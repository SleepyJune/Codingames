using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Code4Life
{
    class Game
    {
        public static int gameTurn = 0;

        public static void InitializeFirstTurn()
        {
            float loopStartTime = Timer.TickCount;

            
            float loopTime = Timer.TickCount - loopStartTime;
            //Console.Error.WriteLine("Initialization Time: " + loopTime);
        }

        public static void InitializeTurn()
        {
            Player.Initialize();
            Sample.Initialize();
            Molecule.Initialize();
        }

        public static void MakeMove()
        {
            PrintStats();

            bool success =
            Strategy.MoveWait() &&
            Strategy.Collect() &&
            Strategy.Analyze() &&
            Strategy.Gather() &&
            Strategy.Produce();
        }

        public static void PrintActions()
        {
            Action.PrintActions();
        }

        public static void PrintStats()
        {
            Player.me.PrintPlayerStats();
            foreach (var sample in Player.me.samples)
            {
                sample.PrintStats();
            }
        }

        public static void CleanUp()
        {
            Action.CleanUp();
            Player.CleanUp();
            Sample.CleanUp();
            Molecule.CleanUp();

            gameTurn++;
        }
    }
}
