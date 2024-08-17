using NAudio.Dsp;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Diagnostics;
using System.IO;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;

namespace VvvfSimulator.Generation.Audio.VvvfSound
{
    public class Audio
    {
        // -------- VVVF SOUND -------------
        public static double CalculateVvvfSound(VvvfValues control, YamlVvvfSoundData sound_data)
        {
            ControlStatus cv = new()
            {
                brake = control.IsBraking(),
                mascon_on = !control.IsMasconOff(),
                free_run = control.IsFreeRun(),
                wave_stat = control.GetControlFrequency()
            };
            PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, cv, sound_data);
            WaveValues value = VvvfCalculate.CalculatePhases(control, calculated_Values, 0);
            double pwm_value = value.U - value.V;
            return pwm_value;
        }

        // Export Audio
        public static void ExportWavFile(GenerationBasicParameter generationBasicParameter, int sampleFrequency, bool raw, string path)
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

                if (DeleteOld) File.Delete(InputPath);
            }

            YamlVvvfSoundData vvvfData = generationBasicParameter.vvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.masconData;
            ProgressData progressData = generationBasicParameter.progressData;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();

            int DownSampledFrequency = 44100;
            string pathTemp = Path.GetDirectoryName(path) + "\\" + "temp-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".wav";

            WaveFormat waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleFrequency, 1);
            BufferedWaveProvider bufferedWaveProvider = new(waveFormat)
            {
                BufferLength = 80000
            };
            ISampleProvider sampleProvider = bufferedWaveProvider.ToSampleProvider();
            WaveFileWriter writer = new(raw ? path : pathTemp, sampleProvider.WaveFormat);

            //TASK DATA PREPARE
            double dt = 1.0 / sampleFrequency;
            progressData.Total = masconData.GetEstimatedSteps(dt) * (raw ? 1 : 1.05);

            while (true)
            {
                control.AddSineTime(dt);
                control.AddSawTime(dt);

                double sound = CalculateVvvfSound(control, vvvfData);
                sound /= 2.0;
                sound *= 0.7;

                AddSample((float)sound, bufferedWaveProvider);
                if (bufferedWaveProvider.BufferedBytes == bufferedWaveProvider.BufferLength)
                {
                    Write(bufferedWaveProvider, sampleProvider, writer);
                }

                progressData.Progress++;

                bool flag_continue = CheckForFreqChange(control, masconData, vvvfData, 1.0 / sampleFrequency);
                bool flag_cancel = progressData.Cancel;

                if (flag_cancel || !flag_continue) break;

            }

            if (bufferedWaveProvider.BufferedBytes > 0)
                Write(bufferedWaveProvider, sampleProvider, writer);

            writer.Close();

            if (!raw)
            {
                DownSample(DownSampledFrequency, pathTemp, path, true);
                progressData.Progress = progressData.Total;
            }
        }
    }
}
