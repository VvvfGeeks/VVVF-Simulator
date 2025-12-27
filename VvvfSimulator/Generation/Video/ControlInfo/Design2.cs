using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Data.BaseFrequency;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationParameter;
using static VvvfSimulator.Generation.Video.ControlInfo.Common;
using static VvvfSimulator.Vvvf.Model.Struct;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace VvvfSimulator.Generation.Video.ControlInfo
{
    public class Design2
    {

        private class StringContent(Font f, string l, Point p)
        {
            public Font font = f;
            public string content = l;
            public Point compensation = p;
        }
        private static void DrawTopicAndValue(Graphics g,Point start, Size size, StringContent topic, StringContent value, StringContent unit, int topic_width)
        {
            SizeF topic_size = g.MeasureString(topic.content, topic.font);
            SizeF val_size = g.MeasureString(value.content, value.font) ;
            SizeF unit_size = g.MeasureString(unit.content, unit.font);

            FilledCornerCurvedRectangle(g, new SolidBrush(Color.FromArgb(0x33, 0x35, 0x33)), start, new Point(start.X + size.Width, start.Y + size.Height), 10);
            LineCornerCurvedRectangle(g, new Pen(Color.FromArgb(0xED, 0xF2, 0xF4), 5), start, new Point(start.X + size.Width, start.Y + size.Height), 10);

            g.DrawLine(new Pen(Color.White, 2), new Point(start.X + topic_width, start.Y + 10), new Point(start.X + topic_width, start.Y + size.Height - 10));

            float topic_x = start.X + topic_width / 2 - topic_size.Width / 2 + topic.compensation.X;
            float topic_y = start.Y + (size.Height - topic_size.Height) / 2 + topic.compensation.Y;
            g.DrawString(topic.content, topic.font, new SolidBrush(Color.White), new PointF(topic_x , topic_y));


            float value_x = start.X + topic_width + (size.Width - topic_width - val_size.Width - unit_size.Width) / 2 + value.compensation.X;
            float value_y = start.Y + (size.Height - val_size.Height) / 2 + value.compensation.Y;
            g.DrawString(value.content, value.font, new SolidBrush(Color.White), new PointF(value_x , value_y));

            float unit_x = value_x + val_size.Width + unit.compensation.X;
            float unit_y = value_y + val_size.Height - unit_size.Height + unit.compensation.Y;
            g.DrawString(unit.content, unit.font, new SolidBrush(Color.White), new PointF(unit_x, unit_y));
        }
        private static string? GetPulseName(Domain Control)
        {
            if (Control.ElectricalState.IsNone) return null;

            int PulseCount = Control.ElectricalState.PulsePattern.PulseMode.PulseCount;

            static string FormatString(double Value)
            {
                if (Value < 10000) return Value.ToString("F2");
                if (Value < 100000) return Value.ToString("F1");
                if (Value < 1000000) return Value.ToString("F0");
                return string.Format("{0:E0}", Value);
            }

            return Control.ElectricalState.PulsePattern.PulseMode.PulseType switch
            {
                PulseTypeName.ASYNC => FormatString(Control.GetCarrierInstance().CalculateBaseCarrierFrequency(Control.GetTime(), Control.ElectricalState)),
                PulseTypeName.SYNC => PulseCount.ToString(),
                PulseTypeName.CHM => "CHM " + PulseCount.ToString(),
                PulseTypeName.SHE => "SHE " + PulseCount.ToString(),
                PulseTypeName.HO => "HO " + PulseCount.ToString(),
                PulseTypeName.ΔΣ => FormatString(Control.ElectricalState.PulseData.GetValueOrDefault(PulseDataKey.UpdateFrequency)),
                _ => PulseCount.ToString(),
            };
        }
        public static Bitmap GetImage(Domain Control, bool Precise)
        {
            int image_width = 1920;
            int image_height = 500;
            double voltage = 0;

            Bitmap image = new(image_width, image_height);
            Bitmap hexagon = new(400, 400), wave_form = new(1520, 400);
            Graphics g = Graphics.FromImage(image);

            Domain CycleControl = Control.Clone();
            PhaseState[] CycleUVW = [];

            // CALCULATE ONE CYCLE OF PWM
            Task CycleCalcTask = Task.Run(() =>
            {
                CycleControl.GetCarrierInstance().UseSimpleFrequency = true;
                CycleUVW = GenerateBasic.WaveForm.GetUVWCycle(CycleControl, MyMath.M_PI_6, (Precise ? 120000 : 6000), Precise);
            });
            Task WaveFormTask = Task.Run(() => {
                Domain WaveFormControl = Control.Clone();
                WaveFormControl.GetCarrierInstance().UseSimpleFrequency = true;
                wave_form = WaveForm.GenerateWaveFormUV.GetImage(WaveFormControl, 1520, 400, 80, 2, Precise ? 60 : 1 ,50);
            });
            CycleCalcTask.Wait();
            Task HexagonRenderTask = Task.Run(() =>
            {
                hexagon = new(Hexagon.Design1.GetImage(ref CycleUVW, CycleControl.GetControlFrequency(), 1000, 1000, 2, true), 400, 400);
            });
            Task VoltageCalcTask = Task.Run(() =>
            {
                voltage = GenerateBasic.Fourier.GetVoltageRate(ref CycleUVW) * 100;
            });
            HexagonRenderTask.Wait();
            VoltageCalcTask.Wait();
            WaveFormTask.Wait();
            g.DrawImage(wave_form, 400, 100);
            g.DrawImage(hexagon, 0, 100);

            Color stat_color, back_color, stat_str_color;
            String stat_str;
            bool stopping = CycleControl.GetBaseWaveAngleFrequency() == 0;
            if (stopping)
            {
                stat_str_color = Color.White;
                stat_color = Color.FromArgb(0x33, 0x35, 0x33);

                back_color = Color.FromArgb(0x81, 0x7F, 0x82);
                stat_str = "Stop";
            }
            else if (CycleControl.IsFreeRun())
            {
                stat_str_color = Color.White;
                stat_color = Color.FromArgb(0x36, 0xd0, 0x36);

                back_color = Color.FromArgb(0x81, 0x7F, 0x82);
                stat_str = "Cruise";
            }
            else if (!CycleControl.IsBraking())
            {
                stat_str_color = Color.White;
                stat_color = Color.FromArgb(0x43,0x92, 0xF1);

                back_color = Color.FromArgb(0x81, 0x7F, 0x82);
                stat_str = "Accelerate";
            }
            else
            {
                stat_str_color = Color.White;
                stat_color = Color.FromArgb(0xe6, 0x7e, 0x00);

                back_color = Color.FromArgb(0x81, 0x7F, 0x82);
                stat_str = "Brake";
            }
            g.FillRectangle(new SolidBrush(stat_color), 0, 0, 400, 100);

            g.FillRectangle(new SolidBrush(back_color), 400, 0, 1520, 100);

            Font stat_Font = new(Fonts.Manager.FugazOne, 40, FontStyle.Regular, GraphicsUnit.Pixel);
            Font topic_Font = new(Fonts.Manager.FugazOne, 40, FontStyle.Regular, GraphicsUnit.Pixel);
            Font value_Font = new(Fonts.Manager.DSEG14ModernItalic, 40, FontStyle.Regular, GraphicsUnit.Pixel);
            Font unit_font = new(Fonts.Manager.FugazOne, 25, FontStyle.Regular, GraphicsUnit.Pixel);

            SizeF stat_str_Size = g.MeasureString(stat_str, stat_Font);
            g.DrawString(stat_str, stat_Font, new SolidBrush(stat_str_color), new PointF((400 - stat_str_Size.Width) / 2 , (100 - stat_str_Size.Height) / 2 + 5));

            // pulse state


            bool useHzUnit = (CycleControl.ElectricalState.PulsePattern?.PulseMode?.PulseType.Equals(PulseTypeName.ASYNC) ?? false) ||
                (CycleControl.ElectricalState.PulsePattern?.PulseMode?.PulseType.Equals(PulseTypeName.ΔΣ) ?? false);
            DrawTopicAndValue(
                g, new Point(420, 10), new Size(480, 80),
                new StringContent(topic_Font, "Pulse", new Point(0, 5)),
                new StringContent(value_Font, stopping ?  "-----" : (GetPulseName(CycleControl) ?? "-----"), new Point(0, 5)),
                new StringContent(unit_font, useHzUnit ? "Hz" : "", new Point(0, 9)),
                200);

            DrawTopicAndValue(
                g, new Point(920, 10), new Size(480, 80),
                new StringContent(topic_Font, "Voltage", new Point(0, 5)),
                new StringContent(value_Font, stopping || Control.ElectricalState.IsNone ? "---.-" : String.Format("{0:F1}", Math.Round(voltage, 2)), new Point(0, 5)),
                new StringContent(unit_font, "%", new Point(0, 9)),
                200);

            DrawTopicAndValue(
                g, new Point(1420, 10), new Size(480, 80),
                new StringContent(topic_Font, "Freq", new Point(0, 5)),
                new StringContent(value_Font, stopping || Control.ElectricalState.IsNone ? "---.-" : String.Format("{0:F1}", CycleControl.ElectricalState.BaseWaveFrequency), new Point(0, 5)),
                new StringContent(unit_font, "Hz", new Point(0, 9)),
                200);

            g.Dispose();
            return image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo(
            GenerationParameter Parameter,
            string output_path,
            int fps
        )
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            StructCompiled baseFreqData = Parameter.BaseFrequencyData;
            ProgressData progressData = Parameter.Progress;

            Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;

            int image_width = 1920;
            int image_height = 500;
            VideoWriter vr = new(output_path, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));

            if (!vr.IsOpened())
            {
                return;
            }

            // PROGRESS INITIALIZE
            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / fps) + 2 * fps;

            bool START_FRAMES = true;
            if (START_FRAMES)
            {
                Domain.SetBraking(false);
                Domain.SetPowerOff(false);
                Domain.SetFreeRun(false);
                Domain.SetControlFrequency(0);
                Domain.SetBaseWaveAngleFrequency(0);

                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap final_image = GetImage(Domain.Clone(), true);

                AddImageFrames(final_image, fps, vr);
                Viewer?.SetImage(final_image);
                final_image.Dispose();
            }

            //PROGRESS ADD
            progressData.Progress += fps;

            while (true)
            {
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap final_image = GetImage(Domain.Clone(), true);

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

                Domain.SetBraking(true);
                Domain.SetPowerOff(false);
                Domain.SetFreeRun(false);
                Domain.SetControlFrequency(0);
                Domain.SetBaseWaveAngleFrequency(0);
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap final_image = GetImage(Domain.Clone(), true);
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
