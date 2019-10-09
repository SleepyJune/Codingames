using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonderWoman
{
    enum ActionType
    {
        MoveAndBuild
    }

    enum Direction
    {
        N,
        NE,
        E,
        SE,
        S,
        SW,
        W,
        NW
    }

    struct Action
    {
        public static List<Action> actions = new List<Action>();
        public static List<Position> directionConstant = new List<Position>()
        {
            new Position(0,-1),
            new Position(1,-1),
            new Position(1,0),
            new Position(1,1),
            new Position(0,1),
            new Position(-1,1),
            new Position(-1,0),
            new Position(-1,-1),
        };

        public ActionType type;
        public int unitIndex;
        public Direction dir1;
        public Direction dir2;

        public Position pos1;
        public Position pos2;

        public Action(Unit unit, ActionType type, int index, Direction dir1, Direction dir2)
        {
            this.type = type;
            this.unitIndex = index;
            this.dir1 = dir1;
            this.dir2 = dir2;

            //Console.Error.WriteLine(unit.pos.x);

            this.pos1 = unit.pos + directionConstant[(int)dir1];
            this.pos2 = pos1 + directionConstant[(int)dir2];            
        }

        public Action(string str)
        {
            var inputs = str.Split(' ');

            string atype = inputs[0];
            int index = int.Parse(inputs[1]);
            string dir1 = inputs[2];
            string dir2 = inputs[3];

            this.type = ActionType.MoveAndBuild;
            this.unitIndex = index;
            this.dir1 = 0;
            this.dir2 = 0;

            var unit = Unit.units[unitIndex];

            this.pos1 = unit.pos;
            this.pos2 = unit.pos;
                        
            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (direction.ToString() == dir1)
                {
                    this.dir1 = direction;
                    this.pos1 += directionConstant[(int)direction];
                }                               
            }

            foreach (Direction direction in Enum.GetValues(typeof(Direction)))
            {
                if (direction.ToString() == dir2)
                {
                    this.dir2 = direction;
                    this.pos2 = this.pos1 + directionConstant[(int)direction];
                }
            }

        }

        public static void AddAction(Action action)
        {
            Action.actions.Add(action);
        }

        public static void CleanUp()
        {
            actions = new List<Action>();
        }

        public static void PrintActions()
        {
            string str = "";

            foreach (var action in actions)
            {
                if (action.type == ActionType.MoveAndBuild)
                {
                    str += "MOVE&BUILD " + action.unitIndex + " " + action.dir1.ToString() + " " + action.dir2.ToString();
                }

                break;
            }

            Console.WriteLine(str);
        }
    }
}
