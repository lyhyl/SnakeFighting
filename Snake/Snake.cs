using Snake.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Snake
{
    public class Snake
    {
        #region Visual
        private List<Vector> deathBody = new List<Vector>();
        private float beginPercentage = 1, endPercentage = .3f;
        #endregion

        private Vector? target = null;
        private Vector direction = Vector.YAxis;
        private List<Vector> body = new List<Vector>();

        public float Length { set; get; }
        public float Width { set; get; }
        public float Velocity { set; get; }
        public float TurningRadius { set; get; }

        public Snake() : this(300, 100)
        {
        }

        public Snake(float length, float velocity)
        {
            if (length <= 0)
                throw new ArgumentException("`length` must greater than 0");
            if (velocity <= 0)
                throw new ArgumentException("`velocity` must greater than 0");

            body.Add(Vector.Zero);
            body.Add(Vector.YAxis * -length);

            Length = length;
            Width = 6;
            Velocity = velocity;
            TurningRadius = 20;
        }

        private void TurnHead(float elapsedTime)
        {
            if (body.Count < 2 || !target.HasValue)
                return;
            float d = Velocity * elapsedTime;
            Vector dir = target.Value - body[0];
            dir.Normalize();
            if (d >= TurningRadius * 2)
            {
                direction = dir;
                target = null;
            }
            else
            {
                Vector bdir = body[0] - body[1];
                bdir.Normalize();
                float cosIdealAng = d / (TurningRadius * 2);
                float cosCurAng = Math.Min(Math.Max(-1, Vector.Dot(dir, bdir)), 1);
                double curAng = Math.Acos(cosCurAng);
                double idealAng = Math.Acos(cosIdealAng);
                double maxAng = Math.PI / 2 - idealAng;

                if (Math.Abs(curAng) > Math.Abs(maxAng))
                {
                    bool negWay = Vector.Cross(bdir, dir) * maxAng < 0;
                    double sinx = negWay ? -cosIdealAng : cosIdealAng;
                    double cosx = Math.Sin(idealAng);
                    direction.X = (float)(bdir.X * cosx - bdir.Y * sinx);
                    direction.Y = (float)(bdir.X * sinx + bdir.Y * cosx);
                }
                else
                {
                    direction = dir;
                    target = null;
                }
            }
            body.Insert(0, body.First());
        }

        public void HeadTo(Vector target)
        {
            this.target = target;
        }

        public void Move(float elapsedTime)
        {
            TurnHead(elapsedTime);

            if (body.Count > 0)
                body[0] += direction * Velocity * elapsedTime;

            float curLen = 0;
            for (int i = 1; i < body.Count; i++)
            {
                Vector dt = body[i] - body[i - 1];
                float segLen = dt.Length;
                if (curLen + segLen < Length)
                    curLen += segLen;
                else
                {
                    body.RemoveRange(i, body.Count - i);
                    float remainLen = Length - curLen;
                    dt.Normalize();
                    body.Add(body[i - 1] + dt * remainLen);
                    break;
                }
            }
        }

        public void Render(Graphics g)
        {
            float dtPercentage = beginPercentage - endPercentage;
            float prevPerc = 1;
            float curLen = 0;
            g.DrawEllipse(Pens.Gray, body[0].X - Width, body[0].Y - Width, Width * 2, Width * 2);
            for (int i = 0; i < body.Count - 1; i++)
            {
                //g.DrawLine(Pens.Gray, body[i], body[i + 1]);
                Vector dir = body[i] - body[i + 1];
                float dlen = dir.Length;
                curLen += dlen;
                float curPerc = 1 - curLen / Length;
                float prevRPerc = prevPerc * dtPercentage + endPercentage;
                float curRPerc = curPerc * dtPercentage + endPercentage;
                Vector nor = new Vector(-dir.Y / dlen, dir.X / dlen);
                nor *= Width;
                //g.DrawEllipse(Pens.Gray,
                //    body[i + 1].X - Width * curRPerc, body[i + 1].Y - Width * curRPerc,
                //    Width * curRPerc * 2, Width * curRPerc * 2);
                g.DrawLine(Pens.Black, body[i] + nor * prevRPerc, body[i + 1] + nor * curRPerc);
                g.DrawLine(Pens.Black, body[i] - nor * prevRPerc, body[i + 1] - nor * curRPerc);
                prevPerc = curPerc;
            }
            g.DrawLine(Pens.Red, body[0], body[0] + direction * 10);
        }
    }
}
