using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
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
}
