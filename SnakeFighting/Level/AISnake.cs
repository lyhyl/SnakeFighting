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
        private bool prevActMeetConstraint = false;

        public AISnake(Vector offset) : base(offset)
        {
        }
        
        protected override void OnActionMeetConstraint(EventArgs e)
        {
            base.OnActionMeetConstraint(e);
            prevActMeetConstraint = true;
        }

        protected override Action TakeAction(double elapsedTime)
        {
            if (prevActMeetConstraint)
            {
                Vector tar = MathExtension.VZero;
                tar.X = ((rand.NextDouble() - .5) * 2 * 400);
                tar.Y = ((rand.NextDouble() - .5) * 2 * 400);
                return new Action() { Toward = tar - Body[0] };
            }
            else
            {
                prevActMeetConstraint = false;
                return new Action();
            }
        }
    }
}
