using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.IO;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Vvvf.Calculation;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Vvvf.Model.Struct;

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


        private delegate double[] GetSampleDelegate(Domain Control, Data.Vvvf.Struct SoundData);

        public static void ExportWavLine(GenerationParameter GenParam, int SamplingFreq, bool UseRaw, string Path)
        {
            GetSampleDelegate SampleGen = (Domain control, Data.Vvvf.Struct sound_data) =>
            {
                PhaseState value = Common.CalculatePhsaseState(control, MyMath.M_PI_6);
                double pwm_value = (value.U - value.V) / 2.0;
                return [pwm_value];
            };
            ExportWavFile(GenParam, SampleGen, SamplingFreq, UseRaw, [Path]);
        }

        public static void ExportWavPhases(GenerationParameter GenParam, int SamplingFreq, bool UseRaw, string Path)
        {
            GetSampleDelegate SampleGen = (Domain control, Data.Vvvf.Struct sound_data) =>
            {
                PhaseState value = Common.CalculatePhsaseState(control, 0);
                return [value.U - 1, value.V - 1, value.W - 1];
            };

            string[] ExportPath = [
                System.IO.Path.GetDirectoryName(Path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Path) + "_U.wav",
                System.IO.Path.GetDirectoryName(Path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Path) + "_V.wav",
                System.IO.Path.GetDirectoryName(Path) + "\\" + System.IO.Path.GetFileNameWithoutExtension(Path) + "_W.wav",
            ];
            ExportWavFile(GenParam, SampleGen, SamplingFreq, UseRaw, ExportPath);
        }

        public static void ExportWavPhaseCurrent(GenerationParameter GenParam, int SamplingFreq, bool UseRaw, string Path)
        {
            GetSampleDelegate SampleGen = (Domain control, Data.Vvvf.Struct sound_data) =>
            {
                PhaseState value = Common.CalculatePhsaseState(control, 0);
                double pwm_value = (2 * value.U - value.V - value.W) / 4.0;
                return [pwm_value];
            };
            ExportWavFile(GenParam, SampleGen, SamplingFreq, UseRaw, [Path]);
        }

        private static void ExportWavFile(GenerationParameter Parameter, GetSampleDelegate GetSample, int SamplingFreq, bool UseRaw, string[] Path)
        {
            const double VolumeFactor = 0.35;
            double dt = 1.0 / SamplingFreq;
            Parameter.Progress.Total = Parameter.BaseFrequencyData.GetEstimatedSteps(dt) * (UseRaw ? 1 : Math.Pow(1.05, Path.Length));

            Domain Domain = new(Parameter.TrainData.MotorSpec);
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
                Data.Vvvf.Analyze.Calculate(Domain, Parameter.VvvfData);
                double[] Samples = GetSample(Domain, Parameter.VvvfData);
                
                for(int i = 0; i < ((Samples.Length < Path.Length) ? Samples.Length : Path.Length); i++)
                {
                    Writer[i].AddSample(Samples[i] * VolumeFactor);
                }

                Parameter.Progress.Progress++;
                bool flag_continue = Data.BaseFrequency.Analyze.CheckForFreqChange(Domain, Parameter.BaseFrequencyData, Parameter.VvvfData, 1.0 / SamplingFreq);
                bool flag_cancel = Parameter.Progress.Cancel;
                if (flag_cancel || !flag_continue) break;
            }

            for (int i = 0; i < Path.Length; i++)
            {
                Writer[i].Close();
                if (!UseRaw)
                {
                    DownSample(DownSampledFrequency, ExportPath[i], Path[i], true);
                    Parameter.Progress.Progress *= 1.05;
                }
            }

            Parameter.Progress.Progress = Parameter.Progress.Total;
        }
    }
}
