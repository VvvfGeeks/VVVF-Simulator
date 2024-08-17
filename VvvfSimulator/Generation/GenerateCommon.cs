using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.Yaml.MasconControl;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using YamlMasconData = VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlMasconData;

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


        public static void AddEmptyFrames(int image_width, int image_height,int frames, VideoWriter vr)
        {
            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = OpenCvSharp.Mat.FromImageData(img);
            for (int i = 0; i < frames; i++) { vr.Write(mat); }
            g.Dispose();
            image.Dispose();
        }

        public static void AddImageFrames(Bitmap image, int frames, VideoWriter vr)
        {
            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            byte[] img = ms.GetBuffer();
            Mat mat = OpenCvSharp.Mat.FromImageData(img);
            for (int i = 0; i < frames; i++) { vr.Write(mat); }
        }

        

        public class GenerationBasicParameter
        {
            public YamlMasconDataCompiled masconData { get; set; }
            public YamlVvvfSoundData vvvfData { get; set; }
            public ProgressData progressData { get; set; }

            public GenerationBasicParameter(YamlMasconDataCompiled yaml_Mascon_Data_Compiled, YamlVvvfSoundData yaml_VVVF_Sound_Data, ProgressData progressData)
            {
                this.masconData = yaml_Mascon_Data_Compiled;
                this.vvvfData = yaml_VVVF_Sound_Data;
                this.progressData = progressData;
            }
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
