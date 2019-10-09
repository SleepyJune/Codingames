
using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace GhostInACell
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] inputs;
            int factoryCount = int.Parse(Console.ReadLine()); // the number of factories
            int linkCount = int.Parse(Console.ReadLine()); // the number of links between factories

            for (int i = 0; i < linkCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int factory1 = int.Parse(inputs[0]);
                int factory2 = int.Parse(inputs[1]);
                int distance = int.Parse(inputs[2]);

                Factory.AddLink(factory1, factory2, distance);
            }

            Game.InitializeFirstTurn();

            // game loop
            while (true)
            {
                float loopStartTime = Timer.TickCount;

                int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
                for (int i = 0; i < entityCount; i++)
                {
                    var str = Console.ReadLine();

                    EntityMessage message = new EntityMessage(str);
                    message.ProcessMessage();
                }

                Game.InitializeTurn();
                Game.MakeMove();
                Game.PrintActions(); //Print the move
                Game.CleanUp();

                float loopTime = Timer.TickCount - loopStartTime;
                Console.Error.WriteLine("LoopTime: " + loopTime);
            }
        }
    }

    class Algorithm
    {
        public static void CalculateShortestPaths(Factory start)
        {
            Dictionary<Factory, int> dist = new Dictionary<Factory, int>();
            Dictionary<Factory, Factory> prev = new Dictionary<Factory, Factory>();

            HashSet<Factory> q = new HashSet<Factory>();

            foreach (var factory in Factory.factories) //initialize
            {
                dist.Add(factory, 999);
                prev.Add(factory, null);

                q.Add(factory);
            }

            dist[start] = 0;

            while (q.Count > 0)
            {
                Factory factory = q.OrderBy(f => dist[f]).First();
                q.Remove(factory);

                foreach (var pair in factory.links)
                {
                    Factory neighbour = pair.Key;
                    int distance = pair.Value;

                    int alternativeDistance = dist[factory] + distance + 1;

                    if (alternativeDistance < dist[neighbour])
                    {
                        dist[neighbour] = alternativeDistance;
                        prev[neighbour] = factory;
                    }
                }
            }

            start.shortestPaths = new PathInfo(start, dist, prev);
        }

        public static void CalculateOptimalPaths(Factory start)
        {
            Dictionary<Factory, int> dist = new Dictionary<Factory, int>();
            Dictionary<Factory, int> inter = new Dictionary<Factory, int>();
            Dictionary<Factory, Factory> prev = new Dictionary<Factory, Factory>();

            HashSet<Factory> q = new HashSet<Factory>();

            foreach (var factory in Factory.factories) //initialize
            {
                inter.Add(factory, 0);
                dist.Add(factory, 999);
                prev.Add(factory, null);

                q.Add(factory);
            }

            dist[start] = 0;
            
            while (q.Count > 0)
            {
                Factory factory = q.OrderBy(f => dist[f]).First();
                q.Remove(factory);

                foreach (var pair in factory.links)
                {
                    Factory neighbour = pair.Key;
                    int distance = pair.Value;

                    int alternativeDistance = dist[factory] + distance;
                    int alternativeInter = inter[factory] + 1;

                    if (alternativeDistance <= dist[neighbour] && alternativeInter >= inter[neighbour])
                    {
                        inter[neighbour] = alternativeInter;
                        dist[neighbour] = alternativeDistance;
                        prev[neighbour] = factory;
                    }
                }
            }

            start.optimalPaths = new PathInfo(start, dist, prev);
        }

        public static void CalculateFactoryValue(Factory start)
        {
            foreach (var factory in Factory.factories)
            {
                Factory end = factory;
                
                var prevFactory = start.optimalPaths.previousFactory;
                Factory parent = prevFactory[end];
                                

                while (parent != start && prevFactory[end] != null)
                {                    
                    end.pathValue++;
                    end = parent;
                    parent = prevFactory[end];
                }
                end.pathValue++;
            }
        }

        public static Troop UseBFS(Troop troop, Factory end, bool getDirectPath = false)
        {
            Factory start = troop.start;

            foreach (var factory in Factory.factories) //initialize
            {
                factory.shortestDistance = 999;
            }

            HashSet<int> set = new HashSet<int>();
            Queue<Factory> q = new Queue<Factory>();

            q.Enqueue(start);
            start.shortestDistance = 0;

            Factory current;

            // bfs loop
            while (q.Count > 0)
            {
                current = q.Dequeue();

                foreach (var pair in current.links.OrderBy(p => p.Value))
                {
                    Factory node = pair.Key;
                    int distance = pair.Value;

                    int shortest = Math.Max(current.shortestDistance, distance);

                    if (node == end) //found goal
                    {
                        if (end.shortestDistance > shortest)
                        {

                            end.shortestDistance = shortest;
                            end.parent = current;
                        }
                    }
                    else if (!set.Contains(node.id))
                    {
                        set.Add(node.id);

                        node.parent = current;
                        node.shortestDistance = shortest;

                        //Console.Error.WriteLine(current.id+"-"+node.id + ": " + node.shortestDistance);

                        q.Enqueue(node);
                    }
                    else if (node.shortestDistance > shortest)
                    {
                        node.parent = current;
                        node.shortestDistance = shortest;
                    }
                }
            }

            Factory n = end;
            int totalDistance = 0;

            while (n.parent != start)
            {
                totalDistance += n.links[n.parent] + 1;
                n = n.parent;
            }
            totalDistance += n.links[n.parent] + 1;

            int directDistance = 0;
            if (start.links.TryGetValue(end, out directDistance))
            {
                if (directDistance * 1.5 <= totalDistance || getDirectPath)
                {
                    troop.turns = directDistance + 1;
                    troop.endTime = Game.gameTime + troop.turns;
                    troop.end = end;

                    return troop;
                }
            }

            troop.turns = totalDistance + 1;
            troop.endTime = Game.gameTime + troop.turns;
            troop.end = n;

            return troop;
        }
    }


    class PathInfo
    {
        public Dictionary<Factory, int> shortestDistance = new Dictionary<Factory, int>();
        public Dictionary<Factory, Factory> previousFactory = new Dictionary<Factory, Factory>();

        public Factory start;

        public PathInfo(Factory start, Dictionary<Factory, int> dist, Dictionary<Factory, Factory> prev)
        {
            this.start = start;

            this.shortestDistance = dist;
            this.previousFactory = prev;
        }

        public int GetPathLength(Factory end)
        {
            if (start == end)
            {
                return 0;
            }

            Factory current = end;
            int totalDistance = previousFactory[current].links[current] + 1;

            while (previousFactory[current] != start)
            {
                totalDistance += previousFactory[current].links[current] + 1;
                current = previousFactory[current];
            }

            totalDistance += previousFactory[current].links[current] + 1;

            return totalDistance;
        }
    }

    class Game
    {
        public static int gameTime = 0;
        public static int bomb = 2;

        public static void InitializeFirstTurn()
        {
            Factory.CalculatePaths();
            Factory.CalculateValues();
        }

        public static void InitializeTurn()
        {
            Bomb.AddOldBombs();
            
            Factory.ProcessFactoryStates();
            Factory.ProcessNeighbourCounts();
            Factory.CalculateAllArmyScore();
        }

        public static void MakeMove()
        {
            Strategy.AddMissions();

            while (Strategy.ExecuteMissions()) ;
        }

        public static void PrintActions()
        {
            Action.PrintActions();
        }

        public static void CleanUp()
        {
            Factory.CleanUp();
            Troop.CleanUp();
            Action.CleanUp();
            Bomb.CleanUp();
            Strategy.CleanUp();

            gameTime++;
        }
    }

    class Strategy
    {
        public static List<Mission> missions = new List<Mission>();
        public static List<Mission> curExecuted = new List<Mission>();
        public static List<Mission> prevExecuted = new List<Mission>();

        public static void AddMissions()
        {
            var orderedFactories = Factory.factories
                .OrderByDescending(factory =>
                    factory.stats.allyScore * factory.oldProduction);

            foreach (var factory in orderedFactories)
            {
                //if the factory will always be ally's
                if (factory.isAlly && FactoryState.WillFactoryBeAlly(factory.states))
                {
                    int nearbyEnemyThreatCount = factory.GetNearbyEnemyThreats();
                    if (nearbyEnemyThreatCount == 0)
                        //&& Factory.globalStats.totalAllyCount - 10 >= (2*Factory.globalStats.totalEnemyCount/3.0))
                    {
                        if (factory.oldProduction < 3 && factory.armyAvailable >= 10)
                        {
                            Mission newMission = new Mission(MissionType.Inc, factory);
                            missions.Add(newMission);
                        }
                        else if (factory.oldProduction < 3) //send inc support before becoming ally's?
                        {
                            Mission newMission = new Mission(MissionType.IncSupport, factory);
                            missions.Add(newMission);
                        }
                    }
                    else
                    {
                        if (nearbyEnemyThreatCount > 0)
                        {
                            Mission newMission = new Mission(MissionType.Reinforce, factory);
                            newMission.troopsNeeded = nearbyEnemyThreatCount;
                            missions.Add(newMission);
                        }                        
                    }
                }
                else
                {
                    if (Game.bomb > 0 && Game.gameTime > 0)
                    {
                        Mission bombMission = new Mission(MissionType.Bomb, factory);
                        missions.Add(bombMission);
                    }

                    if (factory.isAlly)
                    {
                        Mission newMission = new Mission(MissionType.Defend, factory);
                        missions.Add(newMission);
                    }
                    else
                    {
                        if (!FactoryState.WillFactoryBeAlly(factory.states))
                        {
                            Mission newMission = new Mission(MissionType.Capture, factory);
                            missions.Add(newMission);
                        }
                    }


                    //double missionScore = factory.stats.allyScore * factory.oldProduction;
                    //Console.Error.WriteLine(factory.id + ": " + missionScore);
                    //return;
                }
            }
        }

        public static void SolveMissions()
        {
            foreach (var mission in missions)
            {
                if (mission.missionExecuted)
                {
                    continue;
                }

                if (mission.type == MissionType.Inc || mission.type == MissionType.IncSupport)
                {
                    if (missions.Any(m => m.type == MissionType.Reinforce))
                    {
                        mission.successRating = MissionSuccessRating.Impossible;
                        continue;
                    }
                }

                foreach (var prevMission in prevExecuted)
                {
                    if (prevMission == mission)
                    {
                        mission.prevMission = prevMission;
                    }
                }

                mission.CleanUp();
                mission.solver.Solve();
                mission.solver.RateMission();
                mission.solver.CalculateReward();
                //calculate reward if waited 1 turn
            }
        }

        public static bool ClearedPrereq(Mission mission)
        {
            foreach (var prereq in mission.prereqs.Keys)
            {
                if (!missions.Any(m => m == prereq))
                {
                    return false;
                }
            }

            return true;
        }

        public static void ExecuteFinalReinforcement()
        {
            foreach (var mission in missions)
            {
                if (mission.missionExecuted)
                {
                    continue;
                }

                Mission newMission =
                    mission.factory.isAlly ?
                    new Mission(MissionType.FinalReinforce, mission.factory) :
                    new Mission(MissionType.FinalReinforce, mission.factory.GetNearestAlly());

                if (mission.factory != null)
                {
                    missions.Add(newMission);
                    newMission.solver.Solve();

                    newMission.missionExecuted = true;
                    newMission.solver.Execute();

                    Console.Error.WriteLine("Final Reinforce: " + newMission.factory.id);
                    return;
                }
            }
        }

        public static bool CheckLaterRewards(int index)
        {
            while (index + 1 >= missions.Count)
            {
                double currentReward = missions[index].reward;
                double nextReward = missions[index + 1].reward;

                return currentReward >= 2 * nextReward;
            }

            return false;
        }

        public static bool CheckBomb(Mission mission)
        {
            if (mission.type == MissionType.Bomb)
            {
                if (missions.Any(m => m.type != MissionType.Bomb && m.isPossible()))
                {
                    //return false;
                }
            }

            return true;
        }

        public static bool ExecuteMissions()
        {
            Strategy.SolveMissions();

            missions.Sort();
            missions.Reverse(); //highest reward first

            foreach (var mission in missions)
            {
                Console.Error.WriteLine(mission.type.ToString() + " " + mission.factory.id + ": " + mission.successRating.ToString()
                    + " " + mission.reward);
            }

            for (int i = 0; i < missions.Count; i++)
            {
                Mission mission = missions[i];

                if (mission.missionExecuted)
                {
                    continue;
                }

                if (mission.isPossible())
                {
                    Console.Error.WriteLine("Execute " + mission.type.ToString() + " " + mission.factory.id);// + "\n");
                    
                    mission.missionExecuted = true;
                    mission.solver.Execute();
                    curExecuted.Add(mission);
                  

                    //string message = mission.type.ToString() + " " + mission.factory.id;
                    //Action.AddAction(new Action(message));

                    return true;
                }

                /*if (Strategy.CheckLaterRewards(i))
                {
                    break;
                }*/
            }

            ExecuteFinalReinforcement();

            return false;
        }

        public static void CleanUp()
        {
            missions = new List<Mission>();

            prevExecuted = curExecuted;
            curExecuted = new List<Mission>();
        }
    }

    class Bomb : Troop, Entity
    {
        public static Dictionary<int, Bomb> bombs = new Dictionary<int, Bomb>();

        public static Dictionary<int, Bomb> enemyBombs = new Dictionary<int, Bomb>();
        public static List<Bomb> oldEnemyBombs = new List<Bomb>();

        public static int enemyBombCount = 0;

        public Bomb()
        {
            this.id = idCount++;
        }

        public static Bomb GetBombByID(int id)
        {
            Bomb troop = new Bomb();
            troops.Add(troop);
            bombs.Add(troop.id, troop);
            return troop;
        }

        public new static void CleanUp()
        {
            bombs = new Dictionary<int, Bomb>();
            enemyBombs = new Dictionary<int, Bomb>();

            oldEnemyBombs.RemoveAll(bomb => bomb.endTime >= Game.gameTime);
        }

        public static int isBombIncoming(Factory factory)
        {
            foreach (var bomb in enemyBombs.Values)
            {
                if (bomb.end != null)
                {
                    return bomb.endTime;
                }
            }

            return -1;
        }

        public void AddMockBombTroop()
        {
            Troop testTroop = new Troop
            {
                start = this.start,
                end = this.end,
                count = 0,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = this.turns + 4,
                endTime = this.endTime + 4,
            };

            Troop testTroop2 = new Troop
            {
                start = this.start,
                end = this.end,
                count = 0,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = this.turns + 5,
                endTime = this.endTime + 5,
            };

            this.end.incoming.Add(testTroop);
            this.end.incoming.Add(testTroop2);
        }

        public new void ProcessMessage(EntityMessage message)
        {
            if (message.arg1 == 1)
            {
                isAlly = true;
                isEnemy = false;

                team = Team.Ally;
                start = Factory.GetFactoryByID(message.arg2);
                end = Factory.GetFactoryByID(message.arg3);

                turns = message.arg4;
                endTime = Game.gameTime + turns;

                end.incoming.Add(this);
                AddMockBombTroop();
            }
            else
            {
                isAlly = false;
                isEnemy = true;

                turns = 0;
                endTime = Game.gameTime;

                team = Team.Enemy;

                enemyBombs.Add(id, this);

                start = Factory.GetFactoryByID(message.arg2);                

                this.DeduceBomb();
            }

            start = Factory.GetFactoryByID(message.arg2);            
        }

        public void DeduceBomb()
        {
            if (oldEnemyBombs.Count == 0)
            {
                if (Game.gameTime == 1)
                {
                    end = Factory.ally.First();
                    turns = end.links[start];
                    endTime = Game.gameTime + turns;

                    oldEnemyBombs.Add(this);
                }


            }
        }

        public static void AddOldBombs()
        {
            if (enemyBombs.Count > 0)
            {
                foreach (var bomb in oldEnemyBombs)
                {
                    if (bomb.end != null)
                    {
                        bomb.turns = bomb.endTime - Game.gameTime;
                        bomb.end.incoming.Add(bomb);
                    }                    
                }
            }
        }

    }

    public enum Team
    {
        Neutral,
        Ally,
        Enemy,
    }   

    interface Entity
    {
        void ProcessMessage(EntityMessage message);
    }

    class EntityMessage
    {
        public int id;
        public string entityType;
        public int arg1;
        public int arg2;
        public int arg3;
        public int arg4;
        public int arg5;

        public EntityMessage(string str)
        {
            var inputs = str.Split(' ');
            this.id = int.Parse(inputs[0]);
            this.entityType = inputs[1];
            this.arg1 = int.Parse(inputs[2]);
            this.arg2 = int.Parse(inputs[3]);
            this.arg3 = int.Parse(inputs[4]);
            this.arg4 = int.Parse(inputs[5]);
            this.arg5 = int.Parse(inputs[6]);
        }

        public void ProcessMessage()
        {
            if (entityType == "FACTORY")
            {
                Entity factory = Factory.GetFactoryByID(id);
                factory.ProcessMessage(this);
            }

            if (entityType == "TROOP")
            {
                Entity troop = Troop.GetTroopByID(id);
                troop.ProcessMessage(this);
            }

            if (entityType == "BOMB")
            {
                Entity bomb = Bomb.GetBombByID(id);
                bomb.ProcessMessage(this);
            }
        }
    }

    class Troop : Entity, IEquatable<Troop>, IComparable<Troop>
    {
        public static HashSet<Troop> troops = new HashSet<Troop>();
        public static HashSet<Troop> ally = new HashSet<Troop>();
        public static HashSet<Troop> enemy = new HashSet<Troop>();

        public static Dictionary<string, Troop> hashTroops = new Dictionary<string, Troop>();

        public static int idCount = 0;

        public static int lastEndTime = 0;

        public int id;

        public Team team;
        public Factory start;
        public Factory end;
        public int count;
        public int armyAvailable = 0;
        public int turns;

        public Troop original;

        public int endTime;

        public bool isAlly;
        public bool isEnemy;

        public bool inTransit;

        public Troop()
        {
            this.id = idCount++;
        }

        public static void CleanUp()
        {
            troops = new HashSet<Troop>();
            ally = new HashSet<Troop>();
            enemy = new HashSet<Troop>();

            lastEndTime = 0;
        }

        public void ProcessMessage(EntityMessage message)
        {
            start = Factory.GetFactoryByID(message.arg2);
            end = Factory.GetFactoryByID(message.arg3);
            count = message.arg4;
            armyAvailable = count;
            turns = message.arg5;
            endTime = Game.gameTime + turns;

            if (message.arg1 == 1)
            {
                SetTeam(Team.Ally);
            }
            else
            {
                SetTeam(Team.Enemy);
            }

            inTransit = true;

            end.incoming.Add(this);
            //hashTroops.Add(this.GetStringKey(), this);

            lastEndTime = Math.Max(lastEndTime, endTime); //get the last troop endTime;
        }

        public void SetTeam(Team team)
        {
            this.team = team;
            if (team == Team.Ally)
            {
                ally.Add(this);
                isAlly = true;
                isEnemy = false;

                Factory.globalStats.totalAllyCount += this.count;
            }

            if (team == Team.Enemy)
            {
                enemy.Add(this);
                isAlly = false;
                isEnemy = true;

                Factory.globalStats.totalEnemyCount += this.count;
            }
        }

        public static Troop GetTroopByID(int id)
        {
            Troop troop = new Troop();
            troops.Add(troop);
            return troop;
        }

        public string GetStringKey()
        {
            return this.start.id + " " + this.end.id + " " + this.endTime + " " + this.count;
        }

        public override bool Equals(object obj)
        {
            Troop troop = obj as Troop;
            return troop.id == this.id;
        }

        public override int GetHashCode()
        {
            return this.id;
        }

        public bool Equals(Troop troop)
        {
            return troop.id == this.id;
        }

        public int CompareTo(Troop troop)
        {
            var compare = this.endTime.CompareTo(troop.endTime);
            if (compare == 0) //if end time is the same
            {
                return this.id.CompareTo(troop.id);
            }
            else
            {
                return compare;
            }
        }
    }

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

        public int pathValue;

        public int count;
        public int armyAvailable;

        public int production;
        public int oldProduction;
        public int disabledTurns;

        public Factory nearestEnemy;
        public int nearestEnemyDistance;

        public Dictionary<Factory, int> links; //links with factory and distance
        public SortedSet<Troop> incoming; //incoming troops

        public PathInfo shortestPaths;
        public PathInfo optimalPaths;

        public Factory parent;
        public int shortestDistance;

        public List<FactoryState> states;

        public Factory(int id)
        {
            this.id = id;

            pathValue = 0;

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

        public int GetNearbyEnemyThreats2(Factory target)
        {
            foreach (var pair in links)
            {
                Factory enemy = pair.Key;
                int distance = pair.Value;

                if (enemy.isEnemy && distance <= 2 && target != enemy)
                {
                    int nearbyAllyCount = GetNearbyAllySupportCount(distance);
                    if (nearbyAllyCount < enemy.count)
                    {
                        return enemy.count - nearbyAllyCount;
                    }
                }
            }

            return 0;
        }

        public int GetNearbyEnemyThreats()
        {
            foreach (var pair in links)
            {
                Factory enemy = pair.Key;
                int distance = pair.Value;

                if (enemy.isEnemy && distance <= 2)
                {
                    int nearbyAllyCount = armyAvailable + GetNearbyAllySupportCount(distance);
                    //FactoryState lastState = enemy.states.Last();

                    if (enemy.isEnemy && nearbyAllyCount < enemy.count)
                    {                        
                        return enemy.count - nearbyAllyCount;
                    }
                }
            }

            return 0;
        }

        public int GetNearbyAllySupportCount(int turns)
        {
            int supportCount = 0;

            foreach (var pair in links)
            {
                Factory ally = pair.Key;
                int distance = pair.Value;

                if (ally.isAlly && distance < turns)
                {
                    int productionTurns = turns - distance;
                    int production = Math.Max(0, productionTurns * ally.production);

                    supportCount += ally.armyAvailable + supportCount;
                }
            }

            return supportCount;
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

        public static void CalculatePaths()
        {
            foreach (var factory in Factory.factories)
            {
                Algorithm.CalculateShortestPaths(factory);
                Algorithm.CalculateOptimalPaths(factory);
            }                
        }

        public static void CalculateValues()
        {
            foreach (var factory in Factory.factories)
            {
                Algorithm.CalculateFactoryValue(factory);
            }
        }

        public int GetDistance(Factory factory)
        {
            return this.shortestPaths.shortestDistance[factory];
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

        public void AddMockBombTroop(int disabledTurns)
        {
            //for factory state when production is disabled
            Troop testTroop = new Troop
            {
                start = this,
                end = this,
                count = 0,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = disabledTurns,
                endTime = Game.gameTime + disabledTurns - 1,
            };

            //for factory state when production is restored
            Troop testTroop2 = new Troop
            {
                start = this,
                end = this,
                count = 0,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = disabledTurns,
                endTime = Game.gameTime + disabledTurns,
            };

            this.incoming.Add(testTroop);
            this.incoming.Add(testTroop2);
        }

        public void ProcessMessage(EntityMessage message)
        {
            count = message.arg2;

            disabledTurns = message.arg4;
            production = disabledTurns > 0 ? 0 : message.arg3;
            oldProduction = Math.Max(production, oldProduction);

            if (disabledTurns > 0)
            {
                AddMockBombTroop(disabledTurns);
            }

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

    class FactoryGlobalStats
    {
        public int enemyTotalProduction;
        public int allyTotalProduction;
        public int neutralTotalProduction;

        public int totalAllyCount;
        public int totalEnemyCount;

        public double totalEnemyCountWeighted;
        public double totalAllyCountWeighted;

        public bool allyProductionMaxed;

        public FactoryGlobalStats()
        {

        }
    }

    class FactoryState
    {
        public int id;
        public Team team;
        public bool isAlly;
        public bool isEnemy;
        public bool isNeutral;

        public int count;
        public int production;

        public int oldProduction;
        public int disabledEndTime;

        public int gameTime = 0;

        public Troop troop;

        public FactoryState(Factory factory, int gameTurns)
        {
            this.id = factory.id;
            SetTeam(factory.team);

            this.count = factory.count;
            this.production = factory.production;
            this.gameTime = gameTurns;

            this.oldProduction = factory.oldProduction;
            this.disabledEndTime = Game.gameTime + factory.disabledTurns;
        }

        public FactoryState(FactoryState factory, Troop troop, bool sameTime)
        {
            id = factory.id;
            count = factory.count;
            gameTime = troop.endTime;
            production = gameTime >= factory.disabledEndTime ? factory.oldProduction : 0;
            oldProduction = factory.oldProduction;
            this.troop = troop;
            disabledEndTime = factory.disabledEndTime;
            SetTeam(factory.team); //team stays the same (default = neutral)
                        
            if (factory.team != Team.Neutral)
            {
                int timeDiff = troop.endTime - factory.gameTime;
                this.count += timeDiff * production;
            }

            if (troop is Bomb) //same time bomb?
            {
                int bombCount = Math.Max(this.count / 2, 10);
                this.count -= bombCount;
                this.count = Math.Max(0, this.count);

                this.oldProduction = Math.Max(oldProduction, production);
                this.production = 0;
                this.disabledEndTime = troop.endTime + 5;

                //Console.Error.WriteLine("Bomb " + troop.id + ": " + this.count);
            }
            else
            {
                this.count += (troop.team == factory.team) ? troop.count : -troop.count;

                if (this.count < 0 && sameTime == false) //if the troop captured the factory
                {
                    this.count *= -1;
                    SetTeam(troop.team); //team become the troops team
                }
            }
            
        }

        public static List<FactoryState> CalculateFactoryState(Factory factory, SortedSet<Troop> troops, bool mockState = true)
        {
            List<FactoryState> states = new List<FactoryState>();

            FactoryState lastState
                    = new FactoryState(factory, Game.gameTime);

            states.Add(lastState); //add atleast 1 state to the list

            List<Troop> troopList = troops.ToList();

            for (int i = 0; i < troopList.Count; i++)
            {
                Troop troop = troopList[i];
                bool sameTime = false;

                if (i + 1 < troopList.Count && troop.endTime == troopList[i + 1].endTime) //if troop came at same time
                {
                    sameTime = true;
                    Troop nextTroop = troopList[i + 1];
                    Troop combinedTroop = new Troop
                    {
                        start = troop.start,
                        end = troop.end,
                        count = Math.Abs(troop.count+(troop.team==nextTroop.team?1:-1)*nextTroop.count),
                        team = troop.count>=nextTroop.count?troop.team:nextTroop.team,
                        isAlly = troop.count>=nextTroop.count?troop.isAlly:nextTroop.isAlly,
                        isEnemy = troop.count>=nextTroop.count?troop.isEnemy:nextTroop.isEnemy,
                        turns = troop.turns - Game.gameTime,
                        endTime = troop.endTime,
                    };

                    troop = combinedTroop;
                    i++;
                }

                FactoryState newState = new FactoryState(lastState, troop, false);
                states.Add(newState);

                if (mockState == false)
                {
                    if (newState.isAlly)
                    {
                        factory.armyAvailable = Math.Min(factory.armyAvailable, newState.count);
                    }
                    else
                    {
                        factory.armyAvailable = Math.Min(factory.armyAvailable, -newState.count);
                    }
                }

                lastState = newState;
            }

            return states;
        }

        public static bool IsFactoryCaptured(List<FactoryState> states)
        {
            Team originalTeam = states.First().team;
            for (int i = 0; i < states.Count; i++)
            {
                FactoryState state = states[i];

                if (i + 1 < states.Count && state.gameTime == states[i + 1].gameTime) //if states occur at same time
                {
                    continue;
                }

                if (originalTeam != state.team)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool WillFactoryBeAlly(List<FactoryState> states)
        {
            FactoryState firstState = states.First();
            bool allyLock = firstState.isAlly;

            for (int i = 0; i < states.Count; i++)
            {
                FactoryState state = states[i];

                if (i + 1 < states.Count && state.gameTime == states[i + 1].gameTime) //if states occur at same time
                {
                    continue;
                }

                if (allyLock && state.isEnemy)
                {
                    return false;
                }

                if (allyLock == false && state.isAlly)
                {
                    allyLock = true;
                }
            }

            return allyLock;
        }

        public static FactoryState GetFactoryState(Factory factory, int gameTime)
        {
            Troop testTroop = new Troop
            {
                start = factory,
                end = factory,
                count = 0,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = gameTime - Game.gameTime,
                endTime = gameTime,
            };

            return GetFactoryState(factory, testTroop);
        }

        public static FactoryState GetFactoryState(Factory factory, Troop testTroop)
        {
            SortedSet<Troop> mockTroops = new SortedSet<Troop>(factory.incoming);
            mockTroops.Add(testTroop);

            var states = FactoryState.CalculateFactoryState(factory, mockTroops);
            foreach (var state in states)
            {
                if (state.gameTime == testTroop.endTime)
                {
                    return state;
                }
            }

            return null;
        }

        public void SetTeam(Team team)
        {
            this.team = team;
            if (team == Team.Ally)
            {
                isAlly = true;
                isEnemy = false;
                isNeutral = false;
            }
            else if (team == Team.Enemy)
            {
                isAlly = false;
                isEnemy = true;
                isNeutral = false;
            }
            else
            {
                isAlly = false;
                isEnemy = false;
                isNeutral = true;
            }
        }
    }

    class FactoryStats
    {
        public int enemyCount; //enemy neighbour count
        public int allyCount;

        public int averageEnemyDistance;
        public int enemyFactoryCount;

        public double enemyCountWeighted;
        public double allyCountWeighted;

        public double enemyScore;
        public double allyScore;
        public double combinedScore;
        public double differenceScore;

        public double missionPrority;

        public FactoryStats()
        {

        }
    }

    public enum MoveType
    {
        Move,
        Wait,
        Bomb,
        Inc,
        Msg,
    }

    class Action
    {
        public static List<Action> actions = new List<Action>();

        public MoveType move;

        public Factory start;
        public Factory end;
        public int numRobots;

        public string message;

        public Action()
        {
            this.move = MoveType.Wait;
        }

        public Action(MoveType move, Factory start, Factory end, int numRobots)
        {
            this.move = move;
            this.start = start;
            this.end = end;
            this.numRobots = numRobots;
        }

        public Action(MoveType move, Factory start)
        {
            this.move = move;
            this.start = start;
        }

        public Action(string message)
        {
            this.move = MoveType.Msg;
            this.message = message;
        }

        public static void CleanUp()
        {
            actions = new List<Action>();
        }

        public static void AddAction(Action action, Troop troop = null)
        {
            if (action.move == MoveType.Move)
            {
                action.start.count -= action.numRobots;
                action.start.armyAvailable -= action.numRobots;

                if (troop != null && troop.inTransit == false)
                {
                    action.end.incoming.Add(troop);
                    action.end.states = FactoryState.CalculateFactoryState(action.end, action.end.incoming);
                }                

                //Console.Error.WriteLine("Move " + action.start.id + "-" + action.end.id + ": " + troop.count);
            }
            else if (action.move == MoveType.Inc)
            {
                action.start.count -= 10;
                action.start.armyAvailable -= 10;
            }
            else if (action.move == MoveType.Bomb)
            {
                action.end.incoming.Add(troop);
                Game.bomb--;
            }

            Action.actions.Add(action);

        }

        public static void PrintActions()
        {
            string str = "WAIT;";

            foreach (var action in actions)
            {
                switch (action.move)
                {
                    case MoveType.Move:
                        str += "MOVE " + action.start.id + " " + action.end.id + " " + action.numRobots;
                        break;
                    case MoveType.Inc:
                        str += "INC " + action.start.id;
                        break;
                    case MoveType.Bomb:
                        str += "BOMB " + action.start.id + " " + action.end.id;
                        break;
                    case MoveType.Msg:
                        str += "MSG " + action.message;
                        break;
                    default:
                        str += "WAIT";
                        break;
                }

                str += ";";
            }

            str = str.Remove(str.Length - 1); //remove extra ;

            Console.WriteLine(str);
        }
    }

    class Timer
    {
        private static DateTime loadTime = DateTime.Now;

        public static float TickCount
        {
            get
            {
                return (int)DateTime.Now.Subtract(loadTime).TotalMilliseconds;
            }
        }
    }

    public enum MissionSuccessRating
    {
        Impossible,
        HasPrereq,
        NotEnoughTroops,
        Unlikely,
        Possible,
        Guaranteed,
    }

    public enum MissionType
    {
        Bomb,
        Defend,
        Capture,
        Inc,
        IncSupport,
        Reinforce,
        FinalReinforce,
    }

    class Mission : IComparable<Mission>
    {
        public MissionType type;
        public Factory factory;
        public int troopsNeeded;

        public SortedSet<Troop> acceptedMission;
        public List<Troop> finalEnlistedTroops;

        public double reward;

        public Dictionary<Mission, HashSet<Troop>> prereqs;

        public FactoryState finalState;

        public Mission prevMission;

        public int troopsUsed;

        public bool missionExecuted = false;

        public MissionSuccessRating successRating;
        public int missionEndTime;

        public MissionSolver solver;
        public MissionPlanner planner;

        public Mission(MissionType type, Factory factory)
        {
            this.type = type;
            this.factory = factory;
            this.acceptedMission = new SortedSet<Troop>();
            this.prereqs = new Dictionary<Mission, HashSet<Troop>>();

            this.planner = new MissionPlanner(this);

            switch (type)
            {
                case MissionType.Capture:
                    solver = new CaptureMissionSolver(this);
                    break;
                case MissionType.Defend:
                    solver = new CaptureMissionSolver(this);
                    break;
                case MissionType.IncSupport:
                    solver = new IncSupportMissionSolver(this);
                    break;
                case MissionType.Reinforce:
                    solver = new ReinforceMissionSolver(this);
                    break;
                case MissionType.FinalReinforce:
                    solver = new FinalReinforceMissionSolver(this);
                    break;
                case MissionType.Inc:
                    solver = new IncreaseMissionSolver(this);
                    break;
                case MissionType.Bomb:
                    solver = new BombMissionSolver(this);
                    break;
                default:
                    break;
            }
        }

        public void CleanUp()
        {
            prereqs = new Dictionary<Mission, HashSet<Troop>>();

            acceptedMission = new SortedSet<Troop>();
            successRating = MissionSuccessRating.Impossible;
            finalEnlistedTroops = new List<Troop>();
        }

        public void EnlistTroops(bool isReinforcement = false)
        {
            foreach (var ally in Factory.ally)
            {
                if (factory != ally)
                {
                    planner.MakeMockTroop(ally, isReinforcement);
                }
            }

            planner.AddInTransitTroops();
        }

        public bool isPossible()
        {
            return successRating >= MissionSuccessRating.Possible;
        }

        public int CompareTo(Mission missionB)
        {
            return this.reward.CompareTo(missionB.reward);            
        }

        public override bool Equals(object obj)
        {
            Mission mission = obj as Mission;
            return this.type == mission.type && this.factory == mission.factory;
        }

        public override int GetHashCode()
        {
            return ((int)type * 100) + factory.id;
        }
    }

    class MissionPlanner
    {
        public Mission mission;

        public MissionPlanner(Mission mission)
        {
            this.mission = mission;
        }

        public void GetTroopPath(Troop troop, Factory end)
        {
            Factory start = troop.start;
            int totalDistance = 0;

            int directDistance = 0;
            if (start.links.TryGetValue(end, out directDistance))
            {
                if (directDistance <= 3)
                {
                    troop.end = end;
                    troop.turns = directDistance + 1;
                    troop.endTime = Game.gameTime + troop.turns;
                    return;
                }
            }

            var prevFactory = start.optimalPaths.previousFactory;
            Factory parent = prevFactory[end];

            while (parent != start)
            {
                totalDistance += end.links[parent] + 1;
                end = parent;
                parent = prevFactory[end];
            }
            totalDistance += end.links[parent] + 1;

            troop.turns = totalDistance + 1;
            troop.endTime = Game.gameTime + troop.turns;
            troop.end = end;
        }

        public void AddPrereq(Troop troop)
        {
            Mission newMission = new Mission(MissionType.Capture, troop.end);
            HashSet<Troop> reqTroops;

            if (Strategy.curExecuted.Contains(newMission)
                || FactoryState.WillFactoryBeAlly(troop.end.states))
            {
                return; //already cleared prereq
            }

            if (mission.prereqs.TryGetValue(newMission, out reqTroops))
            {
                reqTroops.Add(troop);
            }
            else
            {
                mission.prereqs.Add(newMission, new HashSet<Troop> { troop });
            }
        }

        public void AddInTransitTroops()
        {
            foreach (var troop in Troop.ally)
            {
                if (troop.isAlly && troop.end != mission.factory 
                    && troop.end.isAlly && troop.original == null)
                {
                    Troop incomingTroop = new Troop
                    {
                        start = troop.end,
                        end = mission.factory,
                        count = troop.armyAvailable,
                        team = Team.Ally,
                        isAlly = true,
                        isEnemy = false,
                        inTransit = true,
                        original = troop,
                    };

                    GetTroopPath(incomingTroop, mission.factory);

                    incomingTroop.end = troop.end;
                    incomingTroop.start = troop.start;
                    incomingTroop.turns = troop.turns + incomingTroop.turns + 1;
                    incomingTroop.endTime = Game.gameTime + incomingTroop.turns;

                    mission.acceptedMission.Add(incomingTroop);
                }
            }
        }

        public void MakeMockTroop(Factory ally, bool isReinforcement = false)
        {
            Troop mockTroop = new Troop
            {
                start = ally,
                count = ally.armyAvailable,
                team = Team.Ally,
                isAlly = true,
                isEnemy = false,
            };

            if (mission.factory == ally)
            {
                return;
            }

            var nearbyThreats = ally.GetNearbyEnemyThreats2(mission.factory);
            if (nearbyThreats > 0 && ally != mission.factory)
            {
                //mockTroop.count -= Math.Max(0, nearbyThreats);
            }

            GetTroopPath(mockTroop, mission.factory);

            int distance = ally.links[mockTroop.end];
            if (Bomb.bombs.Values.Any(b => b.isAlly && b.end == mission.factory && b.turns > distance))
            {
                return;
            }

            if (mockTroop.end.team != Team.Ally && mockTroop.end != mission.factory)
            {
                //troop travel through enemy land
                if (mockTroop.end.count != 0)
                {
                    AddPrereq(mockTroop);

                    if (isReinforcement)
                    {
                        return;
                    }

                }
            }

            if (ally.oldProduction < 3 && isReinforcement)
            {
                mockTroop.count -= 10;
            }

            if (mission.factory.id == 1)
            {
                Console.Error.WriteLine(ally.id + ": " + mockTroop.count);
            }


            if (mockTroop.count > 0)// && ally.GetNearbyEnemyThreats() == 0)
            {
                mission.acceptedMission.Add(mockTroop); //ally factory accepts the mission
            }

            if (mission.prevMission != null)
            {

            }            
        }
    }

    class MissionReward
    {
        public int allyTroopGained;
        public int enemyTroopLost;

        public int combinedReward;

        public MissionReward(int gained, int lost, int turns)
        {
            allyTroopGained = gained;
            enemyTroopLost = lost;

            combinedReward = gained + lost;

            //combinedRewardWeighted = (double)combinedReward / Math.Max(1, turns);
        }

        public static void GetCaptureRate(Mission mission)
        {

        }

        public static bool GetCaptureProbability(Mission mission, int defendingArmy, int turns)
        {
            int maxEnemyTroopCount = 0;

            foreach (var pair in mission.factory.shortestPaths.shortestDistance)
            {
                Factory neighbour = pair.Key;
                int distance = pair.Value;

                if (distance <= 4)
                {
                    if (neighbour.isAlly)
                    {
                        int production = Math.Max(0, neighbour.production * (turns - 2));
                        defendingArmy += neighbour.count + production;
                    }
                    else if (neighbour.team == Team.Enemy)
                    {
                        int production = Math.Max(0, neighbour.production * (turns - 1));
                        maxEnemyTroopCount += neighbour.count + production;
                    }
                }
            }

            return defendingArmy > maxEnemyTroopCount;
        }
    }




    class BombMissionSolver : MissionSolver
    {
        public Mission mission;

        public BombMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            Factory factory = mission.factory;

            mission.finalEnlistedTroops = new List<Troop>();

            if (Game.bomb <= 0)
            {
                mission.successRating = MissionSuccessRating.Impossible;
                return;
            }

            foreach (var pair in factory.links.OrderBy(p => p.Value))
            {
                Factory neighbour = pair.Key;
                int distance = pair.Value;

                int missionEndTime = Game.gameTime + distance + 1;

                FactoryState lastState = FactoryState.GetFactoryState(factory, missionEndTime);

                if (lastState.team == Team.Enemy && neighbour.isAlly
                    && !Bomb.bombs.Values.Any(b => b.isAlly && b.end == mission.factory)) //no bombs going there
                {
                    int finalArmyCount = lastState.count + (distance + 1) * lastState.production;


                    Bomb bomb = new Bomb
                    {
                        isAlly = true,
                        isEnemy = false,
                        team = Team.Ally,
                        start = neighbour,
                        end = mission.factory,
                        turns = distance + 1,
                        endTime = missionEndTime,
                    };

                    /*int inTransitEnemyCount = 0;
                    foreach (var troop in Troop.enemy.Where(t=>t.end == mission.factory).OrderBy(t=>t.endTime))
                    {
                        if (distance == troop.turns)
                        {
                            inTransitEnemyCount += troop.count;
                        }
                    }*/

                    if (lastState.production >= 2)//Factory.globalStats.enemyTotalProduction / 2
                        //|| lastState.count >= Factory.globalStats.totalEnemyCount / 2)
                    {
                        bomb.count = lastState.count;
                        mission.finalEnlistedTroops.Add(bomb);
                        mission.successRating = MissionSuccessRating.Guaranteed;
                        break;
                    }
                }
            }
        }

        public void Execute()
        {
            if (mission.finalEnlistedTroops.Count > 0)
            {
                Troop troop = mission.finalEnlistedTroops.First();

                Action newAction = new Action(MoveType.Bomb, troop.start, mission.factory, 1);
                Action.AddAction(newAction, troop);
            }
        }

        public void RateMission()
        {

        }

        public void CalculateReward()
        {
            if (mission.finalEnlistedTroops.Count > 0)
            {
                Troop bomb = mission.finalEnlistedTroops.First();

                //pow(production,3)/Time_To_Bomb
                mission.reward = Math.Pow(mission.factory.production, 3) / bomb.turns;
                //bomb.count
            }
            else
            {
                mission.reward = 0;
            }            
        }
    }

    class CaptureMissionSolver : MissionSolver
    {
        public Mission mission;

        public CaptureMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            mission.EnlistTroops();

            if (mission.acceptedMission.Count == 0)
            {
                mission.successRating = MissionSuccessRating.Impossible;
                return;
            }

            SortedSet<Troop> mockTroops = new SortedSet<Troop>(mission.factory.incoming);
            List<Troop> finalEnlisted = new List<Troop>();
            Factory factory = mission.factory;

            foreach (var mockTestTroop in mission.acceptedMission)
            {
                mockTroops.Add(mockTestTroop);
                finalEnlisted.Add(mockTestTroop);
                mission.troopsUsed += mockTestTroop.count;
                mission.missionEndTime = mockTestTroop.endTime;

                FactoryState lastState
                        = new FactoryState(factory, Game.gameTime);

                bool turnedEnemy = factory.isEnemy;
                bool allyLock = factory.isAlly;
                int armyAvailable = allyLock ? factory.count : 0;

                var ordered = mockTroops.ToList();

                if (mission.prereqs.Values.Any(r => r.Contains(mockTestTroop)))
                {
                    mission.successRating = MissionSuccessRating.HasPrereq;
                    mission.finalEnlistedTroops = finalEnlisted;
                    break;
                }

                //iterate through the mock states
                for (int i = 0; i < ordered.Count; i++)
                {
                    Troop troop = ordered[i];
                    FactoryState newState = new FactoryState(lastState, troop, false);

                    //if two troops arrives at the same time
                    if (i + 1 < ordered.Count && troop.endTime == ordered[i + 1].endTime)
                    {
                        newState = new FactoryState(lastState, troop, true);
                        lastState = newState;
                        continue;
                    }

                    if (newState.isEnemy)
                    {
                        turnedEnemy = true;

                        if (allyLock)
                        {
                            allyLock = false;
                            //break;
                        }
                    }

                    if (allyLock == false && newState.isAlly)
                    {
                        allyLock = true;
                        armyAvailable = newState.count;
                    }

                    if (factory.isAlly && troop == mockTestTroop)
                    {
                        armyAvailable = newState.count;
                    }

                    armyAvailable = Math.Min(armyAvailable, newState.count);
                    lastState = newState;
                }

                if (lastState.isAlly)//mission is possible
                {
                    int finalStateCount = mockTestTroop.count - armyAvailable + 1;

                    if (finalStateCount <= 0)
                    {
                        return; //we didn't need to send anything
                    }

                    if (mission.type == MissionType.Capture && turnedEnemy)
                    {
                        if (mission.acceptedMission.Sum(troop => troop.count) >= mission.factory.count)
                        {
                            mission.successRating = MissionSuccessRating.Possible;

                            mission.finalEnlistedTroops = new List<Troop>(mission.acceptedMission);
                            return;
                        }
                    }
                    else
                    {
                        mockTestTroop.count = mockTestTroop.count - armyAvailable + 1; //don't oversend
                    }


                    mission.troopsUsed += -armyAvailable + 1;
                    mission.finalState = lastState;

                    mission.successRating = MissionSuccessRating.Possible;

                    mission.finalEnlistedTroops = finalEnlisted;

                    return; //job's done
                }
                else
                {
                    mission.successRating = MissionSuccessRating.NotEnoughTroops;
                }
            }
        }

        public void Execute()
        {
            foreach (Troop troop in mission.finalEnlistedTroops)
            {
                if (troop.inTransit == false)
                {
                    Action newAction = new Action(MoveType.Move, troop.start, troop.end, troop.count);
                    Action.AddAction(newAction, troop);
                }
                else
                {
                    troop.original.armyAvailable = Math.Max(0, troop.original.armyAvailable - troop.count);
                }
            }
        }

        public void RateMission()
        {
            if (mission.type == MissionType.Capture)
            {
                /*if (mission.missionEndTime - Game.gameTime > 8)
                {
                    mission.successRating = MissionSuccessRating.Unlikely;
                }*/

                /*if (mission.successRating == MissionSuccessRating.Possible
                    && mission.factory.isEnemy
                    && !GetCaptureProbability())
                {
                    mission.successRating = MissionSuccessRating.Unlikely;
                }*/
            }
        }

        public bool GetCaptureProbability()
        {
            int attackingArmy = mission.finalEnlistedTroops.Sum(t => t.end == mission.factory ? t.count : 0);
            int maxEnemyTroopCount = 0;
            int turns = mission.missionEndTime - Game.gameTime;
            Troop nearestAttacker = mission.finalEnlistedTroops.FirstOrDefault(t=> t.end == mission.factory);

            if (nearestAttacker != null)
            {
                turns = nearestAttacker.turns;
            }

            foreach (var pair in mission.factory.shortestPaths.shortestDistance)
            {
                Factory neighbour = pair.Key;
                int distance = pair.Value;

                if (distance <= turns)
                {
                    if (neighbour.team == Team.Enemy)
                    {
                        int production = Math.Max(0, neighbour.production * (distance - turns - 1));
                        maxEnemyTroopCount += neighbour.count + production;
                    }
                }
            }

            return attackingArmy > maxEnemyTroopCount;
        }

        public void CalculateReward()
        {
            double value = mission.factory.oldProduction
                + 0.01 * Math.Pow(mission.factory.pathValue, 0.5) + 0.1;

            int turns = 0;//mission.missionEndTime - Game.gameTime;

            foreach (var ally in Factory.ally)
            {
                turns = Math.Max(turns, ally.shortestPaths.shortestDistance[mission.factory]);
            }

            if (mission.type == MissionType.Capture)
            {
                if (mission.factory.isNeutral)
                {
                    //Atk neutral
                    //Value[id]/(pow(Turns to get there,2)*Required_To_Take_Neutral(id,Turns to get there))

                    double troopUsed = (double)(Math.Max(1, mission.factory.states.Last().count)) / (mission.factory.oldProduction + 1);

                    double finalScore = value / (Math.Pow(turns, 3) * troopUsed);

                    mission.reward = finalScore;

                }
                else
                {
                    //Atk enemy
                    //Value[id]/(pow(Turns to get there,2)*8)

                    /*int enemyProduction = 0;
                    foreach (var pair in mission.factory.links)
                    {
                        var enemy = pair.Key;
                        int distance = pair.Value;

                        if (enemy.isEnemy && distance < turns)
                        {
                            enemyProduction += enemy.production;
                        }
                    }

                    double finalTroopUsed = enemyProduction + mission.acceptedMission.Sum(T => T.count);
                    */



                    double finalScore = value / (Math.Pow(turns, 3) * 8);

                    mission.reward = finalScore;
                }
            }

            if (mission.type == MissionType.Defend)
            {
                //Defend
                //Value[id]/(pow(Turns to get there,2)*Needed_Reinforcements(id,Turns to get there+5))

                double finalScore = value / (Math.Pow(turns, 2) * mission.troopsUsed);

                mission.reward = finalScore;
            }

        }

    }

    class FinalReinforceMissionSolver : MissionSolver
    {
        public Mission mission;

        public FinalReinforceMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            mission.EnlistTroops(mission.type == MissionType.FinalReinforce);

            var troopsNeeded = mission.acceptedMission.Sum(t => t.count); //send all

            mission.successRating = MissionSuccessRating.Guaranteed;
            mission.finalEnlistedTroops = mission.acceptedMission.ToList();
            mission.missionEndTime = Game.gameTime;//mission.finalEnlistedTroops.Last().endTime;
        }

        public void Execute()
        {
            foreach (Troop troop in mission.finalEnlistedTroops)
            {
                if (troop.inTransit == false)
                {
                    Action newAction = new Action(MoveType.Move, troop.start, troop.end, troop.count);
                    Action.AddAction(newAction, troop);
                }
            }
        }

        public void RateMission()
        {

        }

        public void CalculateReward()
        {
            if (mission.type == MissionType.IncSupport)
            {
                int turns = mission.missionEndTime;

                double finalScore = 1 / (Math.Pow(turns, 1.6) * 5);
                mission.reward = finalScore;
            }
        }
    }

    class IncreaseMissionSolver : MissionSolver
    {
        public Mission mission;

        public IncreaseMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            Factory ally = mission.factory;

            SortedSet<Troop> mockTroops = new SortedSet<Troop>(ally.incoming);
            Troop testTroop = new Troop
            {
                start = ally,
                end = ally,
                count = 10,
                team = Team.Enemy,
                isAlly = false,
                isEnemy = true,
                turns = 1,
                endTime = Game.gameTime + 1,
            };
            mockTroops.Add(testTroop);

            var mockStates = FactoryState.CalculateFactoryState(ally, mockTroops);
            //FactoryState lastState = mockStates.Last();

            if (FactoryState.IsFactoryCaptured(mockStates) == false)
            {
                mission.successRating = MissionSuccessRating.Guaranteed;
                mission.acceptedMission.Add(testTroop);
                mission.missionEndTime = Game.gameTime + 1;
            }
        }

        public void Execute()
        {
            Action newAction = new Action(MoveType.Inc, mission.factory);
            Action.AddAction(newAction);
        }

        public void RateMission()
        {

        }

        public void CalculateReward()
        {
            double value = 1 + 0.01 * mission.factory.armyAvailable;
            mission.reward = value / (Math.Pow(10, 2) * 10);
        }
    }

    class IncSupportMissionSolver : MissionSolver
    {
        public Mission mission;

        public IncSupportMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            mission.EnlistTroops();

            int troopsNeeded = 10 - mission.factory.armyAvailable;

            List<Troop> finalEnlisted = new List<Troop>();

            int enlistedCount = 0;
            foreach (var mockTestTroop in mission.acceptedMission)
            {
                var turns = mockTestTroop.turns;
                var production = turns * mission.factory.production;

                mission.missionEndTime = mockTestTroop.endTime;

                if (production >= troopsNeeded)
                {
                    mission.successRating = MissionSuccessRating.Guaranteed;
                    mission.finalEnlistedTroops = finalEnlisted;
                    return; //nothing to do here
                }

                enlistedCount += mockTestTroop.count;
                finalEnlisted.Add(mockTestTroop);
                
                if (enlistedCount + production >= troopsNeeded)
                {                    
                    mockTestTroop.count -= Math.Max(0,(enlistedCount - (troopsNeeded + production)));

                    mission.successRating = MissionSuccessRating.Guaranteed;
                    mission.finalEnlistedTroops = finalEnlisted;
                    
                    return;
                }
            }
        }

        public void Execute()
        {
            foreach (Troop troop in mission.finalEnlistedTroops)
            {
                if (troop.inTransit == false)
                {
                    Action newAction = new Action(MoveType.Move, troop.start, troop.end, troop.count);
                    Action.AddAction(newAction, troop);
                }
            }

            if (mission.type == MissionType.IncSupport)
            {
                mission.factory.armyAvailable = 0;//u called for support, don't even try to send army
            }
        }

        public void RateMission()
        {

        }

        public void CalculateReward()
        {
            if (mission.type == MissionType.IncSupport)
            {
                int turns = mission.missionEndTime + 11;

                double value = 1 + 0.01 * mission.factory.armyAvailable;

                double finalScore = value / (Math.Pow(11, 2) * 10);
                mission.reward = finalScore;
            }
        }
    }

    interface MissionSolver
    {
        void Solve();
        void Execute();
        void RateMission();
        void CalculateReward();
    }

    class ReinforceMissionSolver : MissionSolver
    {
        public Mission mission;

        public ReinforceMissionSolver(Mission mission)
        {
            this.mission = mission;
        }

        public void Solve()
        {
            mission.EnlistTroops();

            mission.troopsNeeded = mission.factory.GetNearbyEnemyThreats();

            if (mission.troopsNeeded <= 0)
            {
                mission.successRating = MissionSuccessRating.Impossible;
                return;
            }

            List<Troop> finalEnlisted = new List<Troop>();

            foreach (var troop in mission.acceptedMission)
            {
                mission.troopsNeeded -= troop.count;
                finalEnlisted.Add(troop);

                if (mission.troopsNeeded <= 0)
                {
                    troop.count = troop.count + mission.troopsNeeded;
                    mission.successRating = MissionSuccessRating.Possible;
                    mission.finalEnlistedTroops = finalEnlisted;
                    return;
                }
            }            
        }

        public void Execute()
        {
            foreach (Troop troop in mission.finalEnlistedTroops)
            {
                if (troop.inTransit == false)
                {
                    Action newAction = new Action(MoveType.Move, troop.start, troop.end, troop.count);
                    Action.AddAction(newAction, troop);
                }
            }
        }

        public void RateMission()
        {

        }

        public void CalculateReward()
        {
            double value = mission.factory.oldProduction
                + 0.01 * Math.Pow(mission.factory.pathValue, 0.5) + 0.1;

            int turns = 0;//mission.missionEndTime - Game.gameTime;

            foreach (var ally in Factory.ally)
            {
                turns = Math.Max(turns, ally.shortestPaths.shortestDistance[mission.factory]);
            }

            double finalScore = value / (Math.Pow(turns, 2));

            mission.reward = finalScore;
        }
    }

}
