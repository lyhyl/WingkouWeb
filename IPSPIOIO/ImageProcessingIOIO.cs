using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
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

        public string Process(string uri)
        {
            Image<Bgr, Byte> img;
            using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(uri)))
            {
                Bitmap bmp = new Bitmap(ms);
                img = new Image<Bgr, Byte>(bmp);
            }
            Mat original = img.Mat;

            // Convert to gray-scale.
            CvInvoke.CvtColor(original, original, ColorConversion.Bgr2Gray);

            // Resize to native resolution.
            Mat preview = new Mat();
            double scale = NATIVE_RESOLUTION / original.Cols;
            CvInvoke.Resize(original, preview, new Size(), scale, scale, Inter.Area);

            // Down-sample.
            Mat inp = new Mat();
            CvInvoke.Resize(preview, inp, new Size(), SCALE, SCALE, Inter.Area);

            // Negative: bigger number = darker.
            Mat scalar = new Mat(1, 1, DepthType.Cv64F, 1);
            scalar.SetTo(new MCvScalar(255));
            CvInvoke.Subtract(scalar, inp, inp);

            // Convert to S16: we need more color resolution and negative numbers.
            inp.ConvertTo(inp, DepthType.Cv16S);

            // We scale such that for each line we can subtract GRAY_RESOLUTION and it will correspond
            // to darkening by SCALE.
            Mat white = new Mat(1, 1, DepthType.Cv64F, 1);
            white.SetTo(new MCvScalar(1));
            CvInvoke.Multiply(inp, white, inp, GRAY_RESOLUTION / SCALE / 255);

            // Now is the actual algorithm!
            Point lastP = Point.Empty;
            double residualDarkness = average(inp) / GRAY_RESOLUTION * SCALE;
            double totalLength = 0;
            List<Line> lines = new List<Line>();

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
                residualDarkness = average(inp) / GRAY_RESOLUTION * SCALE;

                float p = (float)((residualDarkness - THRESHOLD) / (1 - THRESHOLD));
            }

            return GetImage(preview.Size, lines);
        }

        private string GetImage(Size size, List<Line> lines)
        {
            string base64 = string.Empty;
            using (Bitmap bmp = new Bitmap(size.Width, size.Height))
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

        private double average(Mat inp)
        {
            double total = CvInvoke.Sum(inp).V0;
            return total / (inp.Cols * inp.Rows);
        }

        private Line nextLine(Mat image, int numAttempts, Point startPoint)
        {
            Matrix<Byte> mask = new Matrix<Byte>(image.Size);
            mask.SetZero();
            Matrix<Byte> bestMask = new Matrix<Byte>(image.Size);
            bestMask.SetZero();
            Line bestLine = new Line(Point.Empty, Point.Empty);
            double bestScore = double.NegativeInfinity;
            double bestAvgCover = double.NegativeInfinity;
            for (int i = 0; i < numAttempts; ++i)
            {
                Line line;
                generateRandomLine(image.Size, startPoint, out line);

                // Calculate the score for this line as the average darkness over it.
                // This way to calculate this is crazy inefficient, but compact...
                mask.SetValue(new MCvScalar(0));
                CvInvoke.Line(mask, line.A, line.B, new MCvScalar(GRAY_RESOLUTION));
                double score = CvInvoke.Mean(image, mask).V0;
                double avgCover = score / line.Length;

                if (UseIOIO2)
                {
                    if (avgCover > bestAvgCover * 0.95 && score > bestScore)
                    {
                        bestScore = score;
                        bestAvgCover = avgCover;
                        Matrix<Byte> t = mask;
                        mask = bestMask;
                        bestMask = t;
                        bestLine = line;
                    }
                }
                else
                {
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestAvgCover = avgCover;
                        Matrix<Byte> t = mask;
                        mask = bestMask;
                        bestMask = t;
                        bestLine = line;
                    }
                }
            }
            CvInvoke.Subtract(image, bestMask, image, bestMask, image.Depth);
            return bestLine;
        }

        private void generateRandomLine(Size s, Point pStart, out Line line)
        {
            line = new Line(pStart, Point.Empty);
            do
            {
                line.B = new Point(
                    (int)(random_.NextDouble() * s.Width),
                    (int)(random_.NextDouble() * s.Height));
            } while (line.A == line.B);
        }

        private Line scaleLine(Line line, double scale)
        {
            return new Line(
                new Point((int)(line.A.X * scale), (int)(line.A.Y * scale)),
                new Point((int)(line.B.X * scale), (int)(line.B.Y * scale)));
        }
    }

    internal struct Line
    {
        public Point A, B;
        public Line(Point a, Point b)
        {
            A = a;
            B = b;
        }
        public double Length => MathExt.Hypot(A.X - B.X, A.Y - B.Y);
    }

    public static class MathExt
    {
        public static double Hypot(double x, double y) => Math.Sqrt(x * x + y * y);
    }
}
