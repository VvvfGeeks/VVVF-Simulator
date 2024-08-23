using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using System.IO.Ports;
using System.Windows;
using VvvfSimulator.Properties;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.Generation.Audio.VvvfSound
{
    public class RealTime
    {
        // --------- VVVF SOUND ------------
        private static int Generate(
            BufferedWaveProvider provider, 
            YamlVvvfSoundData sound_data, 
            VvvfValues control, 
            RealTimeParameter realTime_Parameter,
            SerialPort? serial
        )
        {
            try
            {
                serial?.Open();
            }
            catch
            {
                MessageBox.Show("USB device was not recognized!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return 0;
            }

            static void AddSample(float value, BufferedWaveProvider provider)
            {
                byte[] soundSample = BitConverter.GetBytes((float)value);
                if (!BitConverter.IsLittleEndian) Array.Reverse(soundSample);
                provider.AddSamples(soundSample, 0, 4);
            }

            int end_result;
            while (true)
            {
                int CalcCount = Settings.Default.RealtimeVvvfCalculateDivision;
                double Dt = 1.0 / Settings.Default.RealtimeVvvfSamplingFrequency;

                end_result = RealTimeFrequencyControl(control, realTime_Parameter, CalcCount * Dt);
                if (end_result != -1) break;

                byte[] data = new byte[CalcCount];

                for(int i = 0; i < CalcCount; i++)
                {
                    control.AddSineTime(Dt);
                    control.AddSawTime(Dt);
                    control.AddGenerationCurrentTime(Dt);

                    PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(control, sound_data);
                    WaveValues value = Vvvf.Calculate.CalculatePhases(control, calculated_Values, 0);
                    char cvalue = (char)(value.U << 4 | value.V << 2 | value.W);
                    data[i] = (byte)cvalue;

                    double sound_byte = value.U - value.V;
                    sound_byte /= 2.0;
                    sound_byte *= 0.7;

                    AddSample((float)sound_byte, provider);
                }

                try
                {
                    serial?.BaseStream.WriteAsync(data, 0, data.Length);
                }catch
                {
                    break;
                }

                while (provider.BufferedBytes + CalcCount > Properties.Settings.Default.RealTime_VVVF_BuffSize) ;
            }

            try
            {
                serial?.Write(new byte[] { 0xFF }, 0, 1);
                serial?.Close();
            }
            catch
            {
                MessageBox.Show("USB device was removed!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                end_result = 0;
            }

            return end_result;

        }
        public static void Calculate(YamlVvvfSoundData Sound, RealTimeParameter Param, SerialPort? Serial)
        {
            Param.Quit = false;
            Param.VvvfSoundData = Sound;

            VvvfValues Control = new();
            Control.ResetMathematicValues();
            Control.ResetControlValues();
            Param.Control = Control;

            BufferedWaveProvider bufferedWaveProvider = new(WaveFormat.CreateIeeeFloatWaveFormat(Settings.Default.RealtimeVvvfSamplingFrequency, 1))
            {
                DiscardOnBufferOverflow = true
            };
            MMDevice mmDevice = new MMDeviceEnumerator().GetDevice(Param.AudioDeviceId);
            WasapiOut wavPlayer = new(mmDevice, AudioClientShareMode.Shared, false, 0);

            wavPlayer.Init(bufferedWaveProvider);
            wavPlayer.Play();

            int stat;
            try
            {
                stat = Generate(bufferedWaveProvider, Sound, Control, Param, Serial);
            }
            finally
            {
                wavPlayer.Stop();
                wavPlayer.Dispose();
                mmDevice.Dispose();
                bufferedWaveProvider.ClearBuffer();
            }


        }
    }
}
