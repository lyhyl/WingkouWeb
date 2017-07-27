using System.Drawing;

namespace IPSPIOIO
{
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
}
