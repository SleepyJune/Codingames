using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class MinionAggro
    {
        public static List<MinionAggro> aggroData = new List<MinionAggro>();

        public Entity unit;
        public Entity target;

        public int turns;

        public static void ApplyData()
        {
            foreach (var aggro in aggroData)
            {
                if (aggro.turns > 0)
                {
                    Entity aggroUnit;
                    if (Strategy.allUnits.TryGetValue(aggro.unit.id, out aggroUnit))
                    {
                        aggroUnit.target = aggro.target;
                        aggroUnit.moveAttackTime = aggroUnit.GetMoveAttackTime(aggro.target);
                        aggroUnit.isAggroed = true;
                    }
                }
            }            
        }

        public static void CleanUp()
        {
            List<MinionAggro> newList = new List<MinionAggro>();

            foreach (var aggro in aggroData)
            {
                if (aggro.turns > 0)
                {
                    aggro.turns -= 1;
                    newList.Add(aggro);
                }
            }

            aggroData = newList;
        }
    }
}
