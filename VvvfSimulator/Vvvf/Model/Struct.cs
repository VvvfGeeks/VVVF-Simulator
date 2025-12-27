using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using VvvfSimulator.Vvvf.Modulation;

namespace VvvfSimulator.Vvvf.Model
{
    public class Struct
    {
        public class Domain(Data.TrainAudio.Struct.MotorSpecification MotorSpec)
        {
            public Domain Clone()
            {
                Domain Copy = (Domain)MemberwiseClone();

                Copy.DeltaSigmaInstances = [DeltaSigmaInstances[0].Clone(), DeltaSigmaInstances[1].Clone(), DeltaSigmaInstances[2].Clone()];
                Copy.CarrierInstance = CarrierInstance.Clone();
                Copy.Motor = Motor.Clone();

                return Copy;
            }

            #region ControlParameter
            private bool Brake = false;
            private bool FreeRun = false;
            private double ControlFrequency = 0;
            private bool PowerOff = false;
            private double FreeFrequencyChange = 0.0;
            private bool AllowBaseWaveTimeChange = true;

            public double GetControlFrequency() { return ControlFrequency; }
            public void SetControlFrequency(double b) { ControlFrequency = b; }
            public void AddControlFrequency(double b) { ControlFrequency += b; }

            public bool IsPowerOff() { return PowerOff; }
            public void SetPowerOff(bool b) { PowerOff = b; }

            public bool IsFreeRun() { return FreeRun; }
            public void SetFreeRun(bool b) { FreeRun = b; }

            public bool IsBraking() { return Brake; }
            public void SetBraking(bool b) { Brake = b; }

            public bool IsBaseWaveTimeChangeAllowed() { return AllowBaseWaveTimeChange; }
            public void SetBaseWaveTimeChangeAllowed(bool b) { AllowBaseWaveTimeChange = b; }

            public double GetFreeFrequencyChange() { return FreeFrequencyChange; }
            public void SetFreeFrequencyChange(double d) { FreeFrequencyChange = d; }

            public void ProcessControlParameter(double DeltaTime, Data.Vvvf.Struct.JerkSettings JerkSetting)
            {
                {
                    double MaxVoltageFreq;
                    Data.Vvvf.Struct.JerkSettings.Jerk Pattern = IsBraking() ? JerkSetting.Braking : JerkSetting.Accelerating;

                    if (!IsPowerOff())
                    {
                        SetFreeFrequencyChange(Pattern.On.FrequencyChangeRate);
                        MaxVoltageFreq = Pattern.On.MaxControlFrequency;
                        if (IsFreeRun())
                        {
                            if (GetControlFrequency() > MaxVoltageFreq)
                                SetControlFrequency(GetBaseWaveFrequency());
                        }
                    }
                    else
                    {
                        SetFreeFrequencyChange(Pattern.Off.FrequencyChangeRate);
                        MaxVoltageFreq = Pattern.Off.MaxControlFrequency;
                        if (IsFreeRun())
                        {
                            if (GetControlFrequency() > MaxVoltageFreq)
                                SetControlFrequency(MaxVoltageFreq);
                        }
                    }
                }

                if (!IsPowerOff())
                {
                    if (!IsFreeRun())
                        SetControlFrequency(GetBaseWaveFrequency());
                    else
                    {
                        double updatedControlFrequency = GetControlFrequency() + GetFreeFrequencyChange() * DeltaTime;
                        if (GetBaseWaveFrequency() <= updatedControlFrequency)
                        {
                            SetControlFrequency(GetBaseWaveFrequency());
                            SetFreeRun(false);
                        }
                        else
                        {
                            SetControlFrequency(updatedControlFrequency);
                            SetFreeRun(true);
                        }
                    }
                }
                else
                {
                    double freq_change = GetFreeFrequencyChange() * DeltaTime;
                    double final_freq = GetControlFrequency() - freq_change;
                    SetControlFrequency(final_freq > 0 ? final_freq : 0);
                    SetFreeRun(true);
                }
            }

            #endregion

            #region MathematicalParameter
            private double BaseWaveAngleFrequency = 0;
            private double BaseWaveTime = 0;
            private double LastT = 0;
            private double T = 0;

            public void SetTimeAll(double Time)
            {
                SetTime(Time);
                SetBaseWaveTime(Time);
                CarrierInstance.Time = Time;
            }

            public void AddTimeAll(double Delta)
            {
                AddTime(Delta);
                AddBaseWaveTime(Delta);
                CarrierInstance.Time += Delta;
            }

            public void ResetTimeAll()
            {
                SetTimeAll(0);
                LastT = 0;
                ResetDeltaSigmaInstance();
                CarrierInstance.ResetIFrequencyTime(0);
            }

            public void SetTime(double Value) {
                LastT = T;
                T = Value;
            }
            public double GetTime() { return T; }
            public void AddTime(double Value) { SetTime(T + Value); }

            public double GetLastTime() => LastT;
            public double GetDeltaTime() => T - LastT;

            public void SetBaseWaveAngleFrequency(double b) { BaseWaveAngleFrequency = b; }
            public double GetBaseWaveAngleFrequency() { return BaseWaveAngleFrequency; }
            public double GetBaseWaveFrequency() { return BaseWaveAngleFrequency * MyMath.M_1_2PI; }

