using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class FireBall : Spell
    {
        public FireBall(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "FIREBALL";

            spellType = SpellType.Position;

            manaCost = 60;
            range = 900;
            flyTime = 0.9f;
            radius = 50;
            castTime = 0;
            cooldown = 6;            
        }

        public float GetDamage(Entity target)
        {
            return hero.mana * 0.2f + 55 * target.GetDistance(hero) / 1000f;
        }

        public float GetMaxDamage()
        {
            return hero.maxMana * 0.2f + 55 * 900 / 1000;
        }

        public override bool SpellLogic()
        {
            //var enemies = GetEnemiesInRange();
                        
            foreach (var enemy in Strategy.enemyHeros.Where(unit => unit.GetDistance(hero) <= range))
            {
                var dir = (enemy.pos - hero.pos).Normalized();
                var endPos = hero.pos + dir * range;

                int unitsHit = 0;
                float totalDamage = 0;

                foreach (var unit in Strategy.neutralUnits.Values.Where(unit => unit.GetDistance(hero) <= range))
                {
                    var distanceToNeutral = unit.GetDistance(enemy);
                    if (distanceToNeutral <= 300 && Strategy.myHeros.Any(myHero => myHero.GetDistance(unit) > distanceToNeutral))
                    {
                        CastSpell(unit.pos);
                        return true;
                    }
                }

                foreach (var unit in GetEnemiesInRange())
                {
                    var projectionInfo = unit.pos.ProjectOn(hero.pos, endPos);

                    if (projectionInfo.IsOnSegment)
                    {
                        if (projectionInfo.SegmentPoint.Distance(unit.pos) <= radius)
                        {    
                            var unitDamage = GetDamage(unit);
                        
                            if (unit is Hero) //broken?
                            {
                                unitsHit += 5;

                                if (unitDamage >= unit.health)
                                {
                                    totalDamage += unitDamage * 9999;
                                }

                                totalDamage += unitDamage * 3;
                            }
                            else
                            {
                                unitsHit += 1;

                                totalDamage += unitDamage;
                            }
                        }
                    }
                }

                //var threshold = 5 * Strategy.enemyHeros.Count + 2;                

                var threshold = 5 * Strategy.enemyHeros.Count * GetMaxDamage();
                
                if (totalDamage >= threshold / 3)
                {
                    CastSpell(enemy.pos);
                    return true;
                }                
            }           

            return false;
        }
    }
}
