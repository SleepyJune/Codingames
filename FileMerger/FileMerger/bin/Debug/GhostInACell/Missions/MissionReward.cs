using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class MissionReward
    {
        public int allyTroopGained;
        public int enemyTroopLost;

        public int combinedReward;

        public MissionReward(int gained, int lost, int turns)
        {
            allyTroopGained = gained;
            enemyTroopLost = lost;

            combinedReward = gained + lost;

            //combinedRewardWeighted = (double)combinedReward / Math.Max(1, turns);
        }

        public static void GetCaptureRate(Mission mission)
        {

        }

        public static bool GetCaptureProbability(Mission mission, int defendingArmy, int turns)
        {
            int maxEnemyTroopCount = 0;

            foreach (var pair in mission.factory.shortestPaths.shortestDistance)
            {
                Factory neighbour = pair.Key;
                int distance = pair.Value;

                if (distance <= 4)
                {
                    if (neighbour.isAlly)
                    {
                        int production = Math.Max(0, neighbour.production * (turns - 2));
                        defendingArmy += neighbour.count + production;
                    }
                    else if (neighbour.team == Team.Enemy)
                    {
                        int production = Math.Max(0, neighbour.production * (turns - 1));
                        maxEnemyTroopCount += neighbour.count + production;
                    }
                }
            }

            return defendingArmy > maxEnemyTroopCount;
        }
    }
}
