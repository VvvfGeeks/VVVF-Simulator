using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Reflection;
using System.Resources;
using VvvfSimulator.Yaml.MasconControl;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;

namespace VvvfSimulator.Generation
{
    public class GenerateCommon
    {
        private static FontFamily __LoadFont(ResourceReader reader, string[] resource_paths)
        {
            PrivateFontCollection pfc = new();
            foreach (string path in resource_paths)
            {
                reader.GetResourceData(path, out string T, out byte[] font);
                unsafe
                {
                    fixed (byte* ptr = font)
                    {
                        pfc.AddMemoryFont((nint)ptr + 4, font.Length - 4);
                    }
                }
            }
            return pfc.Families[0];
        }
        public static void LoadFont()
        {
            using Stream ress = Assembly.GetExecutingAssembly().GetManifestResourceStream("VvvfSimulator.g.resources") ?? throw new ArgumentNullException();
            using ResourceReader resr = new(ress);
            string prefix = "gui/resource/fonts/";
            dSEG14ModernItalicFont = __LoadFont(resr, [prefix + "fonts-dseg_v046/dseg14-modern/dseg14modern-italic.ttf"]);
            fugazOneFont = __LoadFont(resr, [prefix + "fugaz_one/fugazone-regular.ttf"]);
        }
        private static FontFamily? dSEG14ModernItalicFont;
        public static FontFamily DSEG14ModernItalicFont => dSEG14ModernItalicFont ?? new("DSEG14 Modern");
        private static FontFamily? fugazOneFont;
        public static FontFamily FugazOneFont => fugazOneFont ?? new("Fugaz One");

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
