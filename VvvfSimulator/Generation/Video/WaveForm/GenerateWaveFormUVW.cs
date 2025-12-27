using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Data.BaseFrequency;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationParameter;
using static VvvfSimulator.Vvvf.Calculation.Common;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.Generation.Video.WaveForm
{
    public class GenerateWaveFormUVW
    {
        private static readonly int image_width = 1500;
        private static readonly int image_height = 1000;
        private static readonly int calculate_div = 10;
        public static Bitmap GetImage(Domain Domain)
        {
            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);

            PhaseState? LastValue = null;

            Domain.ResetTimeAll();
            for (int i = 0; i < image_width * calculate_div; i++)
            {
                double dt = Math.PI / (120000.0 * calculate_div);
                Domain.SetTimeAll(dt * i);

                PhaseState Value = CalculatePhsaseState(Domain, 0);

                if(LastValue == null)
                {
                    LastValue = Value;
                    continue;
                }

                //U
                g.DrawLine(new Pen(Color.Black),
                    (int)Math.Round(i / (double)calculate_div),
                    LastValue.U * -100 + 300,
                    (int)Math.Round(((LastValue.U != Value.U) ? i : i + 1) / (double)calculate_div),
                    Value.U * -100 + 300
                );

                //V
                g.DrawLine(new Pen(Color.Black),
                    (int)Math.Round(i / (double)calculate_div),
                    LastValue.V * -100 + 600,
                    (int)Math.Round(((LastValue.V != Value.V) ? i : i + 1) / (double)calculate_div),
                    Value.V * -100 + 600
                );

                //W
                g.DrawLine(new Pen(Color.Black),
                    (int)Math.Round(i / (double)calculate_div),
                    LastValue.W * -100 + 900,
                    (int)Math.Round(((LastValue.W != Value.W) ? i : i + 1) / (double)calculate_div),
                    Value.W * -100 + 900
                );

                LastValue = Value;
            }

            g.Dispose();
            return image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo(GenerationParameter Parameter, String fileName)
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            StructCompiled baseFreqData = Parameter.BaseFrequencyData;
            ProgressData progressData = Parameter.Progress;

            Domain Domain = new(Parameter.TrainData.MotorSpec);

            int fps = 60;
            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));
            if (!vr.IsOpened()) return;

            // PROGRESS INITIALIZE
            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / fps) + 2 * fps;

            bool START_FRAMES = true;
            if (START_FRAMES)
            {
                Domain.SetFreeRun(false);
                Domain.SetBraking(false);
                Domain.SetPowerOff(false);
                Domain.SetControlFrequency(0);
                Domain.SetBaseWaveAngleFrequency(0);

                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap final_image = GetImage(Domain.Clone());

                AddImageFrames(final_image, fps, vr);
                Viewer?.SetImage(final_image);
                final_image.Dispose();
            }

            //PROGRESS ADD
            progressData.Progress += fps;

            while (true)
            {
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap final_image = GetImage(Domain.Clone());

                MemoryStream ms = new();
                final_image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                vr.Write(mat);
                ms.Dispose();
                mat.Dispose();
                Viewer?.SetImage(final_image);
                final_image.Dispose();

                if (!Data.BaseFrequency.Analyze.CheckForFreqChange(Domain, baseFreqData, vvvfData, 1.0 / fps)) break;
                if (progressData.Cancel) break;
                progressData.Progress++;
            }

            bool END_FRAMES = true;
            if (END_FRAMES)
            {
                Domain.SetFreeRun(false);
                Domain.SetBraking(true);
                Domain.SetPowerOff(false);
                Domain.SetControlFrequency(0);
                Domain.SetBaseWaveAngleFrequency(0);
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap final_image = GetImage(Domain.Clone());
                AddImageFrames(final_image, fps, vr);
                Viewer?.SetImage(final_image);
                final_image.Dispose();
            }

            //PROGRESS ADD
            progressData.Progress += fps;

            vr.Release();
            vr.Dispose();

            Viewer?.Close();
        }

    }
}
