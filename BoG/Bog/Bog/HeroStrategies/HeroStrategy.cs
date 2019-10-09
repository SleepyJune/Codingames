using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class HeroStrategy : Strategy
    {
        public Hero hero;

        public HeroStrategy(Hero newHero)
        {
            hero = newHero;
        }

        public void MakeMove()
        {
            DebugPrints();

            if (TryUsingPotions())
            {
                return;
            }

            if (MinionTanking())
            {
                if (TryHittingHero())
                {
                    return;
                }

                if (TryCastSpell())
                {
                    return;
                }

                if (TryHittingTower())
                {
                    return;
                }

                if (TryHittingNeutral())
                {
                    return;
                }

                if (TryPurchaseItem())
                {
                    return;
                }
                                
                if (TryLastHit())
                {
                    return;
                }

                if (TryLastHit(true)) //deny minions
                {
                    return;
                }

                if (TryLaneControl())
                {
                    return;
                }                

                if (TryWalkingToUnit())
                {
                    return;
                }
            }
            else
            {
                if (TryRetreating())
                {
                    return;
                }
            }

            Action.AddAction(new Action());
        }

        public virtual bool TryRetreating()
        {
            //var nearestBush = hero.GetNearestRetreatBush();

            var retreatPos = GetRetreatPos();

            if (TryKittingToPos(retreatPos))
            {
                return true;
            }
            else
            {
                Action.AddAction(new Action(retreatPos));
                Action.AddExtraText("Retreating");
                return true;
            }           

            //return false;
        }

        public bool TryKittingToPos(Vector retreatPos)
        {
            var dir = (retreatPos - hero.pos).Normalized();
            var pos = hero.pos + dir * hero.speed * .8f;

            foreach (var enemy in enemyHeros.OrderBy(unit => unit.health))
            {
                var distance = enemy.GetDistance(pos);

                if (distance <= hero.attackRange)
                {
                    float allyDamage = 0;

                    if (!enemyMinions.Any(unit => unit.GetDistance(pos) <= 300))
                    {
                        foreach (var ally in allyUnits.Values.Where(unit => unit.target == enemy))
                        {
                            allyDamage += ally.damage;
                        }

                        if (allyDamage + hero.damage >= enemyHeros.Sum(unit => unit.damage))
                        {
                            Action.AddAction(new Action(pos, enemy));
                            Action.AddExtraText("Kitting " + enemy.heroType.ToString());
                            return true;
                        }
                    }
                }
            }

            //new
            /*foreach (var enemy in enemyMinions
                                    .Where(unit => unit.GetDistance(pos) <= hero.attackRange)
                                    .OrderBy(unit => unit.GetDistance(pos)))
            {
                Action.AddAction(new Action(pos, enemy));
                Action.AddExtraText("Kitting Minion");
                return true;
            }*/

            return false;
        }

        public Vector GetRetreatPos()
        {
            if (enemyHeros.Any(enemy => enemy.isUnderEnemyTower()))
            {
                var bush = hero.GetNearestRetreatBush();

                if (bush != null)
                {
                    return bush.pos;
                }
            } 

            foreach (var unit in allyUnits.Values.OrderByDescending(unit => unit.id))
            {
                if (unit is Unit && !unit.WillUnitDie() && unit.pos.isSafe())
                {
                    //if enemy hero is not close to tower than the ally minion
                    if (!enemyHeros.Any(enemy => enemy.GetDistance(myTower) <= unit.GetDistance(myTower)))
                    {
                        var dir = (myTower.pos - unit.pos).Normalized();
                        var pos = unit.pos + dir * 5;

                        return pos;
                    }
                }
            }

            return myTower.pos;
        }

        public bool TryHittingNeutral()
        {
            foreach (var neutral in neutralUnits.Values.OrderBy(unit => unit.GetDistance(hero)))
            {
                if (neutral.isAggroed && neutral.GetDistance(hero) <= hero.attackRange)
                {
                    Action.AddAction(new Action(neutral));
                    Action.AddExtraText("Farming Neutral: " + neutral.id);
                    return true;
                }
            }

            return false;
        }

        public bool TryHittingTower()
        {
            if (hero.GetDistance(enemyTower) <= hero.attackRange)
            {
                Action.AddAction(new Action(enemyTower));
                Action.AddExtraText("Attacking Tower");
                return true;
            }
            else if (hero.isTargetInMoveAttackRange(enemyTower))
            {
                var dir = (hero.pos - enemyTower.pos).Normalized();
                var pos = enemyTower.pos + dir * hero.attackRange;

                if (!enemyUnits.Values.Any(unit => unit.WillAggro(hero, pos)))
                {
                    Action.AddAction(new Action(enemyTower));
                    Action.AddExtraText("Attacking Tower");
                    return true;
                }
            }

            return false;
        }

        public bool TryHittingHero()
        {
            if (hero.isUnderEnemyTower()) 
            {
                return false;
            }                        

            foreach (var enemy in enemyHeros.OrderBy(unit => unit.health))//.OrderBy(unit => unit.GetDistance(hero.pos)))
            {
                var distanceToHero = hero.GetDistance(enemy);

                if (distanceToHero <= hero.attackRange)
                {
                    //var dir = (hero.pos - enemy.pos).Normalized();
                    //var pos = enemy.pos + dir * hero.attackRange;

                    if (!enemy.isRanged)
                    {
                        if (distanceToHero > enemy.attackRange + 50)
                        {
                            if (enemy.isUnderEnemyTower())
                            {
                                var aggroUnits = enemyUnits.Values.Where(unit => unit is Unit && unit.GetDistance(hero.pos) <= 300 + unit.speed * .8f);
                                float sumDamage = 0;

                                foreach (var unit in aggroUnits)
                                {
                                    sumDamage += unit.damage;
                                }

                                if (sumDamage <= hero.damage)
                                {
                                    Action.AddAction(new Action(enemy));
                                    Action.AddExtraText("Attacking " + enemy.heroType.ToString());
                                    return true;
                                }

                                /*Action.AddAction(new Action(enemy));
                                Action.AddExtraText("Attacking " + enemy.heroType.ToString());
                                return true;*/
                            }
                            else
                            {
                                var bush = hero.GetNearestRetreatBush();

                                var dir = (bush.pos - hero.pos).Normalized();

                                var distanceLeft = Math.Max(0, hero.attackRange - distanceToHero);

                                var distance = Math.Min(distanceLeft, .75f * hero.speed);
                                var pos = hero.pos + dir * distance;

                                if (distanceLeft == 0)
                                {
                                    Action.AddAction(new Action(enemy));
                                    Action.AddExtraText("Attacking " + enemy.heroType.ToString());
                                    return true;
                                }
                                else
                                {
                                    Action.AddAction(new Action(pos, enemy));
                                    Action.AddExtraText("Kitting " + enemy.heroType.ToString());
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            if (enemy.isUnderEnemyTower() && hero.GetDistance(myTower) <= 50)
                            {
                                //Hero otherHero = myHeros.FirstOrDefault(unit => unit.id != hero.id);

                                var bush = hero.GetNearestRetreatBush();

                                if (bush != null)
                                {
                                    Action.AddAction(new Action(bush.pos));
                                    Action.AddExtraText("Moving to Bush");
                                    return true;
                                }

                                Action.AddAction(new Action(enemy));
                                Action.AddExtraText("Attacking " + enemy.heroType.ToString());
                                return true;
                            }
                            else
                            {
                                return TryRetreating();                            
                            }

                        }
                    }
                    else
                    {
                        var aggroUnits = enemyUnits.Values.Where(unit => unit is Unit && unit.GetDistance(hero.pos) <= 300 + unit.speed * .8f);
                        float sumDamage = 0;

                        foreach (var unit in aggroUnits)
                        {
                            if (!unit.WillUnitDie()) //expensive?
                            {
                                sumDamage += unit.damage;
                            }
                        }

                        if (sumDamage <= hero.damage)
                        {
                            Action.AddAction(new Action(enemy));
                            Action.AddExtraText("Attacking " + enemy.heroType.ToString());
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool TryLastHit(bool denyMinion = false)
        {
            if (denyMinion)
            {
                if (enemyUnits.Values.Count >= allyUnits.Values.Count)
                {
                    return false;
                }
            }

            var minions = denyMinion ?
                    allyUnits.Values.OrderBy(unit => unit.GetDistance(hero.pos)) :
                    enemyUnits.Values.OrderBy(unit => unit.GetDistance(hero.pos));

            foreach (var enemy in minions)
            {
                if (enemy is Unit)
                {
                    var moveAttackTime = hero.GetMoveAttackTime(enemy);
                    //Console.Error.WriteLine("AttackTime: " + moveAttackTime);

                    if (moveAttackTime < 1)
                    {
                        float time1 = 1;
                        float time2 = 1;

                        var canLastHit = enemy.CanLastHit(hero.damage, moveAttackTime, out time1, out time2);
                        if (canLastHit)
                        {
                            Console.Error.WriteLine("LastHit: " + enemy.id);

                            if (time1 <= moveAttackTime)
                            {
                                Action.AddAction(new Action(enemy)); //attack immediately
                                Action.AddExtraText("LastHit " + enemy.id);
                                return true;
                            }
                            else
                            {
                                var avgTime = (time1 + time2) / 2;

                                var attackTime = hero.GetAttackTime(enemy);
                                var moveTime = Math.Max(0, avgTime - attackTime);

                                var distance = moveTime * hero.speed;

                                var points = Geometry.CircleCircleIntersection(hero.pos, enemy.pos, distance, hero.attackRange);

                                foreach (var pos in points)
                                {
                                    if (pos.isSafe())//point is safe
                                    {
                                        Console.Error.WriteLine("avgTime: " + avgTime);

                                        Action.AddAction(new Action(pos, enemy));
                                        Action.AddExtraText("LastHit " + enemy.id);
                                        return true;
                                    }
                                }                                
                            }
                        }
                    }
                }
            }

            return false;
        }

        public bool TryLaneControl()
        {
            /*if (enemyUnits.Values.Count <= allyUnits.Values.Count)
            {
                return false;
            }*/

            var maxDistance = hero.speed * .75f;

            foreach (var enemy in enemyUnits.Values
                                        .Where(unit => hero.GetDistance(unit) < hero.attackRange + maxDistance)
                                        .OrderBy(unit => unit.GetDistance(hero.pos)))
            {
                if (enemy is Unit)
                {
                    if (true)//enemy.health >= hero.damage * 2)
                    {
                        /*var ally = allyUnits.Values
                                        .Where(unit => unit is Unit && unit.GetDistance(enemy) + 5 <= hero.attackRange)
                                        .OrderBy(unit => unit.GetDistance(enemy)).FirstOrDefault();

                        if (ally != null)
                        {*/

                        var dir = (hero.pos - enemy.pos).Normalized();
                        var pos = enemy.pos + dir * hero.attackRange;

                        if (!enemy.WillAggro(hero, pos) && pos.isSafe()) //pos is safe
                        {
                            Action.AddAction(new Action(pos, enemy));
                            Action.AddExtraText("Farming " + enemy.id);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool TryWalkingToUnit()
        {
            foreach (var unit in allyUnits.Values.OrderByDescending(unit => unit.GetDistance(myTower)))
            {
                if(unit is Unit)
                {
                    var dir = (myTower.pos - unit.pos).Normalized();
                    var pos = unit.pos + dir * 5;

                    bool isPosSafe = true;

                    foreach (var enemy in enemyUnits.Values)
                    {
                        if (enemy is Unit && enemy.WillAggro(hero, pos))
                        {
                            isPosSafe = false;
                            break;
                        }
                        else if (enemy is Hero && !enemy.isRanged
                            && (enemy.GetDistance(pos) <= 300
                            || enemy.GetDistance(myTower) <= unit.GetDistance(myTower)) )
                        {
                            isPosSafe = false;
                            break;
                        }
                    }

                    if (isPosSafe && pos.isSafe())
                    {
                        if (TryKittingToPos(pos)) //new
                        {
                            return true;
                        }
                        else
                        {
                            Action.AddAction(new Action(pos));
                            Action.AddExtraText("Walking");
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool TryCastSpell()
        {
            foreach (var spell in hero.spells)
            {
                if (spell != null)
                {
                    if (spell.TryCastSpell())
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryUsingPotions()
        {
            var healthPercent = hero.health / hero.maxHealth;
            if (healthPercent <= .25 && enemyHeros.Any(unit => unit.GetDistance(hero) <= unit.attackRange + unit.speed))
            {
                Item item = GetPurchasableItem(true);

                if (item != null)
                {
                    Action.AddAction(new Action(MoveType.Buy, item));
                    Action.AddExtraText("Buying " + item.name);
                    return true;
                }
            }            

            return false;
        }

        public bool TryPurchaseItem()
        {
            if (hero.isInEnemyHeroRange())
            {
                return false;
            }
                        
            Item item = GetPurchasableItem();

            if (item != null)
            {
                if (item.isPotion || hero.itemsOwned < 3)
                {
                    Action.AddAction(new Action(MoveType.Buy, item));
                    Action.AddExtraText("Buying " + item.name);
                    return true;
                }
                else if(!item.isPotion && hero.itemsOwned >= 3) //new
                {
                    if (TrySellingItem(item))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TrySellingItem(Item betterItem)
        {
            var otherHero = GetOtherHero();

            if (otherHero != null && otherHero.itemsOwned < hero.itemsOwned)
            {
                return false;
            }
            
            List<Item> itemList;

            if (Strategy.myHeroItems.TryGetValue(hero.id, out itemList))
            {                
                var itemToSell = itemList.OrderBy(item => item.cost).FirstOrDefault();

                if (itemToSell != null && itemToSell.cost * 2 < betterItem.cost)
                {
                    Action.AddAction(new Action(MoveType.Sell, itemToSell));
                    Action.AddExtraText("Selling " + itemToSell.name);
                    return true;
                }
            }

            return false;
        }

        public Item GetPurchasableItem(bool healOnly = false)
        {
            foreach (var item in items.Values.OrderByDescending(item => item.damage))
            {
                if (myGold > item.cost)
                {
                    if (item.isPotion)
                    {
                        if (item.health > 0) //buy health potion
                        {
                            var lostHealth = hero.maxHealth - hero.health;
                            var healthPercent = hero.health / hero.maxHealth;

                            if (healthPercent <= .5 && lostHealth > item.health)
                            {
                                return item;
                            }
                        }
                    }
                    else
                    {
                        if (healOnly)
                        {
                            continue;
                        }

                        if (myGold - item.cost <= 200 && !enemyHeros.Any(unit => unit.isRanged))
                        {
                            continue;
                        }

                        Hero otherHero = myHeros.FirstOrDefault(unit => unit.id != hero.id);

                        if (otherHero == null || hero.itemsOwned <= otherHero.itemsOwned)
                        {
                            if (item.damage > 0 && item.itemValue > .8) //buy damage
                            {
                                return item;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public Hero GetOtherHero()
        {
            return myHeros.FirstOrDefault(unit => unit.id != hero.id);
        }

        public bool MinionTanking()
        {
            Vector heroPos = hero.pos;

            int minionCount = 0;

            /*if (hero.aggroUnits.Count > 0)
            {
                return false;
            }*/           

            foreach (var ally in allyUnits.Values)
            {
                if (ally is Unit && ally.GetDistance(myTower) > myTower.GetDistance(heroPos) + 5)
                {
                    if (!ally.WillUnitDie())
                    {
                        minionCount++;

                        if (hero.isUnderEnemyTower())
                        {
                            if (minionCount > 1)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        public void DebugPrints()
        {
            //Console.Error.WriteLine("Damage: " + hero.damage);

            /*foreach (var unit in allyUnits.Values)
            {
                Console.Error.WriteLine("Unit " + unit.id + ": " + unit.PredictHealth(1));
            }*/

            /*foreach (var unit in enemyUnits.Values)
                {
                    if (unit is Unit)
                    {
                        //Console.Error.WriteLine("Unit " + unit.id + ": " + unit.PredictHealth(1));
                        if (unit.id == 7)
                        {
                            float time1;
                            float time2;

                            var moveAttackTime = hero.GetMoveAttackTime(unit);
                            var canLastHit = unit.CanLastHit(hero.damage, moveAttackTime, out time1, out time2);

                            //Console.Error.WriteLine("LastHit " + canLastHit + " - Time1: " + time1 + " Time2: " + time2);
                        }
                    }
                }*/
        }
    }
}
