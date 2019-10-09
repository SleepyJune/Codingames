using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmasRush
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

        public static bool operator ==(Position value1, Position value2)
        {
            return value1.x == value2.x
                && value1.y == value2.y;
        }

        public static bool operator !=(Position value1, Position value2)
        {
            return !(value1 == value2);
        }

        public override bool Equals(object obj)
        {
            return (obj is Position) ? this == (Position)obj : false;
        }

        public bool Equals(Position other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (int)(this.x + this.y * 1000);
        }

        public override string ToString()
        {
            return "{" + this.x + ", " + this.y + "}";
        }
    }
}
