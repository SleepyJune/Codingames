using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DonPanic2
{
    class Pathfinding
    {
        public static List<Elevator> elevators = new List<Elevator>();

        public static Dictionary<Node, Node> nodeTable = new Dictionary<Node, Node>();

        public static int numAdditionalElevators;

        public static int exitFloor;
        public static int exitPos;

        public static int numFloors;
        public static int floorWidth;

        public static List<Node> actionList = new List<Node>();

        public static bool calculatedPath = false;

        public static void GetPath(Node current)
        {
            if (!calculatedPath)
            {
                var goal = CalculateShortestPath(current);
                actionList = TracePath(current, goal);

                calculatedPath = true;
            }
            
            foreach (var action in actionList)
            {
                if (action.floor == current.floor && action.pos == current.pos)
                {
                    var nextAction = action.next;

                    if (nextAction == null || nextAction.isExecuted)
                    {
                        break;
                    }

                    nextAction.isExecuted = true;

                    Console.WriteLine(nextAction.moveType.ToString().ToUpper());

                    return;
                }
            }

            Console.WriteLine("Wait");
        }

        public static void InitializePaths()
        {
            InitializeNodes();
            InitializeNeighbours();
        }

        public static void InitializeNodes()
        {
            int floorElevatorsLeft = numAdditionalElevators;

            for (int y = 0; y <= numFloors; y++)
            {
                for (int x = 0; x <= floorWidth; x++)
                {
                    for (int isLeft = 0; isLeft < 2; isLeft++)
                    {
                        int dir = isLeft == 1 ? -1 : 1;

                        for (int elevatorsLeft = 0; elevatorsLeft <= floorElevatorsLeft; elevatorsLeft++)
                        {
                            Node newNode = new Node(y, x, dir, elevatorsLeft);
                            nodeTable.Add(newNode, newNode);
                        }
                    }
                }

                if (!elevators.Any(e => e.floor == y))
                {
                    floorElevatorsLeft -= 1; //if no elevator this floor, gotta create 1
                }                            
            }

            Console.Error.WriteLine("Num Nodes: " + nodeTable.Count);
        }

        public static void InitializeNeighbours()
        {
            foreach (var current in nodeTable.Values)
            {
                foreach(var moveType in Enum.GetValues(typeof(MoveType)).Cast<MoveType>())
                {
                    int dir = current.direction;
                    int isElevator = 0;
                    int elevatorsLeft = current.elevatorsLeft;

                    if (elevators.Any(e => e.floor == current.floor && e.pos == current.pos)) //this position is an elevator
                    {
                        isElevator = 1;

                        if (moveType != MoveType.Wait)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (moveType == MoveType.Block)
                        {
                            dir = -current.direction;
                        }
                        else if (moveType == MoveType.Elevator)
                        {
                            isElevator = 1;
                            elevatorsLeft -= 1;
                        }
                    }

                    int yPos = current.floor + isElevator;
                    int xPos = current.pos + dir * (isElevator == 1 ? 0 : 1);

                    Node realNode = Node.GetNode(yPos, xPos, dir, elevatorsLeft);

                    if (realNode != null)
                    {
                        current.neighbours.Add(realNode, moveType);
                    }
                }
            }
        }

        public static float DistanceToGoal(Node node)
        {
            return (float)(Math.Abs(node.floor - exitFloor) + Math.Abs(node.pos - exitPos));
        }

        public static Node CalculateShortestPath(Node dummyStart)
        {
            HashSet<Node> closedSet = new HashSet<Node>();
            HashSet<Node> openSet = new HashSet<Node>();

            foreach (var node in Pathfinding.nodeTable.Values)
            {
                node.gScore = 9999;
                node.fScore = 9999;
                node.parent = null;
            }

            Node startNode = Node.GetNode(dummyStart.floor, dummyStart.pos, dummyStart.direction, dummyStart.elevatorsLeft);
            startNode.gScore = 0;
            startNode.fScore = DistanceToGoal(startNode);
            startNode.parent = null;

            openSet.Add(startNode);

            Node current = null;
            int loops = 0;

            while (openSet.Count > 0)
            {
                current = openSet.OrderBy(n => n.fScore).FirstOrDefault();

                //current.Print();
                
                if (current.floor == exitFloor && current.pos == exitPos)
                {
                    break;
                }

                openSet.Remove(current);
                closedSet.Add(current);

                foreach (var pair in current.neighbours)
                {
                    var neighbour = pair.Key;
                    var move = pair.Value;

                    if (closedSet.Contains(neighbour))
                    {
                        continue;
                    }
                    
                    int cost = 1;

                    if (move == MoveType.Block || move == MoveType.Elevator)
                    {
                        cost = 4;
                    }

                    var alternativeDistance = current.gScore + cost;
                    if (!openSet.Contains(neighbour))
                    {
                        openSet.Add(neighbour);
                    }
                    else if (alternativeDistance >= neighbour.gScore)
                    {
                        continue;
                    }

                    neighbour.parent = current;
                    neighbour.gScore = alternativeDistance;
                    neighbour.fScore = alternativeDistance + DistanceToGoal(neighbour);
                    neighbour.moveType = move;
                }

                loops++;
            }

            Console.Error.WriteLine("Loops: " + loops);// + " Steps: " + step);

            return current;
        }

        public static List<Node> TracePath(Node start, Node goal)
        {
            if (goal == null)
            {
                Console.Error.WriteLine("Failed");
                return null;
            }

            List<Node> actions = new List<Node>();

            Node current = goal;
            actions.Add(current);

            while (!current.Equals(start) && current.parent != null)
            {
                current.parent.next = current;
                current = current.parent;
                actions.Add(current);
            }

            actions.Reverse();
            foreach (var node in actions)
            {
                node.Print();
            }

            return actions;
        }
    }
}
