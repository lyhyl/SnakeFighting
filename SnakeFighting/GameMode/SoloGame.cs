using SnakeFighting.Helper;
using SnakeFighting.Level;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System;
using System.Windows;

namespace SnakeFighting.GameMode
{
    public class SoloGame : Game
    {
        private object locker = new object();
        private PlayerSnake player = new PlayerSnake();
        private List<AISnake> aiSnakes = new List<AISnake>();
        private List<Snake> snakes = new List<Snake>();

        public SoloGame(Form form) : base(form)
        {
            AISnake ai = new AISnake();
            aiSnakes.Add(ai);

            snakes.Add(ai);
            snakes.Add(player);
        }

        protected override void OnMouseClick(MouseButtons button, Vector location)
        {
            if (button == MouseButtons.Right)
                lock (locker)
                    player.HeadTo(location);
        }

        protected override void RunLogical(long elapsedTime)
        {
            lock (locker)
            {
                double ets = elapsedTime / 1000.0f;
                player.Move(ets);
                foreach (var sk in aiSnakes)
                    sk.Move(ets);
            }
            CheckCollision();
        }

        private void CheckCollision()
        {
            // self collision
            for (int i = 0; i < snakes.Count; i++)
            {
                Snake sk = snakes[i];
                double pt;
                double w = sk.Width * sk.WidthBeginPercentage;
                bool es = CheckSelfCollision(sk.PrevHead, sk.Body[0], w, sk, out pt);
                if (es)
                    sk.pen = Pens.Red;
                else
                    sk.pen = Pens.Black;
            }
            // pair collision
            for (int i = 0; i < snakes.Count; i++)
                for (int j = i + 1; j < snakes.Count; j++)
                {
                    Snake sa = snakes[i], sb = snakes[j];
                    // Snake A eat B?
                    double apt, bpt;
                    double saw = sa.Width * sa.WidthBeginPercentage;
                    double sbw = sb.Width * sb.WidthBeginPercentage;
                    bool aeb = CheckHeadCollision(sa.PrevHead, sa.Body[0], saw, sb, out apt);
                    bool bea = CheckHeadCollision(sb.PrevHead, sb.Body[0], sbw, sa, out bpt);
                    if (aeb)
                        sa.pen = Pens.Blue;
                    if (bea)
                        sb.pen = Pens.Blue;
                }
        }

        private bool CheckHeadCollision(Vector ph, Vector ch, double w, Snake sk, out double it)
        {
            double curlen = 0;
            for (int i = 0; i < sk.Body.Count - 1; i++)
            {
                double s, t, distsq;
                distsq = IntersectionSolver.ClosestSegSeg(ph, ch, sk.Body[i], sk.Body[i + 1], out s, out t);
                double len = (sk.Body[i] - sk.Body[i + 1]).Length;
                it = curlen + t * len;
                double plen = it / sk.Length;
                double tw = MathHelper.Map(plen, sk.WidthBeginPercentage, sk.WidthEndPercentage) * sk.Width;
                double ttw = tw + w;
                if (distsq <= ttw * ttw)
                    return true;
                curlen += len;
            }
            it = 0;
            return false;
        }

        private bool CheckSelfCollision(Vector ph, Vector ch, double w, Snake sk, out double it)
        {
            double thr = sk.TurningRadius * Math.PI; // half
            double curlen = 0;
            int i = 0;
            for (; i < sk.Body.Count - 1; i++)
            {
                double len = (sk.Body[i] - sk.Body[i + 1]).Length;
                if (curlen + len >= thr)
                    break;
                curlen += len;
            }
            for (; i < sk.Body.Count - 1; i++)
            {
                double s, t, distsq;
                distsq = IntersectionSolver.ClosestSegSeg(ph, ch, sk.Body[i], sk.Body[i + 1], out s, out t);
                double len = (sk.Body[i] - sk.Body[i + 1]).Length;
                it = curlen + t * len;
                double plen = it / sk.Length;
                double tw = MathHelper.Map(plen, sk.WidthBeginPercentage, sk.WidthEndPercentage) * sk.Width;
                double ttw = tw + w;
                if (it >= thr && distsq <= ttw * ttw)
                    return true;
                curlen += len;
            }
            it = 0;
            return false;
        }

        protected override void Render(Graphics graphics)
        {
            lock (locker)
            {
                player.Render(graphics);
                foreach (var sk in aiSnakes)
                    sk.Render(graphics);
            }
        }
    }
}
