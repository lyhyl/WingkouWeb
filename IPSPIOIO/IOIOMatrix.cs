using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;

namespace IPSPIOIO
{
    internal class IOIOMatrix
    {
        public int Width { private set; get; }
        public int Height { private set; get; }

        private int length => Width * Height;
        private double[] data;
        private int P(int x, int y) => y * Width + x;

        private IOIOMatrix(double[] dat, int w, int h)
        {
            Width = w;
            Height = h;
            data = dat;
        }

        public IOIOMatrix(int w, int h)
        {
            Width = w;
            Height = h;
            data = new double[length];
        }

        public IOIOMatrix(Bitmap image)
        {
            Width = image.Width;
            Height = image.Height;
            data = new double[length];

            var bitmapRead = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppPArgb);
            var bitmapLength = bitmapRead.Stride * bitmapRead.Height;
            var bitmapBGRA = new byte[bitmapLength];
            Marshal.Copy(bitmapRead.Scan0, bitmapBGRA, 0, bitmapLength);
            image.UnlockBits(bitmapRead);

            for (int i = 0; i < length; i++)
            {
                var b = bitmapBGRA[i * 4];
                var g = bitmapBGRA[i * 4 + 1];
                var r = bitmapBGRA[i * 4 + 2];
                data[i] = r * 0.3 + g * 0.59 + b * 0.11;
            }
        }

        public Bitmap CreateBitmap()
        {
            Bitmap image = new Bitmap(Width, Height, PixelFormat.Format32bppPArgb);

            var bitmapRead = image.LockBits(
                new Rectangle(0, 0, image.Width, image.Height),
                ImageLockMode.WriteOnly,
                PixelFormat.Format32bppPArgb);
            byte[] dat = new byte[length * 4];
            for (int i = 0; i < length; i++)
            {
                byte g = (byte)Math.Round(data[i]);
                dat[i * 4] = g;
                dat[i * 4 + 1] = g;
                dat[i * 4 + 2] = g;
                dat[i * 4 + 3] = 0xff;
            }
            Marshal.Copy(dat, 0, bitmapRead.Scan0, dat.Length);
            image.UnlockBits(bitmapRead);

            return image;
        }

        public void Invert()
        {
            for (int i = 0; i < length; i++)
                data[i] = 0xff - data[i];
        }

        public void Multiply(double v)
        {
            for (int i = 0; i < length; i++)
                data[i] *= v;
        }

        public double Average() => data.Average();

        public double Average(Line mask)
        {
            var l = BresenhamLine(mask.A.X, mask.A.Y, mask.B.X, mask.B.Y);
            return l.Select(p => data[P(p.X, p.Y)]).Average();
        }

        public void Subtract(Line mask, double v)
        {
            foreach (var p in BresenhamLine(mask.A.X, mask.A.Y, mask.B.X, mask.B.Y))
                data[P(p.X, p.Y)] -= v;
        }

        private IEnumerable<Point> BresenhamLine(int x0, int y0, int x1, int y1)
        {
            bool steep = Math.Abs(y1 - y0) > Math.Abs(x1 - x0);
            if (steep)
            {
                int t;
                t = x0; // swap x0 and y0
                x0 = y0;
                y0 = t;
                t = x1; // swap x1 and y1
                x1 = y1;
                y1 = t;
            }
            if (x0 > x1)
            {
                int t;
                t = x0; // swap x0 and x1
                x0 = x1;
                x1 = t;
                t = y0; // swap y0 and y1
                y0 = y1;
                y1 = t;
            }
            int dx = x1 - x0;
            int dy = Math.Abs(y1 - y0);
            int error = dx / 2;
            int ystep = (y0 < y1) ? 1 : -1;
            int y = y0;
            for (int x = x0; x <= x1; x++)
            {
                yield return new Point((steep ? y : x), (steep ? x : y));
                error = error - dy;
                if (error < 0)
                {
                    y += ystep;
                    error += dx;
                }
            }
            yield break;
        }
    }
}
