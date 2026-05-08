using OpenCvSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace VvvfSimulator.Generation
{
    public class GenerateCommon
    {
        public static void AddEmptyFrames(VideoWriter vr, int image_width, int image_height,int frames, Action? action = null)
        {
            Bitmap image = new(image_width, image_height);
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new SolidBrush(Color.White), 0, 0, image_width, image_height);
            AddImageFrames(vr, image, frames, action);
            g.Dispose();
            image.Dispose();
        }
        public static void AddImageFrames(VideoWriter vr, Bitmap image, int frames, Action? action = null)
        {
            MemoryStream ms = new();
            image.Save(ms, ImageFormat.Png);
            Mat mat = OpenCvSharp.Mat.FromImageData(ms.GetBuffer());
            for (int i = 0; i < frames; i++) { vr.Write(mat); action?.Invoke(); }
            ms.Dispose();
            mat.Dispose();
        }
        public class GenerationParameter(
            Data.BaseFrequency.StructCompiled BaseFrequencyData, 
            Data.Vvvf.Struct VvvfData, 
            Data.TrainAudio.Struct TrainData,
            GUI.TaskViewer.TaskProgress Progress)
        {
            public Data.BaseFrequency.StructCompiled BaseFrequencyData { get; set; } = BaseFrequencyData;
            public Data.Vvvf.Struct VvvfData { get; set; } = VvvfData;
            public Data.TrainAudio.Struct TrainData { get; set; } = TrainData;
            public GUI.TaskViewer.TaskProgress Progress { get; set; } = Progress;
        }

    }
}
