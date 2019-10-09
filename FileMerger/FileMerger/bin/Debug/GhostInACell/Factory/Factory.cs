using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    class Factory : Entity, IEquatable<Factory>
    {
        public static List<Factory> factories = new List<Factory>();
        public static List<Factory> ally = new List<Factory>();
        public static List<Factory> enemy = new List<Factory>();
        public static List<Factory> neutral = new List<Factory>();

        public static FactoryGlobalStats globalStats = new FactoryGlobalStats();
        public FactoryStats stats;

        public int id;

        public Team team;
        public bool isAlly;
        public bool isEnemy;
        public bool isNeutral;

        public int count;
        public int armyAvailable;

        public int production;
        public int oldProduction;
        public int disabledTurns;

        public Factory nearestEnemy;
        public int nearestEnemyDistance;

        public Dictionary<Factory, int> links; //links with factory and distance
        public SortedSet<Troop> incoming; //incoming troops

        public ShortestPaths shortestPaths;

        public Factory parent;
        public int shortestDistance;

        public List<FactoryState> states;

        public Factory(int id)
        {
            this.id = id;

            SetTeam(Team.Neutral);

            links = new Dictionary<Factory, int>();
            incoming = new SortedSet<Troop>();
            states = new List<FactoryState>();

            stats = new FactoryStats();
        }

        public static void CleanUp()
        {
            foreach (var factory in factories)
            {
                factory.incoming = new SortedSet<Troop>();
                factory.states = new List<FactoryState>();

                factory.stats = new FactoryStats();
            }

            Factory.globalStats = new FactoryGlobalStats();
        }

        public static void ProcessNeighbourCounts()
        {
            Factory.globalStats.allyProductionMaxed = true;

            foreach (var factory in factories)
            {
                if (factory.isAlly)
                {
                    factory.stats.missionPrority += 1;
                }

                foreach (var pair in factory.links)
                {
                    Factory neighbour = pair.Key;
                    double distance = pair.Value;

                    if (neighbour.isAlly)
                    {
                        factory.stats.missionPrority += 1 / distance;
                    }

                    if (factory.isAlly)
                    {
                        if (neighbour.isAlly)
                        {
                            factory.stats.allyCount += neighbour.count;
                            factory.stats.allyCountWeighted += neighbour.count / distance;
                            Factory.globalStats.totalAllyCountWeighted += neighbour.count / distance;
                        }
                        else if (neighbour.team == Team.Enemy)
                        {
                            factory.stats.enemyCount += neighbour.count;
                            factory.stats.enemyCountWeighted += neighbour.count / distance;
                            Factory.globalStats.totalEnemyCountWeighted += neighbour.count / distance;

                            factory.stats.averageEnemyDistance += (int)distance;
                            factory.stats.enemyFactoryCount += 1;
                        }

                        if (factory.production < 3)
                        {
                            Factory.globalStats.allyProductionMaxed = false;
                        }
                    }
                }

                factory.stats.averageEnemyDistance = factory.stats.enemyFactoryCount == 0 ? 999 :
                    factory.stats.averageEnemyDistance / factory.stats.enemyFactoryCount;
            }
        }

        public static void ProcessFactoryStates()
        {
            foreach (var factory in factories)
            {
                factory.states = FactoryState.CalculateFactoryState(factory, factory.incoming, false);
            }
        }

        public static void CalculateAllArmyScore()
        {
            foreach (var factory in factories)
            {
                FactoryState lastState = factory.states.Last(); //assume the lastState for now

                double allyCount = 0;
                double enemyCount = 0;

                if (lastState.isAlly)
                {
                    allyCount += lastState.count;
                }
                else if (lastState.team == Team.Enemy)
                {
                    enemyCount += lastState.count;
                }

                factory.CalculateArmyScore(allyCount, enemyCount);
                factory.GetNearestEnemy();
            }
        }

        public void AddArmyScore(double allyScore, double enemyScore)
        {
            stats.allyScore += allyScore;
            stats.enemyScore += enemyScore;

            stats.combinedScore += allyScore + enemyScore;
            stats.differenceScore += allyScore - enemyScore;
        }

        public void CalculateArmyScore(double allyCount, double enemyCount)
        {
            foreach (var factory in Factory.factories)
            {
                if (factory == this)
                {
                    factory.AddArmyScore(allyCount, enemyCount);
                }
                else
                {
                    int distance = shortestPaths.shortestDistance[factory];

                    double allyScore = allyCount / distance; //ally count based on distance
                    double enemyScore = enemyCount / distance;

                    factory.AddArmyScore(allyScore, enemyScore);
                }
            }
        }

        public static void CalculateShortestPaths()
        {
            foreach (var factory in Factory.factories)
            {
                Algorithm.CalculateShortestPaths(factory);
            }
        }

        public void GetNearestEnemy()
        {
            int shortest = 999;
            Factory nearest = null;

            foreach (var factory in Factory.enemy)
            {
                if (factory != this)
                {
                    int distance = 999;
                    if (shortestPaths.shortestDistance.TryGetValue(factory, out distance))
                    {
                        if (distance < shortest)
                        {
                            shortest = distance;
                            nearest = factory;
                        }
                    }
                }
            }

            nearestEnemy = nearest;
            nearestEnemyDistance = shortest;
        }

        public Factory GetNearestAlly()
        {
            int shortest = 999;
            Factory nearest = null;

            foreach (var factory in Factory.ally)
            {
                if (factory != this)
                {
                    int distance = 999;
                    if (shortestPaths.shortestDistance.TryGetValue(factory, out distance))
                    {
                        if (distance < shortest)
                        {
                            shortest = distance;
                            nearest = factory;
                        }
                    }
                }
            }

            return nearest;
        }

        public void ProcessMessage(EntityMessage message)
        {
            count = message.arg2;

            disabledTurns = message.arg4;
            production = disabledTurns > 0 ? 0 : message.arg3;
            oldProduction = Math.Max(production, oldProduction);

            if (message.arg1 == 1)
            {
                SetTeam(Team.Ally);
                Factory.globalStats.allyTotalProduction += production;
                Factory.globalStats.totalAllyCount += count;
            }
            else if (message.arg1 == -1)
            {
                SetTeam(Team.Enemy);
                Factory.globalStats.enemyTotalProduction += production;
                Factory.globalStats.totalEnemyCount += count;
            }
            else
            {
                SetTeam(Team.Neutral);
                Factory.globalStats.neutralTotalProduction += production;
            }

            armyAvailable = this.isAlly ? count : -count;
        }

        public static void AddLink(int factoryID1, int factoryID2, int distance)
        {
            Factory factory1 = GetFactoryByID(factoryID1);
            Factory factory2 = GetFactoryByID(factoryID2);

            factory1.links.Add(factory2, distance);
            factory2.links.Add(factory1, distance);
        }

        public static Factory GetFactoryByID(int id)
        {
            Factory factory;
            if (id < Factory.factories.Count)
            {
                return Factory.factories[id];
            }
            else
            {
                factory = new Factory(id);
                factories.Add(factory);
                return factory;
            }
        }

        public void SetTeam(Team team)
        {
            this.team = team;
            if (team == Team.Ally)
            {
                if (!ally.Contains(this))
                {
                    ally.Add(this);
                }
                enemy.Remove(this);
                neutral.Remove(this);

                isAlly = true;
                isEnemy = false;
                isNeutral = false;
            }

            if (team == Team.Enemy)
            {
                if (!enemy.Contains(this))
                {
                    enemy.Add(this);
                }
                ally.Remove(this);
                neutral.Remove(this);

                isAlly = false;
                isEnemy = true;
                isNeutral = false;
            }

            if (team == Team.Neutral)
            {
                if (!neutral.Contains(this))
                {
                    neutral.Add(this);
                }
                ally.Remove(this);
                enemy.Remove(this);

                isAlly = false;
                isEnemy = false;
                isNeutral = true;
            }
        }

        public override bool Equals(object obj)
        {
            Factory factory = obj as Factory;
            return factory.id == this.id;
        }

        public bool Equals(Factory factory)
        {
            return factory.id == this.id;
        }

        public override int GetHashCode()
        {
            return this.id;
        }
    }
}
