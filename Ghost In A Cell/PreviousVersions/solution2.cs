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

    public EntityMessage(int id, string entityType, int arg1, int arg2, int arg3, int arg4, int arg5)
    {
        this.id = id;
        this.entityType = entityType;
        this.arg1 = arg1;
        this.arg2 = arg2;
        this.arg3 = arg3;
        this.arg4 = arg4;
        this.arg5 = arg5;
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

class Factory
{
    public static Dictionary<int, Factory> factories = new Dictionary<int, Factory>();

    public int id;

    public Team team;
    public int numRobots;
    public int production;

    Dictionary<Factory, int> links;

    public Factory(int id)
    {
        this.id = id;

        team = Team.Neutral;
        numRobots = 0;
        production = 0;

        links = new Dictionary<Factory, int>();
    }

    public void ProcessMessage(EntityMessage message)
    {
        if (message.arg1 == 1)
        {
            team = Team.Ally;
        }
        else if (message.arg1 == -1)
        {
            team = Team.Enemy;
        }
        else
        {
            team = Team.Neutral;
        }

        numRobots = message.arg2;
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
            return new Factory(id);
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

    int id;

    Team team;
    Factory start;
    Factory end;
    int numRobots;
    int turns;

    public Troop(int id)
    {
        team = Team.Neutral;
        numRobots = 0;
        turns = 0;
    }

    public void ProcessMessage(EntityMessage message)
    {
        team = message.arg1 == 1 ? Team.Ally : Team.Enemy;
        start = Factory.GetFactoryByID(message.arg2);
        end = Factory.GetFactoryByID(message.arg3);
        numRobots = message.arg4;
        turns = message.arg5;
    }

    public static Troop GetTroopByID(int id)
    {
        Troop troop;
        if (troops.TryGetValue(id, out troop))
        {
            return troop;
        }
        else
        {
            return new Troop(id);
        }
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
                inputs = Console.ReadLine().Split(' ');
                int entityId = int.Parse(inputs[0]);
                string entityType = inputs[1];
                int arg1 = int.Parse(inputs[2]);
                int arg2 = int.Parse(inputs[3]);
                int arg3 = int.Parse(inputs[4]);
                int arg4 = int.Parse(inputs[5]);
                int arg5 = int.Parse(inputs[6]);

                EntityMessage message = new EntityMessage(entityId, entityType, arg1, arg2, arg3, arg4, arg5);
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // Any valid action, such as "WAIT" or "MOVE source destination cyborgs"
            Console.WriteLine("WAIT");
        }
    }
}