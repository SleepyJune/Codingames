using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
    class Program
    {
        static void Main(string[] args)
        {
            Game.InitializeFirstTurn();

            // game loop
            while (true)
            {
                float loopStartTime = Timer.TickCount;

                int myShipCount = int.Parse(Console.ReadLine()); // the number of remaining ships
                int entityCount = int.Parse(Console.ReadLine()); // the number of entities (e.g. ships, mines or cannonballs)
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

                //float loopTime = Timer.TickCount - loopStartTime;
                //Console.Error.WriteLine("LoopTime: " + loopTime);
            }
        }
    }
}
