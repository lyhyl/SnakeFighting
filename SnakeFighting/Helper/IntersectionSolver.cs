using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnakeFighting.Helper
{
    public static class IntersectionSolver
    {
        public static readonly double Epsilon = 1e-8f;

        public static Vector ClosestPtSeg(Vector c, Vector a, Vector b, out double t)
        {
            Vector ab = b - a;
            t = Vector.Multiply(c - a, ab) / Vector.Multiply(ab, ab);
            t = MathHelper.Clamp(0, t, 1);
            return a + t * ab;
        }

        public static double ClosestSegSeg(
            Vector p1, Vector q1,
            Vector p2, Vector q2,
            out double s, out double t)
        {
            Vector d1 = q1 - p1, d2 = q2 - p2;
            Vector r = p1 - p2;
            double a = Vector.Multiply(d1, d1);
            double e = Vector.Multiply(d2, d2);
            double f = Vector.Multiply(d2, r);
            if (a <= Epsilon && e <= Epsilon)
            {
                s = t = 0;
                return Vector.Multiply(r, r);
            }
            if (a <= Epsilon)
            {
                s = 0;
                t = MathHelper.Clamp(f / e, 0, 1);
            }
            else
            {
                double c = Vector.Multiply(d1, r);
                if (e <= Epsilon)
                {
                    t = 0;
                    s = MathHelper.Clamp(-c / a, 0, 1);
                }
                else
                {
                    double b = Vector.Multiply(d1, d2);
                    double denom = a * e - b * b;
                    if (denom != 0)
                        s = MathHelper.Clamp((b * f - c * e) / denom, 0, 1);
                    else
                        s = 0;
                    t = (b * s + f) / e;
                    if (t < 0)
                    {
                        t = 0;
                        s = MathHelper.Clamp(-c / a, 0, 1);
                    }
                    else if (t > 1)
                    {
                        t = 1;
                        s = MathHelper.Clamp((b - c) / a, 0, 1);
                    }
                }
            }
            Vector c1 = p1 + d1 * s;
            Vector c2 = p2 + d2 * t;
            Vector dc = c1 - c2;
            return Vector.Multiply(dc, dc);
        }

        public static double SignedTriArea(Vector a, Vector b, Vector c)
        {
            return (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
        }

        public static bool TestSegSeg(
            Vector a, Vector b,
            Vector c, Vector d,
            out Vector ipt)
        {
            double a1 = SignedTriArea(a, b, d);
            double a2 = SignedTriArea(a, b, c);
            if (a1 * a2 < 0)
            {
                double a3 = SignedTriArea(c, d, a);
                double a4 = a3 + a2 - a1;
                if (a3 * a4 < 0)
                {
                    double t = a3 / (a3 - a4);
                    ipt = a + t * (b - a);
                    return true;
                }
            }
            ipt = new Vector(0,0);
            return false;
        }
    }
}
