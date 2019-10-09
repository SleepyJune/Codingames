using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Burning : Spell
    {
        public Burning(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "BURNING";

            spellType = SpellType.Position;

            manaCost = 50;
            range = 250;
            radius = 100;
            castTime = 0;
            cooldown = 5;            
        }

        public float GetDamage(Entity target)
        {
            return hero.manaRegen * 5 + 30;
        }

        public override bool SpellLogic()
        {
            foreach (var enemy in Strategy.enemyHeros.Where(unit => unit.GetDistance(hero) <= range))
            {                
                int unitsHit = 0;

                foreach (var unit in Strategy.enemyUnits.Values)
                {
                    if (unit.GetDistance(enemy) <= radius)
                    {
                        if (unit is Hero)
                        {
                            unitsHit += 2;
                        }
                        else
                        {
                            unitsHit += 1;
                        }
                    }
                }

                var threshold = 2 * Strategy.enemyHeros.Count + 3;

                if (unitsHit >= threshold)
                {
                    CastSpell(enemy.pos);
                    return true;
                }
            }

            return false;
        }
    }
}
