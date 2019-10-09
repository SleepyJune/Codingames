using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class MinionPrediction
    {
        public static void CalculateMinionLine()
        {
            Strategy.allyMinionLine =
                Strategy.allyUnits.Values
                    .Where(unit => unit is Unit)
                    .OrderByDescending(unit => unit.GetDistance(Strategy.myTower))
                    .ToList();
        }
    }
}
