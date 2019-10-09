using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Blink : Spell
    {
        public Blink(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "BLINK";

            spellType = SpellType.Position;

            manaCost = 16;
            range = 200;
            castTime = 0.05f;
            cooldown = 3;            
        }

        public override bool SpellLogic()
        {
            /*var aggroCount = 0;
            foreach (var aggro in owner.aggroUnits)
            {
                if (aggro is Hero)
                {
                    aggroCount += 3;
                }
                else
                {
                    aggroCount += 1;
                }
            }

            if (aggroCount >= 4)
            {
                var bush = owner.GetNearestRetreatBush();

                if (!owner.isInBush(bush))
                {
                    if (owner.GetDistance(bush) <= range)
                    {
                        CastSpell(bush.pos);
                        return true;
                    }
                    else
                    {
                        var dir = (bush.pos - owner.pos).Normalized();
                        var pos = owner.pos + dir * range;

                        CastSpell(pos);
                        return true;
                    }
                }
            }

            foreach (var enemy in Strategy.enemyHeros)
            {
                if (!enemy.isRanged && owner.GetDistance(enemy) <= range)
                {
                    if (!enemy.isUnderEnemyTower())
                    {
                        var bush = owner.GetNearestRetreatBush();

                        if (!owner.isInBush(bush))
                        {
                            if (owner.GetDistance(bush) <= range)
                            {
                                CastSpell(bush.pos);
                                return true;
                            }
                            else
                            {
                                var dir = (bush.pos - owner.pos).Normalized();
                                var pos = owner.pos + dir * range;

                                CastSpell(pos);
                                return true;
                            }
                        }
                    }
                }
            }*/
            
            

            return false;
        }

    }
}
