using OpenCvSharp;
using System;
using System.Collections.Generic;
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
    public class GenerateWaveFormUV
    {

        /// <summary>
        /// Do clone before call this!
        /// </summary>
        /// <param name="Control"></param>
        /// <param name="PWM_Data"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="WaveHeight"></param>
        /// <param name="Delta"></param>
        /// <returns></returns>
        public static Bitmap GetImage(
            VvvfValues Control,
            PwmCalculateValues PWM_Data,
            int Width, 
            int Height, 
            int WaveHeight,
            int WaveWidth,
            int Delta,
            int Spacing
        )
        {
            int Count = (Width - Spacing * 2) * Delta;
            WaveValues[] values = new WaveValues[Count];
            for (int i = 0; i < Count; i++)
            {
                WaveValues value = CalculatePhases(Control, PWM_Data, Math.PI / 6.0);
                values[i] = value;
                Control.SetGenerationCurrentTime(2 / (60.0 * Count) * i);
                Control.SetSawTime(2 / (60.0 * Count) * i);
                Control.SetSineTime(2 / (60.0 * Count) * i);
            }
            return GetImage(ref values, Width, Height, WaveHeight, WaveWidth, Spacing);
        }

        public static Bitmap GetImage(
            ref WaveValues[] UVW,
            int Width,
            int Height,
            int WaveHeight,
            int WaveWidth,
            int Spacing
        )
        {
            Bitmap image = new(Width, Height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);

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
                g.DrawLine(new Pen(Color.Black, WaveWidth), x_1, y_1, x_2, y_2);
            }

            g.Dispose();
            return image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo1(GenerationBasicParameter generationBasicParameter, String fileName)
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            YamlVvvfSoundData vvvfData = generationBasicParameter.VvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.MasconData;
            ProgressData progressData = generationBasicParameter.Progress;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();

            control.SetRandomFrequencyMoveAllowed(false);

            int fps = 60;

            int image_width = 2880;
            int image_height = 540;

            int wave_height = 100;
            int calculate_div = 30;

            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            if (!vr.IsOpened())
            {
                return;
            }

            // Progress Initialize
            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + 120;

            Boolean START_WAIT = true;
            if (START_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

                    vr.Write(mat);
                }
                g.Dispose();
                image.Dispose();
            }

            Boolean loop = true;
            while (loop)
            {

                control.SetSineTime(0);
                control.SetSawTime(0);

                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, vvvfData);

                Bitmap image = GetImage(control.Clone(), calculated_Values, image_width, image_height, wave_height, 2, calculate_div, 100);


                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                vr.Write(mat);
                mat.Dispose();
                ms.Dispose();

                Viewer?.SetImage(image);
                image.Dispose();

                loop = CheckForFreqChange(control, masconData, vvvfData, 1.0 / fps);
                if (progressData.Cancel) loop = false;

                // PROGRESS CHANGE
                progressData.Progress++;
            }

            Boolean END_WAIT = true;
            if (END_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

                    vr.Write(mat);
                }
                g.Dispose();
                image.Dispose();
            }

            vr.Release();
            vr.Dispose();
            Viewer?.Close();
        }

        public void ExportVideo2(GenerationBasicParameter generationBasicParameter, String fileName)
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            YamlVvvfSoundData vvvfData = generationBasicParameter.VvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.MasconData;
            ProgressData progressData = generationBasicParameter.Progress;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();
            control.SetRandomFrequencyMoveAllowed(false);

            int fps = 60;

            int image_width = 2000;
            int image_height = 500;

            int wave_height = 100;
            int calculate_div = 10;

            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            if (!vr.IsOpened())
            {
                return;
            }

            // Progress Initialize
            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + 120;

            Boolean START_WAIT = true;
            if (START_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                g.DrawLine(new Pen(Color.Gray), 0, image_height / 2, image_width, image_height / 2);
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

                    vr.Write(mat);
                }
                g.Dispose();
                image.Dispose();
            }

            Boolean loop = true;
            while (loop)
            {

                control.SetSineTime(0);
                control.SetSawTime(0);

                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, vvvfData);
                Bitmap image = GetImage(control.Clone(), calculated_Values, image_width, image_height, wave_height, 1, calculate_div, 0);

                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                Viewer?.SetImage(image);
                vr.Write(mat);

                image.Dispose();
                loop = CheckForFreqChange(control, masconData, vvvfData, 1.0 / fps);
                if(progressData.Cancel) loop = false;

                // PROGRESS CHANGE
                progressData.Progress++;

            }

            Boolean END_WAIT = true;
            if (END_WAIT)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);
                g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                g.DrawLine(new Pen(Color.Gray), 0, image_height / 2, image_width, image_height / 2);
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

                    vr.Write(mat);
                }
                g.Dispose();
                image.Dispose();
            }

            vr.Release();
            vr.Dispose();
            Viewer?.Close();
        }
    }
}
