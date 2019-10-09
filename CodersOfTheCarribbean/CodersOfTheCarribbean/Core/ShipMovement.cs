using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
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
}
