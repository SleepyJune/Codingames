
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

        public static PositionNode CalculateShortestPaths2(Ship ship, Vector targetPosition)
        {
            float loopStartTime = Timer.TickCount;
            float loopTime = 0;

            float timeAvailable = 40 / Ship.numAllyShips;

            if (Game.timeLeft <= 0)
            {
                Console.Error.WriteLine("Not enough time left.");
                return null;
            }

            if (!targetPosition.isPositionReachable())
            {
                Console.Error.WriteLine("Impossible position: " + targetPosition.toStr());
                return null;
            }

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

                if (loops % 100 == 0)
                {
                    loopTime = Timer.TickCount - loopStartTime;
                    if (loopTime > timeAvailable)
                    {
                        Console.Error.WriteLine("Loops: " + loops + " Steps: " + step);
                        Console.Error.WriteLine("TimeOut: " + loopTime);
                        Game.timeLeft -= (int)loopTime;
                        return null;
                    }
                }

                if (current.pos == targetPosition)
                {
                    break;
                }

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
                    nextExpand.ForEach(n => q.Enqueue(n)); //put all list items into queue
                    nextExpand.Clear();

                    step++; //add 1 more step to count the distance
                }

                loops++;
            }

            loopTime = Timer.TickCount - loopStartTime;
            Console.Error.WriteLine("LoopTime: " + loopTime);
            Console.Error.WriteLine("Loops: " + loops + " Steps: " + step);

            Game.timeLeft -= (int)loopTime;

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

    class PositionNode : IEquatable<PositionNode>
    {
        public Vector pos;
        public Hex hexPos;
        public int rotation;
        public int prevRotation;
        public int speed;
        
        public int step = 0;
        //public int distance = 999;

        public MoveType moveType;

        public List<Hex> checkList;
        public Dictionary<int, Hex> extraCheckList = new Dictionary<int, Hex>();

        public Dictionary<PositionNode, MoveType> neighbours = new Dictionary<PositionNode, MoveType>();

        public PositionNode parent;
        public double gScore = 9999;
        public double fScore = 9999;

        public PositionNode(Vector pos, int rotation, int speed)
        {
            this.pos = pos;
            this.rotation = rotation;
            this.speed = speed;

            this.hexPos = pos.ConvertHex();
            //this.prevRotation = prevRotation;

            this.checkList = new List<Hex>
            {
                (this.pos + Vector.directions[this.rotation]).ConvertHex(),
                (this.pos).ConvertHex(),
                (this.pos - Vector.directions[this.rotation]).ConvertHex(),
            };

            for (int i = 0; i < 6; i++)
            {
                Hex check = (this.pos + Vector.directions[i]).ConvertHex();
                extraCheckList.Add(i, check);                
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is PositionNode)
            {
                return Equals((PositionNode)this);
            }

            return false;
        }

        public bool Equals(PositionNode node)
        {
            return 
                    this.pos == node.pos
                 && this.rotation == node.rotation
                 && this.speed == node.speed;
        }

        public override int GetHashCode()
        {
            return this.speed    * 100 * 100 * 10 + 
                   this.rotation * 100 * 100 + 
                   this.pos.GetHashCode();
        }

        public static PositionNode GetPositionNode(Vector pos, int rotation, int speed)
        {
            if (pos.isInBound())
            {
                PositionNode node = new PositionNode(pos, rotation, speed);
                return Pathfinding.nodeTable[node];
            }
            else
            {
                return null;
            }
        }
    }

    class Game
    {
        public static int timeLeft = 0;
        public static int gameTurn = 0;

        public static void InitializeFirstTurn()
        {
            float loopStartTime = Timer.TickCount;

            Pathfinding.InitializeShortestPaths();

            float loopTime = Timer.TickCount - loopStartTime;
            Console.Error.WriteLine("Initialization Time: " + loopTime);
        }

        public static void InitializeTurn()
        {
            timeLeft = gameTurn == 0 ? 1000 : 50;
            Cannon.GetExplodingMines();
            Ship.GetAllyShipPaths();
        }       

        public static void MakeMove()
        {            
            Strategy.MakeMove();
        }

        public static void PrintActions()
        {
            Action.PrintActions();
        }

        public static void CleanUp()
        {
            Ship.CleanUp();
            Barrel.CleanUp();
            Action.CleanUp();
            Cannon.CleanUp();
            Mine.CleanUp();

            gameTurn++;
        }
    }

    static class ShipMovement
    {
        public static Action Dodge(this Ship ship, Cannon cannon)
        {
            Action action = null;

            if (cannon.turns == 1)
            {
                if (ship.speed == 0 && ship.pos == cannon.pos)
                {
                    Vector pos = ship.pos + Vector.directions[ship.rotation] * (ship.speed + 1);

                    if (pos.isInBound())
                    {
                        action = new Action(MoveType.Faster, ship);
                    }
                }
                else if (ship.speed > 0 && ship.nextPositionInfo.pos == cannon.pos)
                {
                    action = new Action(MoveType.Slower, ship);
                }
                else if (ship.nextPositionInfo.shipBack == cannon.pos)
                {

                }
            }

            return action;
        }

        public static Action MoveShip(this Ship ship, Vector targetPosition)
        {
            Action action = null;

            var path = ship.GetNextSteps(targetPosition);
            if (path != null && path.Count > 1)
            {
                ship.targetPosition = targetPosition;
                var next = path[0];

                foreach (var node in path)
                {
                    Console.Error.WriteLine(node.step + ": S: " + node.speed + " R: " + node.rotation
                        + " " + node.moveType + " " + node.pos.toStr());
                }

                action = new Action(next.moveType, ship);
            }

            return action;
        }

        public static Vector RandomPosition(this Ship ship, Vector targetPosition, int min, int max)
        {
            if (ship.targetPosition != Vector.Undefined 
                && ship.nextPositionInfo.pos.Distance(targetPosition) > 1
                && ship.targetPosition.isRandomPositionSafe()
                && ship.targetPosition.Distance(ship.pos) < max)
            {
                return ship.targetPosition;
            }
            else
            {
                ship.targetPosition = Vector.Undefined;
                return targetPosition.GetRandomPosition(ship, min, max);
            }
        }
    }

    class Strategy
    {
        public static Random random = new Random();

        public static void ShipMakeMove(Ship ship)
        {
            bool waitMove = false;

            if (ship.isShipStuck())
            {
                waitMove = true;

                if (ship.firedLastRound == false)
                {
                    foreach (var enemy in Ship.ships.Values.OrderBy(s => s.pos.Distance(ship.pos)))
                    {
                        if (enemy.team == Team.Enemy && enemy.isAlive)
                        {
                            //Console.Error.WriteLine("ATK " + enemy.id + ":" + enemy.pos.toStr());

                            var predictedPos = ship.GetPredictedFirePos(enemy);

                            if (predictedPos.Distance(ship.currentPositionInfo.shipFront) <= 11)
                            {
                                var action = new Action(MoveType.Fire, ship, predictedPos);
                                Action.AddAction(action);
                                return;
                            }
                        }
                    }
                }
            }

            foreach (var cannon in Cannon.cannons)
            {
                //var shipPredictedPos = ship.GetPredictedPos(cannon.turns);

                if (cannon.turns <= 3 && ship.WillCannonHitShip(cannon))
                {
                    if (false)//(ship.speed == 0 && ship.lastMoveCommand.Distance(ship.pos) > 4)
                    {
                        var action = new Action(MoveType.Faster, ship);
                        Action.AddAction(action);
                        return;
                    }
                    else
                    {
                        int randX = random.Next(0, 22);
                        int randY = random.Next(0, 20);

                        Hex hex = new Hex(randX, randY);

                        Vector randomPos = hex.ConvertCube();
                        Console.Error.WriteLine("Dodging");

                        Action action = ship.Dodge(cannon);

                        if (action != null)
                        {
                            Action.AddAction(action);
                            return;
                        }
                    }
                }
            }

            if (ship.speed == 2) //check ramming into mines
            {
                var forwardPos = ship.pos + 3 * Vector.directions[ship.rotation];
                if (Mine.minePos.Contains(forwardPos.ConvertHex()))
                {
                    ship.targetPosition = Vector.Undefined;
                }
            }

            if (Game.gameTurn >= ship.nextMineTurn)
            {
                foreach (var enemy in Ship.ships.Values)
                {
                    if (enemy.isAlive && enemy.team == Team.Enemy)
                    {
                        if (enemy.nextPositionInfo.pos == ship.currentPositionInfo.minePos
                         || enemy.nextPositionInfo.shipFront == ship.currentPositionInfo.minePos
                         || enemy.nextPositionInfo.shipBack == ship.currentPositionInfo.minePos)
                        {
                            var action = new Action(MoveType.Mine, ship);
                            Action.AddAction(action);
                            return;
                        }
                    }
                }
            }

            foreach (var barrel in Barrel.barrels.OrderBy(b => ship.nextPositionInfo.pos.Distance(b.pos)
                            - Convert.ToInt32(ship.targetPosition == b.pos) * 100))
            {
                bool sameTarget = false;
                foreach (var ally in Ship.ships.Values)
                {
                    if (ally.team == Team.Ally && ally.isAlive && ally != ship
                        && ally.nextPositionInfo.pos.Distance(barrel.pos) < ship.nextPositionInfo.pos.Distance(barrel.pos))
                    {
                        sameTarget = true;
                        break;
                    }
                }

                if (sameTarget)
                {
                    continue;
                }

                if (ship.nextPositionInfo.shipFront == barrel.pos
                    || ship.nextPositionInfo.pos == barrel.pos)
                {
                    continue;
                }


                if (Ship.ships.Values.Any(enemy => enemy.isAlive && enemy.team == Team.Enemy
                    && enemy.nextPositionInfo.pos.Distance(barrel.pos) <= 5
                    && enemy.nextPositionInfo.pos.Distance(barrel.pos) < ship.nextPositionInfo.pos.Distance(barrel.pos)))
                {
                    continue;                   
                }

                Console.Error.WriteLine("Target: " + barrel.pos.ConvertHex().toStr());

                Action action = ship.MoveShip(barrel.pos);

                if (action != null)
                {
                    if (action.move != MoveType.Wait)
                    {
                        Action.AddAction(action);
                        return;
                    }
                    else
                    {
                        waitMove = true;
                        break;
                    }
                }
                else
                {
                    waitMove = true;
                    break;
                }

            }

            if (waitMove)//ship.speed >= 1 && ship.lastMoveCommand != Vector.Undefined)
            {

            }
            else
            {
                Console.Error.WriteLine("Searching Random Position...");
                
                PositionNode target = null;
                target = Pathfinding.GetLongestPath(ship, 6);
                
                /*var shipWithMostRum = Ship.ships.Values.OrderByDescending(s => s.rum).FirstOrDefault();
                if (ship == shipWithMostRum)
                {
                    target = Pathfinding.GetLongestPath(ship, 6, true);
                }
                else
                {
                    target = Pathfinding.GetLongestPath(ship, 6);
                }*/

                var path = ship.GetNextSteps(target);
                if (path != null && path.Count > 1)
                {           
                    
                    var next = path[0];

                    foreach (var node in path)
                    {
                        Console.Error.WriteLine(node.step + ": S: " + node.speed + " R: " + node.rotation
                            + " " + node.moveType + " " + node.pos.toStr());
                    }

                    Action action = new Action(next.moveType, ship);

                    if (action.move != MoveType.Wait)
                    {
                        Action.AddAction(action);
                        return;
                    }
                    else
                    {
                        waitMove = true;                        
                    }
                }
            }
            /*else
            {
                foreach (var enemy in Ship.ships.Values.OrderBy(s => s.pos.Distance(ship.pos)))
                {
                    if (enemy.team == Team.Enemy && enemy.isAlive)
                    {

                        Vector pos = ship.RandomPosition(enemy.nextPositionInfo.shipFront, 3, 7);
                        Console.Error.WriteLine("Random enemy: " + pos.toStr());

                        if (pos == Vector.Undefined)
                        {
                            continue;
                        }


                        Action action = ship.MoveShip(pos);
                        if (action != null)
                        {
                            if (action.move != MoveType.Wait)
                            {
                                Action.AddAction(action);
                                return;
                            }
                            else
                            {
                                waitMove = true;
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }*/

            if (ship.firedLastRound == false)
            {
                foreach (var enemy in Ship.ships.Values.OrderBy(s => s.pos.Distance(ship.pos)))
                {
                    if (enemy.team == Team.Enemy && enemy.isAlive)
                    {
                        //Console.Error.WriteLine("ATK " + enemy.id + ":" + enemy.pos.toStr());

                        var predictedPos = ship.GetPredictedFirePos(enemy);

                        if (predictedPos != Vector.Undefined && 
                            predictedPos.Distance(ship.currentPositionInfo.shipFront) <= 10)
                        {
                            var action = new Action(MoveType.Fire, ship, predictedPos);
                            Action.AddAction(action);
                            return;
                        }
                    }
                }
            }

            if (Game.gameTurn >= ship.nextMineTurn)
            {
                /*foreach (var enemy in Ship.ships.Values)
                {
                    if (enemy.isAlive && enemy.team == Team.Enemy)
                    {
                        if (ship.currentPositionInfo.minePos.Distance(enemy.nextPositionInfo.shipFront) <= 2)
                        {
                            var action = new Action(MoveType.Mine, ship);
                            Action.AddAction(action);
                            return;
                        }
                    }
                }*/

                /*foreach (var ally in Ship.ships.Values)
                {
                    if (ally.isAlive && ally.team == Team.Ally && ally != ship)
                    {
                        if (ship.currentPositionInfo.minePos.Distance(ally.nextPositionInfo.shipFront) > 3)
                        {
                            var action = new Action(MoveType.Mine, ship);
                            Action.AddAction(action);
                            return;
                        }
                    }
                }*/
            }

            Action.AddAction(new Action(ship));
        }

        public static void MakeMove()
        {

            foreach (var ship in Ship.ships.Values)
            {
                if (ship.team == Team.Ally && ship.isAlive)
                {
                    float loopStartTime = Timer.TickCount;

                    ShipMakeMove(ship);
                    //Action.AddAction(new Action(ship));

                    float loopTime = Timer.TickCount - loopStartTime;
                    Console.Error.WriteLine("Finish Moving Ship " + ship.id + ": " + loopTime + "\n");
                }
            }
        }
    }

    class Barrel : Entity, IEquatable<Barrel>
    {
        public static HashSet<Barrel> barrels = new HashSet<Barrel>();

        public int rum;

        public Barrel()
        {

        }

        public override void ProcessMessage(EntityMessage message)
        {
            this.hexPos = message.pos;
            this.pos = message.pos.ConvertCube();

            this.rum = message.arg1;
        }

        public static void CleanUp()
        {
            barrels = new HashSet<Barrel>();
        }

        public static Barrel GetBarrelByID(int id)
        {
            Barrel barrel = new Barrel();
            barrels.Add(barrel);
            return barrel;
        }

        public override bool Equals(object obj)
        {
            if (obj is Barrel)
            {
                return Equals((Barrel)this);
            }

            return false;
        }

        public bool Equals(Barrel barrel)
        {
            return barrel.pos == this.pos;
        }

        public override int GetHashCode()
        {
            return this.pos.GetHashCode();
        }
    }

    class Cannon : Entity, IEquatable<Cannon>
    {
        public static HashSet<Cannon> cannons = new HashSet<Cannon>();

        public Ship ship;
        public int turns;
                
        public Cannon()
        {

        }

        public static void CleanUp()
        {
            cannons = new HashSet<Cannon>();
        }

        public override void ProcessMessage(EntityMessage message)
        {
            this.ship = Ship.GetShipByID(message.arg1);
            this.turns = message.arg2;
            this.hexPos = message.pos;
            this.pos = message.pos.ConvertCube();

            Console.Error.WriteLine(this.pos.toStr() + " -> " + this.turns);
        }

        public static void GetExplodingMines()
        {
            List<Cannon> cannonList = new List<Cannon>();

            foreach (var cannon in Cannon.cannons)
            {
                foreach (var mine in Mine.mines)
                {
                    if (mine.pos == cannon.pos)
                    {
                        foreach (var dir in Vector.directions)
                        {
                            Cannon newCannon = new Cannon();

                            newCannon.ship = cannon.ship;
                            newCannon.turns = cannon.turns;
                            newCannon.hexPos = cannon.hexPos;
                            newCannon.pos = cannon.pos;

                            cannonList.Add(newCannon);
                        }                        
                    }
                }
            }

            cannonList.ForEach(c => Cannon.cannons.Add(c));
        }

        public static Cannon GetCannonByID(int id)
        {
            Cannon cannon = new Cannon();
            cannons.Add(cannon);
            return cannon;
        }

        public override bool Equals(object obj)
        {
            if (obj is Cannon)
            {
                return Equals((Cannon)this);
            }

            return false;
        }

        public bool Equals(Cannon cannon)
        {
            return cannon.pos == this.pos && cannon.ship == this.ship;
        }

        public override int GetHashCode()
        {
            return this.turns * 100 * 100 + this.pos.GetHashCode();
        }
    }

    public static class CannonExtensions
    {

    }

    public enum Team
    {
        Ally,
        Enemy,
    }

    abstract class Entity
    {
        public Hex hexPos { get; set; }
        public Vector pos { get; set; }
        public Team team { get; set; }

        public abstract void ProcessMessage(EntityMessage message);
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

        public int x;
        public int y;

        public Hex pos;

        public EntityMessage(string str)
        {
            var inputs = str.Split(' ');

            id = int.Parse(inputs[0]);
            entityType = inputs[1];
            x = int.Parse(inputs[2]);
            y = int.Parse(inputs[3]);
            arg1 = int.Parse(inputs[4]);
            arg2 = int.Parse(inputs[5]);
            arg3 = int.Parse(inputs[6]);
            arg4 = int.Parse(inputs[7]);

            pos = new Hex(x, y);
        }

        public void ProcessMessage()
        {            
            if (entityType == "SHIP")
            {
                Entity ship = Ship.GetShipByID(id);
                ship.ProcessMessage(this);
            }

            if (entityType == "BARREL")
            {
                Entity barrel = Barrel.GetBarrelByID(id);
                barrel.ProcessMessage(this);
            }

            if (entityType == "MINE")
            {
                Entity mine = Mine.GetMineByID(id);
                mine.ProcessMessage(this);
            }

            if (entityType == "CANNONBALL")
            {
                Entity cannon = Cannon.GetCannonByID(id);
                cannon.ProcessMessage(this);
            }
        }
    }

    class Mine : Entity, IEquatable<Mine>
    {
        public static HashSet<Mine> mines = new HashSet<Mine>();
        public static HashSet<Hex> minePos = new HashSet<Hex>();

        public Mine()
        {

        }

        public override void ProcessMessage(EntityMessage message)
        {
            this.hexPos = message.pos;
            this.pos = message.pos.ConvertCube();

            if (!minePos.Contains(this.hexPos))
            {
                minePos.Add(this.hexPos);
            }
        }

        public static void CleanUp()
        {
            mines = new HashSet<Mine>();
            minePos = new HashSet<Hex>();
        }

        public static Mine GetMineByID(int id)
        {
            Mine mine = new Mine();
            mines.Add(mine);
            return mine;
        }

        public override bool Equals(object obj)
        {
            if (obj is Mine)
            {
                return Equals((Mine)this);
            }

            return false;
        }

        public bool Equals(Mine obj)
        {
            return obj.pos == this.pos;
        }

        public override int GetHashCode()
        {
            return this.pos.GetHashCode();
        }
    }

    class ShipPositionInfo
    {
        public Vector shipFront;
        public Vector shipLeft;
        public Vector shipRight;

        public Vector shipBack;
        public Vector minePos;

        public Vector pos;
        public int rotation;
        public int speed;

        public PositionNode positionNode;

        public ShipPositionInfo(Vector pos, int rotation, int speed)
        {
            this.pos = pos;
            this.rotation = rotation;
            this.speed = speed;

            this.shipFront = this.pos + Vector.directions[this.rotation];
            this.shipRight = this.pos + Vector.directions[(this.rotation + 5) % 6];
            this.shipLeft = this.pos + Vector.directions[(this.rotation + 1) % 6];

            this.shipBack = this.pos - Vector.directions[this.rotation];
            this.minePos = this.pos - Vector.directions[this.rotation] * 2;

            this.positionNode = PositionNode.GetPositionNode(pos, rotation, speed);
        }
    }

    class Ship : Entity, IEquatable<Ship>
    {
        public static Dictionary<int, Ship> ships = new Dictionary<int, Ship>();

        public static int numAllyShips = 0;

        public int id;

        public int speed;
        public int rotation;
        public int rum;

        public bool isAlive = true;

        public int nextMineTurn = 0;

        public Vector prevPos;
        public Vector direction;

        public ShipPositionInfo previousPositionInfo;
        public ShipPositionInfo currentPositionInfo;
        public ShipPositionInfo nextPositionInfo;

        public Vector lastMoveCommand;
        public Vector targetPosition = Vector.Undefined;

        public Action lastActionCommand;

        public HashSet<Hex> enemyShipCollisionCheck = new HashSet<Hex>();

        public List<Cannon> dodgeableCannons = new List<Cannon>();

        public bool firedLastRound;

        public int x, y;


        public Ship(int id)
        {
            this.id = id;
        }

        public static void CleanUp()
        {
            foreach (var ship in ships.Values)
            {
                ship.isAlive = false;
                ship.enemyShipCollisionCheck = new HashSet<Hex>();
                ship.dodgeableCannons = new List<Cannon>();
            }

            Ship.numAllyShips = 0;
        }

        public override void ProcessMessage(EntityMessage message)
        {
            this.prevPos = pos;

            this.hexPos = message.pos;
            this.pos = message.pos.ConvertCube();
            this.rotation = message.arg1;
            this.speed = message.arg2;
            this.rum = message.arg3;
            this.isAlive = true;

            this.x = message.x;
            this.y = message.y;

            this.direction = Vector.directions[this.rotation];

            this.previousPositionInfo = this.currentPositionInfo;
            this.currentPositionInfo = new ShipPositionInfo(this.pos, this.rotation, this.speed);
            this.nextPositionInfo = new ShipPositionInfo(this.GetPredictedPos(1), this.rotation, this.speed);

            //Console.Error.WriteLine(this.id + ": " + this.pos.ConvertHex().toStr());

            //Console.Error.WriteLine("Distance: " + this.pos.Distance(this.nextPositionInfo.pos));

            if (message.arg4 == 1)
            {
                this.team = Team.Ally;
                Ship.numAllyShips++;
            }
            else
            {
                this.team = Team.Enemy;
            }            
        }

        public void CheckDodgeableCannons()
        {
            var ship = this;

            foreach (var cannon in Cannon.cannons)
            {
                if (cannon.turns == 1)
                {
                    if (ship.speed == 0 && ship.pos == cannon.pos)
                    {
                        continue;
                    }
                    else if (ship.speed > 0 && ship.nextPositionInfo.pos == cannon.pos)
                    {
                        continue;
                    }
                }

                dodgeableCannons.Add(cannon);
            }
        }

        public static void GetAllyShipPaths()
        {
            foreach (var ship in Ship.ships.Values)
            {
                if (ship.isAlive && ship.team == Team.Ally)
                {
                    //ship.paths = Pathfinding.CalculateShortestPaths(ship);

                    foreach (var enemy in Ship.ships.Values)
                    {
                        if (enemy.isAlive && ship != enemy
                            && ship.nextPositionInfo.pos.Distance(enemy.nextPositionInfo.pos) <= 5)
                        {
                            ship.enemyShipCollisionCheck.Add((enemy.nextPositionInfo.pos).ConvertHex());
                            ship.enemyShipCollisionCheck.Add((enemy.nextPositionInfo.shipFront).ConvertHex());
                            ship.enemyShipCollisionCheck.Add((enemy.nextPositionInfo.shipBack).ConvertHex());

                            var predPosition = enemy.pos + 2 * Vector.directions[enemy.rotation];
                            ship.enemyShipCollisionCheck.Add((predPosition).ConvertHex());

                            var minePosition = enemy.pos - 2 * Vector.directions[enemy.rotation];
                            ship.enemyShipCollisionCheck.Add((minePosition).ConvertHex());
                        }
                    }

                    ship.CheckDodgeableCannons();
                }
            }            
        }

        public static Ship GetShipByID(int id)
        {
            Ship ship;
            if (Ship.ships.TryGetValue(id, out ship))
            {
                return ship;
            }
            else
            {
                ship = new Ship(id);
                ships.Add(id, ship);
                return ship;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is Ship)
            {
                return Equals((Ship)this);
            }

            return false;
        }

        public bool Equals(Ship ship)
        {
            return ship.id == this.id;
        }

        public override int GetHashCode()
        {
            return this.id;
        }
    }

    static class ShipExtensions
    {
        public static Vector GetPredictedPos(this Ship ship, double turns, int speed = -1)
        {
            if (speed <= 0)
            {
                speed = ship.speed;
            }

            var predictedPos = ship.pos + ship.direction * turns * speed;

            if (ship.pos == ship.prevPos || !predictedPos.isInBound())
            {
                predictedPos = ship.pos;
            }

            return predictedPos;
        }

        public static Vector GetPredictedFirePos(this Ship ship, Ship enemy)
        {
            double turns = GetPredictionByRegression(ship, enemy) + 1;
            var predictedPos = GetPredictedPos(enemy, turns);

            if (turns > 3)
            {
                foreach (var barrel in Barrel.barrels)
                {
                    if (enemy.nextPositionInfo.shipFront.Distance(barrel.pos) <= 4)
                    {
                        return barrel.pos;
                    }
                }

                /*foreach (var mine in Mine.mines)
                {
                    if (predictedPos.Distance(mine.pos) <= 2)
                    {
                        return mine.pos;
                    }
                }*/

                //return Vector.Undefined;
            }

            return predictedPos;
        }

        public static double GetPredictionByRegression(this Ship source, Ship target)
        {
            var sourcePos = source.currentPositionInfo.shipFront; //get front of ship as well
            var targetPos = source.nextPositionInfo.pos;

            var predictedPos = targetPos;
            double collisionTime = 0;

            for (int i = 0; i < 5; i++)
            {
                collisionTime = sourcePos.Distance(predictedPos) / 3;

                var newPredictedPos = target.GetPredictedPos(collisionTime + 1);

                if (newPredictedPos.Distance(predictedPos) < .05)
                {
                    return collisionTime + 1;
                }
                else
                {
                    predictedPos = newPredictedPos;
                }
            }

            return collisionTime + 1;
        }

        public static bool WillCannonHitShip(this Ship ship, Cannon cannon)
        {
            var pos = ship.GetPredictedPos(cannon.turns);

            var front = pos + Vector.directions[ship.rotation];
            var back = pos + Vector.directions[(ship.rotation + 3) % 6];

            return cannon.pos == pos || cannon.pos == front || cannon.pos == back;
        }

        public static bool isShipStuck(this Ship ship)
        {
            if (ship.previousPositionInfo != null && 
                ship.previousPositionInfo.positionNode == ship.currentPositionInfo.positionNode)
            {
                if (ship.lastActionCommand != null && (int)ship.lastActionCommand.move > 2)
                {
                    Console.Error.WriteLine("Ship is stuck!");
                    return true;
                }
            }

            return false;
        }

        public static List<PositionNode> GetNextSteps(this Ship ship, Vector targetPosition)
        {
            PositionNode result = Pathfinding.CalculateShortestPaths(ship, targetPosition);
            return GetNextSteps(ship, result);
        }

        public static List<PositionNode> GetNextSteps(this Ship ship, PositionNode current)
        {            
            PositionNode shipNode = new PositionNode(ship.pos, ship.rotation, ship.speed);

            List<PositionNode> path = new List<PositionNode>();

            int count = 0;
            while (current != null && !current.Equals(shipNode) && current.parent != null && count < 30)
            {
                path.Add(current);
                current = current.parent;
                count++;
            }

            if (count >= 30)
            {
                Console.Error.WriteLine("Can't find shipNode");
                return null;
            }

            if (current != null && current.Equals(shipNode))
            {
                path.Reverse();
                return path;
            }

            return null;
        }
    }

    public enum MoveType
    {
        Wait,
        Fire,
        Mine,
        Move,
        Left, //turn ship to left
        Right,
        Faster,
        Slower,
    }

    class Action
    {
        public static List<Action> actions = new List<Action>();

        public MoveType move;
        public Vector position;
        public Ship ship;
        public Hex hexPosition;

        public Action(Ship ship)
        {
            this.move = MoveType.Wait;
            this.ship = ship;
        }

        public Action(MoveType move, Ship ship)
        {
            this.move = move;
            this.ship = ship;
        }

        public Action(MoveType move, Ship ship, Vector position)
        {
            this.move = move;
            this.position = position;
            this.ship = ship;
        }

        public static void CleanUp()
        {
            actions = new List<Action>();
        }

        public static void AddAction(Action action)
        {
            action.hexPosition = action.position.ConvertHex();
            action.ship.firedLastRound = false;
            
            if (action.move == MoveType.Move)
            {                
                action.ship.lastMoveCommand = action.position;
            }
            else if (action.move == MoveType.Fire)
            {
                action.ship.firedLastRound = true;
            }
            else if (action.move == MoveType.Faster)
            {
                action.ship.lastMoveCommand = Vector.Undefined;
            }
            else if (action.move == MoveType.Slower)
            {
                action.ship.lastMoveCommand = Vector.Undefined;
            }
            else if (action.move == MoveType.Left)
            {
                action.ship.lastMoveCommand = Vector.Undefined;
            }
            else if (action.move == MoveType.Right)
            {
                action.ship.lastMoveCommand = Vector.Undefined;
            }
            else if (action.move == MoveType.Mine)
            {
                action.ship.nextMineTurn = Game.gameTurn + 4;
            }

            action.ship.lastActionCommand = action;

            Action.actions.Add(action);
        }

        public static void PrintActions()
        {
            if (actions.Count == 0)
            {
                Console.WriteLine("WAIT");
                return;
            }

            string str = "";//"WAIT;";
                        
            foreach (var action in actions)
            {
                switch (action.move)
                {
                    case MoveType.Move:
                        str = "MOVE " + action.hexPosition.col + " " + action.hexPosition.row;
                        break;
                    case MoveType.Fire:
                        str = "FIRE " + action.hexPosition.col + " " + action.hexPosition.row;
                        break;
                    case MoveType.Mine:
                        str = "MINE";
                        break;
                    case MoveType.Left:
                        str = "PORT";
                        break;
                    case MoveType.Right:
                        str = "STARBOARD";
                        break;
                    case MoveType.Faster:
                        str = "FASTER";
                        break;
                    case MoveType.Slower:
                        str = "SLOWER";
                        break;
                    default:
                        str = "WAIT";
                        break;
                }

                Console.WriteLine(str);
            }

            //str = str.Remove(str.Length - 1); //remove extra ;

            
        }

    }

    public struct Hex : IEquatable<Hex>
    {
        public int col;
        public int row;

        public Hex(int column, int row)
        {
            this.col = column;
            this.row = row;
        }

        public static bool operator ==(Hex value1, Hex value2)
        {
            return value1.col == value2.col
                && value1.row == value2.row;
        }

        public static bool operator !=(Hex value1, Hex value2)
        {
            return !(value1 == value2);
        }

        public override bool Equals(object obj)
        {
            return (obj is Hex) ? this == (Hex)obj : false;
        }

        public bool Equals(Hex other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return this.col * 100 + this.row;
        }
    }

    public static class HexExtensions
    {
        public static Vector ConvertCube(this Hex hex)
        {
            double x = hex.col - (hex.row - (hex.row & 1)) / 2;
            double z = hex.row;
            double y = -x - z;

            return new Vector(x, y, z);
        }

        public static bool isInBound(this Hex hex)
        {
            return !(hex.col < 0 || hex.col > 22 || hex.row < 0 || hex.row > 20);
        }

        public static bool isInBound2(this Hex hex)
        {
            return !(hex.col <= 0 || hex.col >= 22 || hex.row <= 0 || hex.row >= 20);
        }

        public static string toStr(this Hex hex)
        {
            return "(" + hex.col + " " + hex.row + ")";
        }
    }

    class Timer
    {
        private static DateTime loadTime = DateTime.Now;

        public static float TickCount
        {
            get
            {
                return (int)DateTime.Now.Subtract(loadTime).TotalMilliseconds;
            }
        }
    }

    public struct Vector : IEquatable<Vector>
    {
        public double x;
        public double y;
        public double z;

        public static List<Vector> directions = new List<Vector>{
            new Vector( 1, -1,  0), new Vector( 1,  0, -1), new Vector( 0,  1, -1),
            new Vector(-1,  1,  0), new Vector(-1,  0,  1), new Vector( 0, -1,  1)
        };

        public static Dictionary<Vector, int> directionTable = new Dictionary<Vector, int>{
            {new Vector( 1, -1,  0), 0}, {new Vector( 1,  0, -1), 1}, {new Vector( 0,  1, -1), 2},
            {new Vector(-1,  1,  0), 3}, {new Vector(-1,  0,  1), 4}, {new Vector( 0, -1,  1), 5}
        };

        public Vector(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector Zero
        {
            get { return new Vector(0f, 0f, 0f); }
        }

        public static Vector Undefined
        {
            get { return new Vector(-1337f, -1337f, -1337f); }
        }

        public static bool operator ==(Vector value1, Vector value2)
        {
            return value1.ConvertHex() == value2.ConvertHex();
        }

        public static bool operator !=(Vector value1, Vector value2)
        {
            return !(value1 == value2);
        }

        public static Vector operator +(Vector value1, Vector value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            value1.z += value2.z;
            return value1;
        }

        public static Vector operator -(Vector value)
        {
            value = new Vector(-value.x, -value.y, -value.z);
            return value;
        }

        public static Vector operator -(Vector value1, Vector value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            value1.z -= value2.z;
            return value1;
        }

        public static Vector operator *(Vector value1, Vector value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            value1.z *= value2.z;
            return value1;
        }

        public static Vector operator *(Vector value, double scaleFactor)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            value.z *= scaleFactor;
            return value;
        }

        public static Vector operator *(double scaleFactor, Vector value)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            value.z *= scaleFactor;
            return value;
        }

        public override bool Equals(object obj)
        {
            return (obj is Vector) ? this == (Vector)obj : false;
        }

        public bool Equals(Vector other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return this.ConvertHex().GetHashCode();
        }
    }

    static class VectorExtensions
    {
        public static double Distance(this Vector a, Vector b)
        {
            //return (Math.Abs(value1.x - value2.x) + Math.Abs(value1.y - value2.y) + Math.Abs(value1.z - value2.z))/2;
            return Math.Max(Math.Max(Math.Abs(a.x - b.x), Math.Abs(a.y - b.y)), Math.Abs(a.z - b.z));
        }

        public static Vector Normalized(this Vector value)
        {
            double factor = value.Distance(Vector.Zero);
            factor = 1f / factor;
            Vector result;
            result.x = value.x * factor;
            result.y = value.y * factor;
            result.z = value.z * factor;

            return result;
        }

        public static Vector Direction(this Vector current, Vector previous)
        {
            return (current - previous).Normalized();
        }

        public static Hex ConvertHex(this Vector vec)
        {
            int col = (int)Math.Round(vec.x) + ((int)Math.Round(vec.z) - ((int)Math.Round(vec.z) & 1)) / 2;
            int row = (int)Math.Round(vec.z);

            return new Hex(col, row);
        }

        public static bool isInBound(this Vector vec)
        {
            return vec.ConvertHex().isInBound();
        }

        public static string toStr(this Vector vec)
        {
            return vec.ConvertHex().toStr();
        }

        public static string toCubeStr(this Vector vec)
        {
            return "(" + vec.x + ", " + vec.y + ", " + vec.z + ")";
        }

        public static int GetRotation(this Vector end, Vector start)
        {
            for (int i = 0; i < Vector.directions.Count; i++)
            {
                var pos = start + Vector.directions[i];
                if (pos == end)
                {
                    return i;
                }
            }

            //Console.Error.WriteLine(start.toStr() + " -> " + end.toStr());

            return -1;
        }

        public static Vector GetRandomPosition(this Vector vec, Ship ship, int min, int max)
        {
            Vector randomPos = Vector.Undefined;

            int count = 0;
            int maxCount = 500;

            while (!randomPos.isRandomPositionSafe() 
                || randomPos.Distance(ship.pos) >= max)
            {
                int x = Strategy.random.Next(1, 20);
                int y = Strategy.random.Next(1, 22);

                Hex myPos = ship.pos.ConvertHex();

                if (count >= maxCount)
                {
                    break;
                }

                //randomPos = new Hex(myPos.col + x, myPos.row + y).ConvertCube();
                randomPos = new Hex(x, y).ConvertCube();
                count++;
            }

            if (count == maxCount)
            {
                Console.Error.WriteLine("No random position found.");
                return Vector.Undefined;
            }

            Console.Error.WriteLine("Random position dist: " + randomPos.Distance(ship.pos) + " loops: " + count);

            return randomPos;
        }

        public static bool isRandomPositionSafe(this Vector vec)
        {
            Hex hexPos = vec.ConvertHex();

            if (!hexPos.isInBound2())
            {
                return false;
            }

            /*if (Mine.minePos.Contains(hexPos))
            {
                return false;
            }*/

            foreach (var mine in Mine.mines)
            {
                if (vec.Distance(mine.pos) <= 2)
                {
                    /*if (mine.hexPos.col == 15 && mine.hexPos.row == 6)
                    {
                        Console.Error.WriteLine(vec.toStr() + " Distance: " + mine.pos.Distance(vec));
                    }*/
                    return false;
                }
            }

            /*foreach (var cannon in Cannon.cannons)
            {
                if (cannon.hexPos == hexPos)
                {
                    return false;
                }
            }*/

            foreach (var ship in Ship.ships.Values)
            {
                if (ship.isAlive && 
                    ship.pos.Distance(vec) <= 3)
                {
                    return false;
                }
            }

            return true;
        }

        public static bool isPositionReachable(this Vector vec)
        {
            Hex hexPos = vec.ConvertHex();

            if (!hexPos.isInBound())
            {
                return false;
            }

            if (Mine.minePos.Contains(hexPos))
            {
                return false;
            }

            foreach (var ship in Ship.ships.Values)
            {
                if (ship.isAlive &&
                    (hexPos == ship.hexPos
                 || vec == ship.currentPositionInfo.shipFront
                 || vec == ship.currentPositionInfo.shipBack))
                {
                    return false;
                }
            }

            return true;
        }
    }




}
