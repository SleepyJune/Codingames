using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Deadpool : HeroStrategy
    {
        public Deadpool(Hero newHero) : base(newHero)
        {
            hero = newHero;            
        }

        public override bool TryRetreating()
        {
            if (hero.isInDanger())
            {
                var spell = hero.spells[0];

                if (spell != null)
                {
                    if (spell.canCast)
                    {                        
                        spell.CastSpell();
                        return true;
                    }
                }

                spell = hero.spells[1];

                if (spell != null)
                {
                    foreach (var enemy in enemyHeros.OrderBy(unit => unit.GetDistance(hero)))
                    {
                        var distance = enemy.GetDistance(hero);
                        
                        if (!enemyMinions.Any(unit => unit.GetDistance(hero) <= distance))
                        {
                            spell.CastSpell(enemy.pos);
                            return true;
                        }
                    }
                }
            }

            return base.TryRetreating();
        }
    }
}
