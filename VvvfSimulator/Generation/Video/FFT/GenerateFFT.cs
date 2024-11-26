using NAudio.Dsp;
using OpenCvSharp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;

namespace VvvfSimulator.Generation.Video.FFT
{
    public class GenerateFFT
    {
        private static readonly int pow = 15;
        private static Complex[] FFTNAudio(ref WaveValues[] WaveForm)
        {
            Complex[] fft = new Complex[WaveForm.Length];
            for (int i = 0; i < WaveForm.Length; i++)
            {
                fft[i].X = (float)((WaveForm[i].U - WaveForm[i].V) * FastFourierTransform.HammingWindow(i, WaveForm.Length));;
                fft[i].Y = 0;
            }
            FastFourierTransform.FFT(true, pow, fft);
            Array.Resize(ref fft, fft.Length/2);
            return fft;
        }
        private static (float R, float θ) ConvertComplex(Complex C)
        {
            float R = C.X * C.X + C.Y * C.Y;
            float θ = (float)Math.Atan2(C.Y, C.X);
            return (R, θ);
        }

        /// <summary>
        /// Gets image of FFT.
        /// </summary>
        /// <param name="control">Make sure cloned data is passed</param>
        /// <param name="sound"></param>
        /// <returns></returns>
        public static Bitmap GetImage(VvvfValues control, YamlVvvfSoundData sound)
        {
            control.SetRandomFrequencyMoveAllowed(false);
            WaveValues[] PWM_Array = GenerateBasic.WaveForm.GetUVWSec(control, sound, MyMath.M_PI_6, (int)Math.Pow(2,pow) - 1, false);
            Complex[] FFT = FFTNAudio(ref PWM_Array);

            Bitmap image = new(1000, 1000);
            Graphics g = Graphics.FromImage(image);

            g.FillRectangle(new SolidBrush(Color.White),0,0, 1000, 1000);

            for (int i = 0; i < 1000 - 1; i++)
            {
                var (Ri, _) = ConvertComplex(FFT[(int)(MyMath.M_PI * i)]);
                var (Rii, _) = ConvertComplex(FFT[(int)(MyMath.M_PI * (i + 1))]);
                PointF start = new(i, 1000 - Ri * 2000);
                PointF end = new(i + 1, 1000 - Rii * 2000);
                g.DrawLine(new Pen(Color.Black, 2), start, end);
            }

            g.Dispose();

            return image;

        }

        private BitmapViewerManager? Viewer { get; set; }
        private int _index = 1;
        private string _tempPath = "";
        public void ExportVideo(GenerationBasicParameter generationBasicParameter, string fileName)
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

            DirectoryInfo tempDirInfo = Directory.CreateDirectory(Path.GetDirectoryName(fileName) + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + Path.GetFileNameWithoutExtension(fileName) + "_fft_tmp");
            tempDirInfo.Attributes |= FileAttributes.Hidden;
            _tempPath = tempDirInfo.FullName + "\\";

            int fps = 60;

            int image_width = 1000;
            int image_height = 1000;

            //VideoWriter vr = new(fileName, OpenCvSharp.FourCC.H264, fps, new OpenCvSharp.Size(image_width, image_height));


            //if (!vr.IsOpened())
            //{
            //    return;
            //}

            // Progress Initialize
            progressData.Total = masconData.GetEstimatedSteps(1.0 / fps) + fps * 2;

            bool START_WAIT = true;
            if (START_WAIT)
                GenerateCommon.AddEmptyFrames(image_width, image_height, fps, _tempPath, ref _index);

            // PROGRESS CHANGE
            progressData.Progress += fps;

            bool loop = true;
            while (loop)
            {

                control.SetSineTime(0);
                control.SetSawTime(0);

                Bitmap image = GetImage(control.Clone(), vvvfData);


                MemoryStream ms = new();
                image.Save(ms, ImageFormat.Png);
                byte[] img = ms.GetBuffer();
                File.WriteAllBytes(_tempPath + _index.ToString("D10") + ".png", img);
                _index++;
                //Mat mat = OpenCvSharp.Mat.FromImageData(img);
                //vr.Write(mat);
                //mat.Dispose();
                ms.Dispose();

                Viewer?.SetImage(image);

                image.Dispose();

                loop = GenerateCommon.CheckForFreqChange(control, masconData, vvvfData, 1.0 / fps);
                if (progressData.Cancel) loop = false;

                // PROGRESS CHANGE
                progressData.Progress++;
            }

            bool END_WAIT = true;
            if (END_WAIT)
                GenerateCommon.AddEmptyFrames(image_width, image_height, 60, _tempPath, ref _index);

            // PROGRESS CHANGE
            progressData.Progress += fps;

            //vr.Release();
            //vr.Dispose();

            Viewer?.Close();
            Process? ffmpeg_proc = Process.Start(new ProcessStartInfo()
            {
                FileName = "ffmpeg",
                Arguments = FormatFFmpegArgs(_tempPath, fileName, fps),
                UseShellExecute = true
            }) ?? throw new Exception();
            while (!ffmpeg_proc.HasExited) { }
            if (ffmpeg_proc.ExitCode != 0) throw new Exception() { HResult = ffmpeg_proc.ExitCode };
            tempDirInfo.Delete(true);
        }

        public void ExportImage(string fileName, YamlVvvfSoundData sound_data, double d)
        {
            MainWindow.Invoke(() => Viewer = new BitmapViewerManager());
            Viewer?.Show();

            VvvfValues control = new();

            control.ResetControlValues();
            control.ResetMathematicValues();
            control.SetRandomFrequencyMoveAllowed(false);

            control.SetSineAngleFrequency(d * MyMath.M_2PI);
            control.SetControlFrequency(d);

            Bitmap image = GetImage(control.Clone(), sound_data);

            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            //Mat mat = Mat.FromImageData(img);

            image.Save(fileName, ImageFormat.Png);
            Viewer?.SetImage(image);
            image.Dispose();
        }
    }
}
