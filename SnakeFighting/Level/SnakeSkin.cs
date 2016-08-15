using SnakeFighting.Helper;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnakeFighting.Level
{
    public abstract class SnakeSkin
    {
        public abstract void Render(Graphics g, Snake s);

        public static readonly SnakeSkin Default = new SnakeClassicalSkin();
    }

    public class SnakeClassicalSkin : SnakeSkin
    {
        public override void Render(Graphics g, Snake s)
        {
            if(s.Status.HasFlag(SnakeStatus.AfterStunned))
                BaseRender(g, s);
            else
                BaseRender(g, s);
            if (s.Status.HasFlag(SnakeStatus.Stunned))
                RenderStunned(g, s);
            if (s.Status.HasFlag(SnakeStatus.AfterStunned))
                RenderAfterStunned(g, s);
            if (s.Status.HasFlag(SnakeStatus.Super))
                RenderSuper(g, s);
        }

        private void RenderStunned(Graphics g, Snake s)
        {
            g.DrawString("Stunned!", SystemFonts.DefaultFont, Brushes.Blue, s.Body[0].ToPointF());
        }

        private void RenderAfterStunned(Graphics g, Snake s)
        {
            g.DrawString("@@@", SystemFonts.DefaultFont, Brushes.Blue, s.Body[0].ToPointF());
        }

        private void RenderSuper(Graphics g, Snake s)
        {
            g.DrawString("Super!", SystemFonts.DefaultFont, Brushes.Red, s.Body[0].ToPointF());
        }

        private void BaseRender(Graphics g, Snake s)
        {
            // Head
            float w = (float)(s.Width * 2);
            float tlx = (float)(s.Body[0].X - s.Width);
            float tly = (float)(s.Body[0].Y - s.Width);
            g.DrawEllipse(Pens.Gray, tlx, tly, w, w);

            if (s.Length == 0)
                return;

            // Body
            double prevPerc = 0, curLen = 0;
            for (int i = 0; i < s.Body.Count - 1; i++)
            {
                Vector dir = s.Body[i] - s.Body[i + 1];
                double dlen = dir.Length;
                curLen += dlen;
                double curPerc = curLen / s.Length;
                double curRPerc = MathHelper.Map(curPerc, s.WidthBeginPercentage, s.WidthEndPercentage);
                double prevRPerc = MathHelper.Map(prevPerc, s.WidthBeginPercentage, s.WidthEndPercentage);
                Vector nor = new Vector(-dir.Y * s.Width / dlen, dir.X * s.Width / dlen);
                g.DrawLine(Pens.Black,
                    (s.Body[i] + nor * prevRPerc).ToPointF(),
                    (s.Body[i + 1] + nor * curRPerc).ToPointF());
                g.DrawLine(Pens.Black,
                    (s.Body[i] - nor * prevRPerc).ToPointF(),
                    (s.Body[i + 1] - nor * curRPerc).ToPointF());
                prevPerc = curPerc;
            }

            // Head direction
            Vector hdir = s.Body[0] - s.Body[1];
            hdir.Normalize();
            g.DrawLine(Pens.Red, s.Body[0].ToPointF(), (s.Body[0] + hdir * 10).ToPointF());
        }
    }
}
