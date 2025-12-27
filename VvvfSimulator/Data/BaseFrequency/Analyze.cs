using System;
using System.Collections.Generic;
using System.Linq;
using static VvvfSimulator.Vvvf.Model.Struct;
using static VvvfSimulator.Data.BaseFrequency.StructCompiled;

namespace VvvfSimulator.Data.BaseFrequency
{
    public class Analyze
    {
        private static int GetPointAtNum(double time, StructCompiled ymdc)
        {
            List<Point> SelectSource = ymdc.Points;

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
        private static Point GetPointAtData(double time, StructCompiled ymdc)
        {
            Point Selected = ymdc.Points[GetPointAtNum(time, ymdc)];
            return Selected;
        }
        private static double GetFreqAt(double time, double initial, StructCompiled ymdc)
        {
            Point Selected = GetPointAtData(time,ymdc);

            double A_Frequency = (Selected.EndFrequency - Selected.StartFrequency) / (Selected.EndTime - Selected.StartTime);
            double Frequency = A_Frequency * (time - Selected.StartTime) + Selected.StartFrequency;

            return Frequency + initial;

        }
        public static bool CheckForFreqChange(Domain Control, StructCompiled Data, Vvvf.Struct SoundData, double TimeDelta)
        {
            double ForceOnFrequency;
            bool Braking, IsPowerOn;
            double CurrentTime = Control.GetTime();
            List<Point> SelectSource = Data.Points;
            int DataAt = GetPointAtNum(CurrentTime,Data);
            if (DataAt < 0) return false;
            Point Target = SelectSource[DataAt];
            Point? NextTarget = DataAt + 1 < SelectSource.Count ? SelectSource[DataAt + 1] : null;
            Point? PreviousTarget = DataAt - 1 >= 0 ? SelectSource[DataAt - 1] : null;


            Braking = !Target.IsAccel;
            IsPowerOn = Target.IsPowerOn;
            ForceOnFrequency = -1;

            if (!IsPowerOn && PreviousTarget != null)
                Braking = !PreviousTarget.IsAccel;

            if (NextTarget != null && Control.IsFreeRun() && NextTarget.IsPowerOn)
            {

                double PowerOnFrequency = GetFreqAt(Target.EndTime, 0, Data);
                double FreqPerSec, FreqGoto;
                if (!NextTarget.IsAccel)
                {
                    FreqPerSec = SoundData.JerkSetting.Braking.On.FrequencyChangeRate;
                    FreqGoto = SoundData.JerkSetting.Braking.On.MaxControlFrequency;
                }
                else
                {
                    FreqPerSec = SoundData.JerkSetting.Accelerating.On.FrequencyChangeRate;
                    FreqGoto = SoundData.JerkSetting.Accelerating.On.MaxControlFrequency;
                }

                double TargetFrequency = PowerOnFrequency > FreqGoto ? FreqGoto : PowerOnFrequency;
                double RequireTime = TargetFrequency / FreqPerSec;
                if (Target.EndTime - RequireTime < Control.GetTime())
                {
                    IsPowerOn = true;
                    Braking = !NextTarget.IsAccel;
                    ForceOnFrequency = PowerOnFrequency;
                }
            }

            double NewSineFrequency = GetFreqAt(CurrentTime, 0, Data);
            if (NewSineFrequency < 0) NewSineFrequency = 0;

            Control.SetBraking(Braking);
            Control.SetPowerOff(!IsPowerOn);

            {
                double SineTimeAmplitude = NewSineFrequency == 0 ? 0 : Control.GetBaseWaveFrequency() / NewSineFrequency;
                Control.SetBaseWaveAngleFrequency(NewSineFrequency * Math.PI * 2);
                if (Control.IsBaseWaveTimeChangeAllowed())
                    Control.MultiplyBaseWaveTime(SineTimeAmplitude);
            }

            if (ForceOnFrequency != -1)
            {
                double SineTimeAmplitude = ForceOnFrequency == 0 ? 0 : Control.GetBaseWaveFrequency() / ForceOnFrequency;
                Control.SetBaseWaveAngleFrequency(ForceOnFrequency * Math.PI * 2);
                if (Control.IsBaseWaveTimeChangeAllowed())
                    Control.MultiplyBaseWaveTime(SineTimeAmplitude);
            }


            Control.ProcessControlParameter(TimeDelta, SoundData.JerkSetting);
            Control.AddTimeAll(TimeDelta);

            if (Target == null) return false;
            return true;
        }
    }
}
