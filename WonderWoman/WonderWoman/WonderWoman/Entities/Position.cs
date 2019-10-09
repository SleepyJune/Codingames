using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WonderWoman
{
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
}
