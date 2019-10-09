using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodersOfTheCarribbean
{
    public struct Hex : IEquatable<Hex>
    {
        public int col;
        public int row;

        public Hex(int column, int row)
        {
            this.col = column;
            this.row = row;
        }

        public static bool operator ==(Hex value1, Hex value2)
        {
            return value1.col == value2.col
                && value1.row == value2.row;
        }

        public static bool operator !=(Hex value1, Hex value2)
        {
            return !(value1 == value2);
        }

        public override bool Equals(object obj)
        {
            return (obj is Hex) ? this == (Hex)obj : false;
        }

        public bool Equals(Hex other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return this.col * 100 + this.row;
        }
    }

    public static class HexExtensions
    {
        public static Vector ConvertCube(this Hex hex)
        {
            double x = hex.col;// -(hex.row - (hex.row & 1)) / 2;
            double z = hex.row;
            double y = -x - z;

            return new Vector(x, y, z);
        }

        public static bool isInBound(this Hex hex)
        {
            return !(hex.col < 0 || hex.col > 22 || hex.row < 0 || hex.row > 20);
        }

        public static bool isInBound2(this Hex hex)
        {
            return !(hex.col <= 0 || hex.col >= 22 || hex.row <= 0 || hex.row >= 20);
        }

        public static string toStr(this Hex hex)
        {
            return "(" + hex.col + " " + hex.row + ")";
        }
    }
}
