using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using VvvfSimulator.Data.BaseFrequency;
using VvvfSimulator.Vvvf.Calculation;
using static VvvfSimulator.Data.TrainAudio.Struct;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class Audio
    {
        // -------- TRAIN SOUND --------------
        public static double CalculateHarmonicSounds(Domain control, List<HarmonicData> harmonics)
        {
            Vvvf.Modulation.BaseWave baseWave = control.GetBaseWaveInstance();

            double sound = 0;
            for (int harmonic = 0; harmonic < harmonics.Count; harmonic++)
            {
                HarmonicData harmonic_data = harmonics[harmonic];
                var amplitude_data = harmonic_data.Amplitude;

                if (harmonic_data.Range.Start > baseWave.Frequency) continue;
                if (harmonic_data.Range.End >= 0 && harmonic_data.Range.End < baseWave.Frequency) continue;

                double harmonic_freq = harmonic_data.Harmonic * baseWave.Frequency;

                if (harmonic_data.Disappear != -1 && harmonic_freq > harmonic_data.Disappear) continue;
                double sine_val = Math.Sin(baseWave.Time * baseWave.AngleFrequency * harmonic_data.Harmonic);

                double amplitude = amplitude_data.StartValue + (amplitude_data.EndValue - amplitude_data.StartValue) / (amplitude_data.End - harmonic_data.Amplitude.Start) * (baseWave.Frequency - harmonic_data.Amplitude.Start);
                if (amplitude > amplitude_data.MaximumValue) amplitude = amplitude_data.MaximumValue;
                if (amplitude < amplitude_data.MinimumValue) amplitude = amplitude_data.MinimumValue;

                double amplitude_disappear = (harmonic_freq + 100.0 > harmonic_data.Disappear) ?
                    ((harmonic_data.Disappear - harmonic_freq) / 100.0) : 1;

                sine_val *= amplitude * (harmonic_data.Disappear == -1 ? 1 : amplitude_disappear);
                sound += sine_val;
            }
            return sound;
        }

        public static double CalculateTrainSound(Domain Control, Data.TrainAudio.Struct Data)
        {
            Common.CalculatePhsaseState(Control, 0);

            double motorPwmSound = Control.Motor.Parameter.DiffTe * Math.Pow(10, Data.MotorVolumeDb);
            double motorSound = CalculateHarmonicSounds(Control, Data.HarmonicSound);
            double gearSound = CalculateHarmonicSounds(Control, Data.GearSound);

            double signal = (motorPwmSound + motorSound + gearSound) * Math.Pow(10, Data.TotalVolumeDb);

            return signal;
        }


        public static void ExportWavFile(GenerationParameter Parameter, int SamplingFrequency, bool raw, string path)
        {
            static void AddSample(float value, BufferedWaveProvider provider)
            {
                byte[] soundSample = BitConverter.GetBytes((float)value);
                if (!BitConverter.IsLittleEndian) Array.Reverse(soundSample);
                provider.AddSamples(soundSample, 0, 4);
            }

            static void Write(BufferedWaveProvider bufferedWaveProvider, ISampleProvider sampleProvider, WaveFileWriter writer)
            {
                byte[] buffer = new byte[bufferedWaveProvider.BufferedBytes];
                int bytesRead = sampleProvider.ToWaveProvider().Read(buffer, 0, buffer.Length);
                writer.Write(buffer, 0, bytesRead);
            }

            static void DownSample(int NewSamplingRate, string InputPath, string OutputPath, bool DeleteOld)
            {
                using (var reader = new AudioFileReader(InputPath))
                {
                    var resampler = new WdlResamplingSampleProvider(reader, NewSamplingRate);
                    WaveFileWriter.CreateWaveFile16(OutputPath, resampler);
                }

                if(DeleteOld) File.Delete(InputPath);
            }

            Data.Vvvf.Struct vvvfData = Parameter.VvvfData;
            Data.TrainAudio.Struct soundData = Parameter.TrainData;
            StructCompiled baseFreqData = Parameter.BaseFrequencyData;
            GUI.TaskViewer.TaskProgress progressData = Parameter.Progress;

            Domain Domain = new(soundData.MotorSpec);

            int DownSampledFrequency = 44100;
            string pathTemp = Path.GetDirectoryName(path) + "\\" + "temp-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".wav";

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SamplingFrequency, 1);
            var bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferLength = 4096
            };
            ISampleProvider sampleProvider = bufferedWaveProvider.ToSampleProvider();
            if (soundData.UseFilters) sampleProvider = new MonauralFilter(sampleProvider, soundData.GetFilteres(SamplingFrequency));
            if (soundData.UseConvolutionFilter) sampleProvider = new CppConvolutionFilter(sampleProvider, 4096, soundData.GetImpulseResponse(SamplingFrequency));
            WaveFileWriter writer = new(raw ? path : pathTemp, sampleProvider.WaveFormat);

            progressData.Total = baseFreqData.GetEstimatedSteps(1.0 / SamplingFrequency) + (raw ? 0 : 100);

            Data.BaseFrequency.Analyze.ForwardTime(baseFreqData, Domain, vvvfData, 1.0 / SamplingFrequency, () =>
            {
                Data.Vvvf.Analyze.Calculate(Domain, vvvfData);
                float sound = (float)CalculateTrainSound(Domain, soundData);
                AddSample(sound, bufferedWaveProvider);
                if (bufferedWaveProvider.BufferedBytes == bufferedWaveProvider.BufferLength)
                    Write(bufferedWaveProvider, sampleProvider, writer);
                progressData.Progress++;

                return progressData.Cancel;
            });

            if (bufferedWaveProvider.BufferedBytes > 0)
                Write(bufferedWaveProvider, sampleProvider, writer);

            writer.Close();

            if (!raw)
            {
                DownSample(DownSampledFrequency, pathTemp, path, true);
                progressData.Progress += 100;
            }
        }
    }
}
