using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class AudioResourceManager
    {
        public static string SampleIrPath { get; set; } = "VvvfSimulator.Generation.Audio.TrainSound.Filter.wav";

        public static float[] ReadSample(ISampleProvider Provider)
        {
            var Samples = new List<float>();
            float[] Buffer = new float[1024];

            int Read;
            while ((Read = Provider.Read(Buffer, 0, Buffer.Length)) > 0)
            {
                Samples.AddRange(Buffer.AsSpan(0, Read));
            }

            return Samples.ToArray();
        }
        public static void ReadAudioFileSample(ISampleProvider Provider, out float[] Response, out int SampleRate)
        {
            Response = ReadSample(Provider);
            SampleRate = Provider.WaveFormat.SampleRate;
        }
        public static void ReadAudioFileSample(string path, out float[] Response, out int SampleRate)
        {
            AudioFileReader Reader = new(path);
            ISampleProvider Provider = Reader.ToMono();
            ReadAudioFileSample(Provider, out Response, out SampleRate);
        }
        public static void ReadResourceAudioFileSample(string path, out float[] Response, out int SampleRate)
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream? resource = assembly.GetManifestResourceStream(path);

            if (resource == null)
            {
                Response = [];
                SampleRate = -1;
                return;
            }

            WaveFileReader Reader = new(resource);
            ISampleProvider Provider = Reader.ToSampleProvider().ToMono();
            ReadAudioFileSample(Provider, out Response, out SampleRate);
        }
        public static float[] Resample(float[] Input, int SampleRate, int ResampleRate)
        {
            byte[] RawSource = new byte[Input.Length * 4];
            Buffer.BlockCopy(Input, 0, RawSource, 0, RawSource.Length);
            RawSourceWaveStream Stream = new(RawSource, 0, RawSource.Length, WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1));
            ISampleProvider Provider = Stream.ToSampleProvider().ToMono();
            WdlResamplingSampleProvider Resampler = new(Provider, ResampleRate);
            return ReadSample(Resampler);
        }

    }
}
