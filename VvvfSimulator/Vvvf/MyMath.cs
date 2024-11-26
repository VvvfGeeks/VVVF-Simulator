using System;

namespace VvvfSimulator.Vvvf
{
    public class MyMath
    {

        public const double M_2PI = 6.28318530717;
        public const double M_PI = 3.14159265358;
        public const double M_PI_2 = 1.57079632679;
        public const double M_2_PI = 0.63661977236;
        public const double M_1_PI = 0.31830988618;
        public const double M_1_2PI = 0.15915494309;
        public const double M_PI_180 = 0.01745329251;
        public const double M_PI_4 = 0.78539816339;
        public const double M_PI_6 = 0.52359877559;

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

    }
}
