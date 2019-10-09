using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonderWoman
{
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
}
