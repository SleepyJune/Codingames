using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Stealth : Spell
    {
        public Stealth(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "STEALTH";

            spellType = SpellType.Self;

            manaCost = 30;
            range = 350;
            castTime = 0;
            cooldown = 6;            
        }
    }
}
