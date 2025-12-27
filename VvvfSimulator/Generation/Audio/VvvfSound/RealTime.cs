using NAudio.CoreAudioApi;
using NAudio.Wave;
using System;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Properties;
using VvvfSimulator.Data.Vvvf;
using static VvvfSimulator.Generation.Audio.RealTime;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.Generation.Audio.VvvfSound
{
    public class RealTime
    {
        // --------- VVVF SOUND ------------
        private static int Generate(
            BufferedWaveProvider provider, 
            VvvfSoundParameter Param
        )
        {
            try
            {
                Param.Port?.Open();
            }
            catch
            {
                DialogBox.Show(LanguageManager.GetString("Simulator.RealTime.Message.NoUsbDevice"), "Error", [DialogBoxButton.Ok], DialogBoxIcon.Error);
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

                end_result = RealTimeFrequencyControl(Param.Control, Param, CalcCount * Dt);
                if (end_result != -1) break;

                byte[] data = new byte[CalcCount];
                Analyze.Calculate(Param.Control, Param.VvvfSoundData);
                for (int i = 0; i < CalcCount; i++)
                {
                    Param.Control.AddTimeAll(Dt);
                    PhaseState value = Vvvf.Calculation.Common.CalculatePhsaseState(Param.Control, 0);
                    char cvalue = (char)(value.U << 4 | value.V << 2 | value.W);
                    data[i] = (byte)cvalue;

                    double sound_byte = Param.OutputMode switch
                    {
                        VvvfSoundParameter.Mode.Line => value.U - value.V,
                        VvvfSoundParameter.Mode.Phase => value.U - 1,
                        _ => value.U - value.V * 0.5 - value.W * 0.5
                    };
                    sound_byte *= 0.5;

                    AddSample((float)sound_byte, provider);
                }

                try
                {
                    Param.Port?.BaseStream.WriteAsync(data, 0, data.Length);
                }catch
                {
                    break;
                }

                while (provider.BufferedBytes + CalcCount > Settings.Default.RealTime_VVVF_BuffSize) ;
            }

            try
            {
                Param.Port?.Write([0xFF], 0, 1);
                Param.Port?.Close();
            }
            catch
            {
                DialogBox.Show(LanguageManager.GetString("Simulator.RealTime.Message.UsbDeviceRemoved"), "Error", [DialogBoxButton.Ok], DialogBoxIcon.Error);
                end_result = 0;
            }

            return end_result;

        }
        public static void Calculate(VvvfSoundParameter Param)
        {
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
                stat = Generate(bufferedWaveProvider, Param);
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
