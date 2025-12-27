using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Data.BaseFrequency;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationParameter;
using static VvvfSimulator.Vvvf.MyMath;
using static VvvfSimulator.Vvvf.Model.Struct;
using Point = System.Drawing.Point;

namespace VvvfSimulator.Generation.Video.Hexagon
{
    public class Design1
    {


        /// <summary>
        /// Gets image of Voltage Vector Hexagon
        /// </summary>
        /// <param name="Control">Make sure you do clone.</param>
        /// <param name="Sound"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <param name="Delta"></param>
        /// <param name="Thickness"></param>
        /// <param name="ZeroVectorCircle"></param>
        /// <param name="PreciseDelta"></param>
        /// <returns></returns>
        public static Bitmap GetImage(
            Domain Control,
            int Width,
            int Height,
            int Delta,
            int Thickness,
            bool ZeroVectorCircle,
            bool PreciseDelta
        )
        {
            PhaseState[] PWM_Array = GenerateBasic.WaveForm.GetUVWCycle(Control, 0, Delta, PreciseDelta);

            if (Control.GetControlFrequency() == 0)
                return GetImage(ref PWM_Array, 0, Width, Height, Thickness, ZeroVectorCircle);

            Bitmap image = GetImage(ref PWM_Array, Control.GetControlFrequency(), Width, Height, Thickness, ZeroVectorCircle);
            return image;
        }
        
        public static Bitmap GetImage(
            ref PhaseState[] UVW,
            double ControlFrequency,
            int Width,
            int Height,
            int Thickness,
            bool ZeroVectorCircle
        )
        {
            Bitmap Image = new(Width, Height);
            Graphics Graphic = Graphics.FromImage(Image);
            Graphic.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);

            if (ControlFrequency == 0)
            {
                Graphic.Dispose();
                return Image;
            }

            Common.GetPoints(ref UVW, out PointD[] LinePoints, out PointD[] ZeroPoints);

            double K = 0.9 * Width;
            PointD CenterPosition = new(Width / 2.0, Height / 2.0);

            for (int i = 0; i < LinePoints.Length - 1; i++)
                Graphic.DrawLine(
                    new Pen(Color.Black, Thickness),
                    (K * LinePoints[i] + CenterPosition).ToPoint(),
                    (K * LinePoints[i + 1] + CenterPosition).ToPoint()
                );

            if (ZeroVectorCircle)
            {
                for (int i = 0; i < ZeroPoints.Length; i++)
                {
                    Point ZeroPoint = (K * ZeroPoints[i] + CenterPosition).ToPoint();
                    double Radius = 15 * ((ControlFrequency > 40) ? 1 : (ControlFrequency / 40.0));
                    Graphic.FillEllipse(new SolidBrush(Color.White),
                        (int)Math.Round(ZeroPoint.X - Radius),
                        (int)Math.Round(ZeroPoint.Y - Radius),
                        (int)Math.Round(Radius * 2),
                        (int)Math.Round(Radius * 2)
                    );
                    Graphic.DrawEllipse(new Pen(Color.Black),
                        (int)Math.Round(ZeroPoint.X - Radius),
                        (int)Math.Round(ZeroPoint.Y - Radius),
                        (int)Math.Round(Radius * 2),
                        (int)Math.Round(Radius * 2)
                    );
                }
            }
            
            Graphic.Dispose();
            return Image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo(GenerationParameter Parameter, String fileName, bool circle)
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            StructCompiled baseFreqData = Parameter.BaseFrequencyData;
            ProgressData progressData = Parameter.Progress;

            Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;

            Boolean draw_zero_vector_circle = circle;

            int fps = 60;


            int image_width = 1000;
            int image_height = 1000;

            int hex_div = 60000;

            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            if (!vr.IsOpened())
            {
                return;
            }

            // Progress Initialize
            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / fps) + 120;

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
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                Bitmap final_image = GetImage(Domain.Clone(), image_width, image_height, hex_div, 2, draw_zero_vector_circle, true);
                MemoryStream ms = new();
                final_image.Save(ms, ImageFormat.Png);
                Viewer?.SetImage(final_image);
                final_image.Dispose();
                byte[] img = ms.GetBuffer();
                Mat mat = Mat.FromImageData(img);
                vr.Write(mat);

                loop = Data.BaseFrequency.Analyze.CheckForFreqChange(Domain, baseFreqData, vvvfData, 1.0 / fps);
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

        public void ExportImage(GenerationParameter Parameter, string fileName, bool circle, double d)
        {
            int image_width = 1000;
            int image_height = 1000;
            int hex_div = 60000;

            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;
            Domain.SetBaseWaveAngleFrequency(d * MyMath.M_2PI);
            Domain.SetControlFrequency(d);

            Parameter.Progress.Total = 2;

            Data.Vvvf.Analyze.Calculate(Domain, Parameter.VvvfData);
            Bitmap image = GetImage(Domain.Clone(), image_width, image_height, hex_div, 2, circle, true);
            Parameter.Progress.Progress = 1;

            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = Mat.FromImageData(img);
            image.Save(fileName, ImageFormat.Png);
            Parameter.Progress.Progress = 2;

            Viewer?.SetImage(image);
            image.Dispose();
        }
    }
}
