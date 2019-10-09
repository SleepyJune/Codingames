
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonderWoman
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] inputs;
            int size = int.Parse(Console.ReadLine());
            int unitsPerPlayer = int.Parse(Console.ReadLine());

            GameState.size = size;
            GameState.unitsPerPlayer = unitsPerPlayer;

            // game loop
            while (true)
            {

                List<string> rows = new List<string>();
                for (int i = 0; i < size; i++)
                {
                    string row = Console.ReadLine();
                    rows.Add(row);
                }

                Game.currentState = new GameState(rows);
                Game.currentState.PrintMap();

                for (int i = 0; i < unitsPerPlayer; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int unitX = int.Parse(inputs[0]);
                    int unitY = int.Parse(inputs[1]);

                    var unit = new Unit(unitX, unitY, Team.ally);
                    Game.currentState.units[unit.index] = unit;
                    Unit.units.Add(unit);
                }
                for (int i = 0; i < unitsPerPlayer; i++)
                {
                    inputs = Console.ReadLine().Split(' ');
                    int otherX = int.Parse(inputs[0]);
                    int otherY = int.Parse(inputs[1]);

                    var unit = new Unit(otherX, otherY, Team.enemy);
                    Game.currentState.units[unit.index] = unit;
                    Unit.units.Add(unit);
                }
                int legalActions = int.Parse(Console.ReadLine());
                for (int i = 0; i < legalActions; i++)
                {
                    var action = new Action(Console.ReadLine());

                    //var unit = Unit.units[action.unitIndex];
                    Unit.units[action.unitIndex].AddAction(ref action);
                }

                //Game.InitializeTurn();
                Game.MakeMove();
                Game.PrintActions();
                Game.CleanUp();                
            }
        }
    }

    class Game
    {
        public static int gameTurn = 0;
        public static GameState currentState;

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
            Unit.CleanUp();
            Action.CleanUp();

            gameTurn++;
        }
    }

    class Strategy
    {
        public static void MakeMove()
        {
            for (int x = 0; x < Unit.units.Count;x++ )
            {
                var unit = Unit.units[x];

                if (unit.team == Team.ally)
                {
                    var sorted = unit.actions.OrderByDescending(a => TryAction(Game.currentState, a.unitIndex, a, 1));

                    Action.AddAction(sorted.FirstOrDefault());
                }
            }
        }

        public static int TryAction(GameState state, int unitIndex, Action action, int tryCount)
        {
            state.ApplyAction(action);
            var unit = state.units[unitIndex];

            int possibleRewards = 0;

            /*if (tryCount > 0)
            {
                for (int i = 0; i < unit.actionCount; i++)
                {
                    var nextAction = unit.actions[i];
                    Console.Error.WriteLine("b x: " + nextAction.pos1.x + " y: " + nextAction.pos1.y);
                    possibleRewards += TryAction(state, unitIndex, nextAction, tryCount - 1);
                }
            }*/

            if (unit.actions.Count == 0)
            {
                return possibleRewards - 10;
            }
            else if (state.GetSquare(unit.pos) == '3')
            {
                return possibleRewards + 1;
            }
            else
            {
                return 0;
            }
        }
    }

    struct GameState
    {
        public static int size = 0;
        public static int unitsPerPlayer = 0;

        public Unit[] units;
        public readonly char[,] map;

        public GameState(List<string> rows)
        {
            units = new Unit[unitsPerPlayer*2];
            map = new char[size, size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    map[i, j] = rows[j][i];
                }
            }
        }

        public char GetSquare(Position pos)
        {
            if (pos.x >= 0 && pos.x < size && pos.y >= 0 && pos.y < size)
            {
                return map[pos.x, pos.y];
            }
            else
            {
                return '.';
            }            
        }

        public void ApplyAction(Action action)
        {
            if (action.type == ActionType.MoveAndBuild)
            {                
                var unit = units[action.unitIndex];
                unit.pos = action.pos1;

                //Console.Error.WriteLine("Pos x: " + action.pos1.x + " y: " + action.pos1.y);

                map[action.pos2.x, action.pos2.y] = (char)(map[action.pos2.x, action.pos2.y] + 1);

                unit.GetLegalMoves(ref this);

                PrintMap();


            }
        }

        public void PrintMap()
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Console.Error.Write(map[x, y]);
                }

                Console.Error.Write("\n");
            }

            Console.Error.Write("\n");
        }
    }

    struct Position
    {
        public int x;
        public int y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static Position operator +(Position value1, Position value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;

            return value1;
        }

        public int GetSquareValue()
        {
            char square = Game.currentState.map[x, y];

            if (square == '.')
            {
                return 0;
            }
            else
            {
                //Console.Error.WriteLine("Pos x: " + x + " y: " + y + " value: " + (square - '0'));
                return square - '0';
            }
        }
    }

    enum Team
    {
        ally,
        enemy
    }

    class Unit
    {
        public static List<Unit> units = new List<Unit>();

        public Position pos;
        public Team team;

        public int index;

        public List<Action> actions = new List<Action>();

        public Unit(int x, int y, Team team)
        {
            this.pos = new Position(x, y);
            this.team = team;

            this.index = units.Count;
        }

        public static void CleanUp()
        {
            units = new List<Unit>();
        }

        public void AddAction(ref Action action)
        {
            //actions[actionCount] = action;
            //actionCount+=1;
            actions.Add(action);
        }

        public void GetLegalMoves(ref GameState state)
        {
            for (int dir1 = 0; dir1 < Action.directionConstant.Count; dir1++)
            {
                var pos1 = pos + Action.directionConstant[dir1];
                var currentSquare = state.GetSquare(pos);
                var square1 = state.GetSquare(pos1);

                if (square1 == '.')
                {
                    continue;
                }
                else
                {
                    var currentLevel = currentSquare - '0';
                    var squareLevel = square1 - '0';

                    if (Math.Abs(currentLevel - squareLevel) > 1)
                    {
                        continue;
                    }
                }

                for (int dir2 = 0; dir2 < Action.directionConstant.Count; dir2++)
                {
                    var pos2 = pos1 + Action.directionConstant[dir2];
                    var square2 = state.GetSquare(pos2);

                    if (square1 == '.')
                    {
                        continue;
                    }
                    else
                    {
                        var squareLevel2 = square2 - '0';

                        if (squareLevel2 > 3)
                        {
                            continue;
                        }

                        var action = new Action(this, ActionType.MoveAndBuild, index, (Direction)dir1, (Direction)dir2);
                        AddAction(ref action);
                    }
                }
            }
        }
    }

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




}
