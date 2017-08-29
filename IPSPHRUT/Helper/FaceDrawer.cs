//using Affdex;
using MathNet.Spatial.Euclidean;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;

namespace IPSPHRUT
{
    public class FaceDrawer : IDisposable
    {
        private PrivateFontCollection pfc = new PrivateFontCollection();

        public FaceDrawer()
        {
            pfc.AddFontFile(Path.Combine(Global.PluginRoot, @"Content\fonts\2.ttf"));
        }

        public Image Draw(FaceBase face, Image org)
        {
            int IdxHeight = org.Height / 4;
            int ExtHeight = org.Width / 8;
            double extw = (9.0 / 16.0) * (org.Height + IdxHeight + ExtHeight) - org.Width;
            int ExtWidth = Math.Max(org.Width / 8, (int)Math.Ceiling(extw));
            Image handled = new Bitmap(org.Width + ExtWidth, org.Height + ExtHeight + IdxHeight);
            using (Graphics g = Graphics.FromImage(handled))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.CompositingMode = CompositingMode.SourceOver;
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.InterpolationMode = InterpolationMode.High;
                // draw base
                g.FillRectangle(Brushes.White, new Rectangle(Point.Empty, handled.Size));
                g.DrawImagePixel(org, new PointF(ExtWidth / 2.0f, ExtHeight / 2.0f));
                // draw desc
                g.TranslateTransform(ExtWidth / 2.0f, ExtHeight / 2.0f);
                DrawDesc(g, face, org.Size);
                // draw index
                g.ResetTransform();
                g.TranslateTransform(ExtWidth / 2.0f, org.Height + ExtHeight / 2.0f);
                DrawIndex(g, face, new Size(org.Width, IdxHeight));
                // draw copyright
                g.ResetTransform();
                DrawCopyright(g, handled.Size);
            }
            return handled;
        }

        private void DrawCopyright(Graphics g, Size size)
        {
            string from = "hrut.wingkou.cn";
            using (Font font = new Font(pfc.Families[0], 10))
            {
                SizeF fsize = g.MeasureString(from, font);
                g.TranslateTransform(size.Width - fsize.Width, size.Height - fsize.Height);
                g.DrawString(from, font, Brushes.Black, PointF.Empty);
            }
        }

        private void DrawDesc(Graphics g, FaceBase face, Size size)
        {
            GraphicsState gstate = g.Save();

            CircleF circle = face.EnclosingCircle();
            Color outlineColor = Describer.GenderColor(face);

            using (Image mask = MaskImage(size, circle, Color.FromArgb(128, 255, 255, 255)))
                g.DrawImagePixel(mask);

#if DEBUG
            g.DrawCircle(Pens.Red, circle.Center.X, circle.Center.Y, circle.Radius);
#endif

            string text = Describer.DescribeFace(face);

            Font font = new Font(pfc.Families[0], 24);
            SizeF fsize = g.MeasureString(text, font);
            double fratio = fsize.Height / fsize.Width;

            double range = Global.Random.NextDouble();
            int dir = Global.Random.Next(2);
            double goldenAngle = ((9 * range + 4.5) * (dir * 2 - 1)) * Math.PI / 180;
            float mxlen;
            PointF center;
            MaxTextRegion(
                size, new CircleF(circle.Center, circle.Radius),
                goldenAngle, fratio,
                out mxlen, out center);

            MatchFontSize(g, mxlen, float.MaxValue, text, out font, out fsize);

            g.TransformRectToFixSeg(
                center,
                (float)(goldenAngle * 180 / Math.PI), mxlen,
                fsize.Width, fsize.Height);

            float faceR = fsize.Height / 2;
            string avatarPath = Describer.Avatar(face);
            if (string.IsNullOrEmpty(avatarPath))
                g.DrawOutlineString(text, font, Color.White, outlineColor, PointF.Empty);
            else
            {
                Size ss = new Size((int)fsize.Height, (int)fsize.Height);
                using (Bitmap
                    org = new Bitmap(avatarPath),
                    img = new Bitmap(org, ss))
                {
                    int d = Global.Random.Next(2);
                    g.DrawOutlineString(text, font, Color.White, outlineColor, new PointF(faceR * (1 - d * 2), 0));
                    g.DrawImagePixel(img, new PointF(fsize.Width * d - faceR, 0));
                }
            }
#if DEBUG
            g.DrawCircle(Pens.Yellow, fsize.Width, faceR, faceR);
            g.DrawCircle(Pens.Yellow, 0, faceR, faceR);
            g.DrawRectangles(Pens.Red, new RectangleF[] { new RectangleF(PointF.Empty, fsize) });
#endif

            g.Restore(gstate);
        }

