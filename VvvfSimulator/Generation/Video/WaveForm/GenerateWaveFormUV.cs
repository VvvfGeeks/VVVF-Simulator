using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using VvvfSimulator.Data.BaseFrequency;
using VvvfSimulator.GUI.Util;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.Generation.Video.WaveForm
{
    public class GenerateWaveFormUV
    {
        public static Bitmap GetImage(
            Domain Control,
            int Width, 
            int Height, 
            int WaveHeight,
            int LineWidth,
            int Spacing,
            bool BaseLine,
            int Division
        )
        {
            int Count = (Width - Spacing * 2) * Division;
            PhaseState[] values = GenerateBasic.WaveForm.GetUVW(Control, Math.PI / 6.0, 30.0 * Count, Count);
            return GetImage(ref values, Width, Height, WaveHeight, LineWidth, Spacing, BaseLine);
        }

        public static Bitmap GetImage(
            ref PhaseState[] UVW,
            int Width,
            int Height,
            int WaveHeight,
            int Thikness,
            int Spacing,
            bool BaseLine
        )
        {
            Bitmap image = new(Width, Height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);
            if(BaseLine) g.DrawLine(new Pen(Color.Gray), Spacing, Height / 2, Width - Spacing, Height / 2);

            List<int> points_x = [];
            List<int> points_y = [];

            points_x.Add(Spacing);
            points_y.Add((int)(Height / 2.0));

            int pre_pwm = 0;

            for (int i = 0; i < UVW.Length; i++)
            {
                int pwm = UVW[i].U - UVW[i].V;
                if (pre_pwm != pwm)
                {
                    points_x.Add((int)(i / (double)UVW.Length * (Width - Spacing * 2)) + Spacing);
                    points_y.Add((int)(-pre_pwm * WaveHeight + Height / 2.0));

                    points_x.Add((int)(i / (double)UVW.Length * (Width - Spacing * 2)) + Spacing);
                    points_y.Add((int)(-pwm * WaveHeight + Height / 2.0));
                    pre_pwm = pwm;
                }
            }

            points_x.Add(Width - Spacing);
            points_y.Add((int)(-pre_pwm * WaveHeight + Height / 2.0));

            for (int i = 0; i < points_x.Count - 1; i++)
            {
                int x_1 = points_x[i];
                int x_2 = points_x[i + 1];
                int y_1 = points_y[i];
                int y_2 = points_y[i + 1];
                g.DrawLine(new Pen(Color.Black, Thikness), x_1, y_1, x_2, y_2);
            }

            g.Dispose();
            return image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo(GenerationParameter Parameter, string fileName, int fps,
            int Width, int Height,
            int WaveHeight, int Thikness,
            int Spacing, bool BaseLine,
            int Division
        )
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            StructCompiled baseFreqData = Parameter.BaseFrequencyData;
            GUI.TaskViewer.TaskProgress progressData = Parameter.Progress;

            Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;

            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(Width, Height));

            if (!vr.IsOpened()) return;

            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / fps) + fps * 2;

            PhaseState[] EmptyArray = [];
            Bitmap Empty = GetImage(ref EmptyArray, Width, Height, WaveHeight, Thikness, Spacing, BaseLine);

            AddImageFrames(vr, Empty, fps, () => { progressData.Progress++; });
            Analyze.ForwardTime(baseFreqData, Domain, vvvfData, 1.0 / fps, () =>
            {
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);

                Bitmap image = GetImage(Domain.Clone(), Width, Height, WaveHeight, Thikness, Spacing, BaseLine, Division);
                Viewer?.SetImage(image);
                AddImageFrames(vr, image, 1);
                image.Dispose();
                progressData.Progress++;

                return progressData.Cancel;
            });
            AddImageFrames(vr, Empty, fps, () => { progressData.Progress++; });

            vr.Release();
            vr.Dispose();
            Viewer?.Close();
        }
        public void ExportImage(GenerationParameter Parameter, string fileName,
            double Time, double ForwardStep,
            int Width, int Height,
            int WaveHeight, int Thikness,
            int Spacing, bool BaseLine,
            int Division
        )
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Parameter.Progress.Total = 3;

            Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;
            Analyze.ForwardTime(Parameter.BaseFrequencyData, Domain, Parameter.VvvfData, Time, ForwardStep);
            Parameter.Progress.Progress = 1;

            Data.Vvvf.Analyze.Calculate(Domain, Parameter.VvvfData);
            Bitmap image = GetImage(Domain.Clone(), Width, Height, WaveHeight, Thikness, Spacing, BaseLine, Division);
            Parameter.Progress.Progress = 2;

            image.Save(fileName, ImageFormat.Png);
            Viewer?.SetImage(image);
            image.Dispose();
            Parameter.Progress.Progress = 3;
        }        
    }
}
