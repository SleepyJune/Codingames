using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Game
    {                
        public static void NewRound()
        {
            Strategy.round++;
            CleanUp();
        }

        public static void InitRound()
        {
            Strategy.AddInvisibleUnits();
            Strategy.InitRound();
            MinionAggro.ApplyData();
            Hero.InitAggro();
            Groot.CheckAggro();
        }

        public static void MakeMove(int roundType)
        {
            InitRound();

            Strategy.MakeMove(roundType);
        }

        public static void CleanUp()
        {
            Strategy.InvisibleUnits();
            Strategy.CleanUp();
            Action.CleanUp();
            MinionAggro.CleanUp();
        }
    }
}
