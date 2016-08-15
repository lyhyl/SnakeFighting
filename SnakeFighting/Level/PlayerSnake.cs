using SnakeFighting.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnakeFighting.Level
{
    public class PlayerSnake : Snake
    {
        private Vector? target = null;
        private Vector direction = new Vector(0, 1);
        private bool prevActMeetConstraint = false;

        public PlayerSnake(Vector offset) : base(offset)
        {
        }

        protected override void OnActionMeetConstraint(EventArgs e)
        {
            base.OnActionMeetConstraint(e);
            prevActMeetConstraint = true;
        }

        public void HeadTo(Vector target)
        {
            this.target = target;
            prevActMeetConstraint = false;
        }

        protected override Action TakeAction(double elapsedTime)
        {
            if (prevActMeetConstraint)
                target = null;
            prevActMeetConstraint = false;
            return new Action() { Toward = target.HasValue ? target.Value - Body[0] : MathExtension.VZero };
        }
    }
}
