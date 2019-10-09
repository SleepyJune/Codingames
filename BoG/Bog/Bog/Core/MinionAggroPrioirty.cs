using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog.Core
{
    class MinionAggroPriority : IComparer<Entity>
    {
        public Entity unit;

        public int Compare(Entity target1, Entity target2)
        {
            if (unit.isAggroed && unit.target != null)
            {
                if (unit.target == target1)
                {
                    return -1;
                }
                else if (unit.target == target2)
                {
                    return 1;
                }
            }

            var distance1 = unit.GetDistance(target1);
            var distance2 = unit.GetDistance(target2);

            if (distance1 == distance2)
            {
                if (target1.health <= target2.health)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (distance1 < distance2)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }              
            
            return 0;
        }
    }
}
