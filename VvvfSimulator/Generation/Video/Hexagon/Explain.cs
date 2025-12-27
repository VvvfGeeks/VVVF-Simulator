using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Data.Vvvf;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationParameter;
using static VvvfSimulator.Vvvf.MyMath;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.Generation.Video.Hexagon
{
    public class Explain
    {
        private BitmapViewerManager? Viewer { get; set; }
        private static void DrawArrow(Graphics g, PointD Origin, PointD Vector)
        {
            int arrowSize = 50;
            int arrowLength = 30;
            double arrowAngle = 30 * M_PI_180;
            PointD Norm = Vector.Norm();
            PointD top = Origin + arrowSize * Norm;
            PointD left = top - arrowLength * Norm.Rotate(arrowAngle);
            PointD right = top - arrowLength * Norm.Rotate(-arrowAngle);

            Pen pen = new(Color.Red, 3);
            g.DrawLine(pen, Origin.ToPoint(), top.ToPoint());
            g.DrawLine(pen, top.ToPoint(), left.ToPoint());
            g.DrawLine(pen, top.ToPoint(), right.ToPoint());
        }
        public bool ExportVideo(GenerationParameter Parameter, string Path, bool DrawCircle, double ControlFrequency)
        {
            int fps = 60;
            int widthTotalImage = 1300, heightTotalImage = 500;
            int widthPwmImage = 750, heightPwmImage = 500;
            int sizeVectorImage = 1000;
            double sizeVector = 1.2;
            int division = 6 * 10000;
            double K = sizeVector * sizeVectorImage / division;

            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            ProgressData progressData = Parameter.Progress;
            Domain Domain = new(Parameter.TrainData.MotorSpec);

            VideoWriter Writer = new(Path, FourCC.H264, fps, new OpenCvSharp.Size(widthTotalImage, heightTotalImage));
            if (!Writer.IsOpened()) return false;

            // Progress Initialize
            progressData.Total = division + 120;

            // Analyze
            Domain.SetControlFrequency(ControlFrequency);
            Domain.SetBaseWaveAngleFrequency(ControlFrequency * MyMath.M_2PI);
            Analyze.Calculate(Domain, vvvfData);

            // Graphic
            Bitmap totalImage = new(widthTotalImage, heightTotalImage);
            Graphics totalGraphic = Graphics.FromImage(totalImage);
            totalGraphic.FillRectangle(new SolidBrush(Color.White), 0, 0, widthTotalImage, heightTotalImage);

            Bitmap pwmImage = new(widthPwmImage, heightPwmImage);
            Graphics pwmGraphic = Graphics.FromImage(pwmImage);
            pwmGraphic.FillRectangle(new SolidBrush(Color.White), 0, 0, widthPwmImage, heightPwmImage);

            Bitmap vectorImage = new(sizeVectorImage, sizeVectorImage);
            Graphics vectorGraphic = Graphics.FromImage(vectorImage);
            vectorGraphic.FillRectangle(new SolidBrush(Color.White), 0, 0, sizeVectorImage, sizeVectorImage);

            Bitmap zeroVectorImage = new(sizeVectorImage, sizeVectorImage);
            Graphics zeroVectorGraphic = Graphics.FromImage(zeroVectorImage);

            // Fonts
            Font textFont = new(
                Fonts.Manager.FugazOne,
                40,
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            // Calculate Cycle
            PhaseState[] Cycle = GenerateBasic.WaveForm.GetUVWCycle(Domain, 0, division, false);

            // Calculate Center
            PointD CurrentPoint = new(0, 0);
            PointD MaxValue = new(double.MinValue, double.MinValue);
            PointD MinValue = new(double.MaxValue, double.MaxValue);
            for (int i = 0; i < division; i++)
            {
                PointD Move = K * Common.ToVectorXY(Cycle[i]);
                CurrentPoint += Move;
                MaxValue = PointD.Max(CurrentPoint, MaxValue);
                MinValue = PointD.Min(CurrentPoint, MinValue);
            }

            // Draw Waveform
            static int _PwmToY(int Pwm, int Pos) => Pwm * -50 + 150 * Pos; 
            void _DrawPwm(int X, int Y1, int Y2, int Pos) => pwmGraphic.DrawLine(new Pen(Color.Black), X, _PwmToY(Y1, Pos), Y1 != Y2 ? X : X + 1, _PwmToY(Y2, Pos));
            for (int i = 0; i < widthPwmImage - 1; i++)
            {
                int refIndex1 = (int)(i * (double)division / widthPwmImage);
                int refIndex2 = (int)((i + 1) * (double)division / widthPwmImage);
                _DrawPwm(i, Cycle[refIndex1].U, Cycle[refIndex2].U, 1);
                _DrawPwm(i, Cycle[refIndex1].V, Cycle[refIndex2].V, 2);
                _DrawPwm(i, Cycle[refIndex1].W, Cycle[refIndex2].W, 3);
            }
            totalGraphic.DrawImage(pwmImage, 0, 0);

            // Start Frame
            {
                MemoryStream memory = new();
                totalImage.Save(memory, ImageFormat.Png);
                Viewer?.SetImage(totalImage);
                Mat data = Mat.FromImageData(memory.GetBuffer());
                for (int i = 0; i < 60; i++)
                {
                    progressData.Progress++;
                    Writer.Write(data);
                }
            }

            // Actual Draw
            CurrentPoint = -0.5 * (MaxValue + MinValue) + new PointD(sizeVectorImage / 2, sizeVectorImage / 2);
            bool flag = false;
            for (int i = 0; i < division; i++)
            {
                PointD Move = K * Common.ToVectorXY(Cycle[i]);
                PointD MovedPoint = Move + CurrentPoint;
                vectorGraphic.DrawLine(new Pen(Color.Black), CurrentPoint.ToPoint(), MovedPoint.ToPoint());
                if (Move.IsZero() && DrawCircle)
                {
                    if (!flag)
                    {
                        flag = true;
                        double Radius = 15 * ((ControlFrequency > 40) ? 1 : (ControlFrequency / 40.0));
                        zeroVectorGraphic.FillEllipse(new SolidBrush(Color.White),
                            (int)Math.Round(MovedPoint.X - Radius),
                            (int)Math.Round(MovedPoint.Y - Radius),
                            (int)Math.Round(Radius * 2),
                            (int)Math.Round(Radius * 2)
                        );
                        zeroVectorGraphic.DrawEllipse(new Pen(Color.Black),
                            (int)Math.Round(MovedPoint.X - Radius),
                            (int)Math.Round(MovedPoint.Y - Radius),
                            (int)Math.Round(Radius * 2),
                            (int)Math.Round(Radius * 2)
                        );
                    }

                }
                else
                    flag = false;

                if (progressData.Cancel) break;
                if (i % 100 == 0 || i + 1 == division)
                {
                    Bitmap totalVectorImage = new(sizeVectorImage, sizeVectorImage);
                    Graphics totalVectorGraphic = Graphics.FromImage(totalVectorImage);
                    totalVectorGraphic.DrawImage(vectorImage, 0, 0);
                    totalVectorGraphic.DrawImage(zeroVectorImage, 0, 0);
                    totalVectorGraphic.FillRectangle(new SolidBrush(Color.Red),
                        (int)(MovedPoint.X - 5),
                        (int)(MovedPoint.Y - 5),
                        (int)(10),
                        (int)(10)
                    );
                    DrawArrow(totalVectorGraphic, CurrentPoint, Move);
                    Bitmap resizedVectorImage = new(totalVectorImage, 500, 500);

                    totalGraphic.FillRectangle(new SolidBrush(Color.White), 0, 0, widthTotalImage, heightTotalImage);
                    totalGraphic.DrawImage(pwmImage, 0, 0);
                    totalGraphic.DrawLine(new Pen(new SolidBrush(Color.Red), 2), (int)Math.Round((double)i / division * widthPwmImage), 0, (int)Math.Round((double)i / division * widthPwmImage), heightPwmImage);
                    totalGraphic.DrawString(Cycle[i].U.ToString(), textFont, (Cycle[i].U > 0) ? new SolidBrush(Color.Blue) : new SolidBrush(Color.Red), widthPwmImage + 5, 75);
                    totalGraphic.DrawString(Cycle[i].V.ToString(), textFont, (Cycle[i].V > 0) ? new SolidBrush(Color.Blue) : new SolidBrush(Color.Red), widthPwmImage + 5, 225);
                    totalGraphic.DrawString(Cycle[i].W.ToString(), textFont, (Cycle[i].W > 0) ? new SolidBrush(Color.Blue) : new SolidBrush(Color.Red), widthPwmImage + 5, 375);
                    totalGraphic.DrawImage(resizedVectorImage, 820, 0);

                    Viewer?.SetImage(totalImage);

                    MemoryStream Memory = new();
                    totalImage.Save(Memory, ImageFormat.Png);
                    Mat mat = Mat.FromImageData(Memory.GetBuffer());
                    Writer.Write(mat);

                    resizedVectorImage.Dispose();
                    totalVectorGraphic.Dispose();
                    totalVectorImage.Dispose();
                }

                progressData.Progress++;
                CurrentPoint = MovedPoint;
            }

            // End Frame
            {
                MemoryStream memory = new();
                totalImage.Save(memory, ImageFormat.Png);
                Viewer?.SetImage(totalImage);
                Mat data = Mat.FromImageData(memory.GetBuffer());
                for (int i = 0; i < 60; i++)
                {
                    progressData.Progress++;
                    Writer.Write(data);
                }
            }

            pwmGraphic.Dispose();
            pwmImage.Dispose();
            totalGraphic.Dispose();
            totalImage.Dispose();
            vectorGraphic.Dispose();
            vectorImage.Dispose();
            zeroVectorGraphic.Dispose();
            zeroVectorImage.Dispose();

            Writer.Release();
            Writer.Dispose();

            Viewer?.Close();

            return true;
        }
    }
}
