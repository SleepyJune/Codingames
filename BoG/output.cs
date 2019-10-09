
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

    class Game
    {                
        public static void NewRound()
        {
            Strategy.round++;
            CleanUp();
        }

        public static void InitRound()
        {
            Strategy.AddInvisibleUnits();
            Strategy.InitRound();
            MinionAggro.ApplyData();
            Hero.InitAggro();
            Groot.CheckAggro();
        }

        public static void MakeMove(int roundType)
        {
            InitRound();

            Strategy.MakeMove(roundType);
        }

        public static void CleanUp()
        {
            Strategy.InvisibleUnits();
            Strategy.CleanUp();
            Action.CleanUp();
            MinionAggro.CleanUp();
        }
    }

    class MinionAggro
    {
        public static List<MinionAggro> aggroData = new List<MinionAggro>();

        public Entity unit;
        public Entity target;

        public int turns;

        public static void ApplyData()
        {
            foreach (var aggro in aggroData)
            {
                if (aggro.turns > 0)
                {
                    Entity aggroUnit;
                    if (Strategy.allUnits.TryGetValue(aggro.unit.id, out aggroUnit))
                    {
                        aggroUnit.target = aggro.target;
                        aggroUnit.moveAttackTime = aggroUnit.GetMoveAttackTime(aggro.target);
                        aggroUnit.isAggroed = true;
                    }
                }
            }            
        }

        public static void CleanUp()
        {
            List<MinionAggro> newList = new List<MinionAggro>();

            foreach (var aggro in aggroData)
            {
                if (aggro.turns > 0)
                {
                    aggro.turns -= 1;
                    newList.Add(aggro);
                }
            }

            aggroData = newList;
        }
    }

    class MinionAggroPriority : IComparer<Entity>
    {
        public Entity unit;

        public int Compare(Entity target1, Entity target2)
        {
            if (unit.isAggroed && unit.target != null)
            {
                if (unit.target == target1)
                {
                    return -1;
                }
                else if (unit.target == target2)
                {
                    return 1;
                }
            }

            var distance1 = unit.GetDistance(target1);
            var distance2 = unit.GetDistance(target2);

            if (distance1 == distance2)
            {
                if (target1.health <= target2.health)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                if (distance1 < distance2)
                {
                    return -1;
                }
                else
                {
                    return 1;
                }
            }              
            
            return 0;
        }
    }

    class MinionPrediction
    {
        public static void CalculateMinionLine()
        {
            Strategy.allyMinionLine =
                Strategy.allyUnits.Values
                    .Where(unit => unit is Unit)
                    .OrderByDescending(unit => unit.GetDistance(Strategy.myTower))
                    .ToList();
        }
    }

    class Strategy
    {
        public static Vector team0spawn = new Vector(200, 590, 0);
        public static Vector team1spawn = new Vector(1720, 590, 0);

        public static int myTeam = 0;

        public static int myGold = 0;
        public static int enemyGold = 0;

        public static int round = 0;

        public static Dictionary<string, Item> items = new Dictionary<string, Item>();

        public static Dictionary<int, Entity> allUnits = new Dictionary<int, Entity>();
        public static Dictionary<int, Entity> allyUnits = new Dictionary<int, Entity>();
        public static Dictionary<int, Entity> enemyUnits = new Dictionary<int, Entity>();
        public static Dictionary<int, Entity> neutralUnits = new Dictionary<int, Entity>();

        public static List<Entity> allyMinions = new List<Entity>();
        public static List<Entity> enemyMinions = new List<Entity>();
        
        public static List<Entity> allyMinionLine = new List<Entity>();

        public static List<Hero> myHeros = new List<Hero>();
        public static List<Hero> enemyHeros = new List<Hero>();
        public static List<Hero> allHeros = new List<Hero>();

        public static List<Bush> bushes = new List<Bush>();

        public static Dictionary<int, Hero> hiddenHero = new Dictionary<int, Hero>();

        public static Dictionary<int, List<Item>> myHeroItems = new Dictionary<int, List<Item>>();
                
        public static Dictionary<int, HeroStrategy> myHeroStrategies = new Dictionary<int, HeroStrategy>();
        
        public static Tower myTower;
        public static Tower enemyTower;

        public static void InitRound()
        {
            foreach (var entity in allUnits.Values)
            {
                if (entity is Unit)
                {
                    Unit unit = entity as Unit;
                    unit.GetUnitTarget();
                }
                else if (entity is Tower)
                {
                    Tower tower = entity as Tower;
                    tower.GetUnitTarget();
                }
            }
        }

        public static void MakeMove(int roundType)
        {            
            if (roundType < 0)
            {
                if (myHeros.Count < 1)
                {
                    Console.WriteLine("IRONMAN");
                    //Console.WriteLine("HULK");
                }
                else
                {
                    if (false)//enemyHeros.Any(unit => !unit.isRanged))
                    {
                        Console.WriteLine("DEADPOOL");
                    }
                    else
                    {
                        Console.WriteLine("DOCTOR_STRANGE");
                    }                    
                }
                return;
            }
            else
            {
                foreach(var hero in myHeroStrategies.Values)//.OrderBy(unit => unit.hero.id))
                {
                    //Console.Error.WriteLine(hero.hero.heroType.ToString());
                    hero.MakeMove();
                }
                //Action.AddAction(new Action());
            }

            Action.PrintActions();
        }

        public static void AddInvisibleUnits()
        {
            foreach (var enemy in hiddenHero.Values)
            {
                if (!enemyHeros.Any(unit => unit.id == enemy.id))
                {
                    enemyHeros.Add(enemy);
                    enemyUnits.Add(enemy.id, enemy);

                    Console.Error.WriteLine("invisible");
                }
            }
        }

        public static void InvisibleUnits()
        {
            foreach (var enemy in enemyHeros)
            {
                if (!enemy.isVisible)
                {
                    Console.Error.WriteLine("invisible2");

                    if (!hiddenHero.ContainsKey(enemy.id))
                    {
                        hiddenHero.Add(enemy.id, enemy);
                    }
                }
            }
        }

        public static void CleanUpInvisibleUnits()
        {
            foreach (var enemy in enemyHeros)
            {
                if (enemy.isVisible)
                {
                    if (hiddenHero.ContainsKey(enemy.id))
                    {
                        hiddenHero.Remove(enemy.id);
                    }
                }
            }
        }
                
        public static void CleanUp()
        {
            CleanUpInvisibleUnits();

            allUnits = new Dictionary<int, Entity>();
            
            allyUnits = new Dictionary<int, Entity>();
            enemyUnits = new Dictionary<int, Entity>();
            neutralUnits = new Dictionary<int, Entity>();

            myHeros = new List<Hero>();            
            allHeros = new List<Hero>();
            enemyHeros = new List<Hero>();

            allyMinions = new List<Entity>();
            enemyMinions = new List<Entity>();

            myHeroStrategies = new Dictionary<int, HeroStrategy>();

            allyMinionLine = new List<Entity>();
        }
    }

    class Bush : Entity
    {
        
    }

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

    class Groot : Entity
    {
        public static void CheckAggro()
        {
            foreach (var neutral in Strategy.neutralUnits.Values)
            {
                if (neutral.health < neutral.maxHealth)
                {
                    neutral.target = Strategy.allHeros.Where(hero => hero.GetDistance(neutral) <= 300)
                                                        .OrderBy(hero => hero.GetDistance(neutral)).FirstOrDefault();

                    if (neutral.target != null)
                    {
                        neutral.moveAttackTime = neutral.GetMoveAttackTime(neutral.target);
                        neutral.isAggroed = true;

                        if (neutral.target.isAlly)
                        {
                            Strategy.enemyUnits.Add(neutral.id, neutral);
                        }
                    }
                }
            }
        }
    }

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

    class Item
    {
        public string name;
        public float cost;
        public float damage;
        public float health;
        public float maxHealth;
        public float mana;
        public float maxMana;
        public float speed;
        public float manaRegen;

        public bool isPotion;

        public float itemValue;

        public void CalculateItemValue()
        {
            itemValue =
                (10 * damage + health / 10 + mana / 10 + speed / 3.5f + manaRegen * 50)
                * (1 / cost);
        }

        public void PrintStats()
        {
            Console.Error.WriteLine(name + " (" + cost + ")" + ": " + itemValue);

            string str = "";

            if (damage > 0)
            {
                str += "Dmg: " + damage + " ";
            }

            if (health > 0)
            {
                str += "Hp: " + health + " ";
            }

            if (mana > 0)
            {
                str += "Mana: " + mana + " ";
            }

            if (speed > 0)
            {
                str += "Speed: " + speed + " ";
            }

            if (manaRegen > 0)
            {
                str += "ManaRegen: " + manaRegen;
            }

            Console.Error.WriteLine(str);
        }
    }

    class Tower : Entity
    {
        public void GetUnitTarget()
        {
            var enemies = isAlly ? Strategy.enemyUnits.Values : Strategy.allyUnits.Values;

            target =
                enemies
                .Where(unit => GetDistance(unit) < attackRange)
                .OrderBy(unit => GetDistance(unit))
                .ThenBy(unit => unit.health)
                .FirstOrDefault();

            if (target != null)
            {
                moveAttackTime = GetAttackTime(target);
            }

        }
    }

    class Unit : Entity
    {        
        public void GetUnitTarget()
        {
            var enemies = isAlly ? Strategy.enemyUnits.Values : Strategy.allyUnits.Values;
                        
            target = 
                enemies
                .Where(unit => GetDistance(unit) < speed + attackRange)
                .OrderBy(unit => GetDistance(unit))
                .ThenBy(unit => unit.health)
                .FirstOrDefault();

            if (target != null)
            {
                moveAttackTime = GetMoveAttackTime(target);
            }
        }        
    }

    public enum MoveType
    {
        Wait,
        Move,
        Attack,
        Attack_Nearest,
        Move_Attack,
        Buy,
        Sell,
        Spell,
    }

    class Action
    {
        public static List<Action> actions = new List<Action>();

        public Vector position;
        public Entity target;
        public UnitType targetType;
        public Item item;
        public Spell spell;

        public Hero hero;

        public string extraComment = "";

        public MoveType moveType = MoveType.Wait;

        public Action()
        {

        }

        public Action(Entity target)
        {
            moveType = MoveType.Attack;
            this.target = target;

            var hero = GetOwner();
            
            hero.target = target;
            hero.moveAttackTime = hero.GetMoveAttackTime(target);
            //this.target.health -= hero.damage;
        }

        public Action(Vector pos)
        {
            moveType = MoveType.Move;
            this.position = pos;
        }

        public Action(Vector pos, Entity target)
        {
            moveType = MoveType.Move_Attack;
            this.position = pos;
            this.target = target;

            var hero = GetOwner();
            hero.target = target;
            hero.moveAttackTime = hero.GetMoveAttackTime(pos, target);
            //this.target.health -= hero.damage;
        }

        public Action(MoveType moveType, Item item)
        {
            this.moveType = moveType;
            this.item = item;

            if (moveType == MoveType.Buy)
            {
                Strategy.myGold -= (int)item.cost;

                var hero = GetOwner();

                List<Item> itemList;

                if (Strategy.myHeroItems.TryGetValue(hero.id, out itemList))
                {
                    itemList.Add(item);
                }
                else
                {
                    itemList = new List<Item>();
                    itemList.Add(item);

                    Strategy.myHeroItems.Add(hero.id, itemList);
                }
            }
            else
            {
                Strategy.myGold += (int)Math.Round(item.cost * .5f);

                var hero = GetOwner();

                List<Item> itemList;

                if (Strategy.myHeroItems.TryGetValue(hero.id, out itemList))
                {
                    itemList.Remove(item);
                }
            }
        }

        public Action(Spell spell)
        {
            this.moveType = MoveType.Spell;
            this.spell = spell;
        }

        public Action(Spell spell, Vector pos)
        {
            this.moveType = MoveType.Spell;
            this.spell = spell;
            this.position = pos;
        }

        public Action(Spell spell, Entity target)
        {
            this.moveType = MoveType.Spell;
            this.spell = spell;
            this.target = target;
        }

        public static Hero GetOwner()
        {
            if (Action.actions.Count == 0)
            {
                return Strategy.myHeros.First();
            }
            else
            {
                return Strategy.myHeros[1];
            }
        }

        public static void AddExtraText(string str)
        {
            var lastAction = Action.actions.Last();
            if (lastAction != null)
            {
                lastAction.extraComment = str;
            }
        }

        public static void AddAction(Action action)
        {
            var owner = GetOwner();
            action.hero = owner;

            Action.actions.Add(action);            
        }

        public static void CleanUp()
        {
            actions = new List<Action>();
        }

        public static void RoundPosition(Action action)
        {
            action.position = new Vector((int)Math.Round(action.position.x),
                                         (int)Math.Round(action.position.y),0);
        }

        public static void PrintActions()
        {
            string str = "";
                      
            foreach (var action in actions)
            {
                RoundPosition(action);

                switch (action.moveType)
                {
                    case MoveType.Move:
                        str = "MOVE " + action.position.x + " " + action.position.y;
                        break;
                    case MoveType.Attack:
                        str = "ATTACK " + action.target.id;
                        break;
                    case MoveType.Attack_Nearest:
                        str = "ATTACK_NEAREST " + action.targetType.ToString();
                        break;
                    case MoveType.Move_Attack:
                        str = "MOVE_ATTACK " + action.position.x + " " + action.position.y
                                             + " " + action.target.id;
                        break;
                    case MoveType.Buy:
                        str = "BUY " + action.item.name;
                        break;
                    case MoveType.Sell:
                        str = "SELL " + action.item.name;
                        break;
                    case MoveType.Spell:
                        str = action.spell.spellName;

                        if (action.spell.spellType == SpellType.Targeted)
                        {
                            str += " " + action.target.id;
                        }
                        else if (action.spell.spellType == SpellType.Position)
                        {
                            str += " " + action.position.x + " " + action.position.y;
                        }

                        //str += ";" + action.spell.spellName;
                        break;
                    default:
                        str = "WAIT";
                        break;
                }

                str += ";" + action.extraComment;
                //str += ";" + action.hero.heroType.ToString();

                Console.WriteLine(str);

                if (actions.Count != Strategy.myHeros.Count)
                {
                    Console.Error.WriteLine("INCORRECT NUMBER OF OUTPUT!!!");
                }
            }
        }
    }

    class Geometry
    {
        public static Vector[] CircleCircleIntersection(Vector center1, Vector center2, float radius1, float radius2)
        {
            var D = center1.Distance(center2);
            //The Circles dont intersect:
            if (D > radius1 + radius2 || (D <= Math.Abs(radius1 - radius2)))
            {
                return new Vector[] { };
            }

            var A = (radius1 * radius1 - radius2 * radius2 + D * D) / (2 * D);
            var H = (float)Math.Sqrt(radius1 * radius1 - A * A);
            var Direction = (center2 - center1).Normalized();
            var PA = center1 + A * Direction;
            var S1 = PA + H * Direction.Perpendicular();
            var S2 = PA - H * Direction.Perpendicular();
            return new[] { S1, S2 };
        }
    }

    static class Position
    {
        public static bool isEnemyCloseBy(this Vector pos)
        {
            float cooldownTime = 999;

            foreach (var enemy in Strategy.enemyHeros)
            {
                if (enemy.GetDistance(pos) <= enemy.attackRange)
                {
                    return true;
                }

                if (enemy.heroType == HeroType.DOCTOR_STRANGE
                    && enemy.cooldowns[2] <= cooldownTime
                    && enemy.GetDistance(pos) <= 400)
                {
                    return true;
                }

                if (enemy.heroType == HeroType.HULK)
                {
                    if (enemy.cooldowns[0] <= cooldownTime && enemy.GetDistance(pos) <= 300)
                    {
                        return true;
                    }

                    if (enemy.cooldowns[2] <= cooldownTime && enemy.GetDistance(pos) <= 150)
                    {
                        return true;
                    }
                }

                if (enemy.heroType == HeroType.VALKYRIE)
                {
                    if (enemy.cooldowns[1] <= cooldownTime && enemy.GetDistance(pos) <= 250)
                    {
                        return true;
                    }

                    if (enemy.cooldowns[0] <= cooldownTime && enemy.GetDistance(pos) <= 155)
                    {
                        return true;
                    }
                }

            }

            return false;
            //return Strategy.enemyHeros.Any(enemy => enemy.GetDistance(pos) <= 150);
        }

        public static bool isSafe(this Vector pos)
        {
            return pos.isInbound() && !pos.isEnemyCloseBy();
        }

        public static bool isInbound(this Vector pos)
        {
            return pos.x >= 0 && pos.x <= 1920 && pos.y >= 0 && pos.y <= 750;
        }
    }

    public struct Vector : IEquatable<Vector>
    {
        public float x;
        public float y;
        public float z;

        public Vector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector Zero
        {
            get { return new Vector(0f, 0f, 0f); }
        }

        public static Vector Undefined
        {
            get { return new Vector(-1337f, -1337f, -1337f); }
        }

        public static bool operator ==(Vector value1, Vector value2)
        {
            return value1.x == value2.x
                && value1.y == value2.y
                && value1.z == value2.z;
        }

        public static bool operator !=(Vector value1, Vector value2)
        {
            return !(value1 == value2);
        }

        public static Vector operator +(Vector value1, Vector value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            value1.z += value2.z;
            return value1;
        }

        public static Vector operator -(Vector value)
        {
            value = new Vector(-value.x, -value.y, -value.z);
            return value;
        }

        public static Vector operator -(Vector value1, Vector value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            value1.z -= value2.z;
            return value1;
        }

        public static Vector operator *(Vector value1, Vector value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            value1.z *= value2.z;
            return value1;
        }

        public static Vector operator *(Vector value, float scaleFactor)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            value.z *= scaleFactor;
            return value;
        }

        public static Vector operator *(float scaleFactor, Vector value)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            value.z *= scaleFactor;
            return value;
        }

        public override bool Equals(object obj)
        {
            return (obj is Vector) ? this == (Vector)obj : false;
        }

        public bool Equals(Vector other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (int)(this.x + this.y * 1000 + this.z * 1000 * 1000);
        }

        public Vector Normalized()
        {
            float factor = Distance(Vector.Zero);
            factor = 1f / factor;
            Vector result;
            result.x = x * factor;
            result.y = y * factor;
            result.z = z * factor;

            return result;
        }

        public float Distance(Vector targetPos)
        {
            return (float)Math.Sqrt(Math.Pow(x - targetPos.x, 2)
                        + Math.Pow(y - targetPos.y, 2)
                        + Math.Pow(z - targetPos.z, 2));
        }

        public Vector Perpendicular()
        {
            return new Vector(-y, x, 0);
        }
    }

    static class VectorExtensions
    {
        public struct ProjectionInfo
        {
            public bool IsOnSegment;
            public Vector LinePoint;
            public Vector SegmentPoint;

            public ProjectionInfo(bool isOnSegment, Vector segmentPoint, Vector linePoint)
            {
                IsOnSegment = isOnSegment;
                SegmentPoint = segmentPoint;
                LinePoint = linePoint;
            }
        }

        public static ProjectionInfo ProjectOn(this Vector point, Vector segmentStart, Vector segmentEnd)
        {
            var cx = point.x;
            var cy = point.y;
            var ax = segmentStart.x;
            var ay = segmentStart.y;
            var bx = segmentEnd.x;
            var by = segmentEnd.y;
            var rL = ((cx - ax) * (bx - ax) + (cy - ay) * (by - ay)) /
                     ((float)Math.Pow(bx - ax, 2) + (float)Math.Pow(by - ay, 2));
            var pointLine = new Vector(ax + rL * (bx - ax), ay + rL * (by - ay), 0);
            float rS;
            if (rL < 0)
            {
                rS = 0;
            }
            else if (rL > 1)
            {
                rS = 1;
            }
            else
            {
                rS = rL;
            }

            var isOnSegment = rS.CompareTo(rL) == 0;
            var pointSegment = isOnSegment ? pointLine : new Vector(ax + rS * (bx - ax), ay + rS * (@by - ay), 0);

            return new ProjectionInfo(isOnSegment, pointSegment, pointLine);
        }
    }

    class Deadpool : HeroStrategy
    {
        public Deadpool(Hero newHero) : base(newHero)
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
                        spell.CastSpell();
                        return true;
                    }
                }

                spell = hero.spells[1];

                if (spell != null)
                {
                    foreach (var enemy in enemyHeros.OrderBy(unit => unit.GetDistance(hero)))
                    {
                        var distance = enemy.GetDistance(hero);
                        
                        if (!enemyMinions.Any(unit => unit.GetDistance(hero) <= distance))
                        {
                            spell.CastSpell(enemy.pos);
                            return true;
                        }
                    }
                }
            }

            return base.TryRetreating();
        }
    }

    class DrStrange : HeroStrategy
    {
        public DrStrange(Hero newHero) : base(newHero)
        {
            hero = newHero;
        }

        public override bool TryRetreating()
        {
            if (hero.isInDanger())
            {
                var spell = hero.spells[1];

                if (spell != null)
                {
                    if (spell.canCast)
                    {
                        var pos = GetRetreatPos();

                        spell.CastSpell(hero);
                        return true;
                    }
                }
            }

            return base.TryRetreating();
        }
    }

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




    class AoeHeal : Spell
    {
        public AoeHeal(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {
            spellName = "AOEHEAL";

            spellType = SpellType.Position;

            manaCost = 50;
            range = 250;
            radius = 100;
            castTime = 0;
            cooldown = 6;            
        }

        public float GetDamage(Entity target)
        {
            return hero.mana * 0.2f;
        }

        public override bool SpellLogic()
        {
            var manaPercent = hero.mana / hero.maxMana;
            if (manaPercent <= .5)
            {
                return false;
            }

            if (!Strategy.enemyUnits.Values.Any(unit => unit.GetDistance(hero) <= unit.attackRange)) //not in danger
            {
                
                float healAmount = GetDamage(null);

                foreach (var ally in Strategy.allyUnits.Values
                                        .Where(unit => unit.GetDistance(hero) <= range)
                                        .OrderBy(unit => unit.health))
                {
                    float totalHealed = 0;

                    foreach (var otherAlly in Strategy.allyUnits.Values.Where(unit => unit.GetDistance(ally) <= radius))
                    {
                        var healthMissing = otherAlly.maxHealth - otherAlly.health;
                                                
                        var realHealAmount = Math.Min(healAmount, healthMissing);

                        if (otherAlly is Hero)
                        {
                            totalHealed += 2 * realHealAmount;
                        }
                        else
                        {
                            totalHealed += realHealAmount;
                        }                        
                    }

                    if (totalHealed >= 4 * healAmount)
                    {
                        CastSpell(ally.pos);
                        return true;
                    }
                }                
            }

            return false;
        }
    }

    class Blink : Spell
    {
        public Blink(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "BLINK";

            spellType = SpellType.Position;

            manaCost = 16;
            range = 200;
            castTime = 0.05f;
            cooldown = 3;            
        }

        public override bool SpellLogic()
        {
            /*var aggroCount = 0;
            foreach (var aggro in owner.aggroUnits)
            {
                if (aggro is Hero)
                {
                    aggroCount += 3;
                }
                else
                {
                    aggroCount += 1;
                }
            }

            if (aggroCount >= 4)
            {
                var bush = owner.GetNearestRetreatBush();

                if (!owner.isInBush(bush))
                {
                    if (owner.GetDistance(bush) <= range)
                    {
                        CastSpell(bush.pos);
                        return true;
                    }
                    else
                    {
                        var dir = (bush.pos - owner.pos).Normalized();
                        var pos = owner.pos + dir * range;

                        CastSpell(pos);
                        return true;
                    }
                }
            }

            foreach (var enemy in Strategy.enemyHeros)
            {
                if (!enemy.isRanged && owner.GetDistance(enemy) <= range)
                {
                    if (!enemy.isUnderEnemyTower())
                    {
                        var bush = owner.GetNearestRetreatBush();

                        if (!owner.isInBush(bush))
                        {
                            if (owner.GetDistance(bush) <= range)
                            {
                                CastSpell(bush.pos);
                                return true;
                            }
                            else
                            {
                                var dir = (bush.pos - owner.pos).Normalized();
                                var pos = owner.pos + dir * range;

                                CastSpell(pos);
                                return true;
                            }
                        }
                    }
                }
            }*/
            
            

            return false;
        }

    }

    class Burning : Spell
    {
        public Burning(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "BURNING";

            spellType = SpellType.Position;

            manaCost = 50;
            range = 250;
            radius = 100;
            castTime = 0;
            cooldown = 5;            
        }

        public float GetDamage(Entity target)
        {
            return hero.manaRegen * 5 + 30;
        }

        public override bool SpellLogic()
        {
            foreach (var enemy in Strategy.enemyHeros.Where(unit => unit.GetDistance(hero) <= range))
            {                
                int unitsHit = 0;

                foreach (var unit in Strategy.enemyUnits.Values)
                {
                    if (unit.GetDistance(enemy) <= radius)
                    {
                        if (unit is Hero)
                        {
                            unitsHit += 2;
                        }
                        else
                        {
                            unitsHit += 1;
                        }
                    }
                }

                var threshold = 2 * Strategy.enemyHeros.Count + 3;

                if (unitsHit >= threshold)
                {
                    CastSpell(enemy.pos);
                    return true;
                }
            }

            return false;
        }
    }

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

    class FireBall : Spell
    {
        public FireBall(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "FIREBALL";

            spellType = SpellType.Position;

            manaCost = 60;
            range = 900;
            flyTime = 0.9f;
            radius = 50;
            castTime = 0;
            cooldown = 6;            
        }

        public float GetDamage(Entity target)
        {
            return hero.mana * 0.2f + 55 * target.GetDistance(hero) / 1000f;
        }

        public float GetMaxDamage()
        {
            return hero.maxMana * 0.2f + 55 * 900 / 1000;
        }

        public override bool SpellLogic()
        {
            //var enemies = GetEnemiesInRange();
                        
            foreach (var enemy in Strategy.enemyHeros.Where(unit => unit.GetDistance(hero) <= range))
            {
                var dir = (enemy.pos - hero.pos).Normalized();
                var endPos = hero.pos + dir * range;

                int unitsHit = 0;
                float totalDamage = 0;

                foreach (var unit in Strategy.neutralUnits.Values.Where(unit => unit.GetDistance(hero) <= range))
                {
                    var distanceToNeutral = unit.GetDistance(enemy);
                    if (distanceToNeutral <= 300 && Strategy.myHeros.Any(myHero => myHero.GetDistance(unit) > distanceToNeutral))
                    {
                        CastSpell(unit.pos);
                        return true;
                    }
                }

                foreach (var unit in GetEnemiesInRange())
                {
                    var projectionInfo = unit.pos.ProjectOn(hero.pos, endPos);

                    if (projectionInfo.IsOnSegment)
                    {
                        if (projectionInfo.SegmentPoint.Distance(unit.pos) <= radius)
                        {    
                            var unitDamage = GetDamage(unit);
                        
                            if (unit is Hero) //broken?
                            {
                                unitsHit += 5;

                                if (unitDamage >= unit.health)
                                {
                                    totalDamage += unitDamage * 9999;
                                }

                                totalDamage += unitDamage * 3;
                            }
                            else
                            {
                                unitsHit += 1;

                                totalDamage += unitDamage;
                            }
                        }
                    }
                }

                //var threshold = 5 * Strategy.enemyHeros.Count + 2;                

                var threshold = 5 * Strategy.enemyHeros.Count * GetMaxDamage();
                
                if (totalDamage >= threshold / 3)
                {
                    CastSpell(enemy.pos);
                    return true;
                }                
            }           

            return false;
        }
    }

    class Pull : Spell
    {
        public Pull(Hero owner, float currentCooldown) : base(owner, currentCooldown)
        {            
            spellName = "PULL";

            spellType = SpellType.Targeted;

            manaCost = 40;
            range = 400;
            flyTime = 0.9f;
            radius = 200;
            castTime = 0.4f;
            cooldown = 5;            
        }

        public override bool SpellLogic()
        {
            foreach (var enemy in Strategy.enemyHeros)
            {
                if (Strategy.enemyHeros.Count > 1 && enemy.heroType == HeroType.IRONMAN)
                {
                    if (enemy.cooldowns[0] < 2)
                    {
                        continue;
                    }
                }

                if (enemy.GetDistance(hero) <= range)
                {
                    int aggroCount = 0;

                    var dir = (hero.pos - enemy.pos).Normalized();
                    var pos = enemy.pos + dir * radius;

                    foreach (var ally in Strategy.allyUnits.Values)
                    {
                        if (ally is Unit && ally.GetDistance(pos) <= 300)
                        {
                            aggroCount += 1;
                        }
                        else if (ally is Tower && ally.GetDistance(pos) <= ally.attackRange)
                        {
                            aggroCount += 3;
                        }
                    }

                    if (aggroCount >= 4)
                    {
                        CastSpell(enemy);
                        return true;
                    }
                }
            }           

            return false;
        }
    }

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
