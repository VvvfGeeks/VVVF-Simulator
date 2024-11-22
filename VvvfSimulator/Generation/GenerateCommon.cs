using OpenCvSharp;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using VvvfSimulator.Yaml.MasconControl;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;

namespace VvvfSimulator.Generation
{
    public class GenerateCommon
    {
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
        public class GenerationBasicParameter(YamlMasconAnalyze.YamlMasconDataCompiled MasconData, YamlVvvfSoundData VvvfData, GenerationBasicParameter.ProgressData Progress)
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
