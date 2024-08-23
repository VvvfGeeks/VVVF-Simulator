using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.Windows;
using VvvfSimulator.Properties;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Generation.Audio.TrainSound.Audio;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class RealTime
    {
        //---------- TRAIN SOUND --------------
        private static int Calculate(BufferedWaveProvider provider, YamlVvvfSoundData sound_data, VvvfValues control, RealTimeParameter realTime_Parameter)
        {
            static void AddSample(float value, BufferedWaveProvider provider)
            {
                byte[] soundSample = BitConverter.GetBytes((float)value);
                if (!BitConverter.IsLittleEndian) Array.Reverse(soundSample);
                provider.AddSamples(soundSample, 0, 4);
            }

            while (true)
            {
                int CalcCount = Settings.Default.RealtimeTrainCalculateDivision;
                double Dt = 1.0 / Settings.Default.RealtimeTrainSamplingFrequency;

                int v = RealTimeFrequencyControl(control, realTime_Parameter, CalcCount * Dt);
                if (v != -1) return v;

                for (int i = 0; i < CalcCount; i++)
                {
                    control.AddSineTime(Dt);
                    control.AddSawTime(Dt);
                    control.AddGenerationCurrentTime(Dt);

                    double value = CalculateTrainSound(control, sound_data , realTime_Parameter.Motor, realTime_Parameter.TrainSoundData);
                    AddSample((float)value, provider);
                }

                while (provider.BufferedBytes - CalcCount > Settings.Default.RealTime_Train_BuffSize) ;
            }
        }

        public static void Generate(YamlVvvfSoundData Sound, RealTimeParameter Param)
        {
            Param.Quit = false;
            Param.VvvfSoundData = Sound;
            Param.Motor = new MotorData() { 
                SIM_SAMPLE_FREQ = Settings.Default.RealtimeTrainSamplingFrequency,
                motor_Specification = Param.TrainSoundData.MotorSpec.Clone(),
            };

            YamlTrainSoundData SoundConfiguration = Param.TrainSoundData;

            VvvfValues control = new();
            control.ResetMathematicValues();
            control.ResetControlValues();
            Param.Control = control;

            while (true)
            {
                var bufferedWaveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(Settings.Default.RealtimeTrainSamplingFrequency, 1));
                ISampleProvider sampleProvider = bufferedWaveProvider.ToSampleProvider();
                if (SoundConfiguration.UseFilteres) sampleProvider = new MonauralFilter(sampleProvider, SoundConfiguration.GetFilteres(Settings.Default.RealtimeTrainSamplingFrequency));
                if (SoundConfiguration.UseConvolutionFilter) sampleProvider = new CppConvolutionFilter(sampleProvider, 4096, SoundConfiguration.ImpulseResponse);

                var mmDevice = new MMDeviceEnumerator().GetDevice(Param.AudioDeviceId);
                WasapiOut wavPlayer = new (mmDevice, AudioClientShareMode.Shared, false, 0);

                wavPlayer.Init(sampleProvider);
                wavPlayer.Play();

                int stat;
                try
                {
                    stat = Calculate(bufferedWaveProvider, Sound, control, Param);
                }
                catch
                {
                    wavPlayer.Stop();
                    wavPlayer.Dispose();

                    mmDevice.Dispose();
                    bufferedWaveProvider.ClearBuffer();

                    MessageBox.Show("An error occured on Audio processing.");

                    throw;
                }

                wavPlayer.Stop();
                wavPlayer.Dispose();

                mmDevice.Dispose();
                bufferedWaveProvider.ClearBuffer();

                if (stat == 0) break;
            }


        }
    }
}
