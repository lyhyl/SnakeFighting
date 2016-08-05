using SnakeFighting.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnakeFighting.Level
{
    public class AISnake : Snake
    {
        private Random rand = new Random();

        protected override Action TakeAction(double elapsedTime)
        {
            if (PrevActInConstraint)
            {
                Vector tar = VectorExtsion.Zero;
                tar.X = ((rand.NextDouble() - .5) * 2 * 100);
                tar.Y = ((rand.NextDouble() - .5) * 2 * 100);
                return new Action() { Toward = tar - Body[0] };
            }
            else
                return new Action();
        }
    }
}
