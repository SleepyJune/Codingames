using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
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
}
