using Snake.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake.GameMode
{
    public class SoloGame : Game
    {
        private object locker = new object();
        private Snake snake = new Snake();
        private List<Snake> aiSnakes = new List<Snake>();

        public SoloGame(Form form) : base(form)
        {
        }

        protected override void OnMouseClick(MouseButtons button, Vector location)
        {
            if (button == MouseButtons.Right)
                lock (locker)
                    snake.HeadTo(location);
        }

        protected override void RunLogical(long elapsedTime)
        {
            lock (locker)
            {
                float ets = elapsedTime / 1000.0f;
                snake.Move(ets);
                foreach (var sk in aiSnakes)
                    sk.Move(ets);
            }
        }

        protected override void Render(Graphics graphics)
        {
            lock (locker)
            {
                snake.Render(graphics);
                foreach (var sk in aiSnakes)
                    sk.Render(graphics);
            }
        }
    }
}
