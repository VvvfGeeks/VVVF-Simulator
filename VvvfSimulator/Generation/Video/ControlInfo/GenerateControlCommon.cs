using System;
using System.Drawing;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.Generation.Video.ControlInfo
{
    public class GenerateControlCommon
    {

        public const double VoltageConvertFactor = 1.1023241394759375;

        /// <summary>
        /// Do clone about control!
        /// </summary>
        /// <param name="Sound"></param>
        /// <param name="Control"></param>
        /// <returns></returns>
        public static double GetVoltageRate(VvvfValues Control, YamlVvvfSoundData Sound, bool Precise)
        {
            WaveValues[] PWM_Array = GenerateBasic.GetUVWCycle(Control, Sound, MyMath.M_PI_6, 120000, Precise);
            return GetVoltageRate(ref PWM_Array);
        }
        public static double GetVoltageRate(ref WaveValues[] UVW)
        {
            double result = FS.GenerateFourierSeries.GetFourierFast(ref UVW, 1);
            result = Math.Abs(result) / VoltageConvertFactor;
            return result;
        }
        public static void FilledCornerCurvedRectangle(Graphics g, Brush br, Point start, Point end, int round_radius)
        {
            int width = end.X - start.X;
            int height = end.Y - start.Y;

            g.FillRectangle(br, start.X + round_radius, start.Y, width - 2 * round_radius, height);
            g.FillRectangle(br, start.X, start.Y + round_radius, round_radius, height - 2 * round_radius);
            g.FillRectangle(br, end.X - round_radius, start.Y + round_radius, round_radius, height - 2 * round_radius);

            g.FillEllipse(br, start.X, start.Y, round_radius * 2, round_radius * 2);
            g.FillEllipse(br, start.X, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2);
            g.FillEllipse(br, start.X + width - round_radius * 2, start.Y, round_radius * 2, round_radius * 2);
            g.FillEllipse(br, start.X + width - round_radius * 2, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2);

        }

        public static Point CenterTextWithFilledCornerCurvedRectangle(Graphics g, String str, Brush str_br, Font fnt, Brush br,
                                                                 Point start, Point end, int round_radius, Point str_compen)
        {
            SizeF strSize = g.MeasureString(str, fnt);

            int width = end.X - start.X;
            int height = end.Y - start.Y;

            FilledCornerCurvedRectangle(g, br, start, end, round_radius);

            Point str_pos = new((int)Math.Round(start.X + width / 2 - strSize.Width / 2 + str_compen.X), (int)Math.Round(start.Y + height / 2 - strSize.Height / 2 + str_compen.Y));

            g.DrawString(str, fnt, str_br, str_pos);

            return str_pos;
        }

        public static void LineCornerCurvedRectangle(Graphics g, Pen pen, Point start, Point end, int round_radius)
        {
            int width = (int)(end.X - start.X);
            int height = (int)(end.Y - start.Y);

            g.DrawLine(pen, start.X + round_radius, start.Y, end.X - round_radius + 1, start.Y);
            g.DrawLine(pen, start.X + round_radius, end.Y, end.X - round_radius + 1, end.Y);

            g.DrawLine(pen, start.X, start.Y + round_radius, start.X, end.Y - round_radius + 1);
            g.DrawLine(pen, end.X, start.Y + round_radius, end.X, end.Y - round_radius + 1);

            g.DrawArc(pen, start.X, start.Y, round_radius * 2, round_radius * 2, -90, -90);
            g.DrawArc(pen, start.X, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, -180, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y, round_radius * 2, round_radius * 2, 0, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, 0, 90);

        }

        public static void TitleStrWithLineCornerCurvedRectangle(Graphics g, String str, Brush str_br, Font fnt, Pen pen,
                                                                 Point start, Point end, int round_radius, Point str_compen)
        {

            SizeF strSize = g.MeasureString(str, fnt);

            int width = end.X - start.X;
            int height = end.Y - start.Y;

            g.DrawLine(pen, start.X + round_radius, start.Y, start.X + width / 2 - strSize.Width / 2 - 10, start.Y);
            g.DrawLine(pen, end.X - round_radius + 1, start.Y, start.X + width / 2 + strSize.Width / 2 + 10, start.Y);

            g.DrawString(str, fnt, str_br, start.X + width / 2 - strSize.Width / 2 + str_compen.X, start.Y - fnt.Height / 2 + str_compen.Y);

            g.DrawLine(pen, start.X + round_radius, end.Y, end.X - round_radius + 1, end.Y);

            g.DrawLine(pen, start.X, start.Y + round_radius, start.X, end.Y - round_radius + 1);
            g.DrawLine(pen, end.X, start.Y + round_radius, end.X, end.Y - round_radius + 1);

            g.DrawArc(pen, start.X, start.Y, round_radius * 2, round_radius * 2, -90, -90);
            g.DrawArc(pen, start.X, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, -180, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y, round_radius * 2, round_radius * 2, 0, -90);
            g.DrawArc(pen, start.X + width - round_radius * 2, start.Y + height - round_radius * 2, round_radius * 2, round_radius * 2, 0, 90);

        }

        public static Point CenterTextWithLineCornerCurvedRectangle(Graphics g, String str, Brush str_br, Font fnt, Pen pen,
                                                                 Point start, Point end, int round_radius, Point str_compen)
        {

            SizeF strSize = g.MeasureString(str, fnt);
            int width = end.X - start.X;
            int height = end.Y - start.Y;
            LineCornerCurvedRectangle(g, pen, start, end, round_radius);

            Point string_pos = new((int)Math.Round(start.X + width / 2 - strSize.Width / 2 + str_compen.X), (int)Math.Round(start.Y + height / 2 - strSize.Height / 2 + str_compen.Y));
            g.DrawString(str, fnt, str_br, string_pos);

            return string_pos;


        }

    }
}
