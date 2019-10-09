using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    class Geometry
    {
        public static Vector[] CircleCircleIntersection(Vector center1, Vector center2, float radius1, float radius2)
        {
            var D = center1.Distance(center2);
            //The Circles dont intersect:
            if (D > radius1 + radius2 || (D <= Math.Abs(radius1 - radius2)))
            {
                return new Vector[] { };
            }

            var A = (radius1 * radius1 - radius2 * radius2 + D * D) / (2 * D);
            var H = (float)Math.Sqrt(radius1 * radius1 - A * A);
            var Direction = (center2 - center1).Normalized();
            var PA = center1 + A * Direction;
            var S1 = PA + H * Direction.Perpendicular();
            var S2 = PA - H * Direction.Perpendicular();
            return new[] { S1, S2 };
        }
    }
}
