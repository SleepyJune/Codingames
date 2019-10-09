using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class IronMan : HeroStrategy
    {        
        public IronMan(Hero newHero) : base(newHero)
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
                        var pos = GetRetreatPos();

                        spell.CastSpell(pos);
                        return true;
                    }
                }
            }

            return base.TryRetreating();
        }
    }
}