            public void SetBaseWaveTime(double t) { BaseWaveTime = t; }
            public double GetBaseWaveTime() { return BaseWaveTime; }
            public void AddBaseWaveTime(double t) { BaseWaveTime += t; }
            public void MultiplyBaseWaveTime(double x) { BaseWaveTime *= x; }
            #endregion

            #region MotorParameter
            public Motor Motor { get; set; } = new(MotorSpec);
            #endregion

            #region ElectricParameter
            public ElectricalParameter ElectricalState { get; set; } = new(2, 0);
            #endregion

            #region Modulation
            private DeltaSigma[] DeltaSigmaInstances = [new(), new(), new()];
            public DeltaSigma GetDeltaSigmaInstance(int Phase) => DeltaSigmaInstances[Phase];
            public int ProcessDeltaSigmaInstance(int Phase, double Input) => GetDeltaSigmaInstance(Phase).Process(Input, GetTime());
            public void ResetDeltaSigmaInstance()
            {
                DeltaSigmaInstances[0].Reset();
                DeltaSigmaInstances[1].Reset();
                DeltaSigmaInstances[2].Reset();
            }

            private Carrier CarrierInstance = new();
            public Carrier GetCarrierInstance() => CarrierInstance;
            #endregion
        }
        public class ElectricalParameter
        {
            [MemberNotNullWhen(false, [
                nameof(PulseData), nameof(CarrierFrequency), nameof(PulsePattern),
                nameof(BaseWaveAmplitude), nameof(PwmLevel)
            ])]
            public bool IsNone { get; } = true;
            public bool IsZeroOutput { get; } = true;
            public int PwmLevel { get; set; }
            public Data.Vvvf.Struct.PulseControl? PulsePattern { get; } = null;
            public CarrierParameter? CarrierFrequency { get; } = null;
            public Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseDataKey, double>? PulseData { get; } = null;
            public double BaseWaveFrequency { get; }
            public double BaseWaveAngleFrequency { get { return MyMath.M_2PI * BaseWaveFrequency; } }
            public double? BaseWaveAmplitude { get; } = null;
            public ElectricalParameter(bool isNone, bool isZeroOutput, int pwmLevel, Data.Vvvf.Struct.PulseControl? pulsePattern, CarrierParameter? carrierFrequency, Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseDataKey, double>? pulseData, double baseWaveFrequency, double? baseWaveAmplitude)
            {
                IsNone = isNone;
                IsZeroOutput = isZeroOutput;
                PwmLevel = pwmLevel;
                PulsePattern = pulsePattern;
                CarrierFrequency = carrierFrequency;
                PulseData = pulseData;
                BaseWaveFrequency = baseWaveFrequency;
                BaseWaveAmplitude = baseWaveAmplitude;
            }
            public ElectricalParameter(int pwmLevel, double baseWaveFrequency)
            {
                PwmLevel = pwmLevel;
                BaseWaveFrequency = baseWaveFrequency;
            }
            public class CarrierParameter
            {
                public RandomFrequency RandomRange { get; set; }
                public object BaseFrequency { get; set; }
                public CarrierParameter(RandomFrequency randomRange, object baseFrequency)
                {
                    if (baseFrequency is not ConstantFrequency && baseFrequency is not VibratoFrequency)
                        throw new Exception();

                    RandomRange = randomRange;
                    BaseFrequency = baseFrequency;
                }
                public CarrierParameter Clone()
                {
                    CarrierParameter Copy = (CarrierParameter)MemberwiseClone();
                    Copy.RandomRange = RandomRange.Clone();
                    Copy.BaseFrequency = BaseFrequency switch
                    {
                        VibratoFrequency Vibrato => Vibrato.Clone(),
                        _ => ((ConstantFrequency)BaseFrequency).Clone(),
                    };
                    return Copy;
                }
                public class RandomFrequency(double range, double interval)
                {
                    public double Range { get; set; } = range;
                    public double Interval { get; set; } = interval;
                    public RandomFrequency Clone()
                    {
                        return (RandomFrequency)MemberwiseClone();
                    }
                }
                public class ConstantFrequency(double value)
                {
                    public double Value { get; set; } = value;
                    public ConstantFrequency Clone()
                    {
                        return (ConstantFrequency)MemberwiseClone();
                    }
                }
                public class VibratoFrequency(double highest, double lowest, double interval)
                {
                    public double Highest { get; set; } = highest;
                    public double Lowest { get; set; } = lowest;
                    public double Interval { get; set; } = interval;
                    public VibratoFrequency Clone()
                    {
                        return (VibratoFrequency)MemberwiseClone();
                    }
                }
            }
        }
        public class PhaseState(int U, int V, int W) : IEquatable<PhaseState>
        {
            public int U = U;
            public int V = V;
            public int W = W;
            public PhaseState Clone()
            {
                return (PhaseState)MemberwiseClone();
            }
            public override bool Equals(object? obj)
            {
                return Equals(obj as PhaseState);
            }
            public bool Equals(PhaseState? other)
            {
                return other != null && U == other.U && V == other.V && W == other.W;
            }
            public override int GetHashCode()
            {
                return HashCode.Combine(U, V, W);
            }
            public static PhaseState Zero()
            {
                return new(0, 0, 0);
            }

        }
    }
}
