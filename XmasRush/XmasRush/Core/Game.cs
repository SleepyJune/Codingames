using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
{
    class Game
    {
        public static void MakeMove()
        {
            //InitRound();

            Strategy.MakeMove();
            //Action.PrintActions();

            Game.CleanUp();
        }

        public static void CleanUp()
        {
            Tile.CleanUp();
            Item.CleanUp();
        }
    }
}
