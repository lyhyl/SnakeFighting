using System;
using System.Drawing;
using System.Windows;

namespace SnakeFighting.Helper
{
    public static class VectorExtsion
    {
        public static PointF ToPointF(this Vector v) => new PointF((float)v.X, (float)v.Y);

        public static readonly Vector Zero = new Vector(0, 0);
    }
}
