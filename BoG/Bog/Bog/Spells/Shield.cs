using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Shield : Spell
    {
        public Shield(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "SHIELD";

            spellType = SpellType.Targeted;

            manaCost = 40;
            range = 500;
            castTime = 0;
            cooldown = 6;            
        }

        public override bool SpellLogic()
        {
            foreach (var ally in Strategy.myHeros.Where(unit => unit.GetDistance(hero) <= range))
            {
                if (ally.isInDanger())
                {
                    CastSpell(ally);
                    return true;
                }
            }

            return false;
        }
    }
}
