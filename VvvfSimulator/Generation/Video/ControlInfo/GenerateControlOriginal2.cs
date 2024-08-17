using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.Generation.Video.ControlInfo.GenerateControlCommon;
using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace VvvfSimulator.Generation.Video.ControlInfo
{
    public class GenerateControlOriginal2
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
        private static String GetPulseName(VvvfValues control)
        {
            PulseMode mode_p = control.GetVideoPulseMode();
            PulseModeName mode = mode_p.PulseName;
            //Not in sync
            if (mode == PulseModeName.Async)
            {
                CarrierFreq carrier_freq_data = control.GetVideoCarrierFrequency();
                String default_s = String.Format(carrier_freq_data.base_freq.ToString("F2"));
                return default_s;
            }

            //Abs
            if (mode == PulseModeName.P_Wide_3)
                return "W 3";

            if (mode.ToString().StartsWith("CHM"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = (contain_wide ? "W " : "") + mode_name_type[1];

                return "CHM " + final_mode_name;
            }
            else if (mode.ToString().StartsWith("SHE"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = (contain_wide) ? "W " : "" + mode_name_type[1];

                return "SHE " + final_mode_name;
            }
            else if (mode.ToString().StartsWith("HOP"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = (contain_wide) ? "W " : "" + mode_name_type[1];

                return "HOP " + final_mode_name;
            }
            else
            {
                String[] mode_name_type = mode.ToString().Split("_");
                return mode_name_type[1];
            }
        }
        public static Bitmap GetImage(VvvfValues Control, YamlVvvfSoundData Sound, bool Precise)
        {
            int image_width = 1920;
            int image_height = 500;
            double voltage = 0;

            Bitmap image = new(image_width, image_height);
            Bitmap hexagon = new(400, 400), wave_form = new(1520, 400);
            Graphics g = Graphics.FromImage(image);

            VvvfValues CycleControl = Control.Clone();
            WaveValues[] CycleUVW = [];

            // CALCULATE ONE CYCLE OF PWM
            Task CycleCalcTask = Task.Run(() =>
            {
                CycleControl.SetRandomFrequencyMoveAllowed(false);
                CycleControl.SetSineTime(0);
                CycleControl.SetSawTime(0);
                CycleUVW = GenerateBasic.GetUVWCycle(CycleControl, Sound, MyMath.M_PI_6, (Precise ? 120000 : 6000), Precise);
            });
            Task WaveFormTask = Task.Run(() => {
                VvvfValues WaveFormControl = Control.Clone();
                WaveFormControl.SetRandomFrequencyMoveAllowed(false);
                WaveFormControl.SetSineTime(0);
                WaveFormControl.SetSawTime(0);
                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(WaveFormControl, new ControlStatus()
                {
                    brake = WaveFormControl.IsBraking(),
                    mascon_on = !WaveFormControl.IsMasconOff(),
                    free_run = WaveFormControl.IsFreeRun(),
                    wave_stat = WaveFormControl.GetControlFrequency()
                }, Sound);
                wave_form = WaveForm.GenerateWaveFormUV.GetImage(WaveFormControl, calculated_Values, 1520, 400, 80, 2, Precise ? 60 : 1 ,50);
            });
            CycleCalcTask.Wait();
            Task HexagonRenderTask = Task.Run(() =>
            {
                hexagon = new(Hexagon.GenerateHexagonOriginal.GetImage(ref CycleUVW, CycleControl.GetControlFrequency(), 1000, 1000, 2, true), 400, 400);
            });
            Task VoltageCalcTask = Task.Run(() =>
            {
                voltage = GetVoltageRate(ref CycleUVW) * 100;
            });
            HexagonRenderTask.Wait();
            VoltageCalcTask.Wait();
            WaveFormTask.Wait();
            g.DrawImage(wave_form, 400, 100);
            g.DrawImage(hexagon, 0, 100);

            Color stat_color, back_color, stat_str_color;
            String stat_str;
            bool stopping = CycleControl.GetSineAngleFrequency() == 0;
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

            Font stat_Font = new(new FontFamily("Fugaz One"), 40, FontStyle.Regular, GraphicsUnit.Pixel);
            Font topic_Font = new(new FontFamily("Fugaz One"), 40, FontStyle.Regular, GraphicsUnit.Pixel);
            Font value_Font = new(new FontFamily("DSEG14 Modern"), 40, FontStyle.Italic, GraphicsUnit.Pixel);
            Font unit_font = new(new FontFamily("Fugaz One"), 25, FontStyle.Regular, GraphicsUnit.Pixel);

            SizeF stat_str_Size = g.MeasureString(stat_str, stat_Font);
            g.DrawString(stat_str, stat_Font, new SolidBrush(stat_str_color), new PointF((400 - stat_str_Size.Width) / 2 , (100 - stat_str_Size.Height) / 2 + 5));

            // pulse state


            bool is_async = CycleControl.GetVideoPulseMode().PulseName.Equals(PulseModeName.Async);
            DrawTopicAndValue(
                g, new Point(420, 10), new Size(480, 80),
                new StringContent(topic_Font, "Pulse", new Point(0, 5)),
                new StringContent(value_Font, stopping ? "-----" : GetPulseName(CycleControl), new Point(0, 5)),
                new StringContent(unit_font, is_async ? "Hz" : "", new Point(0, 9)),
                200);

            DrawTopicAndValue(
                g, new Point(920, 10), new Size(480, 80),
                new StringContent(topic_Font, "Voltage", new Point(0, 5)),
                new StringContent(value_Font, stopping ? "---.-" : String.Format("{0:F1}", Math.Round(voltage, 2)), new Point(0, 5)),
                new StringContent(unit_font, "%", new Point(0, 9)),
                200);

            DrawTopicAndValue(
                g, new Point(1420, 10), new Size(480, 80),
                new StringContent(topic_Font, "Freq", new Point(0, 5)),
                new StringContent(value_Font, stopping ? "---.-" : String.Format("{0:F1}", CycleControl.GetVideoSineFrequency()), new Point(0, 5)),
                new StringContent(unit_font, "Hz", new Point(0, 9)),
                200);

            g.Dispose();
            return image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo(
            GenerationBasicParameter generationBasicParameter,
            String output_path,
            int fps
        )
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            YamlVvvfSoundData vvvfData = generationBasicParameter.vvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();
            control.SetRandomFrequencyMoveAllowed(false);

            int image_width = 1920;
            int image_height = 500;
            VideoWriter vr = new(output_path, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));

            if (!vr.IsOpened())
            {
                return;
            }

            // PROGRESS INITIALIZE
            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + 2 * fps;

            bool START_FRAMES = true;
            if (START_FRAMES)
            {

                ControlStatus cv = new()
                {
                    brake = true,
                    mascon_on = true,
                    free_run = false,
                    wave_stat = 0
                };
                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, cv, vvvfData);
                _ = CalculatePhases(control, calculated_Values, 0);
                Bitmap final_image = GetImage(control, vvvfData, true);

                AddImageFrames(final_image, fps, vr);
                Viewer?.SetImage(final_image);
                final_image.Dispose();
            }

            //PROGRESS ADD
            progressData.Progress += fps;

            while (true)
            {
                Bitmap final_image = GetImage(control,  vvvfData, true);

                MemoryStream ms = new();
                final_image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);
                vr.Write(mat);
                ms.Dispose();
                mat.Dispose();
                Viewer?.SetImage(final_image);
                final_image.Dispose();

                if (!CheckForFreqChange(control, masconData, vvvfData, 1.0 / fps)) break;
                if (progressData.Cancel) break;
                progressData.Progress++;
            }

            bool END_FRAMES = true;
            if (END_FRAMES)
            {

                ControlStatus cv = new()
                {
                    brake = true,
                    mascon_on = true,
                    free_run = false,
                    wave_stat = 0
                };
                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, cv, vvvfData);
                _ = CalculatePhases(control, calculated_Values, 0);
                Bitmap final_image = GetImage(control, vvvfData, true);
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
