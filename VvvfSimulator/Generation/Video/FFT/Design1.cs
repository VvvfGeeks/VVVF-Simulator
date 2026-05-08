using NAudio.Dsp;
using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using VvvfSimulator.Data.BaseFrequency;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.Generation.Video.FFT
{
    public class Design1
    {
        private static Complex[] FFTNAudio(ref PhaseState[] WaveForm, int Size)
        {
            Complex[] fft = new Complex[WaveForm.Length];
            for (int i = 0; i < WaveForm.Length; i++)
            {
                fft[i].X = (float)((WaveForm[i].U - WaveForm[i].V) * FastFourierTransform.HammingWindow(i, WaveForm.Length));;
                fft[i].Y = 0;
            }
            FastFourierTransform.FFT(true, Size, fft);
            Array.Resize(ref fft, fft.Length/2);
            return fft;
        }
        private static (float R, float θ) ConvertComplex(Complex C)
        {
            float R = C.X * C.X + C.Y * C.Y;
            float θ = (float)Math.Atan2(C.Y, C.X);
            return (R, θ);
        }
        public static Bitmap GetImage(Domain Instance, 
            int Width, int Height,
            int Amplitude, int Thikness,
            int Size, int RangeBegin, int RangeEnd
        )
        {
            Instance.GetCarrierInstance().UseSimpleFrequency = true;
            PhaseState[] Signal = GenerateBasic.WaveForm.GetUVWSec(Instance, MyMath.M_PI_6, (int)Math.Pow(2, Size) - 1, false);
            Complex[] FFT = FFTNAudio(ref Signal, Size);

            Bitmap image = new(Width, Height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, Width, Height);

            RangeBegin = Math.Clamp(RangeBegin, 0, FFT.Length - 1);
            RangeEnd = Math.Clamp(RangeEnd, RangeBegin, FFT.Length - 1);
            int Range = RangeEnd - RangeBegin;

            for (int i = 0; i < Range; i++)
            {
                int Xi = Math.Clamp((int)Math.Round((double)i / Range * Width), 0, Width);
                int Xj = Math.Clamp((int)Math.Round((double)(i + 1) / Range * Width), 0, Width);
                double Ri = ConvertComplex(FFT[RangeBegin + i]).R;
                double Rj = ConvertComplex(FFT[RangeBegin + i + 1]).R;
                PointF start = new(Xi, (int)Math.Round(Height - Ri * Amplitude));
                PointF end = new(Xj, (int)Math.Round(Height - Rj * Amplitude));
                g.DrawLine(new Pen(Color.Black, Thikness), start, end);
            }
            g.Dispose();
            return image;
        }

        private BitmapViewerManager? Viewer { get; set; }
        public void ExportVideo(GenerationParameter Parameter, string fileName, int fps,
            int Width, int Height,
            int Amplitude, int Thikness,
            int Size, int RangeBegin, int RangeEnd
        )
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            StructCompiled baseFreqData = Parameter.BaseFrequencyData;
            GUI.TaskViewer.TaskProgress progressData = Parameter.Progress;

            Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;

            VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(Width, Height));

            if (!vr.IsOpened())
                return;

            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / fps) + fps * 2;

            AddEmptyFrames(vr, Width, Height, fps, () => { progressData.Progress++; });

            Data.BaseFrequency.Analyze.ForwardTime(baseFreqData, Domain, vvvfData, 1.0 / fps, () =>
            {
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);

                Bitmap image = GetImage(Domain.Clone(), Width, Height, Amplitude, Thikness, Size, RangeBegin, RangeEnd);
                Viewer?.SetImage(image);
                AddImageFrames(vr, image, 1);
                image.Dispose();
                progressData.Progress++;

                return progressData.Cancel;
            });

            AddEmptyFrames(vr, Width, Height, fps, () => { progressData.Progress++; });

            vr.Release();
            vr.Dispose();

            Viewer?.Close();
        }
        public void ExportImage(GenerationParameter Parameter, string fileName, 
            double Time, double ForwardStep,
            int Width, int Height,
            int Amplitude, int Thikness,
            int Size, int RangeBegin, int RangeEnd
        )
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            Parameter.Progress.Total = 3;

            Domain Domain = new(Parameter.TrainData.MotorSpec);
            Domain.GetCarrierInstance().UseSimpleFrequency = true;
            Analyze.ForwardTime(Parameter.BaseFrequencyData, Domain, Parameter.VvvfData, Time, ForwardStep);
            Parameter.Progress.Progress = 1;

            Data.Vvvf.Analyze.Calculate(Domain, Parameter.VvvfData);
            Bitmap image = GetImage(Domain.Clone(), Width, Height, Amplitude, Thikness, Size, RangeBegin, RangeEnd);
            Parameter.Progress.Progress = 2;

            image.Save(fileName, ImageFormat.Png);
            Viewer?.SetImage(image);
            image.Dispose();
            Parameter.Progress.Progress = 3;
        }
    }
}
