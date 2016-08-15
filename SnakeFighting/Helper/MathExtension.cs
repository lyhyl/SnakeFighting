using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;

namespace SnakeFighting.Helper
{
    public static class MathExtension
    {
        private static Random rand = new Random((int)(~DateTime.Now.ToBinary() & 0xffffffff));

        public static PointF ToPointF(this Vector v) => new PointF((float)v.X, (float)v.Y);

        public static readonly Vector VZero = new Vector(0, 0);

        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                T value = list[j];
                list[j] = list[i];
                list[i] = value;
            }
        }
    }
}
