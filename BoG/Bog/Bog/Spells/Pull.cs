using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Pull : Spell
    {
        public Pull(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "PULL";

            spellType = SpellType.Targeted;

            manaCost = 40;
            range = 400;
            flyTime = 0.9f;
            radius = 200;
            castTime = 0.4f;
            cooldown = 5;            
        }

        public override bool SpellLogic()
        {
            foreach (var enemy in Strategy.enemyHeros)
            {
                if (Strategy.enemyHeros.Count > 1 && enemy.heroType == HeroType.IRONMAN)
                {
                    if (enemy.cooldowns[0] < 2)
                    {
                        continue;
                    }
                }

                if (enemy.GetDistance(hero) <= range)
                {
                    int aggroCount = 0;

                    var dir = (hero.pos - enemy.pos).Normalized();
                    var pos = enemy.pos + dir * radius;

                    foreach (var ally in Strategy.allyUnits.Values)
                    {
                        if (ally is Unit && ally.GetDistance(pos) <= 300)
                        {
                            aggroCount += 1;
                        }
                        else if (ally is Tower && ally.GetDistance(pos) <= ally.attackRange)
                        {
                            aggroCount += 3;
                        }
                    }

                    if (aggroCount >= 4)
                    {
                        CastSpell(enemy);
                        return true;
                    }
                }
            }           

            return false;
        }
    }
}
