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
    public delegate void GameEventHandler(object sender, EventArgs args);
    public abstract class Game
    {
        private bool running = true;
        private Vector offset = VectorExtsion.Zero;

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
            OnStarted();
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
            OnExited();
        }

        private void Form_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.TranslateTransform((float)offset.X, (float)offset.Y);
            Render(e.Graphics);
        }

        private void Form_MouseClick(object sender, MouseEventArgs e)
        {
            OnMouseClick(e.Button, new Vector(e.X, e.Y) - offset);
        }

        protected virtual void OnExited()
        {
            Exited?.Invoke(this, new EventArgs());
        }

        protected virtual void OnStarted()
        {
            Started?.Invoke(this, new EventArgs());
        }

        protected abstract void OnMouseClick(MouseButtons button, Vector location);
        protected abstract void RunLogical(long elapsedTime);
        protected abstract void Render(Graphics graphics);
    }
}
