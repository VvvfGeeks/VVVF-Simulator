using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.Vvvf.Calculate;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;

namespace VvvfSimulator.Generation.Video.ControlInfo
{
    public class GenerateControlOriginal
    {
        private static string[] GetPulseName(VvvfValues control)
        {
            PulseTypeName Type = control.GetVideoPulseMode().PulseType;
            int PulseCount = control.GetVideoPulseMode().PulseCount;

            switch (Type)
            {
                case PulseTypeName.ASYNC:
                    {
                        string[] names = new string[3];
                        int count = 0;

                        CarrierFreq Carrier = control.GetVideoCarrierFrequency();

                        names[count] = String.Format("Async - " + Carrier.BaseFrequency.ToString("F2")).PadLeft(6);
                        count++;

                        if (Carrier.Range != 0)
                        {
                            names[count] = String.Format("Random ± " + Carrier.Range.ToString("F2")).PadLeft(6);
                            count++;
                        }

                        double Dipolar = control.GetVideoCalculatedPulseData().GetValueOrDefault(PulseDataKey.Dipolar, -1);
                        if (Dipolar != -1)
                        {
                            names[count] = String.Format("Dipolar : " + Dipolar.ToString("F0")).PadLeft(6);
                            //count++;
                        }
                        return names;
                    }
                case PulseTypeName.SYNC:
                    {
                        string ModeName = PulseCount + " Pulse";
                        double Dipolar = control.GetVideoCalculatedPulseData().GetValueOrDefault(PulseDataKey.Dipolar, -1);
                        if (Dipolar == -1) return [ModeName];
                        else return [ModeName, "Dipolar : " + Dipolar.ToString("F1")];
                    }
                case PulseTypeName.CHM:
                    {
                        return [PulseCount + " Pulse", "Current Harmonic Minimum"];
                    }
                case PulseTypeName.SHE:
                    {
                        return [PulseCount + " Pulse", "Selective Harmonic Elimination"];
                    }
                case PulseTypeName.HO:
                    {
                        return [PulseCount + " Pulse", "High efficiency Over-modulation"];
                    }
                default:
                    {
                        return ["UNKOWN", "UNKOWN"];
                    }
            }
        }
        public static Bitmap GetImage(VvvfValues control, bool final_show)
        {
            int image_width = 500;
            int image_height = 1080;

            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);

            Color gradation_color;
            if (control.IsFreeRun())
            {
                gradation_color = Color.FromArgb(0xE0, 0xFD, 0xE0);
            }
            else if (!control.IsBraking())
            {
                gradation_color = Color.FromArgb(0xE0, 0xE0, 0xFD);
            }
            else
            {
                gradation_color = Color.FromArgb(0xFD, 0xE0, 0xE0);
            }


            LinearGradientBrush gb = new(
                new System.Drawing.Point(0, 0),
                new System.Drawing.Point(image_width, image_height),
                Color.FromArgb(0xFF, 0xFF, 0xFF),
                gradation_color
            );

            g.FillRectangle(gb, 0, 0, image_width, image_height);

