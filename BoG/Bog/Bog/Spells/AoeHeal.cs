using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class AoeHeal : Spell
    {
        public AoeHeal(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {
            spellName = "AOEHEAL";

            spellType = SpellType.Position;

            manaCost = 50;
            range = 250;
            radius = 100;
            castTime = 0;
            cooldown = 6;            
        }

        public float GetDamage(Entity target)
        {
            return hero.mana * 0.2f;
        }

        public override bool SpellLogic()
        {
            var manaPercent = hero.mana / hero.maxMana;
            if (manaPercent <= .5)
            {
                return false;
            }

            if (!Strategy.enemyUnits.Values.Any(unit => unit.GetDistance(hero) <= unit.attackRange)) //not in danger
            {
                
                float healAmount = GetDamage(null);

                foreach (var ally in Strategy.allyUnits.Values
                                        .Where(unit => unit.GetDistance(hero) <= range)
                                        .OrderBy(unit => unit.health))
                {
                    float totalHealed = 0;

                    foreach (var otherAlly in Strategy.allyUnits.Values.Where(unit => unit.GetDistance(ally) <= radius))
                    {
                        var healthMissing = otherAlly.maxHealth - otherAlly.health;
                                                
                        var realHealAmount = Math.Min(healAmount, healthMissing);

                        if (otherAlly is Hero)
                        {
                            totalHealed += 2 * realHealAmount;
                        }
                        else
                        {
                            totalHealed += realHealAmount;
                        }                        
                    }

                    if (totalHealed >= 4 * healAmount)
                    {
                        CastSpell(ally.pos);
                        return true;
                    }
                }                
            }

            return false;
        }
    }
}
