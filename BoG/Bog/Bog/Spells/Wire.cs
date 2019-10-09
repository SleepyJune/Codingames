using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Wire : Spell
    {
        public Wire(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "WIRE";

            spellType = SpellType.Position;

            manaCost = 50;
            range = 200;
            radius = 50;
            castTime = 0;
            cooldown = 9;            
        }
    }
}