        private void DrawIndex(Graphics g, FaceBase face, Size size)
        {
            GraphicsState gstate = g.Save();

            SizeF tSize = new SizeF(size.Width, size.Height / 3f);
            Color emotionColor = Describer.EmotionColor(face);

            Font font;
            SizeF fsize;
            // header
            string header = $"{DateTime.Now.ToString("MM月dd日")}心情指数:";
            MatchFontSize(g, tSize.Width, tSize.Height, header, out font, out fsize);
            g.DrawOutlineString(header, font, emotionColor, Color.Black, PointF.Empty);
#if DEBUG
            g.DrawRectangles(Pens.Red, new RectangleF[] { new RectangleF(PointF.Empty, fsize) });
#endif
            // detail
            string etext = Describer.DescribeIndexDetail(face);
            MatchFontSize(g, tSize.Width, tSize.Height, etext, out font, out fsize);
            PointF loc = new PointF(size.Width - fsize.Width, tSize.Height * 2);
            g.DrawOutlineString(etext, font, emotionColor, Color.Black, loc, 0.125f, true);
#if DEBUG
            g.DrawRectangles(Pens.Red, new RectangleF[] { new RectangleF(loc, fsize) });
#endif
            // emoji
            float left = (size.Width - fsize.Width) / 2;
            using (Image emoji = Image.FromFile(Describer.DescribEmoji(face)))
            {
                g.DrawImage(emoji,
                    new RectangleF(left, tSize.Height, tSize.Height, tSize.Height),
                    new RectangleF(0, 0, emoji.Width, emoji.Height),
                    GraphicsUnit.Pixel);
            }

            int pid = DirectoryHelper.RandomFile(Path.Combine(Global.PluginRoot, @"Content\progress"));
            using (Image progess = Image.FromFile(Path.Combine(Global.PluginRoot, $@"Content\progress\{pid}.png")))
            using (Image progessx = Image.FromFile(Path.Combine(Global.PluginRoot, $@"Content\progressx\{pid}.png")))
            {
                float ratio = tSize.Height / progess.Height;
                DrawImageMappingColor(g, progess, Color.FromArgb(192, 192, 192),
                    new RectangleF(left + tSize.Height, tSize.Height, progess.Width * ratio, tSize.Height),
                    new RectangleF(0, 0, progess.Width, progess.Height));

                float cw = progess.Width * (float)Math.Pow(face.DominantEmotion / 100, 1 / 3.0);
                if (face.DominantEmotionIndex < 0)
                    cw = progess.Width;
                DrawImageMappingColor(g, progess, emotionColor,
                    new RectangleF(left + tSize.Height, tSize.Height, cw * ratio, tSize.Height),
                    new RectangleF(0, 0, cw, progess.Height));

                DrawImageMappingColor(g, progessx, Color.Black,
                    new RectangleF(left + tSize.Height, tSize.Height, progessx.Width * ratio, tSize.Height),
                    new RectangleF(0, 0, progessx.Width, progessx.Height));
            }

            g.Restore(gstate);
        }

