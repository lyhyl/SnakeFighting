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
        private struct ContactInfo
        {
            public Vector Position;
            public Vector Normal;
            public ContactInfo(Vector p, Vector n)
            {
                Position = p;
                Normal = n;
            }
        }

        private object locker = new object();

        private Random rand = new Random();
        private Timer genCellTimer = new Timer();
        private bool genCell = false;
        private Vector cellPos = new Vector(10000, 10000);

        private PlayerSnake player = new PlayerSnake(MathExtension.VZero);
        private List<AISnake> aiSnakes = new List<AISnake>();
        private List<Snake> snakes = new List<Snake>();

        private Rectangle bound = new Rectangle(-300, -300, 600, 600);

        public SoloGame(Form form) : base(form)
        {
            genCellTimer.Tick += (s, e) => genCell = true;
            genCellTimer.Interval = 5000;
            genCellTimer.Start();

            AISnake ai = new AISnake(new Vector(100, 0));
            aiSnakes.Add(ai);

            snakes.Add(ai);
            snakes.Add(player);
        }

        protected override void OnMouseClick(MouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
                lock (locker)
                    player.HeadTo(e.Location);
        }

        protected override void RunLogical(long elapsedTime)
        {
            GenerateSuperCell();

            lock (locker)
            {
                double ets = elapsedTime / 1000.0f;
                player.Move(ets);
                foreach (var sk in aiSnakes)
                    sk.Move(ets);
            }

            var boundCollision = CheckBoundCollision();
            var selfCollision = CheckSelfCollision();
            var pairCollision = CheckPlayerCollision();
            var scCollision = CheckSuperCellCollision();

            lock (locker)
            {
                // Applay bound collision
                foreach (var col in boundCollision)
                {
                    if (!col.Item1.Status.HasFlag(SnakeStatus.Stunned) &&
                        !col.Item1.Status.HasFlag(SnakeStatus.AfterStunned))
                        col.Item1.Status |= SnakeStatus.Stunned;
                    col.Item1.Rebound(col.Item2.Position, col.Item2.Normal);
                }
                // Apply self collision
                foreach (var col in selfCollision)
                    col.Key.Status |= SnakeStatus.Stunned;
                // Apply pair collision
                Fracture(pairCollision);
                // Apply eat super cell
                scCollision.Shuffle();
                foreach (var col in scCollision)
                    if (!col.Status.HasFlag(SnakeStatus.Stunned))
                    {
                        col.Status |= SnakeStatus.Super;
                        col.Lengthen(20);
                        break;
                    }
            }
        }

        private void GenerateSuperCell()
        {
            if (!genCell)
                return;
            genCell = false;
            double x = rand.NextDouble() * bound.Width + bound.X;
            double y = rand.NextDouble() * bound.Height + bound.Y;
            cellPos = new Vector(x, y);
        }

        private void Fracture(List<Tuple<Snake, Snake, double>> collision)
        {
            foreach (var col in collision)
            {
                switch (col.Item1.Status)
                {
                    case SnakeStatus.Normal:
                        col.Item1.Status = SnakeStatus.Stunned;
                        break;
                    case SnakeStatus.Super:
                        if (col.Item2.Status.HasFlag(SnakeStatus.Super))
                            col.Item1.Status |= SnakeStatus.Stunned;
                        else
                            col.Item2.FractureAt(col.Item3);
                        break;
                }
            }
        }

        private List<Tuple<Snake, Snake, double>> CheckPlayerCollision()
        {
            // pair collision
            var collision = new List<Tuple<Snake, Snake, double>>();
            for (int i = 0; i < snakes.Count; i++)
            {
                Snake sa = snakes[i];
                if (sa.Status.HasFlag(SnakeStatus.AfterStunned))
                    continue;
                for (int j = i + 1; j < snakes.Count; j++)
                {
                    Snake sb = snakes[j];
                    if (sb.Status.HasFlag(SnakeStatus.AfterStunned))
                        continue;
                    // Snake A eat B?
                    double apt, bpt;
                    double saw = sa.Width * sa.WidthBeginPercentage;
                    double sbw = sb.Width * sb.WidthBeginPercentage;
                    if (IsHeadCollision(sa.HeadPrevPosition, sa.Body[0], saw, sb, out apt))
                        collision.Add(new Tuple<Snake, Snake, double>(sa, sb, apt));
                    if (IsHeadCollision(sb.HeadPrevPosition, sb.Body[0], sbw, sa, out bpt))
                        collision.Add(new Tuple<Snake, Snake, double>(sb, sa, bpt));
                }
            }
            return collision;
        }

        private bool IsHeadCollision(Vector ph, Vector ch, double w, Snake sk, out double it)
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

        private Dictionary<Snake, double> CheckSelfCollision()
        {
            var collision = new Dictionary<Snake, double>();
            for (int i = 0; i < snakes.Count; i++)
            {
                Snake sk = snakes[i];
                if (sk.Status.HasFlag(SnakeStatus.AfterStunned))
                    continue;
                double pt, w = sk.Width * sk.WidthBeginPercentage;
                if (IsSelfCollision(sk.HeadPrevPosition, sk.Body[0], w, sk, out pt))
                {
                    if (collision.ContainsKey(sk))
                        collision[sk] = Math.Min(collision[sk], pt);
                    else
                        collision[sk] = pt;
                }
            }

            return collision;
        }

        private bool IsSelfCollision(Vector ph, Vector ch, double w, Snake sk, out double it)
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

        private List<Snake> CheckSuperCellCollision()
        {
            List<Snake> collision = new List<Snake>();
            foreach (var sk in snakes)
            {
                double w = sk.Width + 5;
                if ((sk.Body[0] - cellPos).LengthSquared <= w * w)
                    collision.Add(sk);
            }
            if (collision.Count != 0)
                cellPos = new Vector(10000, 10000);
            return collision;
        }

        private List<Tuple<Snake, ContactInfo>> CheckBoundCollision()
        {
            var collision = new List<Tuple<Snake, ContactInfo>>();
            foreach (var sk in snakes)
            {
                Vector head = sk.Body[0];
                Vector p = head, n = new Vector(0, 0);
                if (head.X < bound.Left)
                {
                    p.X = bound.Left;
                    n.X = 1;
                }
                if (head.X > bound.Right)
                {
                    p.X = bound.Right;
                    n.X = -1;
                }
                if (head.Y < bound.Top)
                {
                    p.Y = bound.Top;
                    n.Y = 1;
                }
                if (head.Y > bound.Bottom)
                {
                    p.Y = bound.Bottom;
                    n.Y = -1;
                }
                if (n != MathExtension.VZero)
                    collision.Add(new Tuple<Snake, ContactInfo>(sk, new ContactInfo(p, n)));
            }
            return collision;
        }

        protected override void Render(Graphics graphics)
        {
            lock (locker)
            {
                player.Render(graphics);
                foreach (var sk in aiSnakes)
                    sk.Render(graphics);
                graphics.DrawEllipse(Pens.Green, (float)(cellPos.X - 5), (float)(cellPos.Y - 5), 10, 10);
                graphics.DrawRectangle(Pens.Brown, bound);
            }
        }
    }
}
