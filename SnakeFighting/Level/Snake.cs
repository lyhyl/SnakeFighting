using SnakeFighting.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;

namespace SnakeFighting.Level
{
    public abstract class Snake
    {
        private struct ConstrainedAction
        {
            public bool InConstraint;
            public bool BodyRequireChange;
            public Vector Toward;
        }

        protected struct Action
        {
            public Vector Toward;
        }

        #region Visual
        private List<Vector> deathBody = new List<Vector>();
        #endregion

        private List<Vector> body = new List<Vector>();
        protected bool PrevActInConstraint { set; get; }

        public IReadOnlyList<Vector> Body => body;

        public Vector PrevHead { private set; get; }

        #region Properties
        public double Length { private set; get; }
        public double Width { private set; get; }
        public double WidthBeginPercentage { private set; get; }
        public double WidthEndPercentage { private set; get; }
        public double Velocity { private set; get; }
        public double TurningRadius { private set; get; }
        #endregion

        public Snake() : this(300, SnakeProperties.Default)
        {
        }

        public Snake(double length, SnakeProperties properties)
        {
            Length = length;
            if (length <= 0)
                throw new ArgumentException("`length` must greater than 0");
            Width = properties.Width;
            WidthBeginPercentage = properties.WidthBeginPercentage;
            WidthEndPercentage = properties.WidthEndPercentage;
            Velocity = properties.Velocity;
            if (Velocity <= 0)
                throw new ArgumentException("`velocity` must greater than 0");
            TurningRadius = properties.TurningRadius;

            body.Add(VectorExtsion.Zero);
            body.Add(new Vector(0, -length));
            PrevActInConstraint = true;
        }

        /// <summary>
        /// Clamp moving direction into maximum turning radius
        /// </summary>
        /// <param name="elapsedTime">Elapsed time</param>
        /// <param name="dir">Direction to be clmap</param>
        /// <param name="cdir">Clamped direction</param>
        /// <returns></returns>
        private bool ClampDirection(double elapsedTime, Vector dir, out Vector cdir)
        {
            cdir = VectorExtsion.Zero;
            double d = Velocity * elapsedTime;
            if (d < TurningRadius * 2)
            {
                Vector bdir = Body[0] - Body[1];
                bdir.Normalize();
                double cosCurAng = MathHelper.Clamp(-1, Vector.Multiply(dir, bdir), 1);
                double curAng = Math.Acos(cosCurAng);

                double cosIdealAng = d / (TurningRadius * 2);
                double idealAng = Math.Acos(cosIdealAng);
                double maxAng = Math.PI / 2 - idealAng;

                if (Math.Abs(curAng) > Math.Abs(maxAng))
                {
                    int negWay = Math.Sign(Vector.CrossProduct(bdir, dir) * maxAng);
                    double sinx = negWay * cosIdealAng;
                    double cosx = Math.Sin(idealAng);
                    cdir.X = (bdir.X * cosx - bdir.Y * sinx);
                    cdir.Y = (bdir.X * sinx + bdir.Y * cosx);
                    return true;
                }
            }
            return false;
        }

        private ConstrainedAction GetConstrainedAction(Action action, double elapsedTime)
        {
            ConstrainedAction caction = new ConstrainedAction();
            if (action.Toward == VectorExtsion.Zero)
            {
                caction.BodyRequireChange = false;
                caction.InConstraint = true;
                caction.Toward = body[0] - body[1];
                caction.Toward.Normalize();
            }
            else
            {
                Vector cdir, dir = action.Toward;
                dir.Normalize();
                caction.BodyRequireChange = true;
                if (ClampDirection(elapsedTime, dir, out cdir))
                {
                    caction.InConstraint = false;
                    caction.Toward = cdir;
                }
                else
                {
                    caction.InConstraint = true;
                    caction.Toward = dir;
                }
            }
            return caction;
        }

        private void MoveBody(double elapsedTime, ConstrainedAction caction)
        {
            // Move head
            if (caction.BodyRequireChange)
                body.Insert(0, body[0]);
            body[0] += caction.Toward * Velocity * elapsedTime;

            // Crawl
            double curLen = 0;
            for (int i = 1; i < Body.Count; i++)
            {
                Vector dt = Body[i] - Body[i - 1];
                double segLen = dt.Length;
                if (curLen + segLen < Length)
                    curLen += segLen;
                else
                {
                    body.RemoveRange(i, Body.Count - i);
                    double remainLen = Length - curLen;
                    dt.Normalize();
                    body.Add(body[i - 1] + dt * remainLen);
                    break;
                }
            }
        }

        protected abstract Action TakeAction(double elapsedTime);

        public void Move(double elapsedTime)
        {
            if (body.Count < 2)
                return;

            Action action = TakeAction(elapsedTime);
            ConstrainedAction caction = GetConstrainedAction(action, elapsedTime);
            PrevActInConstraint = caction.InConstraint;

            PrevHead = body[0];
            MoveBody(elapsedTime, caction);
        }

        public Pen pen = Pens.Black;
        public void Render(Graphics g)
        {
            double dtPercentage = WidthBeginPercentage - WidthEndPercentage;
            double prevPerc = 1;
            double curLen = 0;
            Vector dir;
            float w = (float)(Width * 2);
            g.DrawEllipse(Pens.Gray, (float)(Body[0].X - Width), (float)(Body[0].Y - Width), w, w);
            for (int i = 0; i < Body.Count - 1; i++)
            {
                //g.DrawLine(Pens.Gray, body[i], body[i + 1]);
                dir = Body[i] - Body[i + 1];
                double dlen = dir.Length;
                curLen += dlen;
                double curPerc = 1 - curLen / Length;
                double prevRPerc = prevPerc * dtPercentage + WidthEndPercentage;
                double curRPerc = curPerc * dtPercentage + WidthEndPercentage;
                Vector nor = new Vector(-dir.Y / dlen, dir.X / dlen);
                nor *= Width;
                //g.DrawEllipse(Pens.Gray,
                //    body[i + 1].X - Width * curRPerc, body[i + 1].Y - Width * curRPerc,
                //    Width * curRPerc * 2, Width * curRPerc * 2);
                g.DrawLine(pen, (Body[i] + nor * prevRPerc).ToPointF(), (Body[i + 1] + nor * curRPerc).ToPointF());
                g.DrawLine(pen, (Body[i] - nor * prevRPerc).ToPointF(), (Body[i + 1] - nor * curRPerc).ToPointF());
                prevPerc = curPerc;
            }
            dir = body[0] - body[1];
            dir.Normalize();
            g.DrawLine(Pens.Red, Body[0].ToPointF(), (Body[0] + dir * 10).ToPointF());
        }
    }
}
