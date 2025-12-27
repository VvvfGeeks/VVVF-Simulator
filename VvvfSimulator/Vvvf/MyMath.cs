using System;

namespace VvvfSimulator.Vvvf
{
    public class MyMath
    {
        public const double M_2_PI = 0.63661977236758134307553505349006;
        public const double M_1_PI = 0.31830988618379067153776752674503;
        public const double M_1_2PI = 0.15915494309189533576888376337251;

        public const double M_4PI_3 = 4.1887902047863909846168578443727;

        public const double M_2PI = 6.283185307179586476925286766559;
        public const double M_2PI_3 = 2.0943951023931954923084289221863;

        public const double M_PI = 3.1415926535897932384626433832795;
        public const double M_PI_2 = 1.5707963267948966192313216916398;
        public const double M_PI_3 = 1.0471975511965977461542144610932;
        public const double M_PI_4 = 0.78539816339744830961566084581988;
        public const double M_PI_6 = 0.52359877559829887307710723054658;
        public const double M_PI_12 = 0.26179938779914943653855361527329;
        public const double M_PI_180 = 0.01745329251994329576923690768489;

        public const double M_SQRT3 = 1.7320508075688772935274463415059;
        public const double M_SQRT3_2 = 0.86602540378443864676372317075294;

        public static class Functions
        {
            public static double Triangle(double x)
            {
                double Phase = M_2_PI * x - 4 * Math.Floor(x * M_1_2PI);
                double Value = Phase;
                if (1 <= Phase && Phase < 3)
                    Value = -Phase + 2;
                else if(3 <= Phase)
                    Value = Phase - 4;
                return Value;
            }
            public static double Saw(double x)
            {
                double Phase = M_1_PI * x - 2 * Math.Floor(x * M_1_2PI) - 1;
                return Phase;
            }
            public static double Sine(double x)
            {
                return Math.Sin(x);
            }

            public static double ArcSine(double a)
            {
                return Math.Asin(a);
            }

            public static double Square(double x)
            {
                double fixed_x = x - Math.Floor(x * M_1_2PI) * M_2PI;
                if (fixed_x * M_1_PI > 1) return -1;
                return 1;
            }
        }
        public static class EquationSolver
        {
            public delegate double Function(double x);
            public enum EquationSolverType
            {
                Newton, Bisection
            }

            public class NewtonMethod(Function function, double dx)
            {
                private readonly Function function = function;

                public double Calculate(double begin, double tolerance, int n)
                {
                    double x = begin;
                    for (int i = 0; i < n; i++)
                    {
                        double pre_x = x;
                        x = GetZeroIntersect(x);
                        if (pre_x == x || double.IsNaN(x) || double.IsInfinity(x)) x = pre_x + dx;
                        double fx = Math.Abs(function(x));
                        if (fx < tolerance) return x;
                    }
                    return x;
                }

                private double GetDerivative(double x)
                {
                    double Fxdx = function(x + dx);
                    double Fx = function(x);
                    double Dy = Fxdx - Fx;
                    double Dx = dx;
                    double Derivative = Dy / Dx;
                    return Derivative;
                }

                private double GetZeroIntersect(double x)
                {
                    double zeroX = -function(x) / GetDerivative(x) + x;
                    return zeroX;
                }
            }
            public class BisectionMethod(Function function)
            {
                private readonly Function function = function;

                public double Calculate(double X0, double X1, double Tolerance, int N)
                {
                    double XA = 0;
                    for (int i = 0; i < N; i++)
                    {
                        XA = (X0 + X1) / 2.0;
                        double YA = function(XA);
                        if (function(X0) * YA < 0) X1 = XA;
                        else X0 = XA;
                        if (Math.Abs(YA) < Tolerance) return XA;
                    }
                    return XA;
                }
            }
        }
        public static class LagrangePolynomial
        {
            public static double Calculate(double x, (double x, double y)[] points)
            {
                double y = 0, y_i;
                for(int i = 0; i < points.Length; i++)
                {
                    y_i = 1;
                    for (int j = 0; j < points.Length; j++)
                        if (i != j) y_i *= (x - points[j].x) / (points[i].x - points[j].x);
                    y += y_i * points[i].y;
                }
                return y;
            }
        }
        public class PointD(double X, double Y)
        {
            public double X { get; set; } = X;
            public double Y { get; set; } = Y;
            public bool IsZero()
            {
                return X == 0 && Y == 0;
            }
            public PointD Norm()
            {
                if (IsZero()) return new(0, 0);
                double len = Math.Sqrt(X * X + Y * Y);
                return new(X / len, Y / len);
            }
            public PointD Rotate(double Angle)
            {
                double cos = Math.Cos(Angle);
                double sin = Math.Sin(Angle);
                return new(X * cos - Y * sin, X * sin + Y * cos);
            }
            public System.Drawing.Point ToPoint()
            {
                return new System.Drawing.Point((int)X, (int)Y);
            }
            public static PointD operator +(PointD a, PointD b)
            {
                return new PointD(a.X + b.X, a.Y + b.Y);
            }
            public static PointD operator -(PointD a, PointD b)
            {
                return new PointD(a.X - b.X, a.Y - b.Y);
            }
            public static PointD operator *(double k, PointD a)
            {
                return new PointD(k * a.X, k * a.Y);
            }
            public static PointD Max(PointD a, PointD b)
            {
                return new PointD(a.X > b.X ? a.X : b.X, a.Y > b.Y ? a.Y : b.Y);
            }

            public static PointD Min(PointD a, PointD b)
            {
                return new PointD(a.X < b.X ? a.X : b.X, a.Y < b.Y ? a.Y : b.Y);
            }
        }
    }
}
