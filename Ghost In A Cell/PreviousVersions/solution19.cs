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

class NearestResults
{
    //Dictionary<Factory, int> dist = new Dictionary<Factory, int>();
    public Dictionary<Factory, Factory> prev = new Dictionary<Factory, Factory>();
    public int shortestDistance;

    public NearestResults(int shortestDistance, Dictionary<Factory, Factory> prev)
    {
        this.shortestDistance = shortestDistance;
        this.prev = prev;
    }
}

class BFS
{
    public static void FindNearestEnemy(Factory start)
    {
        Dictionary<Factory, int> dist = new Dictionary<Factory, int>();
        Dictionary<Factory, Factory> prev = new Dictionary<Factory, Factory>();

        List<Factory> q = new List<Factory>();

        foreach (var factory in Factory.factories) //initialize
        {
            dist.Add(factory, int.MaxValue);
            prev.Add(factory, null);

            q.Add(factory);
        }

        dist[start] = 0;

        while (q.Count > 0)
        {
            var factory = q.OrderBy(f => dist[f]).First();
            q.Remove(factory);

            foreach (var pair in factory.links)
            {
                Factory neighbour = pair.Key;
                int distance = pair.Value;

                int alt = dist[factory] + distance;

                if (alt < dist[neighbour])
                {
                    dist[neighbour] = distance;
                    prev[neighbour] = factory;
                }
            }
        }

    }
    
    public static Troop UseBFS(Troop troop, Factory end)
    {
        Factory start = troop.start;
                
        foreach (var factory in Factory.factories) //initialize
        {
            factory.shortestDistance = int.MaxValue;
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

        troop.turns = totalDistance + 1;
        troop.endTime = Game.gameTime + totalDistance + 1;
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
        List<FactoryState> states = factory.states;
        for (int i = states.Count - 1; i >= 0; i--) //count backwards to in case of states of same time
        {
            FactoryState state = states[i];

            if (state.gameTime < gameTime)
            {
                return state;
            }
        }

        return states.Last();
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

class Factory : Entity
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

    public Dictionary<Factory, int> links; //links with factory and distance
    public SortedSet<Troop> incoming; //incoming troops

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

            factory.stats.averageEnemyDistance = factory.stats.enemyFactoryCount == 0 ? int.MaxValue :
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
        return; //do nothing for now

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

            HashSet<Factory> visited = new HashSet<Factory>();

            factory.AddArmyScore(allyCount, enemyCount);
            factory.CalculateArmyScore(visited, allyCount, enemyCount);
        }
    }

    public void AddArmyScore(double allyScore, double enemyScore)
    {
        stats.allyScore += allyScore;
        stats.enemyScore += enemyScore;

        stats.combinedScore += allyScore + enemyScore;
        stats.differenceScore += allyScore - enemyScore;
    }

    public void CalculateArmyScore(HashSet<Factory> visited, double allyScore, double enemyScore)
    {       
        visited.Add(this);

        foreach (var pair in links)
        {
            Factory neighbour = pair.Key;
            int distance = pair.Value;

            double allyScoreWeighted = allyScore / distance;
            double enemyScoreWeighted = enemyScore / distance;

            neighbour.AddArmyScore(allyScoreWeighted, enemyScoreWeighted);

            bool canPropagate = Math.Round(allyScoreWeighted) > 0 || Math.Round(enemyScoreWeighted) > 0;

            if (canPropagate && !visited.Contains(neighbour))
            {                
                neighbour.CalculateArmyScore(visited, allyScoreWeighted, enemyScoreWeighted);
            }
        }
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
    }

    public static void DeduceBomb()
    {
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
        }

        if (team == Team.Enemy)
        {
            enemy.Add(this);
            isAlly = false;
            isEnemy = true;
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
        return this.endTime.CompareTo(troop.endTime);
    }
}

public enum MoveType
{
    Move,
    Wait,
    Bomb,
    Inc,
}

class Action
{
    public static List<Action> actions = new List<Action>();

    public MoveType move;

    public Factory start;
    public Factory end;
    public int numRobots;

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
    Reinforce,
}

class Mission
{
    public MissionType type;
    public Factory factory;
    public int troopsNeeded;

    public SortedSet<Troop> acceptedMission;

    public MissionSolver solver;
    public MissionPlanner planner;

    public int maxTroopCount;

    public Mission(MissionType type, Factory factory)
    {
        this.type = type;
        this.factory = factory;
        this.acceptedMission = new SortedSet<Troop>();

        this.maxTroopCount = 0;

        this.planner = new MissionPlanner(this);

        switch (type)
        {
            case MissionType.Capture:
                solver = new CaptureMissionSolver(this);
                break;
            default:
                break;
        }
    }

    public void EnlistTroops()
    {
        foreach (var ally in Factory.ally)
        {
            if (factory != ally && ally.armyAvailable > 0)
            {
                Troop troop = planner.MakeMockTroop(ally, ally.armyAvailable);
            }
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
        SortedSet<Troop> mockTroops = mission.factory.incoming;
        List<Troop> finalEnlisted = new List<Troop>();
        Factory factory = mission.factory;

        foreach (var mockTestTroop in mission.acceptedMission)
        {
            mockTroops.Add(mockTestTroop);
            finalEnlisted.Add(mockTestTroop);

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
                mockTestTroop.count = Math.Max(1, mockTestTroop.count - armyAvailable + 1); //don't oversend

                foreach (Troop troop in finalEnlisted)
                {
                    Action newAction = new Action(MoveType.Move, troop.start, troop.end, troop.count);
                    Action.AddAction(newAction, troop);
                }
                return;
            }
        }
    }

}

interface MissionSolver
{
    void Solve();   
}

class MissionPlanner
{
    public Mission mission;

    public MissionPlanner(Mission mission)
    {
        this.mission = mission;
    }

    public Troop MakeMockTroop(Factory ally, int count)
    {
        Troop mockTroop = new Troop
        {
            start = ally,
            count = count,
            team = Team.Ally,
            isAlly = true,
            isEnemy = false,
        };

        BFS.UseBFS(mockTroop, mission.factory);

        int distance = ally.links[mockTroop.end];
        if (Bomb.bombs.Values.Any(b => b.isAlly && b.end == mission.factory && b.turns > distance))
        {
            return null;
        }

        mission.acceptedMission.Add(mockTroop); //ally factory accepts the mission

        return mockTroop;
    }
}

class Strategy
{
    public static List<Mission> missions = new List<Mission>();

    public static void AddMission()
    {
        var orderedFactories = Factory.factories
            .OrderBy(factory => 
                (factory.stats.allyCountWeighted - factory.stats.enemyCountWeighted) 
                / (factory.oldProduction+1));

        foreach (var factory in orderedFactories)
        {
            Mission newMission = new Mission(MissionType.Capture, factory);
            newMission.EnlistTroops();
            newMission.solver.Solve();
            missions.Add(newMission);

            double missionScore = (factory.stats.allyCountWeighted - factory.stats.enemyCountWeighted)
                / (factory.oldProduction + 1);

            Console.Error.WriteLine(factory.id + ": " + missionScore);
        }
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

    public static void InitializeTurn()
    {
        Factory.ProcessFactoryStates();
        Factory.ProcessNeighbourCounts();
        Factory.CalculateAllArmyScore();
    }

    public static void MakeMove()
    {
        Strategy.AddMission();
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
            //Console.Error.WriteLine("LoopTime: " + loopTime);
        }
    }
}