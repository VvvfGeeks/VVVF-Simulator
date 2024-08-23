using System;
using System.Collections.Generic;
using System.Linq;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze.YamlMasconDataCompiled;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlMasconData;

namespace VvvfSimulator.Yaml.MasconControl
{
    public class YamlMasconControl
    {

        private static int GetPointAtNum(double time, YamlMasconDataCompiled ymdc)
        {
            List<YamlMasconDataCompiledPoint> SelectSource = ymdc.Points;

            if (time < SelectSource.First().StartTime || SelectSource.Last().EndTime < time) return -1;

            int E_L = 0;
            int E_R = SelectSource.Count - 1;
            int Pos = (E_R - E_L) / 2 + E_L;
            while (true)
            {
                bool time_f = SelectSource[Pos].StartTime <= time && time < SelectSource[Pos].EndTime;
                if (time_f) break;

                if (SelectSource[Pos].StartTime < time)
                    E_L = Pos + 1;
                else if (SelectSource[Pos].StartTime > time)
                    E_R = Pos - 1;

                Pos = (E_R - E_L) / 2 + E_L;
                
            }

            return Pos;
        }
        private static YamlMasconDataCompiledPoint GetPointAtData(double time, YamlMasconDataCompiled ymdc)
        {
            YamlMasconDataCompiledPoint Selected = ymdc.Points[GetPointAtNum(time, ymdc)];
            return Selected;
        }
        private static double GetFreqAt(double time, double initial, YamlMasconDataCompiled ymdc)
        {
            YamlMasconDataCompiledPoint Selected = GetPointAtData(time,ymdc);

            double A_Frequency = (Selected.EndFrequency - Selected.StartFrequency) / (Selected.EndTime - Selected.StartTime);
            double Frequency = A_Frequency * (time - Selected.StartTime) + Selected.StartFrequency;

            return Frequency + initial;

        }

        public static bool CheckForFreqChange(VvvfValues Control, YamlMasconDataCompiled MasconDataCompiled, YamlVvvfSoundData SoundData, double TimeDelta)
        {
            double ForceOnFrequency;
            bool Braking, IsMasconOn;
            double CurrentTime = Control.GetGenerationCurrentTime();
            List<YamlMasconDataCompiledPoint> SelectSource = MasconDataCompiled.Points;
            int DataAt = GetPointAtNum(CurrentTime,MasconDataCompiled);
            if (DataAt < 0) return false;
            YamlMasconDataCompiledPoint Target = SelectSource[DataAt];
            YamlMasconDataCompiledPoint? NextTarget = DataAt + 1 < SelectSource.Count ? SelectSource[DataAt + 1] : null;
            YamlMasconDataCompiledPoint? PreviousTarget = DataAt - 1 >= 0 ? SelectSource[DataAt - 1] : null;


            Braking = !Target.IsAccel;
            IsMasconOn = Target.IsMasconOn;
            ForceOnFrequency = -1;

            if (!IsMasconOn && PreviousTarget != null)
                Braking = !PreviousTarget.IsAccel;

            if (NextTarget != null && Control.IsFreeRun() && NextTarget.IsMasconOn)
            {

                double MasconOnFrequency = GetFreqAt(Target.EndTime, 0, MasconDataCompiled);
                double FreqPerSec, FreqGoto;
                if (!NextTarget.IsAccel)
                {
                    FreqPerSec = SoundData.MasconData.Braking.On.FrequencyChangeRate;
                    FreqGoto = SoundData.MasconData.Braking.On.MaxControlFrequency;
                }
                else
                {
                    FreqPerSec = SoundData.MasconData.Accelerating.On.FrequencyChangeRate;
                    FreqGoto = SoundData.MasconData.Accelerating.On.MaxControlFrequency;
                }

                double TargetFrequency = MasconOnFrequency > FreqGoto ? FreqGoto : MasconOnFrequency;
                double RequireTime = TargetFrequency / FreqPerSec;
                if (Target.EndTime - RequireTime < Control.GetGenerationCurrentTime())
                {
                    IsMasconOn = true;
                    Braking = !NextTarget.IsAccel;
                    ForceOnFrequency = MasconOnFrequency;
                }
            }

            double NewSineFrequency = GetFreqAt(CurrentTime, 0, MasconDataCompiled);
            if (NewSineFrequency < 0) NewSineFrequency = 0;

            Control.SetBraking(Braking);
            Control.SetMasconOff(!IsMasconOn);
            Control.SetFreeRun(Target != null && !Target.IsMasconOn);

            {
                double SineTimeAmplitude = NewSineFrequency == 0 ? 0 : Control.GetSineFrequency() / NewSineFrequency;
                Control.SetSineAngleFrequency(NewSineFrequency * Math.PI * 2);
                if (Control.IsSineTimeChangeAllowed())
                    Control.MultiplySineTime(SineTimeAmplitude);
            }

            if (ForceOnFrequency != -1)
            {
                double SineTimeAmplitude = ForceOnFrequency == 0 ? 0 : Control.GetSineFrequency() / ForceOnFrequency;
                Control.SetSineAngleFrequency(ForceOnFrequency * Math.PI * 2);
                if (Control.IsSineTimeChangeAllowed())
                    Control.MultiplySineTime(SineTimeAmplitude);
            }



            //This is also core of controlling. This should never changed.

            {// Max Frequency Set (Almost Same Thing Exist on YamlVvvfWave)
                double MaxVoltageFreq;
                YamlMasconDataPattern Pattern;
                if (Control.IsBraking()) Pattern = SoundData.MasconData.Braking;
                else Pattern = SoundData.MasconData.Accelerating;

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
                    double freq_change = Control.GetFreeFrequencyChange() * TimeDelta;
                    double final_freq = Control.GetControlFrequency() + freq_change;

                    if (Control.GetSineFrequency() <= final_freq)
                        Control.SetControlFrequency(Control.GetSineFrequency());
                    else
                        Control.SetControlFrequency(final_freq);
                }
            }
            else
            {
                double FreqChange = Control.GetFreeFrequencyChange() * TimeDelta;
                double FinalFrequency = Control.GetControlFrequency() - FreqChange;
                Control.SetControlFrequency(FinalFrequency > 0 ? FinalFrequency : 0);
            }

            Control.AddGenerationCurrentTime(TimeDelta);

            

            if (Target == null) return false;
            return true;
        }
    }
}
