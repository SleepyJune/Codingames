using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    static class Position
    {
        public static bool isEnemyCloseBy(this Vector pos)
        {
            float cooldownTime = 999;

            foreach (var enemy in Strategy.enemyHeros)
            {
                if (enemy.GetDistance(pos) <= enemy.attackRange)
                {
                    return true;
                }

                if (enemy.heroType == HeroType.DOCTOR_STRANGE
                    && enemy.cooldowns[2] <= cooldownTime
                    && enemy.GetDistance(pos) <= 400)
                {
                    return true;
                }

                if (enemy.heroType == HeroType.HULK)
                {
                    if (enemy.cooldowns[0] <= cooldownTime && enemy.GetDistance(pos) <= 300)
                    {
                        return true;
                    }

                    if (enemy.cooldowns[2] <= cooldownTime && enemy.GetDistance(pos) <= 150)
                    {
                        return true;
                    }
                }

                if (enemy.heroType == HeroType.VALKYRIE)
                {
                    if (enemy.cooldowns[1] <= cooldownTime && enemy.GetDistance(pos) <= 250)
                    {
                        return true;
                    }

                    if (enemy.cooldowns[0] <= cooldownTime && enemy.GetDistance(pos) <= 155)
                    {
                        return true;
                    }
                }

            }

            return false;
            //return Strategy.enemyHeros.Any(enemy => enemy.GetDistance(pos) <= 150);
        }

        public static bool isSafe(this Vector pos)
        {
            return pos.isInbound() && !pos.isEnemyCloseBy();
        }

        public static bool isInbound(this Vector pos)
        {
            return pos.x >= 0 && pos.x <= 1920 && pos.y >= 0 && pos.y <= 750;
        }
    }
}
