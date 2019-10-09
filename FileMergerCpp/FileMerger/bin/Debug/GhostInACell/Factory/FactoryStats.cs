using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class FactoryStats
    {
        public int enemyCount; //enemy neighbour count
        public int allyCount;

        public int averageEnemyDistance;
        public int enemyFactoryCount;

        public double enemyCountWeighted;
        public double allyCountWeighted;

        public double enemyScore;
        public double allyScore;
        public double combinedScore;
        public double differenceScore;

        public double missionPrority;

        public FactoryStats()
        {

        }
    }
}
