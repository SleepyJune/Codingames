using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public enum Team
{
    Neutral,
    Ally,
    Enemy,
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

interface Entity
{
    void ProcessMessage(EntityMessage message);
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

class ShortestPaths
{
    public Dictionary<Factory, int> shortestDistance = new Dictionary<Factory, int>();
    public Dictionary<Factory, Factory> previousFactory = new Dictionary<Factory, Factory>();

    public ShortestPaths(Dictionary<Factory, int> dist, Dictionary<Factory, Factory> prev)
    {
        this.shortestDistance = dist;
        this.previousFactory = prev;
    }
}

class BFS
{
    public static void CalculateShortestPaths(Factory start)
    {
        Dictionary<Factory, int> dist = new Dictionary<Factory, int>();
        Dictionary<Factory, Factory> prev = new Dictionary<Factory, Factory>();

        List<Factory> q = new List<Factory>();

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

                int alternativeDistance = dist[factory] + distance;

                if (alternativeDistance < dist[neighbour])
                {
                    dist[neighbour] = alternativeDistance;
                    prev[neighbour] = factory;
                }
            }
        }
                
        start.shortestPaths = new ShortestPaths(dist, prev);
    }

    public static Troop UseBFS(Troop troop, Factory end, bool getDirectPath = false)
    {
        Factory start = troop.start;
                
        foreach (var factory in Factory.factories) //initialize
        {
            factory.shortestDistance = 999;
        }

        Hashtable set = new Hashtable();
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
                else if (set[node.id] == null)
                {
                    set[node.id] = true;

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
            totalDistance += n.links[n.parent];
            n = n.parent;
        }
        totalDistance += n.links[n.parent];

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

class FactoryState
{
    public int id;
    public Team team;
    public bool isAlly;
    public bool isEnemy;

    public int count;
    public int production;

    public int oldProduction;
    public int disabledTime;

    public int gameTime = 0;

    public Troop troop;

    public FactoryState(Factory factory, int gameTurns)
    {
        this.id = factory.id;
        SetTeam(factory.team);

        this.count = factory.count;
        this.production = factory.production;
        this.gameTime = gameTurns;

        this.oldProduction = factory.production;
        this.disabledTime = factory.disabledTurns;
    }

    public FactoryState(FactoryState factory, Troop troop)
    {
        id = factory.id;
        count = factory.count;
        production = factory.disabledTime > 0 ? 0 : factory.oldProduction;
        oldProduction = factory.oldProduction;
        gameTime = troop.endTime;
        this.troop = troop;
        disabledTime = Math.Max(0, factory.disabledTime--);
        SetTeam(factory.team); //team stays the same (default = neutral)

        if (factory.team != Team.Neutral)
        {
            int timeDiff = troop.endTime - factory.gameTime;
            this.count += timeDiff * production;
        }

        if (troop is Bomb)
        {
            int bombCount = Math.Max(this.count / 2, 10);
            this.count -= bombCount;
            this.count = Math.Max(0, this.count);

            this.oldProduction = Math.Max(oldProduction, production);
            this.production = 0;
            this.disabledTime = troop.endTime + 3;

            //Console.Error.WriteLine("Bomb " + troop.id + ": " + this.count);
        }
        else
        {
            this.count += (troop.team == factory.team) ? troop.count : -troop.count;

            if (this.count < 0) //if the troop captured the factory
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

        foreach (var troop in troops)
        {
            FactoryState newState = new FactoryState(lastState, troop);
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
            turns = gameTime - gameTime,
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
            if (state.troop != null && state.troop.id == testTroop.id)
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
        }
        else if (team == Team.Enemy)
        {
            isAlly = false;
            isEnemy = true;
        }
        else
        {
            isAlly = false;
            isEnemy = true;
        }
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
            BFS.CalculateShortestPaths(factory);
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
            isEnemy = true;
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

class Bomb : Troop, Entity
{
    public static Dictionary<int, Bomb> bombs = new Dictionary<int, Bomb>();

    public static Dictionary<int, Bomb> enemyBombs = new Dictionary<int, Bomb>();
    public static List<Bomb> oldBombs = new List<Bomb>();

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

        oldBombs.RemoveAll(bomb => bomb.endTime >= Game.gameTime);
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

            if (Game.gameTime == 1)
            {
                end = Factory.ally.First();
                turns = end.links[start];
                endTime = Game.gameTime + turns;

                oldBombs.Add(this);
            }
        }

        start = Factory.GetFactoryByID(message.arg2);
        //DeduceBomb();
    }

    public static void DeduceBomb()
    {
        if (enemyBombs.Count == 1 && oldBombs.Count == 1)
        {
            var enemyBomb = enemyBombs.First();
            var oldBomb = oldBombs.First();

            //enemyBomb = oldBomb;


        }

        /*if (enemyBombs.Count == 1 && oldBombs.Count == 0)
        {
            Bomb enemyBomb1 = enemyBombs.Values.First();
            oldBombs.Add(enemyBomb1.id, enemyBomb1);
            Bomb.enemyBombCount++;
        }

        if (enemyBombs.Count == 1 && oldBombs.Count == 1)
        {
            Bomb enemyBomb1 = enemyBombs.Values.First();
            Bomb oldBomb1 = oldBombs.Values.First();
            enemyBomb1 = oldBomb1;
        }*/
    }

}

class Troop : Entity, IEquatable<Troop>, IComparable<Troop>
{
    public static HashSet<Troop> troops = new HashSet<Troop>();
    public static HashSet<Troop> ally = new HashSet<Troop>();
    public static HashSet<Troop> enemy = new HashSet<Troop>();

    public static int idCount = 0;

    public static int lastEndTime = 0;

    public int id;

    public Team team;
    public Factory start;
    public Factory end;
    public int count;
    public int turns;

    public int endTime;

    public bool isAlly;
    public bool isEnemy;

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
        if (message.arg1 == 1)
        {
            SetTeam(Team.Ally);
        }
        else
        {
            SetTeam(Team.Enemy);
        }

        start = Factory.GetFactoryByID(message.arg2);
        end = Factory.GetFactoryByID(message.arg3);
        count = message.arg4;
        turns = message.arg5;
        endTime = Game.gameTime + turns;

        end.incoming.Add(this);

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

            action.end.incoming.Add(troop);
            action.end.states = FactoryState.CalculateFactoryState(action.end, action.end.incoming);

            //Console.Error.WriteLine(action.start.id + "-" + action.end.id + ": " + troop.count);
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
}

class Mission : IComparable<Mission>
{
    public MissionType type;
    public Factory factory;
    public int troopsNeeded;

    public SortedSet<Troop> acceptedMission;
    public List<Troop> finalEnlistedTroops;

    public MissionReward reward;

    public HashSet<Mission> prereqs;

    public FactoryState finalState;

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
        this.prereqs = new HashSet<Mission>();

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
                solver = new ReinforceMissionSolver(this);
                break;
            case MissionType.Reinforce:
                solver = new ReinforceMissionSolver(this);
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
        prereqs = new HashSet<Mission>();

        acceptedMission = new SortedSet<Troop>();
        successRating = MissionSuccessRating.Impossible;
    }

    public void EnlistTroops()
    {
        foreach (var ally in Factory.ally)
        {
            if (factory != ally && ally.armyAvailable > 0)
            {
                planner.MakeMockTroop(ally, ally.armyAvailable);
            }
        }
    }

    public bool isPossible()
    {
        return successRating >= MissionSuccessRating.Possible;
    }

    public void CalculateReward()
    {
        Mission mission = this;

        int gained = 0;
        int lost = 0;
                
        int turns = mission.missionEndTime - Game.gameTime;

        if (acceptedMission.Count > 0)
        {
            turns = acceptedMission.First().endTime - Game.gameTime;
        }

        int rewardTime = Math.Max(1, 10-turns);
        if (mission.factory.nearestEnemy != null)
        {
            int defendingArmy = mission.finalState!=null?mission.finalState.count:1;
            //Console.Error.WriteLine(mission.factory.id + ": " + defendingArmy);
            
            if (mission.factory.nearestEnemy.count > defendingArmy)
            {
                int enemyArrivalTurns = Math.Min(10, mission.factory.nearestEnemyDistance - turns + 1);
                rewardTime = Math.Max(0, enemyArrivalTurns);
            }
        }


        int productionReward = mission.factory.production * rewardTime;

        if (mission.type == MissionType.Bomb)
        {
            this.reward = new MissionReward(1337, 0, 0);
            return;
        }

        if (mission.type == MissionType.Capture)
        {
            gained -= 1; //need at least 1 troop to capture factories            

            if (mission.factory.team == Team.Enemy)
            {
                gained += productionReward;
                lost += productionReward;
            }

            if (mission.factory.team == Team.Neutral)
            {
                gained -= mission.acceptedMission.Sum(troop => troop.count) - 1;
                gained += productionReward;
            }            
        }

        if (mission.type == MissionType.Defend)
        {
            gained += productionReward;
        }        

        if (mission.type == MissionType.Inc || mission.type == MissionType.IncSupport)
        {
            productionReward = 1 * rewardTime;
            gained -= 10; //lost 10 troops to production increase;
            gained += productionReward;
        }

        if (mission.type == MissionType.Reinforce)
        {
            gained -= mission.factory.nearestEnemyDistance;
            lost = 0;
        }

        Factory ally = mission.factory.GetNearestAlly();
        if (ally != null)
        {
            this.reward = new MissionReward(gained, lost, ally.shortestPaths.shortestDistance[mission.factory]);
        }
        else
        {
            this.reward = new MissionReward(-999, lost, 0);
        }


    }

    public int CompareTo(Mission missionB)
    {
        Mission missionA = this;

        int timeA = missionA.missionEndTime;
        int timeB = missionB.missionEndTime;

        int endTime = Math.Max(timeA, timeB);

        MissionReward rewardA = missionA.reward;
        MissionReward rewardB = missionB.reward;

        //Console.Error.WriteLine(missionA.factory.id + "-" + missionB.factory.id + ": " +
        //    rewardA.combinedReward + "-" + rewardB.combinedReward);

        /*foreach (var mission in missionA.prereqs)
        {
            if (missionB == mission)
            {
                return rewardA.combinedReward.CompareTo(
                    rewardA.combinedReward + rewardB.combinedReward);
            }
        }

        foreach (var mission in missionB.prereqs)
        {
            if (missionA == mission)
            {
                return (rewardA.combinedReward + rewardB.combinedReward).CompareTo(rewardB);
            }
        }*/

        return rewardA.combinedReward.CompareTo(rewardB.combinedReward);
    }

    public override bool Equals(object obj)
    {
        Mission mission = obj as Mission;
        return this.type == mission.type && this.factory == mission.factory;
    }

    public override int GetHashCode()
    {
        return (int)type * factory.id;
    }
}

interface MissionSolver
{
    void Solve();
    void Execute();
    void RateMission();
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

        int troopsNeeded = 0;
        if (mission.type == MissionType.IncSupport)
        {
            troopsNeeded += 10; //need 10 to increase production
        }

        if (mission.type == MissionType.Reinforce)
        {
            troopsNeeded = mission.acceptedMission.Sum(t => t.count); //send all

            /*foreach (var troop in mission.acceptedMission)
            {
                var ordered = troop.start.links.Keys.OrderBy(n => n.isAlly?n.nearestEnemyDistance:999);
                if (ordered.First() != null)
                {
                    troop.end = ordered.First();
                }
            }*/

            mission.successRating = MissionSuccessRating.Guaranteed;
            mission.finalEnlistedTroops = mission.acceptedMission.ToList();
            mission.missionEndTime = Game.gameTime;//mission.finalEnlistedTroops.Last().endTime;
            return;
        }

        List<Troop> finalEnlisted = new List<Troop>();

        int enlistedCount = 0;
        foreach (var mockTestTroop in mission.acceptedMission)
        {
            enlistedCount += mockTestTroop.count;
            finalEnlisted.Add(mockTestTroop);

            if (enlistedCount >= troopsNeeded)
            {
                mockTestTroop.count -= (enlistedCount - troopsNeeded);

                mission.successRating = MissionSuccessRating.Guaranteed;
                mission.finalEnlistedTroops = finalEnlisted;
                mission.missionEndTime = mockTestTroop.endTime;

                return;
            }
        }
    }

    public void Execute()
    {
        foreach (Troop troop in mission.finalEnlistedTroops)
        {
            Action newAction = new Action(MoveType.Move, troop.start, troop.end, troop.count);
            Action.AddAction(newAction, troop);
        }

        if (mission.type == MissionType.IncSupport)
        {
            mission.factory.armyAvailable = 0;//u called for support, don't even try to send army
        }
    }

    public void RateMission()
    {

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

        foreach (var pair in factory.links.OrderBy(p=>p.Value))
        {
            Factory neighbour = pair.Key;
            int distance = pair.Value;

            FactoryState lastState = FactoryState.GetFactoryState(factory, distance);

            if (lastState.team == Team.Enemy && neighbour.isAlly
                && factory.production == 3
                && !Bomb.bombs.Values.Any(b => b.isAlly && b.end == mission.factory)) //no bombs going there
            {
                int finalArmyCount = lastState.count + (distance + 1) * lastState.production;

                //Console.Error.WriteLine("Bomb" + lastState.id + (closestTroop.end == factory));

                if (finalArmyCount >= Factory.globalStats.totalEnemyCount / 2
                    || lastState.production >= Factory.globalStats.enemyTotalProduction / 2)
                {
                    Bomb bomb = new Bomb
                    {
                        isAlly = true,
                        isEnemy = false,
                        team = Team.Ally,
                        start = neighbour,
                        end = mission.factory,
                        turns = distance + 1,
                        endTime = Game.gameTime + distance + 1,
                    };

                    mission.finalEnlistedTroops = new List<Troop>();
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

            bool allyLock = factory.isAlly;
            int armyAvailable = allyLock ? factory.count : 0;

            var ordered = mockTroops.ToList();

            //iterate through the mock states
            for (int i = 0; i < ordered.Count; i++)
            {
                Troop troop = ordered[i];
                FactoryState newState = new FactoryState(lastState, troop);

                //if two troops arrives at the same time
                if (i + 1 < ordered.Count && troop.endTime == ordered[i + 1].endTime)
                {
                    lastState = newState;
                    continue;
                }

                if (allyLock && newState.isEnemy)
                {
                    allyLock = false;
                    break;
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

            if (allyLock)//mission is possible
            {
                int finalStateCount = mockTestTroop.count - armyAvailable + 1;

                if (finalStateCount <= 0)
                {
                    return; //we didn't need to send anything
                }

                if (true)//mission.factory.team == Team.Enemy)
                {
                    if (MissionReward.GetCaptureProbability(mission, 1, mission.missionEndTime - Game.gameTime))
                    {
                        mockTestTroop.count = mockTestTroop.count - armyAvailable + 1; //don't oversend
                        finalStateCount = 1;
                        lastState.count = 1;
                    }
                    else
                    {
                        if (!MissionReward.GetCaptureProbability(mission, finalStateCount, mission.missionEndTime - Game.gameTime))
                        {
                            mission.successRating = MissionSuccessRating.Unlikely;
                            continue;
                        }
                    }
                }
                else
                {
                    mockTestTroop.count = mockTestTroop.count - armyAvailable + 1; //don't oversend
                }                

                mission.troopsUsed += -armyAvailable + 1;
                mission.finalState = lastState;

                mission.successRating = mission.prereqs.Count > 0 ?
                    MissionSuccessRating.HasPrereq :
                    MissionSuccessRating.Possible;

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
            Action newAction = new Action(MoveType.Move, troop.start, troop.end, troop.count);
            Action.AddAction(newAction, troop);
        }
    }

    public void RateMission()
    {
        if (mission.missionEndTime - Game.gameTime > 6)
        {
            mission.successRating = MissionSuccessRating.Unlikely;
        }

        int defendingCount = mission.finalState != null ? mission.finalState.count : 1;
        if (!MissionReward.GetCaptureProbability(mission, defendingCount, mission.missionEndTime - Game.gameTime))
        {
            mission.successRating = MissionSuccessRating.Unlikely;
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

class MissionPlanner
{
    public Mission mission;

    public MissionPlanner(Mission mission)
    {
        this.mission = mission;
    }

    public void MakeMockTroop(Factory ally, int count)
    {
        Troop mockTroop = new Troop
        {
            start = ally,
            count = count,
            team = Team.Ally,
            isAlly = true,
            isEnemy = false,
        };

        if (mission.factory.team == Team.Neutral)
        {
            BFS.UseBFS(mockTroop, mission.factory, true);
        }
        else
        {
            BFS.UseBFS(mockTroop, mission.factory);
        }

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
                Mission newMission = new Mission(MissionType.Capture, mockTroop.end);

                if (!mission.prereqs.Contains(newMission))
                {
                    mission.prereqs.Add(newMission);
                }
                //return;
            }
        }

        mission.acceptedMission.Add(mockTroop); //ally factory accepts the mission
    }
}

class Strategy
{
    public static List<Mission> missions = new List<Mission>();

    public static void AddMissions()
    {
        var orderedFactories = Factory.factories
            .OrderByDescending(factory =>
                factory.stats.allyScore * factory.oldProduction);

        foreach (var factory in orderedFactories)
        {
            if (Game.bomb > 0 && Game.gameTime > 1)
            {
                Mission bombMission = new Mission(MissionType.Bomb, factory);
                missions.Add(bombMission);
            }

            //if the factory will always be ally's
            if (FactoryState.WillFactoryBeAlly(factory.states))
            {
                if (factory.oldProduction < 3 && factory.armyAvailable > 10)
                {
                    Mission newMission = new Mission(MissionType.Inc, factory);
                    missions.Add(newMission);
                }
                else if (factory.oldProduction < 3)
                {
                    Mission newMission = new Mission(MissionType.IncSupport, factory);
                    missions.Add(newMission);
                }
            }
            else
            {
                if (factory.isAlly)
                {
                    Mission newMission = new Mission(MissionType.Defend, factory);
                    missions.Add(newMission);
                }
                else
                {
                    Mission newMission = new Mission(MissionType.Capture, factory);
                    missions.Add(newMission);                    
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

            mission.CleanUp();
            mission.solver.Solve();
            mission.solver.RateMission();
            mission.CalculateReward();
        }
    }

    public static bool ClearedPrereq(Mission mission)
    {
        foreach (var prereq in mission.prereqs)
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
        var ordered = Factory.ally.OrderBy(ally => ally.nearestEnemyDistance)
                    .ThenByDescending(ally => ally.oldProduction);

        if (ordered.First() != null)
        {
            Mission newMission = new Mission(MissionType.Reinforce, ordered.First());                

            missions.Add(newMission);
            newMission.solver.Solve();

            newMission.missionExecuted = true;
            newMission.solver.Execute();
        }
    }

    public static void ExecuteFinalReinforcement2()
    {
        foreach (var mission in missions)
        {
            if (mission.missionExecuted)
            {
                continue;
            }

            Mission newMission =
                mission.factory.isAlly ?
                new Mission(MissionType.Reinforce, mission.factory) :
                new Mission(MissionType.Reinforce, mission.factory.GetNearestAlly());

            missions.Add(newMission);
            newMission.solver.Solve();

            newMission.missionExecuted = true;
            newMission.solver.Execute();

            //Console.Error.WriteLine("Final Reinforce: " + newMission.factory.id);
            return;
        }
    }

    public static bool CheckLaterRewards(int index)
    {
        double rewards = 0;

        if (index + 1 >= missions.Count)
        {
            return false;
        }

        for (int i = index + 1; i < missions.Count; i++)
        {
            rewards += Math.Max(0, missions[i].reward.combinedReward);
        }

        double currentReward = missions[index].reward.combinedReward;

        return rewards >= currentReward;
    }

    public static bool ExecuteMissions()
    {
        Strategy.SolveMissions();

        missions.Sort(); //lowest reward first
        missions.Reverse();

        foreach (var mission in missions)
        {
            Console.Error.WriteLine(mission.type.ToString() + " " + mission.factory.id + ": " + mission.successRating.ToString() 
                + " " + mission.reward.combinedReward);
        }

        for (int i = 0; i < missions.Count; i++)
        {
            Mission mission = missions[i];

            if (mission.missionExecuted)
            {
                continue;
            }            

            if (mission.isPossible() && ClearedPrereq(mission))
            {                
                mission.missionExecuted = true;
                mission.solver.Execute();

                Console.Error.WriteLine("Execute " + mission.type.ToString() + " " + mission.factory.id);

                //string message = mission.type.ToString() + " " + mission.factory.id;
                //Action.AddAction(new Action(message));

                return true;
            }

            if (mission.successRating == MissionSuccessRating.NotEnoughTroops)
            {
                if (Strategy.CheckLaterRewards(i) == false)
                {
                    break;
                }
            }
        }

        ExecuteFinalReinforcement();

        return false;
    }

    public static void CleanUp()
    {
        missions = new List<Mission>();
    }
}

class Game
{
    public static int gameTime = 0;
    public static int bomb = 2;

    public static void InitializeFirstTurn()
    {
        Factory.CalculateShortestPaths();
    }

    public static void InitializeTurn()
    {
        Factory.ProcessFactoryStates();
        Factory.ProcessNeighbourCounts();
        Factory.CalculateAllArmyScore();
    }

    public static void MakeMove()
    {
        Strategy.AddMissions();

        while (Strategy.ExecuteMissions())
        {

        }
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

class Player
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