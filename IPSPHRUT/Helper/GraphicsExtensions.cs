using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPSPHRUT
{
    public static class GraphicsExtensions
    {
        public static void DrawOutlineString(this Graphics g,
            string text, Font font,
            Color tcolor, Color lcolor,
            PointF pos, float factor = 0.125f, bool alignRight = false)
        {
            using (SolidBrush brushWhite = new SolidBrush(Color.White))
            using (GraphicsPath path = new GraphicsPath())
            {
                StringFormat sf = (StringFormat)StringFormat.GenericDefault.Clone();
                sf.Alignment = alignRight ? StringAlignment.Far : StringAlignment.Near;

                PointF p = alignRight ? new PointF(pos.X + g.MeasureString(text, font).Width, pos.Y) : pos;

                path.AddString(text,
                    font.FontFamily, (int)FontStyle.Regular, font.SizeInPoints * g.DpiY / 72,
                    p, sf);

                using (Pen pen = new Pen(lcolor, font.Size * factor))
                {
                    pen.LineJoin = LineJoin.Round;
                    g.DrawPath(pen, path);
                }

                using (SolidBrush brush = new SolidBrush(tcolor))
                    g.FillPath(brush, path);
            }
        }

        public static void DrawCircle(this Graphics g, Pen pen,
            float centerX, float centerY, float radius)
        {
            g.DrawEllipse(pen, centerX - radius, centerY - radius,
                          radius + radius, radius + radius);
        }

        public static void DrawImagePixel(this Graphics g, Image img)
        {
            var rect = new Rectangle(0, 0, img.Width, img.Height);
            g.DrawImage(img, rect, rect, GraphicsUnit.Pixel);
        }

        public static void DrawImagePixel(this Graphics g, Image img, PointF pos)
        {
            var src = new RectangleF(0, 0, img.Width, img.Height);
            var dest = new RectangleF(pos, img.Size);
            g.DrawImage(img, dest, src, GraphicsUnit.Pixel);
        }

        public static void TransformRectToFixSeg(this Graphics g,
            PointF center,
            float angle, float len,
            float w, float h)
        {
            float scale = len / w;

            //g.TranslateTransform(-w / 2.0f, -h / 2.0f, MatrixOrder.Append);
            //g.RotateTransform(angle, MatrixOrder.Append);
            //g.ScaleTransform(scale, scale, MatrixOrder.Append);
            //g.TranslateTransform(center.X, center.Y, MatrixOrder.Append);

            g.TranslateTransform(center.X, center.Y);
            g.ScaleTransform(scale, scale);
            g.RotateTransform(angle);
            g.TranslateTransform(-w / 2.0f, -h / 2.0f);
        }
    }
}