        private void DrawImageMappingColor(Graphics g, Image image, Color color, RectangleF dest, RectangleF src)
        {
            float[][] cm =
                   {
                    new float[]{ color.R/255f,0,0,0,0 },
                    new float[]{ 0,color.G/255f,0,0,0 },
                    new float[]{ 0,0,color.B/255f,0,0 },
                    new float[]{ 0,0,0,color.A/255f,0 },
                    new float[]{ 0,0,0,0,1 }
                };
            ImageAttributes ia = new ImageAttributes();
            ia.ClearColorMatrix();
            ia.SetColorMatrix(new ColorMatrix(cm), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            g.DrawImage(image,
                new PointF[] {
                        new PointF(dest.Left, dest.Top),
                        new PointF(dest.Right, dest.Top),
                        new PointF(dest.Left, dest.Bottom) },
                src, GraphicsUnit.Pixel, ia);
        }

        private void MatchFontSize(Graphics g, float w, float h, string text, out Font font, out SizeF fsize)
        {
            float fsl = 1, fsr = 300;
            for (int i = 0; i < 16; i++)
            {
                float fsm = (fsl + fsr) / 2;
                using (font = new Font(pfc.Families[0], fsm))
                {
                    fsize = g.MeasureString(text, font);
                    if (fsize.Width < w && fsize.Height < h)
                        fsl = fsm;
                    else
                        fsr = fsm;
                }
            }
            font = new Font(pfc.Families[0], (fsl + fsr) / 2);
            fsize = g.MeasureString(text, font);
        }

        private PointF FarthestPointToPoint(RectangleF rect, PointF pt)
        {
            return new PointF(
                pt.X < rect.X + rect.Width / 2 ? rect.Right : rect.Left,
                pt.Y < rect.Y + rect.Height / 2 ? rect.Bottom : rect.Top);
        }

        private struct RectV
        {
            public double C0, C1, W, H;
            public double Left(double len) => C0 * len;
            public double Top(double len) => C1 * len;
            public double Right(double len) => W - Left(len);
            public double Bottom(double len) => H - Top(len);
        }

        private struct PtV
        {
            public Func<double, double> X, Y;
        }

        private PtV FarthestPointToPoint(RectV rect, PointF pt)
        {
            return new PtV()
            {
                X = pt.X < rect.W / 2 ? (Func<double, double>)rect.Right : (Func<double, double>)rect.Left,
                Y = pt.Y < rect.H / 2 ? (Func<double, double>)rect.Bottom : (Func<double, double>)rect.Top
            };
        }

        private void MaxTextRegion(Size size, CircleF circle, double angle, double fratio, out float mxlen, out PointF center)
        {
            // ew(Ex-Width)=(len*fratio)/2
            // half-rect
            // { w=ew+len*|cosa|/2
            //   h=ew+len*|sina|/2 }
            // { c0=...
            //   c1=... }

            double vx = Math.Cos(angle);
            double vy = Math.Sin(angle);
            double c0 = (fratio + Math.Abs(vx)) / 2;
            double c1 = (fratio + Math.Abs(vy)) / 2;
            double mxlenx = Math.Min(size.Width / (2 * c0), size.Height / (2 * c1));
            RectV rect = new RectV() { C0 = c0, C1 = c1, W = size.Width, H = size.Height };
            PtV fp = FarthestPointToPoint(rect, circle.Center);
            float cx = circle.Center.X, cy = circle.Center.Y;
            double l = 0, r = mxlenx;
            Point2D c = new Point2D(cx, cy);
            for (int i = 0; i < 16; i++)
            {
                double mid = (l + r) / 2;
                double px = fp.X(mid);
                double py = fp.Y(mid);
                double px0 = px + vx * mid / 2;
                double py0 = py + vy * mid / 2;
                double px1 = px - vx * mid / 2;
                double py1 = py - vy * mid / 2;

                Line2D line = new Line2D(new Point2D(px0, py0), new Point2D(px1, py1));
                Point2D cp = line.ClosestPointTo(c, true);

                if (cp.DistanceTo(c) < circle.Radius + mid * fratio / 2)
                    r = mid;
                else
                    l = mid;
            }
            mxlenx = (l + r) / 2;
            mxlen = (float)mxlenx;
            center = new PointF((float)fp.X(mxlenx), (float)fp.Y(mxlenx));
        }

        private RectangleF MaxRectRegion(Size size, CircleF circle)
        {
            RectangleF reg1, reg2;
            RectangleF c = new RectangleF(
                circle.Center.X - circle.Radius,
                circle.Center.Y - circle.Radius,
                circle.Radius * 2, circle.Radius * 2);
            if (circle.Center.X * 2 < size.Width) // right
                reg1 = new RectangleF(c.Right, 0, size.Width - c.Right, size.Height);
            else // left
                reg1 = new RectangleF(0, 0, c.Left, size.Height);
            if (circle.Center.Y * 2 < size.Height) // bottom
                reg2 = new RectangleF(0, c.Bottom, size.Width, size.Height - c.Bottom);
            else // top
                reg2 = new RectangleF(0, 0, size.Width, c.Top);
            return reg1.Width * reg1.Height < reg2.Width * reg2.Height ? reg2 : reg1;
        }

        private float MaxLenInOppDir(Size size, CircleF circle)
        {
            float t = float.MaxValue;

            float cx = circle.Center.X;
            float cy = circle.Center.Y;
            float[] px = { 0, 0, size.Width, size.Width };
            float[] py = { 0, 0, size.Height, size.Height };
            float[] dx = { 1, 0, -1, 0 };
            float[] dy = { 0, 1, 0, -1 };
            float vx = size.Width / 2.0f - cx;
            float vy = size.Height / 2.0f - cy;

            for (int i = 0; i < px.Length; i++)
            {
                float m = vx * dy[i] - vy * dx[i];
                if (m == 0)
                    continue;
                float n = (cy - py[i]) * dx[i] - (cx - px[i]) * dy[i];
                if (n / m < 0)
                    continue;
                t = Math.Min(t, n / m);
            }

            return (float)Math.Max(t * Math.Sqrt(vx * vx + vy * vy) - circle.Radius, 0);
        }

        private Bitmap MaskImage(Size size, CircleF circle, Color color)
        {
            Bitmap bmp = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
            PointF fp = FarthestPointToPoint(new RectangleF(0, 0, size.Width, size.Height), circle.Center);
            float len = (float)Math.Sqrt(
                (circle.Center.X - fp.X) * (circle.Center.X - fp.X) +
                (circle.Center.Y - fp.Y) * (circle.Center.Y - fp.Y));
            GraphicsPath path = new GraphicsPath();
            path.AddEllipse(circle.Center.X - len, circle.Center.Y - len, len * 2, len * 2);
            Color[] colors = { color, Color.FromArgb(0, color), Color.FromArgb(0, color) };
            float[] positions = { 0, 1 - circle.Radius / len, 1 };
            using (PathGradientBrush bs = new PathGradientBrush(path))
            {
                bs.InterpolationColors = new ColorBlend() { Colors = colors, Positions = positions };
                bs.CenterPoint = circle.Center;
                using (Graphics g = Graphics.FromImage(bmp))
                    g.FillRectangle(bs, new Rectangle(Point.Empty, size));
            }
            return bmp;
        }

        public void Dispose()
        {
            pfc.Dispose();
        }
    }
}
