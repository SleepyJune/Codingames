using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Unit : Entity
    {        
        public void GetUnitTarget()
        {
            var enemies = isAlly ? Strategy.enemyUnits.Values : Strategy.allyUnits.Values;
                        
            target = 
                enemies
                .Where(unit => GetDistance(unit) < speed + attackRange)
                .OrderBy(unit => GetDistance(unit))
                .ThenBy(unit => unit.health)
                .FirstOrDefault();

            if (target != null)
            {
                moveAttackTime = GetMoveAttackTime(target);
            }
        }        
    }
}
