using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace DonPanic2
{
    class Player
    {
        static void Main(string[] args)
        {
            string[] inputs;
            inputs = Console.ReadLine().Split(' ');

            Pathfinding.numFloors = int.Parse(inputs[0]); // number of floors
            Pathfinding.floorWidth = int.Parse(inputs[1]); // width of the area

            int nbRounds = int.Parse(inputs[2]); // maximum number of rounds

            Pathfinding.exitFloor = int.Parse(inputs[3]); // floor on which the exit is found
            Pathfinding.exitPos = int.Parse(inputs[4]); // position of the exit on its floor

            int nbTotalClones = int.Parse(inputs[5]); // number of generated clones
            Pathfinding.numAdditionalElevators = int.Parse(inputs[6]); // number of additional elevators that you can build
            int nbElevators = int.Parse(inputs[7]); // number of elevators

            for (int i = 0; i < nbElevators; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                int elevatorFloor = int.Parse(inputs[0]); // floor on which this elevator is found
                int elevatorPos = int.Parse(inputs[1]); // position of the elevator on its floor

                Elevator newElevator = new Elevator(elevatorFloor, elevatorPos);

                Pathfinding.elevators.Add(newElevator);
            }

            Console.Error.WriteLine("Floors : " + Pathfinding.exitFloor +
                                    " Elevators: " + Pathfinding.numAdditionalElevators +
                                    " ExitFloor: " + Pathfinding.exitFloor +
                                    " ExitPos: " + Pathfinding.exitPos);

            Pathfinding.InitializePaths();

            // game loop
            while (true)
            {
                inputs = Console.ReadLine().Split(' ');
                int cloneFloor = int.Parse(inputs[0]); // floor of the leading clone
                int clonePos = int.Parse(inputs[1]); // position of the leading clone on its floor
                string direction = inputs[2]; // direction of the leading clone: LEFT or RIGHT

                int dir = direction == "LEFT" ? -1 : 1;

                Console.Error.WriteLine("Floor: " + cloneFloor + " Pos: " + clonePos + " Dir: " + dir + " Elevators: " + Pathfinding.numAdditionalElevators);

                Node start = new Node(cloneFloor, clonePos, dir, Pathfinding.numAdditionalElevators);

                Pathfinding.GetPath(start);
            }
        }
    }
}