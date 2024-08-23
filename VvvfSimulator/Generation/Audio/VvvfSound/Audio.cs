using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.Generation.Audio.VvvfSound
{
    public class Audio
    {
        private class BufferedWaveFileWriter
        {
            private readonly BufferedWaveProvider Buffer;
            private readonly ISampleProvider SampleProvider;
            private readonly WaveFileWriter Writer;

            public BufferedWaveFileWriter(string Path,int SamplingFrequency)
            {
                Buffer = new(WaveFormat.CreateIeeeFloatWaveFormat(SamplingFrequency, 1))
                {
                    BufferLength = 80000
                };
                SampleProvider = Buffer.ToSampleProvider();
                Writer = new(Path, SampleProvider.WaveFormat);
            }
            public void AddSample(double value)
            {
                byte[] soundSample = BitConverter.GetBytes((float)value);
                if (!BitConverter.IsLittleEndian) Array.Reverse(soundSample);
                Buffer.AddSamples(soundSample, 0, 4);
                if (Buffer.BufferedBytes == Buffer.BufferLength) Write();
            }

            private void Write()
            {
                byte[] buffer = new byte[Buffer.BufferedBytes];
                int bytesRead = Buffer.Read(buffer, 0, buffer.Length);
                Writer.Write(buffer, 0, bytesRead);
            }

            public void Close()
            {
                if (Buffer.BufferedBytes > 0)
                    Write();
                Writer.Close();
            }
        }
        private static void DownSample(int NewSamplingRate, string InputPath, string OutputPath, bool DeleteOld)
        {
            using (var reader = new AudioFileReader(InputPath))
            {
                var resampler = new WdlResamplingSampleProvider(reader, NewSamplingRate);
                WaveFileWriter.CreateWaveFile16(OutputPath, resampler);
            }

            if (DeleteOld) File.Delete(InputPath);
        }


        private delegate double[] GetSampleDelegate(VvvfValues Control, YamlVvvfSoundData SoundData);

        public static void ExportWavLine(GenerationBasicParameter GenParam, int SamplingFreq, bool UseRaw, string Path)
        {
            GetSampleDelegate SampleGen = (VvvfValues control, YamlVvvfSoundData sound_data) =>
            {
                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, sound_data);
                WaveValues value = Calculate.CalculatePhases(control, calculated_Values, MyMath.M_PI_6);
                double pwm_value = value.U - value.V;
                return [pwm_value];
            };
            ExportWavFile(GenParam, SampleGen, SamplingFreq, UseRaw, [Path]);
        }

        public static void ExportWavPhases(GenerationBasicParameter GenParam, int SamplingFreq, bool UseRaw, string Path)
        {
            GetSampleDelegate SampleGen = (VvvfValues control, YamlVvvfSoundData sound_data) =>
            {
                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, sound_data);
                WaveValues value = Calculate.CalculatePhases(control, calculated_Values, 0);
                return [value.U, value.V, value.W];
            };

            string[] ExportPath = [
                System.IO.Path.GetDirectoryName(Path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Path) + "_U.wav",
                System.IO.Path.GetDirectoryName(Path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Path) + "_V.wav",
                System.IO.Path.GetDirectoryName(Path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Path) + "_W.wav",
            ];
            ExportWavFile(GenParam, SampleGen, SamplingFreq, UseRaw, ExportPath);
        }

        private static void ExportWavFile(GenerationBasicParameter GenParam, GetSampleDelegate GetSample, int SamplingFreq, bool UseRaw, string[] Path)
        {
            const double VolumeFactor = 0.35;
            double dt = 1.0 / SamplingFreq;
            GenParam.Progress.Total = GenParam.MasconData.GetEstimatedSteps(dt) * (UseRaw ? 1 : Math.Pow(1.05, Path.Length));

            VvvfValues Control = new();
            Control.ResetControlValues();
            Control.ResetMathematicValues();
            int DownSampledFrequency = 44100;

            string[] ExportPath = new string[Path.Length];
            BufferedWaveFileWriter[] Writer = new BufferedWaveFileWriter[Path.Length];
            for (int i = 0; i < Path.Length; i++)
            {
                if (UseRaw) ExportPath[i] = Path[i];
                else ExportPath[i] = System.IO.Path.GetDirectoryName(Path[i]) + "\\" + DateTime.Now.ToString("yyyyMMddHHmmss") + System.IO.Path.GetFileNameWithoutExtension(Path[i]) + ".temp";
                Writer[i] = new(ExportPath[i], SamplingFreq);
            }
            
            while (true)
            {
                Control.AddSineTime(dt);
                Control.AddSawTime(dt);
                double[] Samples = GetSample(Control, GenParam.VvvfData);
                
                for(int i = 0; i < ((Samples.Length < Path.Length) ? Samples.Length : Path.Length); i++)
                {
                    Writer[i].AddSample(Samples[i] * VolumeFactor);
                }

                GenParam.Progress.Progress++;
                bool flag_continue = CheckForFreqChange(Control, GenParam.MasconData, GenParam.VvvfData, 1.0 / SamplingFreq);
                bool flag_cancel = GenParam.Progress.Cancel;
                if (flag_cancel || !flag_continue) break;
            }

            for (int i = 0; i < Path.Length; i++)
            {
                Writer[i].Close();
                if (!UseRaw)
                {
                    DownSample(DownSampledFrequency, ExportPath[i], Path[i], true);
                    GenParam.Progress.Progress *= 1.05;
                }
            }

            GenParam.Progress.Progress = GenParam.Progress.Total;
        }
    }
}
