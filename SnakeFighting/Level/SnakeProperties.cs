using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnakeFighting.Level
{
    public struct SnakeProperties
    {
        public double Velocity;
        public double Width;
        public double WidthBeginPercentage;
        public double WidthEndPercentage;
        public double TurningRadius;

        public static readonly SnakeProperties Default = new SnakeProperties()
        {
            Velocity = 100,
            Width = 6,
            WidthBeginPercentage = 1,
            WidthEndPercentage = .3f,
            TurningRadius = 10
        };
    }
}
