using OpenCvSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.Properties;
using VvvfSimulator.Yaml.MasconControl;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;

namespace VvvfSimulator.Generation
{
    public class GenerateCommon
    {
        /// <summary>
        /// この関数は、音声生成や、動画生成時の、マスコンの制御状態等を記述する関数です。
        /// この関数を呼ぶたびに、更新されます。
        /// 
        /// This is a function which will control a acceleration or brake when generating audio or video.
        /// It will be updated everytime this function colled.
        /// </summary>
        /// <returns></returns>
        public static bool CheckForFreqChange(VvvfValues control,YamlMasconDataCompiled ymdc, YamlVvvfSoundData yvsd, double add_time)
        {
            return YamlMasconControl.CheckForFreqChange(control, ymdc, yvsd, add_time);
        }


        public static void AddEmptyFrames(int image_width, int image_height, int frames, string path, ref int index)
        {
            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            for (int i = 0; i < frames; i++)
            {
                File.WriteAllBytes(path + index.ToString("D10") + ".png", img);
                index++;
            }
            g.Dispose();
            image.Dispose();
        }

        public static void AddImageFrames(Bitmap image, int frames, string path, ref int index)
        {
            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            //Mat mat = OpenCvSharp.Mat.FromImageData(img);
            for (int i = 0; i < frames; i++)
            {
                File.WriteAllBytes(path + index.ToString("D10") + ".png", img);
                index++;
            }
        }

        public static string FormatFFmpegArgs(string temp_path, string output_path, double framerate)
        {
            string s = Settings.Default.FFmpegArgs;
            s = s.Replace("{framerate}", framerate.ToString());
            s = s.Replace("{input}", $"\"{temp_path}%10d.png\"");
            s = s.Replace("{output}", $"\"{output_path}\"");
            return s;
        }

        public class GenerationBasicParameter(YamlMasconDataCompiled MasconData, YamlVvvfSoundData VvvfData, GenerationBasicParameter.ProgressData Progress)
        {
            public YamlMasconDataCompiled MasconData { get; set; } = MasconData;
            public YamlVvvfSoundData VvvfData { get; set; } = VvvfData;
            public ProgressData Progress { get; set; } = Progress;

            public class ProgressData
            {
                public double Progress = 1;
                public double Total = 1;

                public double RelativeProgress
                {
                    get
                    {
                        return Progress / Total * 100;
                    }
                }

                public bool Cancel = false;
            }

        }

    }
}
