using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
    class Pathfinding
    {
        public static Dictionary<PositionNode, PositionNode> nodeTable = new Dictionary<PositionNode, PositionNode>();

        public static void InitializeShortestPaths()
        {
            InitializeNodes();
            InitializeNeighbours();
        }

        public static void InitializeNodes()
        {
            for (int x = 0; x <= 22; x++)
            {
                for (int y = 0; y <= 20; y++)
                {
                    for (int speed = 0; speed <= 2; speed++)
                    {
                        for (int rotation = 0; rotation < Vector.directions.Count; rotation++)
                        {
                            Hex hexPos = new Hex(x, y);
                            Vector pos = hexPos.ConvertCube();

                            PositionNode newNode = new PositionNode(pos, rotation, speed);
                            nodeTable.Add(newNode, newNode);
                        }
                    }
                }
            }
        }

        public static void InitializeNeighbours()
        {
            List<MoveType> validMoves = new List<MoveType>
            {
                MoveType.Wait,
                MoveType.Left,
                MoveType.Right,
                MoveType.Faster,
                MoveType.Slower,
            };

            foreach (var current in nodeTable.Values)
            {
                foreach (MoveType move in validMoves)
                {
                    int speed = current.speed;
                    int rotation = current.rotation;

                    switch (move)
                    {
                        case MoveType.Faster:
                            speed += 1;
                            break;
                        case MoveType.Slower:
                            speed -= 1;
                            break;
                        case MoveType.Right:
                            rotation = (current.rotation + 5) % 6;
                            break;
                        case MoveType.Left:
                            rotation = (current.rotation + 1) % 6;
                            break;
                        default:
                            break;
                    }

                    if (speed > 2 || speed < 0 ||
                       (speed == 0 && move == MoveType.Wait))
                    {
                        continue;
                    }

                    Vector pos = current.pos + Vector.directions[current.rotation] * speed;
                    PositionNode newNode = new PositionNode(pos, rotation, speed);

                    if (pos.isInBound())
                    {
                        current.neighbours.Add(nodeTable[newNode], move);
                    }

                }
            }
        }

        public static PositionNode GetLongestPath(Ship ship, int radius, bool fleeing = false)
        {
            HashSet<PositionNode> set = new HashSet<PositionNode>();
            Queue<PositionNode> q = new Queue<PositionNode>();
            List<PositionNode> nextExpand = new List<PositionNode>();

            PositionNode shipNode = PositionNode.GetPositionNode(ship.pos, ship.rotation, ship.speed);
            //shipNode.distance = 0;
            shipNode.step = 0;
            shipNode.parent = null;

            set.Add(shipNode);
            q.Enqueue(shipNode);

            int step = 0;
            int loops = 0;

            PositionNode current = null;

            // bfs loop
            while (q.Count != 0)
            {
                current = q.Dequeue();

                foreach (var pair in current.neighbours)
                {
                    var neighbour = pair.Key;
                    var move = pair.Value;

                    int neighbourStep = step + 1;//current.step + 1;

                    neighbour.prevRotation = current.rotation;

                    if (!set.Contains(neighbour) && isPositionSafe(neighbour, ship, neighbourStep))
                    {
                        set.Add(neighbour);

                        neighbour.parent = current;
                        neighbour.moveType = move;
                        neighbour.step = neighbourStep;

                        nextExpand.Add(neighbour);
                    }
                }

                if (q.Count == 0 && nextExpand.Count > 0) //if bfs is done expanding current step and next expand has items
                {
                    if (step >= radius)
                    {
                        break;
                    }

                    nextExpand.ForEach(n => q.Enqueue(n)); //put all list items into queue
                    nextExpand.Clear();

                    step++; //add 1 more step to count the distance
                }

                loops++;
            }

            if (step >= radius && nextExpand.Count > 0)
            {
                PositionNode first = null;
                if (fleeing)
                {
                    first = nextExpand.OrderByDescending(n => CombinedDistanceFromEnemy(n.pos)).FirstOrDefault();
                }
                else
                {
                    first = nextExpand.OrderByDescending(n => n.pos.Distance(ship.pos)).FirstOrDefault();
                }

                return first;

            }

            return null;
        }

        public static double CombinedDistanceFromEnemy(Vector vec)
        {
            double combinedDistance = 0;

            foreach (var enemy in Ship.ships.Values)
            {
                if (enemy.isAlive && enemy.team == Team.Enemy)
                {
                    combinedDistance += enemy.nextPositionInfo.shipFront.Distance(vec);
                }
            }

            return combinedDistance;
        }

        public static PositionNode CalculateShortestPaths(Ship ship, Vector targetPosition)
        {
            float loopStartTime = Timer.TickCount;

            HashSet<PositionNode> closedSet = new HashSet<PositionNode>();
            HashSet<PositionNode> openSet = new HashSet<PositionNode>();

            foreach (var node in Pathfinding.nodeTable.Values)
            {
                node.gScore = 9999;
                node.fScore = 9999;
                node.parent = null;
                //prev.Add(stop, null);
            }

            List<PositionNode> nextExpand = new List<PositionNode>();

            PositionNode shipNode = PositionNode.GetPositionNode(ship.pos, ship.rotation, ship.speed);
            shipNode.gScore = 0;
            shipNode.fScore = shipNode.pos.Distance(targetPosition);
            shipNode.step = 0;
            shipNode.parent = null;

            openSet.Add(shipNode);

            PositionNode current = null;
            int step = 0;
            int loops = 0;

            while (openSet.Count > 0)
            {
                current = openSet.OrderBy(n => n.fScore).FirstOrDefault();

                if (current.pos == targetPosition)
                {
                    break;
                }

                openSet.Remove(current);
                closedSet.Add(current);
                                
                foreach (var pair in current.neighbours)
                {
                    var neighbour = pair.Key;
                    var move = pair.Value;
                    var distance = move == 0 ? 0 : 1;

                    distance += neighbour.speed;

                    var neighbourStep = current.step + 1;

                    if (closedSet.Contains(neighbour) || !isPositionSafe(neighbour, ship, neighbourStep))
                    {
                        continue;
                    }

                    var alternativeDistance = current.gScore + distance;
                    if (!openSet.Contains(neighbour))
                    {
                        //nextExpand.Add(neighbour);
                        openSet.Add(neighbour);
                    }
                    else if (alternativeDistance >= neighbour.gScore)
                    {
                        continue;
                    }

                    neighbour.parent = current;
                    neighbour.gScore = alternativeDistance;
                    neighbour.fScore = alternativeDistance + neighbour.pos.Distance(targetPosition);
                    neighbour.moveType = move;
                    neighbour.step = neighbourStep;
                }

                /*if (openSet.Count == 0 && nextExpand.Count > 0)
                {
                    nextExpand.ForEach(n => openSet.Add(n)); //put all list items into queue
                    nextExpand.Clear();

                    step++; //add 1 more step to count the distance
                    Console.Error.WriteLine("Loops: " + loops + " Steps: " + step);
                }*/

                loops++;
            }

            //float loopTime = Timer.TickCount - loopStartTime;
            //Console.Error.WriteLine("LoopTime: " + loopTime);
            Console.Error.WriteLine("Loops: " + loops + " Steps: " + step);

            return current;
        }

        public static bool isPositionSafe(PositionNode node, Ship ship, int step)
        {
            foreach (var cannon in ship.dodgeableCannons)
            {
                foreach (var check in node.checkList)
                {
                    if (cannon.turns == step && cannon.hexPos == check)
                    {
                        return false;
                    }
                }
            }

            foreach (var check in node.checkList)
            {
                if (Mine.minePos.Contains(check))
                {
                    return false;
                }

                if (ship.enemyShipCollisionCheck.Contains(check))
                {
                    return false;
                }
            }

            if (node.rotation != node.prevRotation)
            {
                if (Mine.minePos.Contains(node.extraCheckList[node.prevRotation]))
                {
                    return false;
                }

                if (ship.enemyShipCollisionCheck.Contains(node.extraCheckList[node.prevRotation]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
