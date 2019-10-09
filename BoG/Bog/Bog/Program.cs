using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace Bog
{
    class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
            
            Strategy.myTeam = int.Parse(Console.ReadLine());

            int bushAndSpawnPointCount = int.Parse(Console.ReadLine()); // useful from wood1, represents the number of bushes and the number of places where neutral units can spawn
            for (int i = 0; i < bushAndSpawnPointCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string entityType = inputs[0]; // BUSH, from wood1 it can also be SPAWN
                int x = int.Parse(inputs[1]);
                int y = int.Parse(inputs[2]);
                int radius = int.Parse(inputs[3]);

                if (entityType == "BUSH")
                {
                    Bush newBush = new Bush();
                    newBush.pos = new Vector(x, y, 0);
                    newBush.attackRange = radius;

                    Strategy.bushes.Add(newBush);
                }
            }

            int itemCount = int.Parse(Console.ReadLine()); // useful from wood2
            for (int i = 0; i < itemCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                string itemName = inputs[0]; // contains keywords such as BRONZE, SILVER and BLADE, BOOTS connected by "_" to help you sort easier
                int itemCost = int.Parse(inputs[1]); // BRONZE items have lowest cost, the most expensive items are LEGENDARY
                int damage = int.Parse(inputs[2]); // keyword BLADE is present if the most important item stat is damage
                int health = int.Parse(inputs[3]);
                int maxHealth = int.Parse(inputs[4]);
                int mana = int.Parse(inputs[5]);
                int maxMana = int.Parse(inputs[6]);
                int moveSpeed = int.Parse(inputs[7]); // keyword BOOTS is present if the most important item stat is moveSpeed
                int manaRegeneration = int.Parse(inputs[8]);
                int isPotion = int.Parse(inputs[9]); // 0 if it's not instantly consumed

                Item newItem = new Item
                {
                    name = itemName,
                    cost = itemCost,
                    damage = damage,
                    health = health,
                    maxHealth = maxHealth,
                    mana = mana,
                    maxMana = maxMana,
                    speed = moveSpeed,
                    manaRegen = manaRegeneration,
                    isPotion = isPotion == 1,
                };

                Strategy.items.Add(itemName, newItem);

                newItem.CalculateItemValue();
                newItem.PrintStats();
            }

            // game loop
            while (true)
            {
                Game.NewRound();

                Strategy.myGold = int.Parse(Console.ReadLine());
                Strategy.enemyGold = int.Parse(Console.ReadLine());
                
                int roundType = int.Parse(Console.ReadLine()); // a positive value will show the number of heroes that await a command
                
                
                int entityCount = int.Parse(Console.ReadLine());
                for (int i = 0; i < entityCount; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int unitId = int.Parse(inputs[0]);
                    int team = int.Parse(inputs[1]);
                    string unitType = inputs[2]; // UNIT, HERO, TOWER, can also be GROOT from wood1
                    int x = int.Parse(inputs[3]);
                    int y = int.Parse(inputs[4]);
                    int attackRange = int.Parse(inputs[5]);
                    int health = int.Parse(inputs[6]);
                    int maxHealth = int.Parse(inputs[7]);
                    int shield = int.Parse(inputs[8]); // useful in bronze
                    int attackDamage = int.Parse(inputs[9]);
                    int movementSpeed = int.Parse(inputs[10]);
                    int stunDuration = int.Parse(inputs[11]); // useful in bronze
                    int goldValue = int.Parse(inputs[12]);
                    int countDown1 = int.Parse(inputs[13]); // all countDown and mana variables are useful starting in bronze
                    int countDown2 = int.Parse(inputs[14]);
                    int countDown3 = int.Parse(inputs[15]);
                    int mana = int.Parse(inputs[16]);
                    int maxMana = int.Parse(inputs[17]);
                    int manaRegeneration = int.Parse(inputs[18]);
                    string heroType = inputs[19]; // DEADPOOL, VALKYRIE, DOCTOR_STRANGE, HULK, IRONMAN
                    int isVisible = int.Parse(inputs[20]); // 0 if it isn't
                    int itemsOwned = int.Parse(inputs[21]); // useful from wood1

                    Entity newUnit;

                    //UNIT, HERO, TOWER
                    if (unitType == "TOWER")
                    {
                        newUnit = new Tower();
                    }
                    else if (unitType == "GROOT")
                    {
                        newUnit = new Groot();
                    }
                    else if (unitType == "HERO")
                    {
                        newUnit = new Hero();

                        Hero newHero = newUnit as Hero;

                        newHero.cooldowns[0] = countDown1;
                        newHero.cooldowns[1] = countDown2;
                        newHero.cooldowns[2] = countDown3;

                        foreach (HeroType type in Enum.GetValues(typeof(HeroType)))
                        {
                            if (type.ToString() == heroType)
                            {
                                newHero.heroType = type;
                                break;
                            }
                        }

                        newHero.InitSpells();
                    }
                    else
                    {
                        newUnit = new Unit();
                    }                    

                    newUnit.id = unitId;
                    newUnit.pos = new Vector(x, y, 0);
                    newUnit.team = Strategy.myTeam == team ? Team.Ally : Team.Enemy;
                    newUnit.attackRange = attackRange;
                    newUnit.health = health;
                    newUnit.maxHealth = maxHealth;
                    newUnit.shield = shield;
                    newUnit.damage = attackDamage;
                    newUnit.speed = movementSpeed;
                    newUnit.stunDuration = stunDuration;
                    newUnit.goldValue = goldValue;
                    
                    //countdown
                    newUnit.mana = mana;
                    newUnit.maxMana = maxMana;
                    newUnit.manaRegen = manaRegeneration;
                    newUnit.isVisible = isVisible == 1;
                    newUnit.itemsOwned = itemsOwned;

                    newUnit.isAlly = Strategy.myTeam == team && team != -1;
                    newUnit.isEnemy = Strategy.myTeam != team && team != -1;
                    newUnit.isNeutral = team == -1;

                    Strategy.allUnits.Add(newUnit.id, newUnit);

                    if (newUnit is Hero)
                    {
                        Strategy.allHeros.Add(newUnit as Hero);
                    }

                    if (newUnit.isAlly)
                    {
                        Strategy.allyUnits.Add(newUnit.id, newUnit);

                        if (newUnit is Hero)
                        {
                            Hero myHero = newUnit as Hero;

                            Strategy.myHeros.Add(myHero);

                            myHero.InitStrategy();                            
                        }
                        else if (newUnit is Tower)
                        {
                            Strategy.myTower = newUnit as Tower;
                        }
                        else if (newUnit is Unit)
                        {
                            Strategy.allyMinions.Add(newUnit);
                        }
                    }
                    else if (newUnit.isEnemy)
                    {
                        Strategy.enemyUnits.Add(newUnit.id, newUnit);

                        if (newUnit is Hero)
                        {
                            Hero hero = newUnit as Hero;

                            Strategy.enemyHeros.Add(hero);
                        }
                        else if (newUnit is Tower)
                        {
                            Strategy.enemyTower = newUnit as Tower;
                        }
                        else if (newUnit is Unit)
                        {
                            Strategy.enemyMinions.Add(newUnit);
                        }
                    }
                    else
                    {
                        Strategy.neutralUnits.Add(newUnit.id, newUnit);
                    }
                }

                Game.MakeMove(roundType);
            }
        }
    }
}