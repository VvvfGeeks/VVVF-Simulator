using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Data.BaseFrequency;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationParameter;
using static VvvfSimulator.Vvvf.Model.Struct;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;

namespace VvvfSimulator.Generation.Video.ControlInfo
{
    public class Design1
    {
        private static string[]? GetPulseName(Domain Domain)
        {
            if (Domain.ElectricalState.IsNone) return null;

            PulseTypeName Type = Domain.ElectricalState.PulsePattern.PulseMode.PulseType;
            int PulseCount = Domain.ElectricalState.PulsePattern.PulseMode.PulseCount;

            switch (Type)
            {
                case PulseTypeName.ASYNC:
                    {
                        string[] names = new string[3];
                        int count = 0;

                        names[count] = string.Format("Async - " + Domain.GetCarrierInstance().CalculateBaseCarrierFrequency(Domain.GetTime(), Domain.ElectricalState).ToString("F2")).PadLeft(6);
                        count++;

                        if (Domain.ElectricalState.CarrierFrequency.RandomRange.Range != 0)
                        {
                            names[count] = string.Format("Random ± " + Domain.ElectricalState.CarrierFrequency.RandomRange.Range.ToString("F2")).PadLeft(6);
                            count++;
                        }

                        double Dipolar = Domain.ElectricalState.PulseData.GetValueOrDefault(PulseDataKey.Dipolar, -1);
                        if (Dipolar != -1)
                        {
                            names[count] = string.Format("Dipolar : " + Dipolar.ToString("F0")).PadLeft(6);
                            //count++;
                        }
                        return names;
                    }
                case PulseTypeName.SYNC:
                    {
                        string ModeName = PulseCount + " Pulse";
                        double Dipolar = Domain.ElectricalState.PulseData.GetValueOrDefault(PulseDataKey.Dipolar, -1);
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
                case PulseTypeName.ΔΣ:
                    {
                        return [string.Format("ΔΣ - " + Domain.ElectricalState.PulseData.GetValueOrDefault(PulseDataKey.UpdateFrequency).ToString("F2")).PadLeft(6)];
                    }
                default:
                    {
                        return ["UNKOWN", "UNKOWN"];
                    }
            }
        }
        public static Bitmap GetImage(Domain Domain, bool final_show)
        {
            int image_width = 500;
            int image_height = 1080;

            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);

            Color gradation_color;
            if (Domain.IsFreeRun())
            {
                gradation_color = Color.FromArgb(0xE0, 0xFD, 0xE0);
            }
            else if (!Domain.IsBraking())
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

            Font title_fnt = new(
               Fonts.Manager.FugazOne,
               40,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            Font val_fnt = new(
               Fonts.Manager.Arial,
               50,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            Font val_mini_fnt = new(
               Fonts.Manager.Arial,
               25,
               FontStyle.Regular,
               GraphicsUnit.Pixel);

            Brush title_brush = Brushes.Black;
            Brush letter_brush = Brushes.Black;

            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 0, image_width, 68 - 0);
            g.DrawString("Pulse Mode", title_fnt, title_brush, 17, 8);
            g.FillRectangle(Brushes.Blue, 0, 68, image_width, 8);
            if (!final_show && !Domain.ElectricalState.IsNone)
            {
                string[] pulse_name = GetPulseName(Domain) ?? [];
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
            double sine_freq = Domain.ElectricalState.BaseWaveFrequency;
            if (!final_show && !Domain.ElectricalState.IsNone)
                g.DrawString(String.Format("{0:f2}", sine_freq).PadLeft(6), val_fnt, letter_brush, 17, 323);

            g.FillRectangle(new SolidBrush(Color.FromArgb(200, 200, 255)), 0, 447, image_width, 513 - 447);
            g.DrawString("Sine Amplitude[%]", title_fnt, title_brush, 17, 452);
            g.FillRectangle(Brushes.Blue, 0, 513, image_width, 8);
            if (!final_show && !Domain.ElectricalState.IsNone)
                g.DrawString(String.Format("{0:f2}", Domain.ElectricalState.BaseWaveAmplitude * 100).PadLeft(6), val_fnt, letter_brush, 17, 548);

            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 669, image_width, 735 - 669);
            g.DrawString("Freerun", title_fnt, title_brush, 17, 674);
            g.FillRectangle(Brushes.LightGray, 0, 735, image_width, 8);
            if (!final_show)
                g.DrawString(Domain.IsPowerOff().ToString(), val_fnt, letter_brush, 17, 750);

            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), 0, 847, image_width, 913 - 847);
            g.DrawString("Brake", title_fnt, title_brush, 17, 852);
            g.FillRectangle(Brushes.LightGray, 0, 913, image_width, 8);
            if (!final_show)
                g.DrawString(Domain.IsBraking().ToString(), val_fnt, letter_brush, 17, 930);

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

                Font simulator_title_fnt = new(
                    Fonts.Manager.FugazOne,
                    40,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);
                Font simulator_title_fnt_sub = new(
                    Fonts.Manager.FugazOne,
                    20,
                    FontStyle.Bold,
                    GraphicsUnit.Pixel);
                Font title_fnt = new(
                    Fonts.Manager.FugazOne,
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
        public void ExportVideo(GenerationParameter Parameter, string output_path, int fps)
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            StructCompiled baseFreqData = Parameter.BaseFrequencyData;
            ProgressData progressData = Parameter.Progress;

            Domain Domain = new(Parameter.TrainData.MotorSpec);

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

            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / fps) + fps * 2;

            while (loop)
            {
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap image = GetImage(Domain.Clone(), final_show);
                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                Viewer?.SetImage(image);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                vr.Write(mat);
                image.Dispose();              

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

                video_finished = !Data.BaseFrequency.Analyze.CheckForFreqChange(Domain, baseFreqData, vvvfData, 1.0 / fps);
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
