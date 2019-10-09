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
}
