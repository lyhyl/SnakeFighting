using System;
using System.Drawing;

namespace Snake.Helper
{
    public struct Vector
    {
        public float X, Y;

        public static readonly Vector Zero = new Vector(0, 0);
        public static readonly Vector XAxis = new Vector(1, 0);
        public static readonly Vector YAxis = new Vector(0, 1);

        public float LengthSq => X * X + Y * Y;

        public float Length => (float)Math.Sqrt(LengthSq);

        public Vector(float x, float y)
        {
            X = x;
            Y = y;
        }

        public void Normalize()
        {
            float len = Length;
            if (len == 0)
                return;
            X /= len;
            Y /= len;
        }

        public static float Dot(Vector a,Vector b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static float Cross(Vector a, Vector b)
        {
            return a.X * b.Y - a.Y * b.X;
        }

        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        public static Vector operator -(Vector v)
        {
            return new Vector(-v.X, -v.Y);
        }

        public static Vector operator *(Vector v, float s)
        {
            return new Vector(v.X * s, v.Y * s);
        }

        public static Vector operator *(float s, Vector v)
        {
            return new Vector(v.X * s, v.Y * s);
        }

        public static implicit operator PointF(Vector v)
        {
            return new PointF(v.X, v.Y);
        }

        public override string ToString()
        {
            return $"({X},{Y})";
        }
    }
}
