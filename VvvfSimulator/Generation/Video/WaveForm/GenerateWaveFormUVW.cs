using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using VvvfSimulator.Data.BaseFrequency;
using VvvfSimulator.GUI.Util;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Vvvf.Calculation.Common;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.Generation.Video.WaveForm
{
    public class GenerateWaveFormUVW
    {
        public static Bitmap GetImage(Domain Domain,
            int Width, int Height,
            int WaveHeight, int Thikness,
            int Division
        )
        {
            Bitmap image = new(Width, Height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);

            PhaseState? LastValue = null;

            Domain.ResetTimeAll();
            for (int Count = 0; Count < Width * Division; Count++)
            {
                Domain.SetTimeAll(Math.PI * Count / (80.0 * Width * Division));

                PhaseState Value = CalculatePhsaseState(Domain, 0);

                if(LastValue == null)
                {
                    LastValue = Value;
                    continue;
                }

                //U
                g.DrawLine(new Pen(Color.Black, Thikness),
                    (int)Math.Round(Count / (double)Division),
                    (int)Math.Round(-WaveHeight * (LastValue.U - 1) + 0.25 * (Height - 2 * WaveHeight)),
                    (int)Math.Round(((LastValue.U != Value.U) ? Count : Count + 1) / (double)Division),
                    (int)Math.Round(-WaveHeight * (Value.U - 1) + 0.25 * (Height - 2 * WaveHeight))
                );

                //V
                g.DrawLine(new Pen(Color.Black, Thikness),
                    (int)Math.Round(Count / (double)Division),
                    (int)Math.Round(-WaveHeight * (LastValue.V - 1) + Height / 2.0),
                    (int)Math.Round(((LastValue.V != Value.V) ? Count : Count + 1) / (double)Division),
                    (int)Math.Round(-WaveHeight * (Value.V - 1) + Height / 2.0)
                );

                //W
                g.DrawLine(new Pen(Color.Black, Thikness),
                    (int)Math.Round(Count / (double)Division),
                    (int)Math.Round(-WaveHeight * (LastValue.W - 1) + 0.25 * (3 * Height + 2 * WaveHeight)),
                    (int)Math.Round(((LastValue.W != Value.W) ? Count : Count + 1) / (double)Division),
                    (int)Math.Round(-WaveHeight * (Value.W - 1) + 0.25 * (3 * Height + 2 * WaveHeight))
                );

                LastValue = Value;
            }

            g.Dispose();
            return image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo(GenerationParameter Parameter, string fileName, int fps,
            int Width, int Height,
            int WaveHeight, int Thikness,
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

            // PROGRESS INITIALIZE
            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / fps) + 2 * fps;

            {
                Domain.SetFreeRun(false);
                Domain.SetBraking(false);
                Domain.SetPowerOff(false);
                Domain.SetControlFrequency(0);
                Domain.GetBaseWaveInstance().AngleFrequency = 0;

                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap image = GetImage(Domain.Clone(), Width, Height, WaveHeight, Thikness, Division);
                AddImageFrames(vr, image, fps, () => { progressData.Progress++; });
                image.Dispose();
            }

            Data.BaseFrequency.Analyze.ForwardTime(baseFreqData, Domain, vvvfData, 1.0 / fps, () =>
            {
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);

                Bitmap image = GetImage(Domain.Clone(), Width, Height, WaveHeight, Thikness, Division);
                Viewer?.SetImage(image);
                AddImageFrames(vr, image, 1);
                image.Dispose();
                progressData.Progress++;

                return progressData.Cancel;
            });

            {
                Domain.SetFreeRun(false);
                Domain.SetBraking(true);
                Domain.SetPowerOff(false);
                Domain.SetControlFrequency(0);
                Domain.GetBaseWaveInstance().AngleFrequency = 0;

                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap image = GetImage(Domain.Clone(), Width, Height, WaveHeight, Thikness, Division);
                AddImageFrames(vr, image, fps, () => { progressData.Progress++; });
                image.Dispose();
            }

            vr.Release();
            vr.Dispose();

            Viewer?.Close();
        }
        public void ExportImage(GenerationParameter Parameter, string fileName,
            double Time, double ForwardStep,
            int Width, int Height,
            int WaveHeight, int Thikness, 
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
            Bitmap image = GetImage(Domain.Clone(), Width, Height, WaveHeight, Thikness, Division);
            Parameter.Progress.Progress = 2;

            image.Save(fileName, ImageFormat.Png);
            Viewer?.SetImage(image);
            image.Dispose();
            Parameter.Progress.Progress = 3;
        }

    }
}
