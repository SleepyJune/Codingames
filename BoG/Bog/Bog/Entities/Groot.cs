using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Groot : Entity
    {
        public static void CheckAggro()
        {
            foreach (var neutral in Strategy.neutralUnits.Values)
            {
                if (neutral.health < neutral.maxHealth)
                {
                    neutral.target = Strategy.allHeros.Where(hero => hero.GetDistance(neutral) <= 300)
                                                        .OrderBy(hero => hero.GetDistance(neutral)).FirstOrDefault();

                    if (neutral.target != null)
                    {
                        neutral.moveAttackTime = neutral.GetMoveAttackTime(neutral.target);
                        neutral.isAggroed = true;

                        if (neutral.target.isAlly)
                        {
                            Strategy.enemyUnits.Add(neutral.id, neutral);
                        }
                    }
                }
            }
        }
    }
}
