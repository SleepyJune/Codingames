using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    enum Team
    {
        Ally,
        Enemy,
    }

    enum UnitType
    {
        Unit,
        Hero,
        Tower,
        Groot,
    }

    class Entity
    {
        public int id;
        public Team team;

        public float attackRange;
        public float health;
        public float shield;
        public float mana;
        public float damage;
        public float speed;

        public float maxHealth;
        public float maxMana;

        public float stunDuration;
        public float goldValue;

        public float manaRegen;

        public int itemsOwned;
        public bool isVisible;

        public Vector pos;

        public bool isAlly;
        public bool isEnemy;
        public bool isNeutral;

        public Entity target;
        public float moveAttackTime;

        public bool isAggroed;
        
        public bool isRanged
        {
            get
            {
                return attackRange > 150;
            }
        }

        public int GetReward()
        {
            if (this is Hero)
            {
                return 300;
            }
            else
            {
                if (isRanged)
                {
                    return 50;
                }
                else
                {
                    return 30;
                }
            }
        }

        public float GetMoveTime(Vector position)
        {
            return GetDistance(position) / speed;
        }

        public float GetMoveAttackTime(Vector position, Entity target)
        {
            var moveDistance = GetDistance(position);
            var moveTime = Math.Max(0, moveDistance / speed);

            var attackDistance = target.GetDistance(position);

            return moveTime + GetAttackTime(attackDistance);
        }

        public float GetMoveAttackTime(Entity target)
        {
            var distance = GetDistance(target);

            if (distance <= attackRange)
            {
                return GetAttackTime(target);
            }
            else
            {
                var moveDistance = distance - attackRange;
                var moveTime = Math.Max(0, moveDistance / speed);

                return moveTime + GetAttackTime(target);
            }
        }

        public float GetAttackTime(float distance)
        {
            float windup = this is Hero ? .1f : .2f;

            if (isRanged)
            {
                return windup + windup * distance / attackRange;
            }
            else
            {
                return windup;
            }
        }

        public float GetAttackTime(Entity target)
        {
            return GetAttackTime(GetDistance(target));
        }

        public bool CanLastHit(float damage, float minTime, out float time1, out float time2)
        {
            time1 = 1;
            time2 = 1;

            var predictedHealth = health;
            var enemyMinions = isAlly ? Strategy.enemyUnits.Values : Strategy.allyUnits.Values;

            var attackers = enemyMinions
                                    .Where(unit => unit.target != null && unit.target == this)
                                    .OrderBy(unit => unit.moveAttackTime);

            if (health - damage <= 0) //if hero can last hit without help
            {
                if (attackers.Count() == 0) //if no attackers
                {
                    time1 = minTime;
                }
                else //check first moveAttackTime is bigger than minTime
                {
                    if (attackers.First().moveAttackTime > minTime)
                    {
                        time1 = minTime;
                    }
                    //else check in the loop
                }
            }

            foreach (var enemy in attackers)
            {
                predictedHealth -= enemy.damage;

                if (predictedHealth <= 0) //if minion attack killed unit
                {
                    time2 = enemy.moveAttackTime;
                    break;
                }
                else
                {
                    if (predictedHealth - damage <= 0) //if minion attack + hero attack can kill unit
                    {
                        if (time1 == 1 && enemy.moveAttackTime > minTime)
                        {
                            time1 = enemy.moveAttackTime;
                        }
                    }
                }
            }

            if (predictedHealth > 0 && predictedHealth - damage <= 0) //if minTime is bigger than all minion attack
            {
                if (time1 == 1)
                {
                    time1 = minTime;
                }
            }

            if (time2 > time1 && time2 > minTime)
            {
                return true;
            }

            return false;
        }

        public bool WillUnitDie()
        {
            var enemyMinions = isAlly ? Strategy.enemyUnits.Values : Strategy.allyUnits.Values;

            var attackers = enemyMinions
                                    .Where(unit => unit.target != null && unit.target == this)
                                    .OrderBy(unit => unit.moveAttackTime);

            var predictedHealth = health;

            foreach (var enemy in attackers)
            {
                predictedHealth -= enemy.damage;

                if (predictedHealth <= 0)
                {
                    return true;
                }
            }     

            return false;
        }

        public bool WillAggro(Entity unit, Vector pos)
        {
            if (isAggroed && target == unit)
            {
                return true;
            }
            else
            {
                if (target != null)
                {
                    return GetDistance(target) > GetDistance(pos);
                }
                else
                {
                    return GetDistance(pos) <= 300;
                }
            }
        }

        public bool isUnderEnemyTower()
        {
            var tower = isAlly? Strategy.enemyTower : Strategy.myTower;

            return GetDistance(tower) <= tower.attackRange;
        }

        public float PredictHealth(float time)
        {
            var predictedHealth = health;
            
            var enemyMinions = isAlly ? Strategy.enemyUnits.Values : Strategy.allyUnits.Values;

            foreach (var enemy in enemyMinions
                                    .Where(unit => unit.target != null && unit.target == this))
            {
                if (time > enemy.moveAttackTime)
                {
                    predictedHealth -= enemy.damage;
                }
            }

            return predictedHealth;
        }

        public float GetDistance(Entity target)
        {
            return GetDistance(target.pos);
        }

        public float GetDistance(Vector targetPos)
        {
            return pos.Distance(targetPos);
        }

        public override bool Equals(object obj)
        {
            if (obj is Entity)
            {
                return Equals((Entity)this);
            }

            return false;
        }

        public bool Equals(Entity obj)
        {
            return obj.id == this.id;
        }

        public override int GetHashCode()
        {
            return this.id;
        }
    }
}
