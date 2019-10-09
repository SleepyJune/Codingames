using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bog
{
    public struct Vector : IEquatable<Vector>
    {
        public float x;
        public float y;
        public float z;

        public Vector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Vector Zero
        {
            get { return new Vector(0f, 0f, 0f); }
        }

        public static Vector Undefined
        {
            get { return new Vector(-1337f, -1337f, -1337f); }
        }

        public static bool operator ==(Vector value1, Vector value2)
        {
            return value1.x == value2.x
                && value1.y == value2.y
                && value1.z == value2.z;
        }

        public static bool operator !=(Vector value1, Vector value2)
        {
            return !(value1 == value2);
        }

        public static Vector operator +(Vector value1, Vector value2)
        {
            value1.x += value2.x;
            value1.y += value2.y;
            value1.z += value2.z;
            return value1;
        }

        public static Vector operator -(Vector value)
        {
            value = new Vector(-value.x, -value.y, -value.z);
            return value;
        }

        public static Vector operator -(Vector value1, Vector value2)
        {
            value1.x -= value2.x;
            value1.y -= value2.y;
            value1.z -= value2.z;
            return value1;
        }

        public static Vector operator *(Vector value1, Vector value2)
        {
            value1.x *= value2.x;
            value1.y *= value2.y;
            value1.z *= value2.z;
            return value1;
        }

        public static Vector operator *(Vector value, float scaleFactor)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            value.z *= scaleFactor;
            return value;
        }

        public static Vector operator *(float scaleFactor, Vector value)
        {
            value.x *= scaleFactor;
            value.y *= scaleFactor;
            value.z *= scaleFactor;
            return value;
        }

        public override bool Equals(object obj)
        {
            return (obj is Vector) ? this == (Vector)obj : false;
        }

        public bool Equals(Vector other)
        {
            return this == other;
        }

        public override int GetHashCode()
        {
            return (int)(this.x + this.y * 1000 + this.z * 1000 * 1000);
        }

        public Vector Normalized()
        {
            float factor = Distance(Vector.Zero);
            factor = 1f / factor;
            Vector result;
            result.x = x * factor;
            result.y = y * factor;
            result.z = z * factor;

            return result;
        }

        public float Distance(Vector targetPos)
        {
            return (float)Math.Sqrt(Math.Pow(x - targetPos.x, 2)
                        + Math.Pow(y - targetPos.y, 2)
                        + Math.Pow(z - targetPos.z, 2));
        }

        public Vector Perpendicular()
        {
            return new Vector(-y, x, 0);
        }
    }

    static class VectorExtensions
    {
        public struct ProjectionInfo
        {
            public bool IsOnSegment;
            public Vector LinePoint;
            public Vector SegmentPoint;

            public ProjectionInfo(bool isOnSegment, Vector segmentPoint, Vector linePoint)
            {
                IsOnSegment = isOnSegment;
                SegmentPoint = segmentPoint;
                LinePoint = linePoint;
            }
        }

        public static ProjectionInfo ProjectOn(this Vector point, Vector segmentStart, Vector segmentEnd)
        {
            var cx = point.x;
            var cy = point.y;
            var ax = segmentStart.x;
            var ay = segmentStart.y;
            var bx = segmentEnd.x;
            var by = segmentEnd.y;
            var rL = ((cx - ax) * (bx - ax) + (cy - ay) * (by - ay)) /
                     ((float)Math.Pow(bx - ax, 2) + (float)Math.Pow(by - ay, 2));
            var pointLine = new Vector(ax + rL * (bx - ax), ay + rL * (by - ay), 0);
            float rS;
            if (rL < 0)
            {
                rS = 0;
            }
            else if (rL > 1)
            {
                rS = 1;
            }
            else
            {
                rS = rL;
            }

            var isOnSegment = rS.CompareTo(rL) == 0;
            var pointSegment = isOnSegment ? pointLine : new Vector(ax + rS * (bx - ax), ay + rS * (@by - ay), 0);

            return new ProjectionInfo(isOnSegment, pointSegment, pointLine);
        }
    }
}
