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

class Node
{

}

class BFS
{

}

class FactoryLink
{
    Factory factory1;
    Factory factory2;
    int distance;

    public FactoryLink(Factory factory1, Factory factory2, int distance)
    {
        this.factory1 = factory1;
        this.factory2 = factory2;
        this.distance = distance;
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

    public static List<FactoryState> CalculateFactoryState(Factory factory, List<Troop> troops, bool mockState = true)
    {
        List<FactoryState> states = new List<FactoryState>();

        FactoryState lastState
                = new FactoryState(factory, Game.gameTime);

        states.Add(lastState); //add atleast 1 state to the list

        var ordered = troops.OrderBy(troop => troop.endTime);
        foreach (var troop in ordered)
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

class Factory : Entity
{
    public static Dictionary<int, Factory> factories = new Dictionary<int, Factory>();
    public static Dictionary<int, Factory> ally = new Dictionary<int, Factory>();
    public static Dictionary<int, Factory> enemy = new Dictionary<int, Factory>();
    public static Dictionary<int, Factory> neutral = new Dictionary<int, Factory>();

    public static int enemyTotalProduction = 0;
    public static int allyTotalProduction = 0;

    public static int totalAllyCount = 0;
    public static int totalEnemyCount = 0;

    public static double totalEnemyCountWeighted = 0;
    public static double totalAllyCountWeighted = 0;

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
    public Dictionary<int, Troop> incoming; //incoming troops

    public int enemyCount; //enemy neighbour count
    public int allyCount;

    public double enemyCountWeighted;
    public double allyCountWeighted;

    public double missionPrority;

    public List<FactoryState> states;

    public Factory(int id)
    {
        this.id = id;

        SetTeam(Team.Neutral);

        count = 0;
        production = 0;

        links = new Dictionary<Factory, int>();
        incoming = new Dictionary<int, Troop>();
        states = new List<FactoryState>();

        enemyCount = 0;
        allyCount = 0;

        enemyCountWeighted = 0;
        allyCountWeighted = 0;

        disabledTurns = 0;
        oldProduction = 0;

        armyAvailable = 0;

        missionPrority = 0;
    }

    public static void CleanUp()
    {
        foreach (var factory in factories.Values)
        {
            factory.incoming = new Dictionary<int, Troop>();
            factory.states = new List<FactoryState>();

            factory.enemyCount = 0;
            factory.allyCount = 0;

            factory.enemyCountWeighted = 0;
            factory.allyCountWeighted = 0;

            factory.armyAvailable = 0;

            factory.missionPrority = 0;
        }

        Factory.enemyTotalProduction = 0;
        Factory.allyTotalProduction = 0;

        Factory.totalAllyCountWeighted = 0;
        Factory.totalEnemyCountWeighted = 0;

        Factory.totalAllyCount = 0;
        Factory.totalEnemyCount = 0;
    }

    public static void ProcessNeighbourCounts()
    {
        foreach (var factory in factories.Values)
        {
            if (factory.isAlly)
            {
                factory.missionPrority += 1;
            }

            foreach (var pair in factory.links)
            {
                Factory neighbour = pair.Key;
                double distance = pair.Value;

                if (neighbour.isAlly)
                {
                    factory.missionPrority += 1 / distance;
                }

                if (factory.isAlly)
                {
                    if (neighbour.isAlly)
                    {
                        factory.allyCount += neighbour.count;
                        factory.allyCountWeighted += neighbour.count / distance;
                        Factory.totalAllyCountWeighted += neighbour.count / distance;
                    }
                    else if (neighbour.team == Team.Enemy)
                    {
                        factory.enemyCount += neighbour.count;
                        factory.enemyCountWeighted += neighbour.count / distance;
                        Factory.totalEnemyCountWeighted += neighbour.count / distance;
                    }
                }
            }
        }
    }

    public static void ProcessFactoryStates()
    {
        foreach (var factory in factories.Values)
        {
            factory.states = FactoryState.CalculateFactoryState(factory, factory.incoming.Values.ToList(), false);
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
            Factory.allyTotalProduction += production;
            Factory.totalAllyCount += count;
        }
        else if (message.arg1 == -1)
        {
            SetTeam(Team.Enemy);
            Factory.enemyTotalProduction += production;
            Factory.totalEnemyCount += count;
        }
        else
        {
            SetTeam(Team.Neutral);
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
        if (factories.TryGetValue(id, out factory))
        {
            return factory;
        }
        else
        {
            factory = new Factory(id);
            factories.Add(factory.id, factory);
            return factory;
        }
    }

    public void SetTeam(Team team)
    {
        this.team = team;
        if (team == Team.Ally)
        {
            if (!ally.ContainsKey(id))
            {
                ally.Add(id, this);
            }
            enemy.Remove(id);
            neutral.Remove(id);

            isAlly = true;
            isEnemy = false;
        }

        if (team == Team.Enemy)
        {
            if (!enemy.ContainsKey(id))
            {
                enemy.Add(id, this);
            }
            ally.Remove(id);
            neutral.Remove(id);

            isAlly = false;
            isEnemy = true;
        }

        if (team == Team.Neutral)
        {
            if (!neutral.ContainsKey(id))
            {
                neutral.Add(id, this);
            }
            ally.Remove(id);
            enemy.Remove(id);

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
        troops.Add(troop.id, troop);
        bombs.Add(troop.id, troop);
        return troop;
    }

    public new static void CleanUp()
    {
        bombs = new Dictionary<int, Bomb>();
        enemyBombs = new Dictionary<int, Bomb>();

        oldBombs.RemoveAll(bomb => bomb.endTime >= Game.gameTime);
    }

    public void ProcessMessage(EntityMessage message)
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

            end.incoming.Add(id, this);
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
                end = Factory.ally.Values.First();
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

class Troop : Entity
{
    public static Dictionary<int, Troop> troops = new Dictionary<int, Troop>();
    public static Dictionary<int, Troop> ally = new Dictionary<int, Troop>();
    public static Dictionary<int, Troop> enemy = new Dictionary<int, Troop>();

    public static int idCount = 0;

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
        troops = new Dictionary<int, Troop>();
        ally = new Dictionary<int, Troop>();
        enemy = new Dictionary<int, Troop>();
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

        end.incoming.Add(id, this);
    }

    public void SetTeam(Team team)
    {
        this.team = team;
        if (team == Team.Ally)
        {
            ally.Add(id, this);
            isAlly = true;
            isEnemy = false;
        }

        if (team == Team.Enemy)
        {
            enemy.Add(id, this);
            isAlly = false;
            isEnemy = true;
        }
    }

    public static Troop GetTroopByID(int id)
    {
        Troop troop = new Troop();
        troops.Add(troop.id, troop);
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

public enum MissionType
{
    Defend,
    Capture,
    Bomb,
    Reinforce,
    Inc,
}

class Mission
{
    public MissionType type;
    public Factory factory;
    public int troopsNeeded;

    public HashSet<Troop> acceptedMission;

    public int maxTroopCount;

    public Mission(MissionType type, Factory factory, int troopsNeeded)
    {
        this.type = type;
        this.factory = factory;
        this.troopsNeeded = troopsNeeded;
        this.acceptedMission = new HashSet<Troop>();

        this.maxTroopCount = 0;
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
    }

    public static void EnlistTroopsForMission(Mission mission)
    {
        foreach (var pair in mission.factory.links.OrderBy(p => p.Key.enemyCountWeighted))
        {
            Factory ally = pair.Key;
            int distance = pair.Value;

            if (ally.isAlly && ally.count > 1 
                && (distance < 10 || mission.type == MissionType.Bomb) 
                && mission.factory != ally)
            {
                int robotsAvailable = ally.armyAvailable;//ally.count - robotsToDefend - 1;

                bool canIncProduction = ally.oldProduction < 3 && ally.armyAvailable > 10;

                //Console.Error.WriteLine(ally.id + ": " + robotsAvailable);

                if (robotsAvailable <= 0 ||
                    Bomb.bombs.Values.Any(b => b.isAlly && b.end == mission.factory && b.turns > distance))
                {
                    continue;
                }

                Troop mockTroop = new Troop
                {
                    start = ally,
                    end = mission.factory,
                    count = robotsAvailable,
                    team = Team.Ally,
                    isAlly = true,
                    isEnemy = false,
                    turns = distance + 1,
                    endTime = Game.gameTime + distance + 1,
                };

                mission.acceptedMission.Add(mockTroop); //ally factory accepts the mission
                mission.maxTroopCount += mockTroop.count;
            }
        }
    }

    public static bool GetCaptureProbability(Mission mission, int maxLeftOver, int turns)
    {
        int maxProtectionCount = 0;

        foreach (var pair in mission.factory.links)
        {
            Factory neighbour = pair.Key;
            int distance = pair.Value + 1;

            if (neighbour.isEnemy && distance <= turns)
            {
                int idleTurns = turns - distance; //num spare turns

                FactoryState state = FactoryState.GetFactoryState(neighbour, Game.gameTime + idleTurns);

                if (state.isEnemy && state.team != Team.Neutral)
                {
                    maxProtectionCount += state.count;
                }
            }
        }

        return maxLeftOver > maxProtectionCount;
    }

    public static void SolveCaptureMission(Mission mission)
    {
        EnlistTroopsForMission(mission);

        List<Troop> mockTestTroops = mission.acceptedMission.ToList();

        List<Troop> newEnlisted = new List<Troop>();
        bool isMissionPossible = false;

        Factory factory = mission.factory;

        foreach (var mockTestTroop in mockTestTroops.OrderBy(troop => troop.endTime))
        {
            newEnlisted.Add(mockTestTroop);

            FactoryState lastState
                    = new FactoryState(factory, Game.gameTime);

            List<Troop> mockTroops = mission.factory.incoming.Values.ToList();
            mockTroops.AddRange(newEnlisted);

            bool allyLock = factory.isAlly;
            int armyAvailable = allyLock ? factory.count : 0;

            var ordered = mockTroops.OrderBy(troop => troop.endTime).ToList();

            //iterate through the mock states
            //ordered by distance
            for (int i = 0; i < ordered.Count; i++)
            {
                Troop troop = ordered[i];
                FactoryState newState = new FactoryState(lastState, troop);

                if (i + 1 < ordered.Count && troop.endTime == ordered[i + 1].endTime)
                {
                    lastState = newState;
                    continue;
                }

                if (allyLock && newState.isEnemy)
                {
                    isMissionPossible = false;
                    allyLock = false;
                    break;
                }

                if (allyLock == false && newState.isAlly)
                {
                    allyLock = true;
                    isMissionPossible = true;
                    armyAvailable = newState.count;

                    //Console.Error.WriteLine(newState.id + ": " + armyAvailable);
                }

                if (factory.isAlly && troop == mockTestTroop)
                {
                    armyAvailable = newState.count;
                }

                //!Bomb.bombs.Values.Any(b => b.isAlly && b.end == mission.factory && b.turns > troop.turns)

                if (troop == mockTestTroop
                    && GetCaptureProbability(mission, newState.count, mockTestTroop.turns) == false)
                {
                    isMissionPossible = false;
                    allyLock = false;
                    break;
                }

                armyAvailable = Math.Min(armyAvailable, newState.count);
                lastState = newState;
            }

            if (allyLock)// isMissionPossible)
            {
                mockTestTroop.count = Math.Max(1, mockTestTroop.count - armyAvailable + 1); //don't oversend

                foreach (Troop troop in newEnlisted)
                {
                    Action newAction = new Action(MoveType.Move, troop.start, troop.end, troop.count);
                    AddAction(newAction, troop);
                }
                break;
            }
        }


    }

    public static void SolveReinforcementMission(Mission mission)
    {
        foreach (var pair in mission.factory.links.OrderBy(pair => pair.Key.count))
        {
            Factory ally = pair.Key;
            int distance = pair.Value;

            if (ally.isAlly && ally.count > 1 && distance < 10)
            {
                int robotsAvailable = ally.armyAvailable;

                if (ally.oldProduction < 3) //need to save up to increase production
                {
                    robotsAvailable -= 10;
                }

                if (robotsAvailable <= 0) //check if we have enough robots to send
                {
                    continue;
                }

                double enemyCountPercent = ally.enemyCountWeighted / Factory.totalEnemyCountWeighted;
                int numRobots = (int)(Factory.totalAllyCount * enemyCountPercent);
                int diff = ally.count - numRobots;

                if (diff > 0)
                {
                    int robotsToSend = Math.Min(robotsAvailable, diff);
                    mission.troopsNeeded -= robotsToSend;

                    Action newAction = new Action(MoveType.Move, ally, mission.factory, robotsToSend);
                    AddAction(newAction);

                    if (mission.troopsNeeded <= 0)
                    {
                        break;
                    }
                }
            }
        }
    }

    public static void MakeMove()
    {
        List<Mission> missions = new List<Mission>();

        missions.AddRange(SendBomb());
        missions.AddRange(CaptureFactories());

        //missions.AddRange(IncreaseAllyFactories());
        //missions.AddRange(SendReinforcements());

        foreach (var mission in missions)
        {
            if (mission.type == MissionType.Defend || mission.type == MissionType.Capture)
            {
                SolveCaptureMission(mission);
            }
            else if (mission.type == MissionType.Inc)
            {
                SolveIncreaseMission(mission);
            }
            else if (mission.type == MissionType.Bomb)
            {
                SolveBombMission(mission);
            }
            else if (mission.type == MissionType.Reinforce)
            {
                SolveReinforcementMission(mission);
            }
        }
    }

    public static void SolveIncreaseMission(Mission mission)
    {
        Factory ally = mission.factory;

        List<Troop> mockTroops = new List<Troop>();
        mockTroops.Add(new Troop
        {
            start = ally,
            end = ally,
            count = 10,
            team = Team.Enemy,
            isAlly = false,
            isEnemy = true,
            turns = 1,
            endTime = Game.gameTime + 1,
        });

        mockTroops.AddRange(ally.incoming.Values.ToList());
        var mockStates = FactoryState.CalculateFactoryState(ally, mockTroops);
        //FactoryState lastState = mockStates.Last();

        if (FactoryState.IsFactoryCaptured(mockStates) == false)
        {

            Action newAction = new Action(MoveType.Inc, mission.factory);
            AddAction(newAction);
        }
    }

    public static void SolveBombMission(Mission mission)
    {
        foreach (var pair in mission.factory.links.OrderBy(pair => pair.Value))
        {
            Factory ally = pair.Key;
            int distance = pair.Value;

            if (ally.isAlly && mission.troopsNeeded - Game.gameTime < distance)
            {
                Bomb bomb = new Bomb
                {
                    isAlly = true,
                    isEnemy = false,
                    team = Team.Ally,
                    start = ally,
                    end = mission.factory,
                    turns = distance + 1,
                    endTime = Game.gameTime + distance + 1,
                };

                Action newAction = new Action(MoveType.Bomb, ally, mission.factory, 1);
                AddAction(newAction, bomb);
                break;
            }
        }
    }

    public static List<Mission> SendBomb()
    {
        List<Mission> possibleMissions = new List<Mission>();

        foreach (var bomb in Bomb.bombs.Values)
        {
            if (bomb.isAlly)
            {
                bool troopSent = false;

                foreach (var troop in Troop.ally.Values)
                {
                    if (bomb.end == troop.end && bomb.turns+1 == troop.turns)
                    {
                        troopSent = true;
                        break;
                    }
                }

                if (troopSent == false)
                {
                    Mission bombMission = new Mission(MissionType.Bomb, bomb.end, 1);
                    EnlistTroopsForMission(bombMission);
                    
                    if (bombMission.acceptedMission.Count > 0)
                    {
                        Troop troop = bombMission.acceptedMission.First();
                        
                        Action newAction = new Action(MoveType.Move, troop.start, troop.end, 1);
                        AddAction(newAction, troop);
                    }
                }
            }
        }

        if (Factory.totalEnemyCount == 0 || Game.bomb <= 0)
        {
            return possibleMissions;
        }

        List<Factory> combinedList = Factory.enemy.Values.ToList();
        combinedList.AddRange(Factory.neutral.Values);

        foreach (var factory in combinedList.OrderByDescending(f => f.missionPrority))
        {
            if (factory.production == 0)
            {
                continue;
            }

            FactoryState lastState = factory.states.Last();
            if (lastState.team == Team.Enemy)
            {
                //might send too early
                if (lastState.count >= Factory.totalEnemyCount / 2
                    && lastState.count > 10)
                {
                    bool alreadySent = false;
                    foreach (var bomb in Bomb.bombs.Values)
                    {
                        if (bomb.team == Team.Ally)
                        {
                            if (bomb.end == factory)
                            {
                                alreadySent = true;
                                break;
                            }
                        }
                    }

                    if (alreadySent == false)
                    {
                        possibleMissions.Add(new Mission(MissionType.Bomb, factory, lastState.gameTime));
                    }
                }

            }
        }

        return possibleMissions;
    }

    public static List<Mission> SendReinforcements()
    {
        List<Mission> possibleMissions = new List<Mission>();

        if (Factory.totalEnemyCount == 0)
        {
            return possibleMissions;
        }

        foreach (var ally in Factory.ally.Values)
        {
            double enemyCountPercent = ally.enemyCountWeighted / Factory.totalEnemyCountWeighted;
            int numRobots = (int)(Factory.totalAllyCount * enemyCountPercent);
            int diff = numRobots - ally.armyAvailable;//ally.count;

            if (ally.production < 3)
            {
                diff += 11; //account for increase production
            }

            if (diff > 0)
            {
                possibleMissions.Add(new Mission(MissionType.Reinforce, ally, diff));
            }
        }

        return possibleMissions.OrderByDescending(mission => mission.troopsNeeded).ToList();
    }

    public static List<Mission> CaptureFactories()
    {
        List<Mission> possibleMissions = new List<Mission>();

        var factoryPrority = Factory.factories.Values
            .OrderByDescending(f => f.missionPrority * (f.oldProduction + 1));

        bool setCaptureMission = false;

        //Defend all ally, and ally to be factories
        foreach (var factory in factoryPrority)
        {
            if (factory.oldProduction == 0
                && factory.isAlly == false
                && factory.count + 1 > .025 * Factory.totalAllyCount)
            {
                continue;
            }

            //Console.Error.WriteLine(factory.id + ": " + factory.states.Last().count);

            if (FactoryState.WillFactoryBeAlly(factory.states)) //nothing to do here
            {
                if (factory.isAlly && factory.count > 10 && factory.oldProduction < 3)
                {
                    possibleMissions.Add(new Mission(MissionType.Inc, factory, 0));
                }
                else
                {
                    double enemyCountPercent = factory.enemyCountWeighted / Factory.totalEnemyCountWeighted;
                    int numRobots = (int)(Factory.totalAllyCount * enemyCountPercent);
                    int diff = numRobots - factory.armyAvailable;//ally.count;

                    if (factory.production < 3)
                    {
                        diff += 11; //account for increase production
                    }

                    if (diff > 0)
                    {
                        possibleMissions.Add(new Mission(MissionType.Reinforce, factory, diff));
                    }
                }
            }
            else
            {
                if (setCaptureMission == false || factory.team == Team.Neutral)
                {
                    possibleMissions.Add(new Mission(MissionType.Capture, factory, 1));
                    setCaptureMission = true;
                }
            }
        }

        return possibleMissions;
    }

    public static List<Mission> CaptureNeutralFactories()
    {
        List<Mission> possibleMissions = new List<Mission>();

        foreach (var factory in Factory.neutral.Values.OrderByDescending(f => f.production))
        {
            if (factory.production == 0 && Factory.totalEnemyCount != 0)
            {
                continue;
            }

            FactoryState lastState = factory.states.Last();
            if (lastState.team != Team.Ally) //if neutral factory doesn't belong to ally in the last state
            {
                possibleMissions.Add(new Mission(MissionType.Capture, factory, lastState.count + 1));
            }
        }

        return possibleMissions;
    }

    public static List<Mission> IncreaseAllyFactories()
    {
        List<Mission> possibleMissions = new List<Mission>();

        foreach (var ally in Factory.ally.Values.OrderBy(f => f.production))
        {
            if (ally.oldProduction > 2)
            {
                continue;
            }

            if (ally.count > 10)
            {
                possibleMissions.Add(new Mission(MissionType.Inc, ally, 0));
            }
        }

        return possibleMissions;
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

        gameTime++;
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
            action.end.incoming.Add(troop.id, troop);
            Game.bomb--;
        }

        Action.actions.Add(action);
    }

    private static DateTime loadTime = DateTime.Now;

    public static float TickCount
    {
        get
        {
            return (int)DateTime.Now.Subtract(loadTime).TotalMilliseconds;
        }
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
            float loopStartTime = Game.TickCount;

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

            float loopTime = Game.TickCount - loopStartTime;
            //Console.Error.WriteLine("LoopTime: " + loopTime);
        }
    }
}