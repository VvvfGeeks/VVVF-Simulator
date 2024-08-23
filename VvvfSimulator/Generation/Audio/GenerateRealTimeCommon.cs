using System;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.Motor.GenerateMotorCore;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlMasconData;

namespace VvvfSimulator.Generation.Audio
{
    public class GenerateRealTimeCommon
    {

        // ---------- COMMON ---------------
        public class RealTimeParameter
        {
            public double FrequencyChangeRate { get; set; } = 0;
            public Boolean IsBraking { get; set; } = false;
            public Boolean Quit { get; set; } = false;
            public Boolean IsFreeRunning { get; set; } = false;

            public VvvfValues Control { get; set; } = new();
            public YamlVvvfSoundData VvvfSoundData { get; set; } = new();
            public MotorData Motor { get; set; } = new();
            public YamlTrainSoundData TrainSoundData { get; set; } = YamlTrainSoundDataManage.CurrentData.Clone();
            public String AudioDeviceId { get; set; } = new NAudio.CoreAudioApi.MMDeviceEnumerator().GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia).ID;
        }

        public static int RealTimeFrequencyControl(VvvfValues Control, RealTimeParameter Param, double dt)
        {
            Control.SetBraking(Param.IsBraking);
            Control.SetMasconOff(Param.IsFreeRunning);

            double sin_new_angle_freq = Control.GetSineAngleFrequency();
            sin_new_angle_freq += Param.FrequencyChangeRate * dt;
            if (sin_new_angle_freq < 0) sin_new_angle_freq = 0;

            if (!Control.IsFreeRun())
            {
                if (Control.IsSineTimeChangeAllowed())
                {
                    if (sin_new_angle_freq != 0)
                    {
                        double amp = Control.GetSineAngleFrequency() / sin_new_angle_freq;
                        Control.MultiplySineTime(amp);
                    }
                    else
                        Control.SetSineTime(0);
                }

                Control.SetControlFrequency(Control.GetSineFrequency());
                Control.SetSineAngleFrequency(sin_new_angle_freq);
            }


            if (Param.Quit) return 0;

            {// Max Frequency Set (Almost Same Thing Exist on YamlVvvfWave)
                double MaxVoltageFreq;
                YamlMasconDataPattern Pattern;
                if (Control.IsBraking()) Pattern = Param.VvvfSoundData.MasconData.Braking;
                else Pattern = Param.VvvfSoundData.MasconData.Accelerating;

                if (!Control.IsMasconOff())
                {
                    Control.SetFreeFrequencyChange(Pattern.On.FrequencyChangeRate);
                    MaxVoltageFreq = Pattern.On.MaxControlFrequency;
                    if (Control.IsFreeRun())
                    {
                        if (Control.GetControlFrequency() > MaxVoltageFreq)
                            Control.SetControlFrequency(Control.GetSineFrequency());
                    }
                }
                else
                {
                    Control.SetFreeFrequencyChange(Pattern.Off.FrequencyChangeRate);
                    MaxVoltageFreq = Pattern.Off.MaxControlFrequency;
                    if (Control.IsFreeRun())
                    {
                        if (Control.GetControlFrequency() > MaxVoltageFreq)
                            Control.SetControlFrequency(MaxVoltageFreq);
                    }
                }
            }

            if (!Control.IsMasconOff()) // mascon on
            {
                if (!Control.IsFreeRun())
                    Control.SetControlFrequency(Control.GetSineFrequency());
                else
                {
                    double freq_change = Control.GetFreeFrequencyChange() * dt;
                    double final_freq = Control.GetControlFrequency() + freq_change;

                    if (Control.GetSineFrequency() <= final_freq)
                    {
                        Control.SetControlFrequency(Control.GetSineFrequency());
                        Control.SetFreeRun(false);
                    }
                    else
                    {
                        Control.SetControlFrequency(final_freq);
                        Control.SetFreeRun(true);
                    }
                }
            }
            else
            {
                double freq_change = Control.GetFreeFrequencyChange() * dt;
                double final_freq = Control.GetControlFrequency() - freq_change;
                Control.SetControlFrequency(final_freq > 0 ? final_freq : 0);
                Control.SetFreeRun(true);
            }

            return -1;
        }
    }

}
