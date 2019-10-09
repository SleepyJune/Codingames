using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonderWoman
{
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
}
