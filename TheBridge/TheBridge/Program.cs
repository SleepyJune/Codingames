using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBridge
{
    class Program
    {
        static void Main(string[] args)
        {
            int M = int.Parse(Console.ReadLine()); // the amount of motorbikes to control
            Pathfinding.bikeMustBeActive = int.Parse(Console.ReadLine()); 
            // the minimum amount of motorbikes that must survive

            for (int i = 0; i < 4; i++)
            {
                string line = Console.ReadLine();
                Console.Error.WriteLine(line);

                Pathfinding.road[i] = new char[line.Length];

                for (int j = 0; j < line.Length; j++)
                {
                    Pathfinding.road[i][j] = line[j];
                }
            }

            // game loop
            while (true)
            {
                Pathfinding.cBikes = new List<Bike>();

                int S = int.Parse(Console.ReadLine()); // the motorbikes' speed
                for (int i = 0; i < M; i++)
                {
                    string[] inputs = Console.ReadLine().Split(' ');
                    int X = int.Parse(inputs[0]); // x coordinate of the motorbike
                    int Y = int.Parse(inputs[1]); // y coordinate of the motorbike
                    int A = int.Parse(inputs[2]); // indicates whether the motorbike is activated "1" or detroyed "0"

                    Bike newBike = new Bike(A, S, X, Y);
                    Pathfinding.cBikes.Add(newBike);

                    Console.Error.WriteLine("Bike: " + newBike.x + ", " + newBike.y);
                }

                Console.WriteLine(Pathfinding.Start());
            }
        }
    }
}
