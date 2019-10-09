using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class FactoryGlobalStats
    {
        public int enemyTotalProduction;
        public int allyTotalProduction;
        public int neutralTotalProduction;

        public int totalAllyCount;
        public int totalEnemyCount;

        public double totalEnemyCountWeighted;
        public double totalAllyCountWeighted;

        public bool allyProductionMaxed;

        public FactoryGlobalStats()
        {

        }
    }
}