            FontFamily title_fontFamily = new("Fugaz One");
            Font title_fnt = new(
               title_fontFamily,
               40,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            FontFamily val_fontFamily = new("Arial Rounded MT Bold");
            Font val_fnt = new(
               val_fontFamily,
               50,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            FontFamily val_mini_fontFamily = new("Arial Rounded MT Bold");
            Font val_mini_fnt = new(
               val_mini_fontFamily,
               25,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            Brush title_brush = Brushes.Black;
            Brush letter_brush = Brushes.Black;

            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 0, image_width, 68 - 0);
            g.DrawString("Pulse Mode", title_fnt, title_brush, 17, 8);
            g.FillRectangle(Brushes.Blue, 0, 68, image_width, 8);
            if (!final_show)
            {
                String[] pulse_name = GetPulseName(control);

                g.DrawString(pulse_name[0], val_fnt, letter_brush, 17, 100);

                if (pulse_name.Length > 1)
                {
                    if (pulse_name.Length == 2)
                    {
                        g.DrawString(pulse_name[1], val_mini_fnt, letter_brush, 17, 170);
                    }
                    else if (pulse_name.Length == 3)
                    {
                        g.DrawString(pulse_name[1], val_mini_fnt, letter_brush, 17, 160);
                        g.DrawString(pulse_name[2], val_mini_fnt, letter_brush, 17, 180);
                    }
                }

            }


            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 226, image_width, 291 - 226);
            g.DrawString("Sine Freq[Hz]", title_fnt, title_brush, 17, 231);
            g.FillRectangle(Brushes.Blue, 0, 291, image_width, 8);
            double sine_freq = control.GetVideoSineFrequency();
            if (!final_show)
                g.DrawString(String.Format("{0:f2}", sine_freq).PadLeft(6), val_fnt, letter_brush, 17, 323);

            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 447, image_width, 513 - 447);
            g.DrawString("Sine Amplitude[%]", title_fnt, title_brush, 17, 452);
            g.FillRectangle(Brushes.Blue, 0, 513, image_width, 8);
            if (!final_show)
                g.DrawString(String.Format("{0:f2}", control.GetVideoSineAmplitude() * 100).PadLeft(6), val_fnt, letter_brush, 17, 548);

            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 669, image_width, 735 - 669);
            g.DrawString("Freerun", title_fnt, title_brush, 17, 674);
            g.FillRectangle(Brushes.LightGray, 0, 735, image_width, 8);
            if (!final_show)
                g.DrawString(control.IsMasconOff().ToString(), val_fnt, letter_brush, 17, 750);

            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 847, image_width, 913 - 847);
            g.DrawString("Brake", title_fnt, title_brush, 17, 852);
            g.FillRectangle(Brushes.LightGray, 0, 913, image_width, 8);
            if (!final_show)
                g.DrawString(control.IsBraking().ToString(), val_fnt, letter_brush, 17, 930);

            g.Dispose();

            return image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        private void GenerateOpening(int image_width, int image_height, VideoWriter vr)
        {
            double change_per_frame = 60 / vr.Fps;
            for (double i = 0; i < 128; i += change_per_frame)
            {
                Bitmap image = new(image_width, image_height);
                Graphics g = Graphics.FromImage(image);

                LinearGradientBrush gb = new(new System.Drawing.Point(0, 0), new System.Drawing.Point(image_width, image_height), Color.FromArgb(0xFF, 0xFF, 0xFF), Color.FromArgb(0xFD, 0xE0, 0xE0));
                g.FillRectangle(gb, 0, 0, image_width, image_height);

                FontFamily simulator_title = new("Fugaz One");
                Font simulator_title_fnt = new(
                    simulator_title,
                    40,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);
                Font simulator_title_fnt_sub = new(
                    simulator_title,
                    20,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);

                FontFamily title_fontFamily = new("Fugaz One");
                Font title_fnt = new(
                    title_fontFamily,
                    40,
                    FontStyle.Regular,
                    GraphicsUnit.Pixel);

                Brush title_brush = Brushes.Black;

                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 0, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 68 - 0);
                g.DrawString("Pulse Mode", title_fnt, title_brush, (int)((i < 40) ? -1000 : (double)((i > 80) ? 17 : 17 * (i - 40) / 40.0)), 8);
                g.FillRectangle(Brushes.Blue, 0, 68, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 226, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 291 - 226);
                g.DrawString("Sine Freq[Hz]", title_fnt, title_brush, (int)((i < 40 + 10) ? -1000 : (double)((i > 80 + 10) ? 17 : 17 * (i - (40 + 10)) / 40.0)), 231);
                g.FillRectangle(Brushes.Blue, 0, 291, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 447, (int)(image_width * (double)(((i > 30) ? 1 : i / 30.0))), 513 - 447);
                g.DrawString("Sine Amplitude[%]", title_fnt, title_brush, (int)((i < 40 + 20) ? -1000 : (i > 80 + 20) ? 17 : 17 * (i - (40 + 20)) / 40.0), 452);
                g.FillRectangle(Brushes.Blue, 0, 513, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 669, (int)(image_width * (double)(((i > 30) ? 1 : i / 30.0))), 735 - 669);
                g.DrawString("Freerun", title_fnt, title_brush, (int)((i < 40 + 30) ? -1000 : (i > 80 + 30) ? 17 : 17 * (i - (40 + 30)) / 40.0), 674);
                g.FillRectangle(Brushes.LightGray, 0, 735, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 847, (int)(image_width * (double)(((i > 30) ? 1 : i / 30.0))), 913 - 847);
                g.DrawString("Brake", title_fnt, title_brush, (int)((i < 40 + 40) ? -1000 : (i > 80 + 40) ? 17 : 17 * (i - (40 + 40)) / 40.0), 852);
                g.FillRectangle(Brushes.LightGray, 0, 913, (int)(image_width * ((i > 30) ? 1 : i / 30.0)), 8);

                g.FillRectangle(new SolidBrush(Color.FromArgb((int)(0xB0 * ((i > 96) ? (128 - i) / 36.0 : 1)), 0x00, 0x00, 0x00)), 0, 0, image_width, image_height);
                int transparency = (int)(0xFF * ((i > 96) ? (128 - i) / 36.0 : 1));
                g.DrawString("C# VVVF Simulator", simulator_title_fnt, new SolidBrush(Color.FromArgb(transparency, 0xFF, 0xFF, 0xFF)), 50, 420);
                g.DrawLine(new Pen(new SolidBrush(Color.FromArgb(transparency, 0xA0, 0xA0, 0xFF))), 0, 464, (int)((i > 20) ? image_width : image_width * i / 20.0), 464);
                g.DrawString("presented by VvvfGeeks", simulator_title_fnt_sub, new SolidBrush(Color.FromArgb(transparency, 0xE0, 0xE0, 0xFF)), 135, 460);

                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                vr.Write(mat);
                Viewer?.SetImage(image);
                image.Dispose();
            }
        }
        public void ExportVideo(GenerationBasicParameter generationBasicParameter, string output_path, int fps)
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            YamlVvvfSoundData vvvfData = generationBasicParameter.VvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.MasconData;
            ProgressData progressData = generationBasicParameter.Progress;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();

            int image_width = 500;
            int image_height = 1080;
            VideoWriter vr = new(output_path, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));

            if (!vr.IsOpened())
            {
                return;
            }

            GenerateOpening(image_width, image_height, vr);

            bool loop = true, video_finished, final_show = false, first_show = true;
            int freeze_count = 0;

            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + fps * 2;

            while (loop)
            {
                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, vvvfData);
                _ = CalculatePhases(control, calculated_Values, 0);

                control.SetSineTime(0);
                control.SetSawTime(0);
                Bitmap image = GetImage(control, final_show);
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                Viewer?.SetImage(image);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                vr.Write(mat);
                image.Dispose();              

                //PROGRESS ADD
                progressData.Progress++;

                if (first_show)
                {
                    freeze_count++;
                    if (freeze_count > fps)
                    {
                        freeze_count = 0;
                        first_show = false;
                    }
                    continue;
                }

                video_finished = !CheckForFreqChange(control, masconData, vvvfData, 1.0 / fps);
                if (video_finished)
                {
                    final_show = true;
                    freeze_count++;
                }
                if (freeze_count > fps) loop = false;
                if (progressData.Cancel) loop = false;


            }
            vr.Release();
            vr.Dispose();

            Viewer?.Close();
        }
    }
}
