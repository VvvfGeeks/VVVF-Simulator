using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using VvvfSimulator.Data.BaseFrequency;
using VvvfSimulator.GUI.Util;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Vvvf.Model.Struct;
using static VvvfSimulator.Vvvf.MyMath;
using Point = System.Drawing.Point;

namespace VvvfSimulator.Generation.Video.Hexagon
{
    public class Design1
    {
        public static double GetNominalCircleSize(double Frequency)
        {
            return 15 * ((Frequency > 40) ? 1 : (Frequency / 40.0));
        }
        public static Bitmap GetImage(
            Domain Control,
            int ImageSize,
            double DrawSize,
            int Thickness,
            double CircleSize,
            int Division,
            bool Precise
        )
        {
            PhaseState[] PWM_Array = GenerateBasic.WaveForm.GetUVWCycle(Control, 0, Division, Precise);

            if (CircleSize < 0) CircleSize = GetNominalCircleSize(Control.GetControlFrequency());

            if (Control.GetControlFrequency() == 0)
                return GetImage(ref PWM_Array, ImageSize, DrawSize, Thickness, CircleSize);

            Bitmap image = GetImage(ref PWM_Array, ImageSize, DrawSize, Thickness, CircleSize);
            return image;
        }
        public static Bitmap GetImage(
            ref PhaseState[] UVW,
            int ImageSize,
            double DrawSize,
            int Thickness,
            double CircleSize
        )
        {
            Bitmap Image = new(ImageSize, ImageSize);
            Graphics Graphic = Graphics.FromImage(Image);
            Graphic.FillRectangle(new SolidBrush(Color.White), 0, 0, ImageSize, ImageSize);

            Common.GetPoints(ref UVW, out PointD[] LinePoints, out PointD[] ZeroPoints);

            double K = DrawSize * ImageSize;
            PointD CenterPosition = new(ImageSize / 2.0, ImageSize / 2.0);

            for (int i = 0; i < LinePoints.Length - 1; i++)
                Graphic.DrawLine(
                    new Pen(Color.Black, Thickness),
                    (K * LinePoints[i] + CenterPosition).ToPoint(),
                    (K * LinePoints[i + 1] + CenterPosition).ToPoint()
                );

            if (CircleSize > 0)
            {
                for (int i = 0; i < ZeroPoints.Length; i++)
                {
                    Point ZeroPoint = (K * ZeroPoints[i] + CenterPosition).ToPoint();
                    double Radius = CircleSize;
                    Graphic.FillEllipse(new SolidBrush(Color.White),
                        (int)Math.Round(ZeroPoint.X - Radius),
                        (int)Math.Round(ZeroPoint.Y - Radius),
                        (int)Math.Round(Radius * 2),
                        (int)Math.Round(Radius * 2)
                    );
                    Graphic.DrawEllipse(new Pen(Color.Black),
                        (int)Math.Round(ZeroPoint.X - Radius),
                        (int)Math.Round(ZeroPoint.Y - Radius),
                        (int)Math.Round(Radius * 2),
                        (int)Math.Round(Radius * 2)
                    );
                }
            }
            
            Graphic.Dispose();
            return Image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo(GenerationParameter Parameter, string fileName, int fps,
            int ImageSize,
            double DrawSize,
            int Thickness,
            double CircleSize,
            int Division,
            bool Precise
        )
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            StructCompiled baseFreqData = Parameter.BaseFrequencyData;
            GUI.TaskViewer.TaskProgress progressData = Parameter.Progress;

            Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;

            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(ImageSize, ImageSize));
            if (!vr.IsOpened()) return;

            // Progress Initialize
            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / fps) + fps * 2;

            AddEmptyFrames(vr, ImageSize, ImageSize, fps, () => { progressData.Progress++; });
            Data.BaseFrequency.Analyze.ForwardTime(baseFreqData, Domain, vvvfData, 1.0 / fps, () =>
            {
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap image = GetImage(Domain.Clone(), ImageSize, DrawSize, Thickness, CircleSize, Division, Precise);
                Viewer?.SetImage(image);
                AddImageFrames(vr, image, 1);
                image.Dispose();
                progressData.Progress++;

                return progressData.Cancel;
            });
            AddEmptyFrames(vr, ImageSize, ImageSize, fps, () => { progressData.Progress++; });

            vr.Release();
            vr.Dispose();
            Viewer?.Close();
        }
        public void ExportImage(GenerationParameter Parameter, string fileName, 
            double Time, double ForwardStep,
            int ImageSize,
            double DrawSize,
            int Thickness,
            double CircleSize,
            int Division,
            bool Precise
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
            Bitmap image = GetImage(Domain.Clone(), ImageSize, DrawSize, Thickness, CircleSize, Division, Precise);
            Parameter.Progress.Progress = 2;

            image.Save(fileName, ImageFormat.Png);
            Viewer?.SetImage(image);
            image.Dispose();
            Parameter.Progress.Progress = 3;
        }
    }
}
