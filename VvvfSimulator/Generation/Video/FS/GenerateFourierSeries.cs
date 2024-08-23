using System;
using System.Drawing;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.Generation.Video.FS
{
    public class GenerateFourierSeries
    {

        public static double GetFourier(ref WaveValues[] UVW, int N)
        {
            double integral = 0;
            double dt = 1.0 / (UVW.Length - 1);

            for (int i = 0; i < UVW.Length; i++)
            {
                double iTime = MyMath.M_2PI * i / (UVW.Length - 1);
                double sum = (UVW[i].U - UVW[i].V) * Math.Sin(N * iTime) * dt;
                integral += sum;
            }
            double bn = integral;
            return bn;
        }

        public static double GetFourierFast(ref WaveValues[] UVW, int N)
        {
            double integral = 0;

            int Ft = 0;
            double Time = 0;

            for (int i = 0; i < UVW.Length; i++)
            {
                int iFt = UVW[i].U - UVW[i].V;

                if (i == 0)
                {
                    Ft = iFt;
                    continue;
                }

                if (Ft == iFt) continue;
                double iTime = MyMath.M_2PI * i / (UVW.Length - 1);
                double sum = (-Math.Cos(N * iTime) + Math.Cos(N * Time)) * Ft / N;
                integral += sum;

                Time = iTime;
                Ft = iFt;
            }
            double bn = integral / MyMath.M_2PI;
            return bn;
        }

        public static double[] GetFourierCoefficients(ref WaveValues[] UVW, int N)
        {
            double[] coefficients = new double[N];
            for (int n = 1; n <= N; n++)
            {
                double result = GetFourierFast(ref UVW, n);
                coefficients[n-1] = result;
            }
            return coefficients;
        }

        /// <summary>
        /// Gets Fourier series coefficients
        /// </summary>
        /// <param name="Control">Make sure you put cloned data.</param>
        /// <param name="Sound"></param>
        /// <param name="Delta"></param>
        /// <param name="N"></param>
        /// <returns></returns>
        public static double[] GetFourierCoefficients(VvvfValues Control, YamlVvvfSoundData Sound, int Delta, int N)
        {
            Control.SetRandomFrequencyMoveAllowed(false);
            WaveValues[] PWM_Array = GenerateBasic.GetUVWCycle(Control, Sound, MyMath.M_PI_6, Delta, false);
            return GetFourierCoefficients(ref PWM_Array, N);
        }

        public static string GetDesmosFourierCoefficientsArray(ref double[] coefficients)
        {
            String array = "C = [";
            for(int i = 0; i < coefficients.Length; i++)
            {
                array += (i == 0 ? "" : " ,") + coefficients[i];
            }
            array += "]";
            return array;
        }

        private static class MagnitudeColor
        {

            private static double Linear(double x, double x1, double x2, double y1, double y2)
            {
                double val = (y2 - y1) / (x2 - x1) * (x - x1) + y1;
                return val;
            }

            private static double[] LinearRGB(double x, double x1, double x2, double r1, double g1, double b1, double r2, double g2, double b2)
            {
                double[] val =
                [
                    Linear(x, x1, x2, r1, r2),
                    Linear(x, x1, x2, g1, g2),
                    Linear(x, x1, x2, b1, b2)
                ];
                return val;
            }

            public static Color GetColor(double rate)
            {
                double[] rgb = [0,0,0];
                if (rate >= 0.85) rgb = [255, 85, 85];
                if (rate < 0.85) rgb = LinearRGB(rate, 0.85, 0.68, 255, 85, 85, 255, 205, 85);
                if (rate < 0.68) rgb = LinearRGB(rate, 0.68, 0.5, 255, 205, 85, 206, 224, 0);
                if (rate < 0.5) rgb = LinearRGB(rate, 0.5, 0.38, 206, 224, 0, 115, 227, 117);
                if (rate < 0.38) rgb = LinearRGB(rate, 0.15, 0.38, 77, 148, 232, 115, 227, 117);
                if (rate < 0.15) rgb = [77, 148, 232];

                Color color = Color.FromArgb((int)rgb[0], (int)rgb[1], (int)rgb[2]);
                return color;
            }
        }

        public static Bitmap GetImage(ref double[] Coefficients)
        {
            Bitmap image = new(1000, 1000);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, 1000, 1000);

            int count = Coefficients.Length;
            if (count == 0) return image;
            int width = 1000 / count;

            for (int i = 0; i < count; i++)
            {
                double result = Coefficients[i];
                double ratio = result / ControlInfo.GenerateControlCommon.VoltageConvertFactor;
                int height = (int)(ratio * 500);
                SolidBrush solidBrush = new(MagnitudeColor.GetColor(ratio));
                if(height < 0) g.FillRectangle(solidBrush, width * i, 500, width, -height);
                else g.FillRectangle(solidBrush, width * i, 500 - height, width, height);

                if(width > 10 && i != 0 && i != count - 1) g.DrawLine(new Pen(Color.Gray), width * i, 0, width * i, 1000);
            }

            g.Dispose();
            return image;

        }

    }
}
