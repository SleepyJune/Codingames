using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Counter : Spell
    {
        public Counter(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "COUNTER";

            spellType = SpellType.Self;

            manaCost = 40;
            range = 350;
            castTime = 0;
            cooldown = 5;            
        }
    }
}
