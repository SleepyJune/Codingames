using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonderWoman
{
    class Game
    {
        public static int gameTurn = 0;
        public static GameState currentState;

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
            Unit.CleanUp();
            Action.CleanUp();

            gameTurn++;
        }
    }
}
