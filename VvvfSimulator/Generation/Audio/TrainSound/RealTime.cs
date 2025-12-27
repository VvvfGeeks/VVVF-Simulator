using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Properties;
using static VvvfSimulator.Generation.Audio.RealTime;
using static VvvfSimulator.Generation.Audio.TrainSound.Audio;
using static VvvfSimulator.Generation.Audio.TrainSound.AudioFilter;

namespace VvvfSimulator.Generation.Audio.TrainSound
{
    public class RealTime
    {
        //---------- TRAIN SOUND --------------
        private static int Calculate(BufferedWaveProvider Provider, TrainSoundParameter Parameter)
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

                int continueFlag = RealTimeFrequencyControl(Parameter.Control, Parameter, CalcCount * Dt);
                if (continueFlag != -1) return continueFlag;

                Data.Vvvf.Analyze.Calculate(Parameter.Control, Parameter.VvvfSoundData);
                for (int i = 0; i < CalcCount; i++)
                {
                    Parameter.Control.AddTimeAll(Dt);
                    double value = CalculateTrainSound(Parameter.Control, Parameter.TrainSoundData);
                    AddSample((float)value, Provider);
                }

                while (Provider.BufferedBytes - CalcCount > Settings.Default.RealTime_Train_BuffSize) ;
            }
        }

        public static void Generate(TrainSoundParameter Param)
        {
            while (true)
            {
                var bufferedWaveProvider = new BufferedWaveProvider(WaveFormat.CreateIeeeFloatWaveFormat(Settings.Default.RealtimeTrainSamplingFrequency, 1));
                ISampleProvider sampleProvider = bufferedWaveProvider.ToSampleProvider();
                if (Param.TrainSoundData.UseFilters) sampleProvider = new MonauralFilter(sampleProvider, Param.TrainSoundData.GetFilteres(Settings.Default.RealtimeTrainSamplingFrequency));
                if (Param.TrainSoundData.UseConvolutionFilter) sampleProvider = new CppConvolutionFilter(sampleProvider, 4096, Param.TrainSoundData.GetImpulseResponse(Settings.Default.RealtimeTrainSamplingFrequency));

                var mmDevice = new MMDeviceEnumerator().GetDevice(Param.AudioDeviceId);
                WasapiOut wavPlayer = new (mmDevice, AudioClientShareMode.Shared, false, 0);

                wavPlayer.Init(sampleProvider);
                wavPlayer.Play();

                int stat;
                try
                {
                    stat = Calculate(bufferedWaveProvider, Param);
                }
                catch(Exception e)
                {
                    wavPlayer.Stop();
                    wavPlayer.Dispose();

                    mmDevice.Dispose();
                    bufferedWaveProvider.ClearBuffer();

                    DialogBox.Show(e.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);

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
