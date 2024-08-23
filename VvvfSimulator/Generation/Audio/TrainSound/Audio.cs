using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using System;
using System.Collections.Generic;
using System.IO;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;
using static VvvfSimulator.Generation.GenerateCommon;
using static VvvfSimulator.Generation.GenerateCommon.GenerationBasicParameter;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze.YamlTrainSoundData;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class Audio
    {
        // -------- TRAIN SOUND --------------
        public static double CalculateMotorSound(VvvfValues control, YamlVvvfSoundData sound_data, MotorData motor)
        {
            PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, sound_data);
            WaveValues value = Calculate.CalculatePhases(control, calculated_Values, 0);

            motor.motor_Param.sitamr = control.GetVideoSineFrequency() * Math.PI * 2 * control.GetSineTime();
            motor.AynMotorControler(new(value.W, value.V, value.U));
            motor.Asyn_Moduleabc();

            double motorTorqueSound = motor.motor_Param.Te - motor.motor_Param.TePre;

            return motorTorqueSound;
        }

        public static double CalculateHarmonicSounds(VvvfValues control, List<HarmonicData> harmonics)
        {
            double sound = 0;
            for (int harmonic = 0; harmonic < harmonics.Count; harmonic++)
            {
                HarmonicData harmonic_data = harmonics[harmonic];
                var amplitude_data = harmonic_data.Amplitude;

                if (harmonic_data.Range.Start > control.GetSineFrequency()) continue;
                if (harmonic_data.Range.End >= 0 && harmonic_data.Range.End < control.GetSineFrequency()) continue;

                double harmonic_freq = harmonic_data.Harmonic * control.GetSineFrequency();

                if (harmonic_data.Disappear != -1 && harmonic_freq > harmonic_data.Disappear) continue;
                double sine_val = Math.Sin(control.GetSineTime() * control.GetSineAngleFrequency() * harmonic_data.Harmonic);

                double amplitude = amplitude_data.StartValue + (amplitude_data.EndValue - amplitude_data.StartValue) / (amplitude_data.End - harmonic_data.Amplitude.Start) * (control.GetSineFrequency() - harmonic_data.Amplitude.Start);
                if (amplitude > amplitude_data.MaximumValue) amplitude = amplitude_data.MaximumValue;
                if (amplitude < amplitude_data.MinimumValue) amplitude = amplitude_data.MinimumValue;

                double amplitude_disappear = (harmonic_freq + 100.0 > harmonic_data.Disappear) ?
                    ((harmonic_data.Disappear - harmonic_freq) / 100.0) : 1;

                sine_val *= amplitude * (harmonic_data.Disappear == -1 ? 1 : amplitude_disappear);
                sound += sine_val;
            }
            return sound;
        }

        public static double CalculateTrainSound(VvvfValues control, YamlVvvfSoundData sound_data, MotorData motor, YamlTrainSoundData train_sound_data)
        {
            double motorPwmSound = CalculateMotorSound(control, sound_data, motor) * Math.Pow(10, train_sound_data.MotorVolumeDb);
            double motorSound = CalculateHarmonicSounds(control, train_sound_data.HarmonicSound);
            double gearSound = CalculateHarmonicSounds(control, train_sound_data.GearSound);

            double signal = (motorPwmSound + motorSound + gearSound) * Math.Pow(10, train_sound_data.TotalVolumeDb);

            return signal;
        }


        public static void ExportWavFile(GenerationBasicParameter generationBasicParameter, YamlTrainSoundData soundData, int SamplingFrequency, bool raw, string path)
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

            YamlVvvfSoundData vvvfData = generationBasicParameter.VvvfData;
            YamlMasconDataCompiled masconData = generationBasicParameter.MasconData;
            ProgressData progressData = generationBasicParameter.Progress;

            VvvfValues control = new();
            control.ResetControlValues();
            control.ResetMathematicValues();

            int DownSampledFrequency = 44100;
            string pathTemp = Path.GetDirectoryName(path) + "\\" + "temp-" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".wav";

            var waveFormat = WaveFormat.CreateIeeeFloatWaveFormat(SamplingFrequency, 1);
            var bufferedWaveProvider = new BufferedWaveProvider(waveFormat)
            {
                BufferLength = 4096
            };
            ISampleProvider sampleProvider = bufferedWaveProvider.ToSampleProvider();
            if (soundData.UseFilteres) sampleProvider = new MonauralFilter(sampleProvider, soundData.GetFilteres(SamplingFrequency));
            if (soundData.UseConvolutionFilter) sampleProvider = new CppConvolutionFilter(sampleProvider, 4096, soundData.ImpulseResponse);
            WaveFileWriter writer = new(raw ? path : pathTemp, sampleProvider.WaveFormat);

            MotorData motor = new()
            {
                motor_Specification = soundData.MotorSpec.Clone(),
                SIM_SAMPLE_FREQ = SamplingFrequency,
            };
            motor.motor_Param.TL = 0.0;

            progressData.Total = masconData.GetEstimatedSteps(1.0 / SamplingFrequency) + (raw ? 0 : 100);

            while (true)
            {
                control.AddSineTime(1.00 / SamplingFrequency);
                control.AddSawTime(1.00 / SamplingFrequency);

                float sound = (float)CalculateTrainSound(control, vvvfData, motor , soundData);

                AddSample(sound, bufferedWaveProvider);
                if (bufferedWaveProvider.BufferedBytes == bufferedWaveProvider.BufferLength)
                    Write(bufferedWaveProvider, sampleProvider, writer);

                progressData.Progress++;

                bool flag_continue = CheckForFreqChange(control, masconData, vvvfData, 1.0 / SamplingFrequency);
                bool flag_cancel = progressData.Cancel;
                if (!flag_continue || flag_cancel) break;
            }

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
