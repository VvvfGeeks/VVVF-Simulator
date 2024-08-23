using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.Vvvf.Calculate;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;

namespace VvvfSimulator.Generation.Video.WaveForm
{
    public class GenerateWaveFormUVW
    {
        private static readonly int image_width = 1500;
        private static readonly int image_height = 1000;
        private static readonly int calculate_div = 10;
        public static Bitmap GetImage(VvvfValues control, YamlVvvfSoundData vvvfData)
        {
            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);

            WaveValues? LastValue = null;

            for (int i = 0; i < image_width * calculate_div; i++)
            {
                double dt = Math.PI / (120000.0 * calculate_div);
                control.SetGenerationCurrentTime(dt * i);
                control.SetSawTime(dt * i);
                control.SetSineTime(dt * i);

                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, vvvfData);
                WaveValues Value = CalculatePhases(control, calculated_Values, 0);

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
        public void ExportVideo(GenerationBasicParameter generationBasicParameter, String fileName)
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            YamlVvvfSoundData vvvfData = generationBasicParameter.VvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.MasconData;
            ProgressData progressData = generationBasicParameter.Progress;

            VvvfValues Control = new();
            Control.ResetControlValues();
            Control.ResetMathematicValues();

            int fps = 60;
            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));
            if (!vr.IsOpened()) return;

            // PROGRESS INITIALIZE
            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + 2 * fps;

            bool START_FRAMES = true;
            if (START_FRAMES)
            {
                Control.SetFreeRun(false);
                Control.SetBraking(false);
                Control.SetMasconOff(false);
                Control.SetControlFrequency(0);
                Control.SetSineAngleFrequency(0);
                Bitmap final_image = GetImage(Control, vvvfData);

                AddImageFrames(final_image, fps, vr);
                Viewer?.SetImage(final_image);
                final_image.Dispose();
            }

            //PROGRESS ADD
            progressData.Progress += fps;

            while (true)
            {
                Bitmap final_image = GetImage(Control.Clone(), vvvfData);

                MemoryStream ms = new();
                final_image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                vr.Write(mat);
                ms.Dispose();
                mat.Dispose();
                Viewer?.SetImage(final_image);
                final_image.Dispose();

                if (!CheckForFreqChange(Control, masconData, vvvfData, 1.0 / fps)) break;
                if (progressData.Cancel) break;
                progressData.Progress++;
            }

            bool END_FRAMES = true;
            if (END_FRAMES)
            {
                Control.SetFreeRun(false);
                Control.SetBraking(true);
                Control.SetMasconOff(false);
                Control.SetControlFrequency(0);
                Control.SetSineAngleFrequency(0);
                Bitmap final_image = GetImage(Control, vvvfData);
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
