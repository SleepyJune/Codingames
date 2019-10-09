using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    public enum SpellType
    {
        Self,
        Targeted,
        Position,
    }

    public enum UsageType
    {
        Defensive,
        Attack,
        Support,
    }

    class Spell
    {
        public string spellName;

        public float manaCost;
        public float range;
        public float castTime;
        public float cooldown;

        public float flyTime;

        public float currentCooldown;

        public float radius;

        public Hero hero;

        public SpellType spellType;
                
        public Spell(Hero owner, float currentCooldown)
        {
            this.hero = owner;
            this.currentCooldown = currentCooldown;
        }

        public virtual float damage
        {
            get
            {
                return 0;
            }
        }

        public virtual bool canCast
        {
            get
            {
                if (hero.mana >= manaCost && currentCooldown <= 0)
                {
                    return true;
                }

                return false;
            }
        }

        public virtual bool SpellLogic()
        {
            return false;
        }

        public bool TryCastSpell()
        {
            if (canCast)
            {
                return SpellLogic();
            }

            return false;
        }

        public IEnumerable<Entity> GetEnemiesInRange()
        {
            return Strategy.enemyUnits.Values.Where(unit => unit.GetDistance(hero) <= range);
        }

        public void CastSpell()
        {
            Action.AddAction(new Action(this));
            Action.AddExtraText(spellName);
        }

        public void CastSpell(Vector position)
        {
            Action.AddAction(new Action(this, position));
            Action.AddExtraText(spellName);
        }

        public void CastSpell(Entity target)
        {
            Action.AddAction(new Action(this, target));
            Action.AddExtraText(spellName);
        }
    }
}
