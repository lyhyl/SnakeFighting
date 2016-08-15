using System;

namespace SnakeFighting.Helper
{
    public static class MathHelper
    {
        public static double Clamp(double v, double min, double max)
        {
            return Math.Min(Math.Max(min, v), max);
        }

        public static int Clamp(int v, int min, int max)
        {
            return Math.Min(Math.Max(min, v), max);
        }

        public static double Map(double v, double min, double max)
        {
            return v * (max - min) + min;
        }
        
        public static double Map(double v, double min0, double max0, double min1, double max1)
        {
            return (v - min0) / (max0 - min0) * (max1 - min1) + min1;
        }
    }
}
