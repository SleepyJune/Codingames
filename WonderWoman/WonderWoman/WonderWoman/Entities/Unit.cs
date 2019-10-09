using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonderWoman
{
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
}
