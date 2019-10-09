using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    public enum HeroType
    {
        DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
    }

    class Hero : Entity
    {
        public HeroType heroType;

        public Spell[] spells = new Spell[3];
        public float[] cooldowns = new float[3];

        public List<Entity> aggroUnits = new List<Entity>();

        public void InitSpells()
        {
            if (heroType == HeroType.IRONMAN)
            {
                spells[0] = new Blink(this, cooldowns[0]);
                spells[1] = new FireBall(this,cooldowns[1]);
                spells[2] = new Burning(this, cooldowns[2]);
            }

            if (heroType == HeroType.DOCTOR_STRANGE)
            {
                spells[0] = new AoeHeal(this, cooldowns[0]);
                spells[1] = new Shield(this, cooldowns[1]);
                spells[2] = new Pull(this, cooldowns[2]);
            }

            if (heroType == HeroType.DEADPOOL)
            {
                spells[0] = new Counter(this, cooldowns[0]);
                spells[1] = new Counter(this, cooldowns[1]);
            }
        }

        public void InitStrategy()
        {
            if (heroType == HeroType.IRONMAN)
            {
                Strategy.myHeroStrategies.Add(id, new IronMan(this));
            }
            else if (heroType == HeroType.DOCTOR_STRANGE)
            {
                Strategy.myHeroStrategies.Add(id, new DrStrange(this));
            }
            else if (heroType == HeroType.DEADPOOL)
            {
                Strategy.myHeroStrategies.Add(id, new Deadpool(this));
            }
            else
            {
                Strategy.myHeroStrategies.Add(id, new HeroStrategy(this));
            }
        }

        public static void InitAggro()
        {
            foreach (var hero in Strategy.myHeros)
            {
                hero.aggroUnits = Strategy.allUnits.Values.Where(unit => unit.target == hero).ToList();
            }
        }

        public bool isTargetInMoveAttackRange(Entity target)
        {
            var distance = .8 * speed;
            return GetDistance(target) <= distance + attackRange;
        }

        public bool isInBush(Bush bush)
        {
            return GetDistance(bush) <= bush.attackRange;
        }

        public bool isInEnemyHeroRange()
        {
            return Strategy.enemyHeros.Any(unit => GetDistance(unit) <= (
                unit.isRanged ? unit.attackRange : unit.speed * .8f + unit.attackRange));
        }

        public bool isInDanger()
        {
            int aggroCount = 0;

            var enemyHero = isAlly? Strategy.enemyHeros : Strategy.myHeros;

            if (!isVisible)
            {
                return false;
            }

            foreach (var hero in enemyHero)
            {
                if (GetDistance(hero) <= hero.attackRange)
                {
                    aggroCount += 3;
                }
            }

            var minions = isAlly? Strategy.enemyUnits : Strategy.allyUnits;
            foreach (var minion in minions.Values)
            {
                if (minion is Unit)
                {
                    if (minion.target != null)
                    {
                        if (minion.target == this || minion.GetDistance(minion.target) > minion.GetDistance(this))
                        {
                            aggroCount += 1;
                        }
                    }
                }
            }

            if (aggroCount >= 3)
            {
                return true;
            }

            return false;
        }

        public Bush GetNearestRetreatBush()
        {
            foreach (var bush in Strategy.bushes.OrderBy(bush => GetDistance(bush)))
            {
                if (!Strategy.enemyHeros.Any(enemy => bush.GetDistance(enemy) <= bush.attackRange))
                {
                    return bush;
                }
            }

            return null;
        }
    }
}
