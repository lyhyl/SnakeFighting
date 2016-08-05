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

        public void HeadTo(Vector target)
        {
            PrevActInConstraint = false;
            this.target = target;
        }

        protected override Action TakeAction(double elapsedTime)
        {
            if (PrevActInConstraint)
                target = null;
            return new Action() { Toward = target.HasValue ? target.Value - Body[0] : VectorExtsion.Zero };
        }
    }
}
