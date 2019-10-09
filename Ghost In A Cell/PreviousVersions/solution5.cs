using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public enum Team
{
    Ally,
    Enemy,
    Neutral,
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
            Factory factory = Factory.GetFactoryByID(id);
            factory.ProcessMessage(this);
        }

        if (entityType == "TROOP")
        {
            Troop troop = Troop.GetTroopByID(id);
            troop.ProcessMessage(this);
        }
    }
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

    public int gameTime = 0;

    public FactoryState(int id, Team team, int count, int production, int gameTurns)
    {
        this.id = id;
        SetTeam(team);      
        
        this.count = count;
        this.production = production;
        this.gameTime = gameTurns;
    }

    public FactoryState(FactoryState factory, Troop troop)
    {
        this.id = factory.id;        
        this.count = factory.count;       
        this.production = factory.production;
        this.gameTime = troop.endTime;

        if (factory.team != Team.Neutral)
        {
            this.count += troop.turns * production;
        }

        this.count += (troop.team == factory.team) ? troop.count : -troop.count;

        if (this.count <= 0) //if the troop captured the factory
        {
            this.count *= -1;
            SetTeam(troop.team); //team become the troops team
        }
        else
        {
            SetTeam(factory.team); //team stays the same
        }
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

class Factory
{
    public static Dictionary<int, Factory> factories = new Dictionary<int, Factory>();
    public static Dictionary<int, Factory> ally = new Dictionary<int, Factory>();
    public static Dictionary<int, Factory> enemy = new Dictionary<int, Factory>();
    public static Dictionary<int, Factory> neutral = new Dictionary<int, Factory>();

    public int id;

    public Team team;
    public bool isAlly;
    public bool isEnemy;

    public int count;
    public int production;

    public Dictionary<Factory, int> links; //links with factory and distance
    public Dictionary<int, Troop> incoming; //incoming troops

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
    }

    public static void CleanUp()
    {
        foreach (var factory in factories.Values)
        {
            factory.incoming = new Dictionary<int, Troop>();
            factory.states = new List<FactoryState>();
        }
    }

    public static void ProcessFactoryStates()
    {
        foreach (var factory in factories.Values)
        {
            FactoryState lastState 
                = new FactoryState(factory.id, factory.team, factory.count, factory.production, Game.gameTime);

            factory.states.Add(lastState); //add atleast 1 state to the list

            var ordered = factory.incoming.Values.OrderBy(troop => troop.endTime);
            foreach (var troop in ordered)
            {
                FactoryState newState = new FactoryState(lastState, troop);
                factory.states.Add(newState);
                lastState = newState;
            }
        }
    }

    public void ProcessMessage(EntityMessage message)
    {
        if (message.arg1 == 1)
        {
            SetTeam(Team.Ally);
        }
        else if (message.arg1 == -1)
        {
            SetTeam(Team.Enemy);
        }
        else
        {
            SetTeam(Team.Neutral);
        }

        count = message.arg2;
        production = message.arg3;
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

class Troop
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
    Inc,
}

class Mission
{
    public MissionType type;
    public Factory factory;
    public int troopsNeeded;

    public Mission(MissionType type, Factory factory, int troopsNeeded)
    {
        this.type = type;
        this.factory = factory;
        this.troopsNeeded = troopsNeeded;
    }
}

class Game
{
    public static int gameTime = 0;

    public static void InitializeTurn()
    {
        Factory.ProcessFactoryStates();
    }

    public static void MakeMove2()
    {
        List<Mission> missions = new List<Mission>();

        missions.AddRange(DefendAllyFactories());
        missions.AddRange(CaptureNeutralFactories());
        missions.AddRange(CaptureEnemyFactories());

        foreach (var mission in missions)
        {           
            if (mission.type == MissionType.Defend || mission.type == MissionType.Capture)
            {
                foreach (var ally in mission.factory.links.Keys)
                {
                    if (ally.isAlly && ally.count > 1)
                    {
                        int robotsToSend = Math.Max(mission.troopsNeeded, ally.count - 1);

                        Action newAction = new Action(MoveType.Move, ally, mission.factory, robotsToSend);
                        AddAction(newAction);

                        mission.troopsNeeded -= robotsToSend;

                        if (mission.troopsNeeded <= 0)
                        {
                            continue;
                        }
                    }
                }
            }
        }
    }

    public static int GetLongestLink(Factory factory)
    {
        return 0;
    }

    public static List<Mission> DefendAllyFactories()
    {
        List<Mission> possibleMissions = new List<Mission>();

        foreach (var ally in Factory.ally.Values)
        {
            if (ally.production == 0)
            {
                continue;
            }

            FactoryState lastState = ally.states.Last();
            if (lastState.team != ally.team)
            {
                possibleMissions.Add(new Mission(MissionType.Defend, ally, lastState.count));
            }
        }

        return possibleMissions;
    }

    public static List<Mission> CaptureNeutralFactories()
    {
        List<Mission> possibleMissions = new List<Mission>();

        foreach (var factory in Factory.neutral.Values)
        {
            if (factory.production == 0)
            {
                continue;
            }

            FactoryState lastState = factory.states.Last();
            if (lastState.team != Team.Ally) //if neutral factory doesn't belong to ally in the last state
            {
                possibleMissions.Add(new Mission(MissionType.Capture, factory, lastState.count));
            }
        }

        return possibleMissions;
    }

    public static List<Mission> CaptureEnemyFactories()
    {
        List<Mission> possibleMissions = new List<Mission>();

        foreach (var factory in Factory.enemy.Values)
        {
            if (factory.production == 0)
            {
                continue;
            }

            FactoryState lastState = factory.states.Last();
            if (lastState.team != Team.Ally) //if neutral factory doesn't belong to ally in the last state
            {
                possibleMissions.Add(new Mission(MissionType.Capture, factory, lastState.count));
            }
        }

        return possibleMissions;
    }

    public static void MakeMove()
    {
        List<Action> possibleActions;

        //get highest production neutral factory
        possibleActions = FindNeutralFactories();
        if (possibleActions.Count > 0)
        {
            var bestAction = possibleActions.OrderByDescending(a => a.end.production).FirstOrDefault();

            AddAction(bestAction);
            return;
        }

        //get highest production ally factory
        possibleActions = FindAllyFactories();
        if (possibleActions.Count > 0)
        {
            var bestAction = possibleActions.OrderByDescending(a => a.end.production).FirstOrDefault();

            AddAction(bestAction);
            return;
        }

        //get lowest enemy factory
        possibleActions = FindEnemyFactories();
        if (possibleActions.Count > 0)
        {
            var bestAction = possibleActions.OrderByDescending(a => a.end.production).FirstOrDefault();

            AddAction(bestAction);
            return;
        }

        //get factory to increase production
        possibleActions = FindIncreaseProduction();
        if (possibleActions.Count > 0)
        {
            var bestAction = possibleActions.OrderBy(a => a.start.production).FirstOrDefault();

            AddAction(bestAction);
            return;
        }

        AddAction(new Action()); //wait        
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

        gameTime++;
    }

    public static void AddAction(Action action)
    {
        Action.actions.Add(action);

        if (action.move == MoveType.Move)
        {
            action.start.count -= action.numRobots;
        }
    }

    public static List<Action> FindIncreaseProduction()
    {
        List<Action> possibleActions = new List<Action>();

        foreach (var ally in Factory.ally.Values)
        {
            if (ally.production < 3 && ally.count > 10 && GetIncomingTroops(ally) <= 0)
            {
                possibleActions.Add(new Action(MoveType.Inc, ally));
            }
        }

        return possibleActions;
    }

    public static List<Troop> FindAllyTroops(Factory factory)
    {
        return factory.incoming.Values.Where(t => t.isAlly).ToList();
    }

    public static int GetIncomingTroops(Factory factory)
    {
        int ally = 0;
        int enemy = 0;

        foreach (var troop in factory.incoming.Values)
        {
            if (troop.isAlly)
            {
                ally += troop.count;
            }
            else
            {
                enemy += troop.count;
            }
        }

        return enemy - ally;
    }

    public static int GetRobotsToSend(Factory start, Factory end)
    {
        int num = end.isAlly ? -end.count - 1 : end.count + 1;

        int distance = start.links[end] + 1;

        int produced = distance * end.production;
        int troopsProduced = end.isAlly ? -produced : produced;
        num += troopsProduced; //account for troops produced while traveling

        num += GetIncomingTroops(end); //account for incoming troops;

        return num;
    }

    public static int GetTroopCount(Factory factory, int turns)
    {
        int num = factory.isAlly ? -factory.count - 1 : factory.count + 1;

        int distance = turns + 1;

        int produced = distance * factory.production;
        int troopsProduced = factory.isAlly ? -produced : produced;
        num += troopsProduced; //account for troops produced while traveling

        num += GetIncomingTroops(factory); //account for incoming troops;

        return num;
    }    

    public static List<Action> FindAllyFactories()
    {
        List<Action> possibleActions = new List<Action>();

        foreach (var ally in Factory.ally.Values)
        {
            if (ally.production == 0)
            {
                continue;
            }

            foreach (var ally2 in ally.links.Keys)
            {
                if (ally2.isAlly)
                {
                    int robotsToSend = GetRobotsToSend(ally2, ally);

                    //Console.Error.WriteLine(ally.id + ": " + robotsToSend);

                    if (robotsToSend > 0 && ally.count > robotsToSend)
                    {
                        possibleActions.Add(new Action(MoveType.Move, ally2, ally, robotsToSend));
                    }
                }
            }
        }

        return possibleActions;
    }

    public static List<Action> FindEnemyFactories()
    {
        List<Action> possibleActions = new List<Action>();

        foreach (var enemy in Factory.enemy.Values)
        {
            if (enemy.production == 0)
            {
                continue;
            }

            foreach (var ally in enemy.links.Keys)
            {
                if (ally.isAlly) //factory is reachable by one of the ally
                {
                    int robotsToSend = GetRobotsToSend(ally, enemy);

                    List<Troop> allyTroops = FindAllyTroops(enemy);

                    if (robotsToSend <= 0)
                    {
                        break;
                    }

                    if (ally.count - GetIncomingTroops(ally) > robotsToSend) //check if we have enough robots
                    {
                        possibleActions.Add(new Action(MoveType.Move, ally, enemy, robotsToSend));
                    }
                }
            }
        }

        return possibleActions;
    }

    public static List<Action> FindNeutralFactories()
    {
        List<Action> possibleActions = new List<Action>();

        foreach (var neutral in Factory.neutral.Values)
        {
            if (neutral.production == 0)
            {
                continue;
            }

            foreach (var ally in neutral.links.Keys)
            {
                if (ally.team == Team.Ally) //factory is reachable by one of the ally
                {
                    int robotsToSend = neutral.count + 1;

                    List<Troop> allyTroops = FindAllyTroops(neutral);

                    if (robotsToSend + GetIncomingTroops(neutral) <= 0)
                    {
                        break;
                    }

                    if (ally.count > robotsToSend) //check if we have enough robots
                    {
                        possibleActions.Add(new Action(MoveType.Move, ally, neutral, neutral.count + 1));
                    }
                }
            }
        }

        return possibleActions;
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
            int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. factories and troops)
            for (int i = 0; i < entityCount; i++)
            {
                var str = Console.ReadLine();

                EntityMessage message = new EntityMessage(str);
                message.ProcessMessage();
            }

            Game.InitializeTurn();
            Game.MakeMove2();
            Game.PrintActions(); //Print the move
            Game.CleanUp();
        }
    }
}