using System.Drawing;

namespace VvvfSimulator.Generation.Video.FS
{
    public class GenerateFourierSeries
    {
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
                double ratio = result / GenerateBasic.Fourier.VoltageConvertFactor;
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
