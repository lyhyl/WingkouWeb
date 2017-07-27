using System;

namespace IPSPIOIO
{
    internal static class MathExt
    {
        public static double Hypot(double x, double y) => Math.Sqrt(x * x + y * y);
    }
}
