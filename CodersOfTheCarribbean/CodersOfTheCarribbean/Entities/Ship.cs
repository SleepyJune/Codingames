using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
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
}
