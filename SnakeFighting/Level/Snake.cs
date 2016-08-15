using SnakeFighting.Helper;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace SnakeFighting.Level
{
    public class StatusChangedEventArgs : EventArgs
    {
        public SnakeStatus PrevStatus { private set; get; }
        public StatusChangedEventArgs(SnakeStatus prevStatus)
        {
            PrevStatus = prevStatus;
        }
    }
    public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
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

        private List<Vector> body = new List<Vector>();
        public double Length { private set; get; }

        #region Visual
        private List<Vector> deathBody = new List<Vector>();
        private SnakeSkin skin = SnakeSkin.Default;
        #endregion

        #region Game runnning status
        private Timer stunnedTimer;
        private Timer afterStunnedTimer;
        private Timer superTimer;
        /// <summary>
        /// May not use it directly
        /// </summary>
        private SnakeStatus _status = SnakeStatus.Normal;
        public SnakeStatus Status
        {
            set
            {
                SnakeStatus ps = _status;
                var e = new StatusChangedEventArgs(ps);
                _status = value;
                OnStatusChanged(e);
                StatusChanged?.Invoke(this, e);
            }
            get { return _status; }
        }
        public event StatusChangedEventHandler StatusChanged;
        public int StunnedCount { private set; get; }

        public IReadOnlyList<Vector> Body => body;
        public Vector HeadPrevPosition { private set; get; }
        #endregion

        #region Properties
        public double Width { private set; get; }
        public double WidthBeginPercentage { private set; get; }
        public double WidthEndPercentage { private set; get; }
        public double TurningRadius { private set; get; }
        public double Velocity { private set; get; }
        #endregion

        public Snake(Vector location) : this(location, 100, SnakeProperties.Default)
        {
        }

        public Snake(Vector location, double length, SnakeProperties properties)
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
            
            StunnedCount = 0;
            stunnedTimer = new Timer(3000);
            stunnedTimer.AutoReset = false;
            stunnedTimer.Elapsed += StunnedTimer_Elapsed;
            afterStunnedTimer = new Timer(2000);
            afterStunnedTimer.AutoReset = false;
            afterStunnedTimer.Elapsed += AfterStunnedTimer_Elapsed;
            superTimer = new Timer(3000);
            superTimer.AutoReset = false;
            superTimer.Elapsed += SuperTimer_Elapsed;

            body.Add(location);
            body.Add(new Vector(0, -length) + location);
        }

        private void SuperTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _status &= ~SnakeStatus.Super;
        }

        private void AfterStunnedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _status &= ~SnakeStatus.AfterStunned;
        }

        private void StunnedTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _status &= ~SnakeStatus.Stunned;
            Status |= SnakeStatus.AfterStunned;
        }

        protected virtual void OnStatusChanged(StatusChangedEventArgs e)
        {
            if (!e.PrevStatus.HasFlag(SnakeStatus.Stunned) && _status.HasFlag(SnakeStatus.Stunned))
            {
                StunnedCount++;
                if (stunnedTimer.Enabled)
                    stunnedTimer.Stop();
                stunnedTimer.Start();
            }
            if (!e.PrevStatus.HasFlag(SnakeStatus.AfterStunned) && _status.HasFlag(SnakeStatus.AfterStunned))
            {
                if (afterStunnedTimer.Enabled)
                    afterStunnedTimer.Stop();
                afterStunnedTimer.Start();
            }
            if (!e.PrevStatus.HasFlag(SnakeStatus.Super) && _status.HasFlag(SnakeStatus.Super))
            {
                if (superTimer.Enabled)
                    superTimer.Stop();
                superTimer.Start();
            }
        }

        protected virtual void OnActionMeetConstraint(EventArgs e)
        {
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
            cdir = MathExtension.VZero;
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
            if (action.Toward == MathExtension.VZero)
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
                    dt /= segLen;
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

            if (Length == 0)
                return;

            if (Status.HasFlag(SnakeStatus.Stunned))
                return;

            Action action = TakeAction(elapsedTime);
            ConstrainedAction caction = GetConstrainedAction(action, elapsedTime);
            if (caction.InConstraint)
                OnActionMeetConstraint(new EventArgs());

            HeadPrevPosition = body[0];
            MoveBody(elapsedTime, caction);
        }

        public void Rebound(Vector p, Vector nor)
        {
            nor.Normalize();
            Vector dir = body[0] - body[1];
            Vector ndir = -2 * Vector.Multiply(dir, nor) * nor + dir;
            ndir.Normalize();
            Vector dt = body[0] - p;
            Vector h = p + ndir * dt.Length;
            body[0] = p;
            body.Insert(0, h);
        }

        public void FractureAt(double length)
        {
            if (length > Length)
                return;
            Length = length;
            double curlen = 0;
            for (int i = 0; i < body.Count - 1; i++)
            {
                Vector dir = body[i + 1] - body[i];
                double len = dir.Length;
                if (curlen + len > length)
                {
                    body.RemoveRange(i + 1, body.Count - (i + 1));
                    dir *= (length - curlen) / len;
                    body.Add(body[i] + dir);
                    break;
                }
                curlen += len;
            }
        }

        public void Lengthen(double dt)
        {
            if (dt <= 0)
                return;
            Length += dt;
        }

        public void Render(Graphics g)
        {
            skin.Render(g, this);
        }
    }
}
