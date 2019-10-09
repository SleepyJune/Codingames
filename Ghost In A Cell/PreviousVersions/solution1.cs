using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

public enum Team
{
    Ally,
    Enemy,
    Neutral,
}

class EntityMessage
{
    int id;
    string entityType;    
    int arg1;
    int arg2;
    int arg3;
    int arg4;
    int arg5;

    public void EntityMessage(int id, string entityType, int arg1, int arg2, int arg3, int arg4, int arg5)
    {
        this.id = id;
        this.entityType = entityType;
        this.args1 = args1;
        this.args2 = args2;
        this.args3 = args3;
        this.args4 = args4;
        this.args5 = args5;
    }
}

class Factory
{
    int team;
    int numRobots;
    int production;

    public void Factory()
    {
        team = Team.Neutral;
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
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");


            // Any valid action, such as "WAIT" or "MOVE source destination cyborgs"
            Console.WriteLine("WAIT");
        }
    }
}