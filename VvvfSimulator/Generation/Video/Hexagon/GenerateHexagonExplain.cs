using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.Vvvf.Calculate;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.Generation.Video.Hexagon
{
    public class GenerateHexagonExplain
    {
        private BitmapViewerManager? Viewer { get; set; }
        public bool ExportVideo(GenerationBasicParameter generationBasicParameter, String output_path, bool circle, double d)
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            YamlVvvfSoundData vvvfData = generationBasicParameter.VvvfData;
            ProgressData progressData = generationBasicParameter.Progress;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();

            int fps = 60;

            int image_width = 1300;
            int image_height = 500;

            int pwm_image_width = 750;
            int pwm_image_height = 500;

            int hexagon_image_size = 1000;

            int hex_div_seed = 10000;
            int hex_div = 6 * hex_div_seed;

            Boolean draw_zero_vector_circle = circle;

            VideoWriter vr = new(output_path, FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));
            if (!vr.IsOpened()) return false;

            // Progress Initialize
            progressData.Total = hex_div + 120;

            Boolean START_WAIT = false;
            if (START_WAIT)
            {
                Bitmap free_image = new(image_width, image_height);
                Graphics free_g = Graphics.FromImage(free_image);
                free_g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                MemoryStream free_ms = new();
                free_image.Save(free_ms, ImageFormat.Png);
                byte[] free_img = free_ms.GetBuffer();
                Mat free_mat = OpenCvSharp.Mat.FromImageData(free_img);

                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

                    vr.Write(free_mat);
                }
                free_g.Dispose();
                free_image.Dispose();
            }

            control.SetSineTime(0);
            control.SetSawTime(0);

            control.SetControlFrequency(d);
            control.SetSineAngleFrequency(d * MyMath.M_2PI);

            Bitmap PWM_wave_image = new(pwm_image_width, pwm_image_height);
            Graphics PWM_wave_g = Graphics.FromImage(PWM_wave_image);
            PWM_wave_g.FillRectangle(new SolidBrush(Color.White), 0, 0, pwm_image_width, pwm_image_height);

            Bitmap whole_image = new(image_width, image_height);
            Graphics whole_g = Graphics.FromImage(whole_image);
            whole_g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);

            Bitmap hexagon_image = new(hexagon_image_size, hexagon_image_size);
            Graphics hexagon_g = Graphics.FromImage(hexagon_image);
            hexagon_g.FillRectangle(new SolidBrush(Color.White), 0, 0, hexagon_image_size, hexagon_image_size);

            Boolean drawn_circle = false;
            Bitmap zero_circle_image = new(hexagon_image_size, hexagon_image_size);
            Graphics zero_circle_g = Graphics.FromImage(zero_circle_image);

            int[] points_U = new int[hex_div];
            int[] points_V = new int[hex_div];
            int[] points_W = new int[hex_div];

            double[] x_min_max = [50000, 0];
            double[] hexagon_coordinate = [100, 500];

            PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, vvvfData);

            for (int i = 0; i < hex_div; i++)
            {
                control.SetSineTime((double)i / (hex_div) * ((control.GetSineFrequency() == 0) ? 0 : 1 / control.GetSineFrequency()));
                control.SetSawTime((double)i / (hex_div) * ((control.GetSineFrequency() == 0) ? 0 : 1 / control.GetSineFrequency()));
                control.SetGenerationCurrentTime((double)i / (hex_div) * ((control.GetSineFrequency() == 0) ? 0 : 1 / control.GetSineFrequency()));

                WaveValues value = CalculatePhases(control, calculated_Values, 0);

                points_U[i] = value.U;
                points_V[i] = value.V;
                points_W[i] = value.W;

                double move_x = -0.5 * value.W - 0.5 * value.V + value.U;
                double move_y = -0.866025403784438646763 * value.W + 0.866025403784438646763 * value.V;
                double int_move_x = 200 * move_x / (double)hex_div_seed;
                double int_move_y = 200 * move_y / (double)hex_div_seed;
                hexagon_coordinate[0] = hexagon_coordinate[0] + int_move_x;
                hexagon_coordinate[1] = hexagon_coordinate[1] + int_move_y;

                if (x_min_max[0] > hexagon_coordinate[0]) x_min_max[0] = hexagon_coordinate[0];
                if (x_min_max[1] < hexagon_coordinate[0]) x_min_max[1] = hexagon_coordinate[0];

            }

            hexagon_coordinate = [100, 500];
            double moved_x = (image_width - x_min_max[1] - x_min_max[0]) / 2.0;

            int jump_add = hex_div / pwm_image_width;
            for (int i = 0; i < pwm_image_width - 1; i++)
            {
                for (int ix = 0; ix < 3; ix++)
                {
                    int curr_val, next_val;
                    if (ix == 0)
                    {
                        curr_val = points_U[i * jump_add];
                        next_val = points_U[(i + 1) * jump_add];
                    }
                    else if (ix == 1)
                    {
                        curr_val = points_V[i * jump_add];
                        next_val = points_V[(i + 1) * jump_add];
                    }
                    else
                    {
                        curr_val = points_W[i * jump_add];
                        next_val = points_W[(i + 1) * jump_add];
                    }

                    curr_val *= -50;
                    next_val *= -50;

                    curr_val += 150 * (ix + 1);
                    next_val += 150 * (ix + 1);

                    PWM_wave_g.DrawLine(new Pen(Color.Black), i, curr_val, ((curr_val != next_val) ? i : i + 1), next_val);
                }
            }

            whole_g.DrawImage(PWM_wave_image, 0, 0);

            bool only_wave_show = true;
            if (only_wave_show)
            {
                MemoryStream ms = new();
                whole_image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                Mat mat = OpenCvSharp.Mat.FromImageData(img);

                Viewer?.SetImage(whole_image);

                for (int i = 0; i < 60; i++)
                {
                    vr.Write(mat);
                }
            }

            Font text_font = new(
                new FontFamily("Fugaz One"),
                40,
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            for (int i = 0; i < hex_div; i++)
            {
                whole_g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
                whole_g.DrawImage(PWM_wave_image, 0, 0);
                whole_g.DrawLine(new Pen(Color.Red), (int)Math.Round((double)i / (double)hex_div * (double)pwm_image_width), 0, (int)Math.Round((double)i / (double)hex_div * (double)pwm_image_width), pwm_image_height);

                int pwm_U = points_U[i];
                int pwm_V = points_V[i];
                int pwm_W = points_W[i];

                whole_g.DrawString(pwm_U.ToString(), text_font, (pwm_U > 0) ? new SolidBrush(Color.Blue) : new SolidBrush(Color.Red), pwm_image_width + 5, 75); ;
                whole_g.DrawString(pwm_V.ToString(), text_font, (pwm_V > 0) ? new SolidBrush(Color.Blue) : new SolidBrush(Color.Red), pwm_image_width + 5, 225);
                whole_g.DrawString(pwm_W.ToString(), text_font, (pwm_W > 0) ? new SolidBrush(Color.Blue) : new SolidBrush(Color.Red), pwm_image_width + 5, 375);

                double move_x = 0;
                double move_y = 0;
                if (!(pwm_U == pwm_V && pwm_V == pwm_W))
                {
                    move_x = -0.5 * pwm_W - 0.5 * pwm_V + pwm_U;
                    move_y = -0.866025403784438646763 * pwm_W + 0.866025403784438646763 * pwm_V;
                }

                double int_move_x = 200 * move_x / (double)hex_div_seed;
                double int_move_y = 200 * move_y / (double)hex_div_seed;

                hexagon_g.DrawLine(new Pen(Color.Black),
                    (int)(hexagon_coordinate[0] + moved_x),
                    (int)(hexagon_coordinate[1]),
                    (int)(hexagon_coordinate[0] + moved_x + int_move_x),
                    (int)(hexagon_coordinate[1] + int_move_y)
                );

                if (move_x == 0 && move_y == 0 && draw_zero_vector_circle)
                {
                    if (!drawn_circle)
                    {
                        drawn_circle = true;
                        zero_circle_g.FillEllipse(new SolidBrush(Color.White),
                            (int)(hexagon_coordinate[0] - 2 + moved_x),
                            (int)hexagon_coordinate[1] - 2,
                            4,
                            4
                        );
                        zero_circle_g.DrawEllipse(new Pen(Color.Black),
                            (int)(hexagon_coordinate[0] - 2 + moved_x),
                            (int)hexagon_coordinate[1] - 2,
                            4,
                            4
                        );
                    }

                }
                else
                    drawn_circle = false;

                Bitmap hexagon_image_with_dot = new(hexagon_image_size, hexagon_image_size);
                Graphics hexagon_g_with_dot = Graphics.FromImage(hexagon_image_with_dot);
                //hexagon_g_with_dot.FillRectangle(new SolidBrush(Color.White), 0, 0, hexagon_image_size, hexagon_image_size);
                hexagon_g_with_dot.FillRectangle(new SolidBrush(Color.Red),
                    (int)(hexagon_coordinate[0] + moved_x - 5),
                    (int)(hexagon_coordinate[1] - 5),
                    (int)(10),
                    (int)(10)
                );

                hexagon_coordinate[0] = hexagon_coordinate[0] + int_move_x;
                hexagon_coordinate[1] = hexagon_coordinate[1] + int_move_y;

                if (i % 10 == 0 || i + 1 == hex_div)
                {
                    Bitmap resized_hexagon = new(450, 450);
                    Graphics resized_hexagon_g = Graphics.FromImage(resized_hexagon);
                    resized_hexagon_g.FillRectangle(new SolidBrush(Color.White), 0, 0, 450, 450);
                    resized_hexagon_g.DrawImage(new Bitmap(hexagon_image, 450, 450), 0, 0);
                    resized_hexagon_g.DrawImage(new Bitmap(hexagon_image_with_dot, 450, 450), 0, 0);
                    resized_hexagon_g.DrawImage(new Bitmap(zero_circle_image, 450, 450), 0, 0);

                    whole_g.DrawImage(resized_hexagon, 820, 25);

                    MemoryStream ms = new();
                    whole_image.Save(ms, ImageFormat.Png);
                    byte[] img = ms.GetBuffer();
                    Mat mat = OpenCvSharp.Mat.FromImageData(img);

                    Viewer?.SetImage(whole_image);

                    for (int frame = 0; frame < 1; frame++)
                    {
                        vr.Write(mat);
                    }

                    resized_hexagon_g.Dispose();
                    resized_hexagon.Dispose();
                }

                hexagon_g_with_dot.Dispose();
                hexagon_image_with_dot.Dispose();

                if (progressData.Cancel) break;

                // PROGRESS CHANGE
                progressData.Progress++;
            }



            Boolean END_WAIT = true;
            if (END_WAIT)
            {
                MemoryStream free_ms = new();
                whole_image.Save(free_ms, ImageFormat.Png);
                byte[] free_img = free_ms.GetBuffer();
                Mat free_mat = OpenCvSharp.Mat.FromImageData(free_img);

                for (int i = 0; i < 60; i++)
                {
                    // PROGRESS CHANGE
                    progressData.Progress++;

                    vr.Write(free_mat);
                }
            }

            PWM_wave_g.Dispose();
            PWM_wave_image.Dispose();
            whole_g.Dispose();
            whole_image.Dispose();
            hexagon_g.Dispose();
            hexagon_image.Dispose();
            zero_circle_g.Dispose();
            zero_circle_image.Dispose();

            vr.Release();
            vr.Dispose();

            Viewer?.Close();

            return true;
        }
    }
}
