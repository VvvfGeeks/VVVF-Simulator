using System;

namespace VvvfSimulator.Vvvf
{
    public class MyMath
    {

        public const double M_2_PI = 0.63661977236;
        public const double M_1_PI = 0.31830988618;
        public const double M_1_2PI = 0.15915494309;
        public const double M_2PI = 6.28318530717;
        public const double M_PI = 3.14159265358;
        public const double M_PI_2 = 1.57079632679;
        public const double M_PI_3 = 1.04719755119;
        public const double M_PI_4 = 0.78539816339;
        public const double M_PI_6 = 0.52359877559;
        public const double M_PI_180 = 0.01745329251;

        public const double M_SQRT3 = 1.73205080757;
        public const double M_SQRT3_2 = 0.86602540378;

        public static class Functions
        {
            public static double Saw(double x)
            {
                double val;
                double fixed_x = x - (double)((int)(x * M_1_2PI) * M_2PI);
                if (0 <= fixed_x && fixed_x < M_PI_2)
                    val = M_2_PI * fixed_x;
                else if (M_PI_2 <= fixed_x && fixed_x < 3.0 * M_PI_2)
                    val = -M_2_PI * fixed_x + 2;
                else
                    val = M_2_PI * fixed_x - 4;

                return -val;
            }

            public static double Sine(double x)
            {
                return Math.Sin(x);
            }

            public static double Square(double x)
            {
                double fixed_x = x - (double)((int)(x * M_1_2PI) * M_2PI);
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
            public System.Drawing.Point ToPoint()
            {
                return new System.Drawing.Point((int)X, (int)Y);
            }
            public static PointD operator +(PointD a, PointD b)
            {
                return new PointD(a.X + b.X, a.Y + b.Y);
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
