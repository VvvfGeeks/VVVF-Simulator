using OpenCvSharp;
using System.Drawing;
using System.Drawing.Imaging;
using VvvfSimulator.Data.BaseFrequency;
using VvvfSimulator.GUI.Util;
using static VvvfSimulator.Generation.GenerateCommon;

namespace VvvfSimulator.Generation.Video.FS
{
    public class Design1
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

        public static Bitmap GetImage(ref double[] Coefficients,
            int Width, int Height
        )
        {
            Bitmap image = new(Width, Height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);

            int Count = Coefficients.Length;
            if (Count == 0) return image;
            int BarWidth = Width / Count;

            for (int i = 0; i < Count; i++)
            {
                double result = Coefficients[i];
                double ratio = result / GenerateBasic.Fourier.VoltageConvertFactor;
                int height = (int)(ratio * Height / 2);
                SolidBrush solidBrush = new(MagnitudeColor.GetColor(ratio));
                if(height < 0) g.FillRectangle(solidBrush, BarWidth * i, Height / 2 + height, BarWidth, height);
                else g.FillRectangle(solidBrush, BarWidth * i, Height / 2 - height, BarWidth, height);

                if(BarWidth > 10 && i != 0 && i != Count - 1) g.DrawLine(new Pen(Color.Gray), BarWidth * i, 0, BarWidth * i, Height);
            }

            g.Dispose();
            return image;

        }
        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo(GenerationParameter Parameter, string fileName, int fps,
            int Width, int Height, (int Begin, int End) N,
            int Division, bool Precise
        )
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            StructCompiled baseFreqData = Parameter.BaseFrequencyData;
            GUI.TaskViewer.TaskProgress progressData = Parameter.Progress;

            Vvvf.Model.Struct.Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;

            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(Width, Height));

            if (!vr.IsOpened())
                return;

            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / fps) + fps * 2;

            AddEmptyFrames(vr, Width, Height, fps, () => { progressData.Progress++; });

            Data.BaseFrequency.Analyze.ForwardTime(baseFreqData, Domain, vvvfData, 1.0 / fps, () =>
            {
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);

                double[] Coefficients = GenerateBasic.Fourier.GetFourierCoefficients(Domain.Clone(), Division, Precise, GenerateBasic.Fourier.PrimitiveOfSine, N);
                Bitmap image = GetImage(ref Coefficients, Width, Height);
                Viewer?.SetImage(image);
                AddImageFrames(vr, image, 1);
                image.Dispose();
                progressData.Progress++;

                return progressData.Cancel;
            });

            AddEmptyFrames(vr, Width, Height, fps, () => { progressData.Progress++; });

            vr.Release();
            vr.Dispose();

            Viewer?.Close();
        }
        public void ExportImage(GenerationParameter Parameter, string fileName, double Time, double ForwardStep,
            int Width, int Height, (int Begin, int End) N,
            int Division, bool Precise
        )
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Parameter.Progress.Total = 3;

            Vvvf.Model.Struct.Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;
            Analyze.ForwardTime(Parameter.BaseFrequencyData, Domain, Parameter.VvvfData, Time, ForwardStep);
            Parameter.Progress.Progress = 1;

            Data.Vvvf.Analyze.Calculate(Domain, Parameter.VvvfData);
            double[] Coefficients = GenerateBasic.Fourier.GetFourierCoefficients(Domain.Clone(), Division, Precise, GenerateBasic.Fourier.PrimitiveOfSine, N);
            Bitmap image = GetImage(ref Coefficients, Width, Height);
            Parameter.Progress.Progress = 2;

            image.Save(fileName, ImageFormat.Png);
            Viewer?.SetImage(image);
            image.Dispose();
            Parameter.Progress.Progress = 3;
        }
    }
}
