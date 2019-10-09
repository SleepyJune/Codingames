using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
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
}
