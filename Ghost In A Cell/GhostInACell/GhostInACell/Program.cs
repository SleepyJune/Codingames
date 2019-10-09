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
}