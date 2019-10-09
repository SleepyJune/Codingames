
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

    class Bike
    {
        public int speed;
        public int active;

        public int x;
        public int y;

        public Bike(int active, int speed, int x, int y)
        {
            this.speed = speed;
            this.active = active;

            this.x = x;
            this.y = y;
        }

        public Bike(Bike oldBike)
        {
            this.speed = oldBike.speed;
            this.active = oldBike.active;

            this.x = oldBike.x;
            this.y = oldBike.y;
        }
    }

    public enum MoveType
    {
        Speed,
        Jump,
        Up,
        Down,
        Slow,
        Wait,
    }

    class Pathfinding
    {
        public static char[][] road = new char[4][];
        public static List<Bike> cBikes = new List<Bike>();

        public static int bikeMustBeActive;

        public static string Start()
        {
            //List<Node> nodeList = new List<Node>();

            List<Bike> bikes = new List<Bike>();

            for (int i = 0; i < cBikes.Count; i++)
            {
                bikes.Add(cBikes[i]);
            }

            int sol = FindSolution(99, bikes, 0);
                
            if (sol >= 0)
            {
                return ((MoveType)sol).ToString().ToUpper();
            }
            else
            {
                return "WAIT";
            }
        }

        public static int FindSolution(int actionNum, List<Bike> bikes, int steps)
        {
            MoveType action = (MoveType)actionNum;

            if (actionNum != 99)
            {
                int numBikesActive = 0;
                bool goal = false;

                for (int i = 0; i < bikes.Count; i++)
                {
                    if (bikes[i].active == 0)
                    {
                        continue;
                    }

                    if (bikes[i].speed == 1 && action == MoveType.Slow)
                    {
                        return -1; //stopped
                    }

                    if (action == MoveType.Speed && bikes[i].speed < 50)
                    {
                        bikes[i].speed += 1;
                    }

                    if (action == MoveType.Slow && bikes[i].speed > 0)
                    {
                        bikes[i].speed -= 1;
                    }

                    if (action != MoveType.Jump)
                    {
                        for (int z = 0; z < 2; z++)
                        {
                            int y = z;

                            if (action == MoveType.Up)
                            {
                                y = -z;
                            }
                            else if (action == MoveType.Down)
                            {
                                y = z;
                            }

                            if (bikes[i].y + y >= 0 && bikes[i].y + y < road.Length)
                            {
                                bikes[i].y += y;
                            }
                            else if (z > 0)
                            {
                                return -1;//can't go up or down
                            }

                            for (int x = 1; x < bikes[i].speed; x++)
                            {
                                int xPos = bikes[i].x + x;
                                
                                if (road[0].Length > xPos && road[bikes[i].y][xPos] == '0')
                                {
                                    bikes[i].active = 0;
                                }
                            }

                            if (!(action == MoveType.Up || action == MoveType.Down))
                            {
                                break;
                            }
                        }
                    }

                    bikes[i].x += bikes[i].speed;

                    if (road[0].Length > bikes[i].x && road[bikes[i].y][bikes[i].x] == '0')
                    {
                        bikes[i].active = 0;
                    }

                    string spaces = new String(' ', steps-1);
                    Console.Error.WriteLine(spaces  + steps + "." + action.ToString() + ": " + bikes[i].x + ", " + bikes[i].y);

                    if (bikes[i].x >= road[0].Length)
                    {
                        goal = true;
                    }

                    /*if (steps > 6 && bikes[i].speed > 0)
                    {
                        goal = true;
                    }*/

                    numBikesActive += bikes[i].active;
                }


                if (numBikesActive < bikeMustBeActive)
                {
                    //Console.Error.WriteLine("Failed");
                    return -1;
                }
                else if (goal)
                {
                    return actionNum;
                }
            }

            foreach (var newAction in Enum.GetValues(typeof(MoveType)).Cast<MoveType>())
            {
                List<Bike> newBikes = new List<Bike>();
                for (int i = 0; i < bikes.Count; i++)
                {
                    if (bikes[i].active > 0)
                    {
                        Bike newBike = new Bike(bikes[i]);
                        newBikes.Add(newBike);
                    }
                }

                if (FindSolution((int)newAction, newBikes, steps + 1) >= 0)
                {
                    if (actionNum == 99)
                    {
                        return (int)newAction;
                    }
                    else
                    {
                        return actionNum;
                    }
                }
            }

            return -1; //did not find solution
        }
    }




}
