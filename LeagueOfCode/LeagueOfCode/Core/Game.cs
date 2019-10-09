using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueOfCode
{
    class Game
    {
        public static void MakeMove()
        {
            //InitRound();

            Strategy.MakeMove();
            Action.PrintActions();

            Game.CleanUp();
        }

        public static void CleanUp()
        {
            Drafting.CleanUp();
            Action.CleanUp();
            Strategy.CleanUp();
            Player.CleanUp();
            MonsterTrading.CleanUp();
            SummonGroup.CleanUp();
        }
    }
}
