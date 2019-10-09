using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GhostInACell
{
    public enum MoveType
    {
        Move,
        Wait,
        Bomb,
        Inc,
        Msg,
    }

    class Action
    {
        public static List<Action> actions = new List<Action>();

        public MoveType move;

        public Factory start;
        public Factory end;
        public int numRobots;

        public string message;

        public Action()
        {
            this.move = MoveType.Wait;
        }

        public Action(MoveType move, Factory start, Factory end, int numRobots)
        {
            this.move = move;
            this.start = start;
            this.end = end;
            this.numRobots = numRobots;
        }

        public Action(MoveType move, Factory start)
        {
            this.move = move;
            this.start = start;
        }

        public Action(string message)
        {
            this.move = MoveType.Msg;
            this.message = message;
        }

        public static void CleanUp()
        {
            actions = new List<Action>();
        }

        public static void AddAction(Action action, Troop troop = null)
        {
            if (action.move == MoveType.Move)
            {
                action.start.count -= action.numRobots;
                action.start.armyAvailable -= action.numRobots;

                if (troop != null && troop.inTransit == false)
                {
                    action.end.incoming.Add(troop);
                    action.end.states = FactoryState.CalculateFactoryState(action.end, action.end.incoming);
                }                

                //Console.Error.WriteLine("Move " + action.start.id + "-" + action.end.id + ": " + troop.count);
            }
            else if (action.move == MoveType.Inc)
            {
                action.start.count -= 10;
                action.start.armyAvailable -= 10;
            }
            else if (action.move == MoveType.Bomb)
            {
                action.end.incoming.Add(troop);
                Game.bomb--;
            }

            Action.actions.Add(action);

        }

        public static void PrintActions()
        {
            string str = "WAIT;";

            foreach (var action in actions)
            {
                switch (action.move)
                {
                    case MoveType.Move:
                        str += "MOVE " + action.start.id + " " + action.end.id + " " + action.numRobots;
                        break;
                    case MoveType.Inc:
                        str += "INC " + action.start.id;
                        break;
                    case MoveType.Bomb:
                        str += "BOMB " + action.start.id + " " + action.end.id;
                        break;
                    case MoveType.Msg:
                        str += "MSG " + action.message;
                        break;
                    default:
                        str += "WAIT";
                        break;
                }

                str += ";";
            }

            str = str.Remove(str.Length - 1); //remove extra ;

            Console.WriteLine(str);
        }
    }
}
