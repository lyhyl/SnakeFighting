using SnakeFighting.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace SnakeFighting.GameMode
{
    public delegate void GameEventHandler(object sender, EventArgs e);
    public class MouseClickEventArgs : EventArgs
    {
        public Vector Location { private set; get; }
        public MouseButtons Button { private set; get; }
        public MouseClickEventArgs(Vector location,MouseButtons button)
        {
            Location = location;
            Button = button;
        }
    }
    public abstract class Game
    {
        private bool running = true;
        private Vector offset = MathExtension.VZero;

        protected double LockFPS { set; get; }
        protected Form MainForm { set; get; }

        public event GameEventHandler Exited;
        public event GameEventHandler Started;

        public Game(Form form)
        {
            MainForm = form;
            LockFPS = 60;
            offset = new Vector(MainForm.Width / 2, MainForm.Height / 2);
            form.SizeChanged += (s, e) => offset = new Vector(MainForm.Width / 2, MainForm.Height / 2);
            form.MouseClick += Form_MouseClick;
            form.Paint += Form_Paint;
        }

        public void Run()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            long prevTime = watch.ElapsedMilliseconds;
            OnStarted(new EventArgs());
            while (running)
            {
                long currTime = watch.ElapsedMilliseconds;
                long elapsedTime = currTime - prevTime;
                if (elapsedTime < 1000 / LockFPS)
                    continue;
                prevTime = currTime;
                RunLogical(elapsedTime);
                MainForm.Invalidate();
            }
            OnExited(new EventArgs());
        }

        private void Form_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform((float)offset.X, (float)offset.Y);
            Render(e.Graphics);
        }

        private void Form_MouseClick(object sender, MouseEventArgs e)
        {
            OnMouseClick(new MouseClickEventArgs(new Vector(e.X, e.Y) - offset, e.Button));
        }

        protected virtual void OnExited(EventArgs e)
        {
            Exited?.Invoke(this, e);
        }

        protected virtual void OnStarted(EventArgs e)
        {
            Started?.Invoke(this, e);
        }

        protected abstract void OnMouseClick(MouseClickEventArgs e);
        protected abstract void RunLogical(long elapsedTime);
        protected abstract void Render(Graphics graphics);
    }
}
