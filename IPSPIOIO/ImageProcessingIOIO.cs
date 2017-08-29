using ImageProcessingServicePlugin;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IPSPIOIO
{
    public class ImageProcessingIOIO : IIPSPlugin
    {
        public string Name => nameof(ImageProcessingIOIO);

        public string Description => "From project IOIO";

        public void Dispose() { }

        private static bool UseIOIO2 = true;
        /**
         * This tells the algorithm when to stop: when the average darkness in the image is below this
         * threshold.
         */
        private static double THRESHOLD = 0.2;

        /**
         * Since we're doing all the image calculations in fixed-point, this is a scaling factor we are
         * going to use.
         */
        private static readonly double GRAY_RESOLUTION = 128;

        /**
         * This is the ratio between the line width to the overall output image width. It is important,
         * since it tells our algorithm how much darkness a single line contributes.
         */
        private static readonly double NATIVE_RESOLUTION = 450.0;

        /**
         * Set to true for one continuous stroke, false for discontinuous lines.
         */
        private static readonly bool CONTINUOUS = true;

        /**
         * By how much to down-sample the image.
         */
        private static readonly double SCALE = 0.2;

        /**
         * How many candidates to consider for each line segment.
         */
        private static readonly int NUM_CANDIDATES = 1024;

        private static Random random_ = new Random();

        public string Process(string uri) => Process(uri, null);

        public string Process(string uri, Action<double> callback)
        {
            Bitmap _original = null;
            IOIOMatrix original = null;

            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(uri)))
            {
                _original = new Bitmap(ms);
                original = new IOIOMatrix(_original);
            }

            // Resize to native resolution.
            float scale = (float)NATIVE_RESOLUTION / original.Width;
            var _preview = ResizeImage(_original, scale);
            var preview = new IOIOMatrix(_preview);

            // Down-sample.
            var _inp = ResizeImage(_preview, (float)SCALE);
            var inp = new IOIOMatrix(_inp);

            inp.Invert();

            inp.Multiply(GRAY_RESOLUTION / SCALE / 255);

            // Now is the actual algorithm!
            Point lastP = Point.Empty;
            double residualDarkness = inp.Average() / GRAY_RESOLUTION * SCALE;
            double totalLength = 0;
            List<Line> lines = new List<Line>();

            double deltaDarkness = residualDarkness - THRESHOLD;

            while (residualDarkness > THRESHOLD)
            {
                Line bestLine = nextLine(inp, NUM_CANDIDATES, lastP);
                Line scaledLine = scaleLine(bestLine, 1 / SCALE);
                lastP = bestLine.B;
                Line line = new Line(
                    new Point(scaledLine.A.X, scaledLine.A.Y),
                    new Point(scaledLine.B.X, scaledLine.B.Y));
                totalLength += line.Length;
                lines.Add(line);
                residualDarkness = inp.Average() / GRAY_RESOLUTION * SCALE;

                callback?.Invoke(1 - (residualDarkness - THRESHOLD) / deltaDarkness);
            }

            return GetImage(preview.Width, preview.Height, lines);
        }

        private static Bitmap ResizeImage(Bitmap original, float scale)
        {
            var preview = new Bitmap((int)(scale * original.Width), (int)(scale * original.Height));
            using (Graphics g = Graphics.FromImage(preview))
                g.DrawImage(original, new Rectangle(0, 0, preview.Width, preview.Height));
            return preview;
        }

        private string GetImage(int width, int height, List<Line> lines)
        {
            string base64 = string.Empty;
            using (Bitmap bmp = new Bitmap(width, height))
            {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.FillRectangle(Brushes.White, new Rectangle(0, 0, bmp.Width, bmp.Height));
                    foreach (var l in lines)
                        g.DrawLine(Pens.Black, l.A, l.B);
                }
                using (MemoryStream ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Jpeg);
                    base64 = Convert.ToBase64String(ms.ToArray());
                }
            }
            return base64;
        }

        private Line nextLine(IOIOMatrix image, int numAttempts, Point startPoint)
        {
            Line bestLine = new Line(Point.Empty, Point.Empty);
            double bestScore = double.NegativeInfinity;
            double bestAvgCover = double.NegativeInfinity;
            for (int i = 0; i < numAttempts; ++i)
            {
                Line line = generateRandomLine(image.Width, image.Height, startPoint);

                double score = image.Average(line);
                double avgCover = score / line.Length;

                if (UseIOIO2)
                {
                    if (avgCover > bestAvgCover * 0.95 && score > bestScore)
                    {
                        bestScore = score;
                        bestAvgCover = avgCover;
                        bestLine = line;
                    }
                }
                else
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestAvgCover = avgCover;
                        bestLine = line;
                    }
                }
            }
            image.Subtract(bestLine, GRAY_RESOLUTION);
            return bestLine;
        }

        private Line generateRandomLine(int w, int h, Point pStart)
        {
            var line = new Line(pStart, Point.Empty);
            do
            {
                line.B = new Point(
                    (int)(random_.NextDouble() * w),
                    (int)(random_.NextDouble() * h));
            } while (line.A == line.B);
            return line;
        }

        private Line scaleLine(Line line, double scale)
        {
            return new Line(
                new Point((int)(line.A.X * scale), (int)(line.A.Y * scale)),
                new Point((int)(line.B.X * scale), (int)(line.B.Y * scale)));
        }
    }
}
