using System;
using System.Collections.Generic;
using System.Linq;
using static VvvfSimulator.Data.Util;

namespace VvvfSimulator.Data.Vvvf
{
    public class Struct
    {
        public int Level { get; set; } = 2;
        public JerkSettings JerkSetting { get; set; } = new();
        public MinimumBaseFrequency MinimumFrequency { get; set; } = new();
        public List<PulseControl> AcceleratePattern { get; set; } = [];
        public List<PulseControl> BrakingPattern { get; set; } = [];
        public override string ToString()
        {
            return GetPropertyValues(this);
        }
        public void SortAcceleratePattern(bool Inverse)
        {
            AcceleratePattern.Sort((a, b) => Math.Sign(Inverse ? (a.ControlFrequencyFrom - b.ControlFrequencyFrom) : (b.ControlFrequencyFrom - a.ControlFrequencyFrom)));
        }
        public void SortBrakingPattern(bool Inverse)
        {
            BrakingPattern.Sort((a, b) => Math.Sign(Inverse ? (a.ControlFrequencyFrom - b.ControlFrequencyFrom) : (b.ControlFrequencyFrom - a.ControlFrequencyFrom)));
        }
        public bool HasCustomPwm()
        {
            List<PulseControl> pattern = [];
            pattern.AddRange(AcceleratePattern);
            pattern.AddRange(BrakingPattern);
            return pattern.Any(ycd => ycd.PulseMode.PulseType is PulseControl.Pulse.PulseTypeName.CHM or PulseControl.Pulse.PulseTypeName.SHE);
        }

        public class JerkSettings
        {
            public Jerk Braking { get; set; } = new Jerk();
            public Jerk Accelerating { get; set; } = new Jerk();

            public override string ToString()
            {
                return GetPropertyValues(this);
            }

            public class Jerk
            {
                public JerkInfo On { get; set; } = new JerkInfo();
                public JerkInfo Off { get; set; } = new JerkInfo();

                public override string ToString()
                {
                    return GetPropertyValues(this);
                }

                public class JerkInfo
                {
                    public double FrequencyChangeRate { get; set; } = 60;
                    public double MaxControlFrequency { get; set; } = 60;

                    public override string ToString()
                    {
                        return GetPropertyValues(this);
                    }
                }
            }
        }
        public class MinimumBaseFrequency
        {
            public double Accelerating { get; set; } = -1.0;
            public double Braking { get; set; } = -1.0;

            public override string ToString()
            {
                return GetPropertyValues(this);
            }
        }
        public class PulseControl
        {
            public double ControlFrequencyFrom { get; set; } = -1;
            public double RotateFrequencyFrom { get; set; } = -1;
            public double RotateFrequencyBelow { get; set; } = -1;
            public bool EnableFreeRunOn { get; set; } = true;
            public bool StuckFreeRunOn { get; set; } = false;
            public bool EnableFreeRunOff { get; set; } = true;
            public bool StuckFreeRunOff { get; set; } = false;
            public bool EnableNormal { get; set; } = true;

            public Pulse PulseMode { get; set; } = new();
            public AmplitudeValue Amplitude { get; set; } = new AmplitudeValue();
            public AsyncControl AsyncModulationData { get; set; } = new AsyncControl();

            public override string ToString()
            {
                return GetPropertyValues(this);
            }
            public PulseControl Clone()
            {
                PulseControl clone = (PulseControl)MemberwiseClone();

                //Deep copy
                clone.Amplitude = Amplitude.Clone();
                clone.AsyncModulationData = AsyncModulationData.Clone();
                clone.PulseMode = PulseMode.Clone();

                return clone;
            }

            public class FunctionValue
            {
                public FunctionType Type { get; set; } = FunctionType.Proportional;
                public double Start { get; set; } = 0;
                public double StartValue { get; set; } = 0;
                public double End { get; set; } = 1;
                public double EndValue { get; set; } = 100;
                public double Degree { get; set; } = 2;
                public double CurveRate { get; set; } = 0;
                public override string ToString()
                {
                    return GetPropertyValues(this);
                }
                public FunctionValue Clone()
                {
                    FunctionValue clone = (FunctionValue)MemberwiseClone();
                    return clone;
                }

                public enum FunctionType
                {
                    Proportional, Inv_Proportional, Pow2_Exponential, Sine
                }
            }
            public class Pulse
            {
                public override string ToString()
                {
                    return GetPropertyValues(this);
                }
                public Pulse Clone()
                {
                    Pulse Copy = (Pulse)MemberwiseClone();
                    List<PulseHarmonic> clone_pulse_harmonics = [];
                    for (int i = 0; i < PulseHarmonics.Count; i++)
                    {
                        clone_pulse_harmonics.Add(PulseHarmonics[i].Clone());
                    }
                    Dictionary<PulseDataKey, PulseDataValue> ClonePulseData = [];
                    for (int i = 0; i < PulseData.Count; i++)
                    {
                        ClonePulseData.Add(PulseData.Keys.ToArray()[i], PulseData.Values.ToArray()[i].Clone());
                    }
                    Copy.PulseData = ClonePulseData;
                    Copy.PulseHarmonics = clone_pulse_harmonics;
                    Copy.DiscreteTime = DiscreteTime.Clone();
                    Copy.CarrierWave = CarrierWave.Clone();
                    return Copy;
                }

                //
                // Fundamental Configuration
                //
                public PulseTypeName PulseType { get; set; }
                public int PulseCount { get; set; } = 1;
                public enum PulseTypeName
                {
                    ASYNC, SYNC, CHM, SHE, HO, ΔΣ
                };

                //
                // Alternative Modes
                //
                public PulseAlternative Alternative { get; set; } = PulseAlternative.Default;
                public enum PulseAlternative
                {
                    Default, CP, Square,
                    Alt1, Alt2, Alt3, Alt4, Alt5, Alt6, Alt7, Alt8, Alt9, Alt10,
                    Alt11, Alt12, Alt13, Alt14, Alt15, Alt16, Alt17, Alt18, Alt19, Alt20,
                    Alt21, Alt22, Alt23, Alt24, Alt25, Alt26, Alt27, Alt28, Alt29, Alt30,
                }

                //
                // Discrete Time Configuration
                //
                public DiscreteTimeConfiguration DiscreteTime { get; set; } = new();
                public class DiscreteTimeConfiguration
                {
                    public bool Enabled { get; set; } = false;
                    public int Steps { get; set; } = 2;
                    public DiscreteTimeMode Mode { get; set; } = DiscreteTimeMode.Middle;
                    public override string ToString()
                    {
                        return GetPropertyValues(this);
                    }
                    public DiscreteTimeConfiguration Clone()
                    {
                        return (DiscreteTimeConfiguration)MemberwiseClone();
                    }

                    public enum DiscreteTimeMode
                    {
                        Left, Middle, Right
                    }
                }

                //
                // Compare Base Wave
                //
                public BaseWaveType BaseWave { get; set; } = BaseWaveType.Sine;
                public enum BaseWaveType
                {
                    Sine, Saw, Square, ModifiedSine1, ModifiedSine2, ModifiedSaw1, SV, DPWM30, DPWM60C, DPWM60P, DPWM60N, DPWM120P, DPWM120N
                }

                //
                // Compare Wave Harmonics
                //
                public List<PulseHarmonic> PulseHarmonics { get; set; } = [];
                public class PulseHarmonic
                {
                    public double Harmonic { get; set; } = 3;
                    public bool IsHarmonicProportional { get; set; } = true;
                    public double Amplitude { get; set; } = 0.2;
                    public bool IsAmplitudeProportional { get; set; } = true;
                    public double InitialPhase { get; set; } = 0;
                    public PulseHarmonicType Type { get; set; } = PulseHarmonicType.Sine;
                    public override string ToString()
                    {
                        return GetPropertyValues(this);
                    }
                    public PulseHarmonic Clone()
                    {
                        return (PulseHarmonic)MemberwiseClone();
                    }

                    public enum PulseHarmonicType
                    {
                        Sine, Saw, Square
                    }
                }

                //
                // Carrier Wave
                //
                public CarrierWaveConfiguration CarrierWave { get; set; } = new();
                public class CarrierWaveConfiguration
                {
                    public CarrierWaveType Type { get; set; } = CarrierWaveType.Triangle;
                    public CarrierWaveOption Option { get; set; } = CarrierWaveOption.FallStart;
                    public override string ToString()
                    {
                        return GetPropertyValues(this);
                    }
                    public CarrierWaveConfiguration Clone()
                    {
                        CarrierWaveConfiguration Copy = (CarrierWaveConfiguration)MemberwiseClone();
                        return Copy;
                    }

                    public enum CarrierWaveType
                    {
                        Triangle, Saw, Sine
                    }
                    public enum CarrierWaveOption
                    {
                        RaiseStart, FallStart, TopStart, BottomStart
                    }
                }

                //
                // Pulse Custom Variable
                //
                public Dictionary<PulseDataKey, PulseDataValue> PulseData { get; set; } = [];
                public enum PulseDataKey
                {
                    Dipolar, PulseWidth, Phase, UpdateFrequency
                }
                public class PulseDataValue
                {
                    public PulseDataValueMode Mode { get; set; } = PulseDataValueMode.Const;
                    public double Constant { get; set; } = -1;
                    public FunctionValue MovingValue { get; set; } = new FunctionValue();
                    public override string ToString()
                    {
                        return GetPropertyValues(this);
                    }
                    public PulseDataValue Clone()
                    {
                        PulseDataValue clone = (PulseDataValue)MemberwiseClone();
                        clone.MovingValue = this.MovingValue.Clone();
                        return clone;
                    }

                    public enum PulseDataValueMode
                    {
                        Const, Moving
                    }
                }
            }
            public class AsyncControl
            {
                public RandomModulation RandomData { get; set; } = new();
                public CarrierFrequency CarrierWaveData { get; set; } = new();
                public override string ToString()
                {
                    return GetPropertyValues(this);
                }
                public AsyncControl Clone()
                {
                    AsyncControl clone = (AsyncControl)MemberwiseClone();
                    clone.RandomData = RandomData.Clone();
                    clone.CarrierWaveData = CarrierWaveData.Clone();
                    return clone;
                }

                public class RandomModulation
                {
                    public Parameter Range { get; set; } = new();
                    public Parameter Interval { get; set; } = new();
                    public override string ToString()
                    {
                        return GetPropertyValues(this);
                    }
                    public RandomModulation Clone()
                    {
                        RandomModulation clone = (RandomModulation)MemberwiseClone();
                        clone.Range = Range.Clone();
                        clone.Interval = Interval.Clone();
                        return clone;
                    }

                    public class Parameter
                    {
                        public ValueMode Mode { get; set; }
                        public double Constant { get; set; } = 0;
                        public FunctionValue MovingValue { get; set; } = new FunctionValue();
                        public override string ToString()
                        {
                            return GetPropertyValues(this);
                        }
                        public Parameter Clone()
                        {
                            Parameter clone = (Parameter)MemberwiseClone();
                            clone.MovingValue = this.MovingValue.Clone();
                            return clone;
                        }

                        public enum ValueMode
                        {
                            Const, Moving
                        }
                    }
                }
                public class CarrierFrequency
                {
                    public ValueMode Mode { get; set; }
                    public double Constant { get; set; } = -1.0;
                    public FunctionValue MovingValue { get; set; } = new FunctionValue();
                    public VibratoValue VibratoData { get; set; } = new VibratoValue();
                    public TableValue CarrierFrequencyTable { get; set; } = new TableValue();
                    public override string ToString()
                    {
                        return GetPropertyValues(this);
                    }
                    public CarrierFrequency Clone()
                    {
                        CarrierFrequency clone = (CarrierFrequency)MemberwiseClone();
                        clone.MovingValue = MovingValue.Clone();
                        clone.VibratoData = VibratoData.Clone();
                        clone.CarrierFrequencyTable = CarrierFrequencyTable.Clone();
                        return clone;
                    }

                    public enum ValueMode
                    {
                        Const, Moving, Vibrato, Table
                    }
                    public class VibratoValue
                    {
                        public Parameter Highest { get; set; } = new();
                        public Parameter Lowest { get; set; } = new();
                        public Parameter Interval { get; set; } = new();
                        public BaseWaveType BaseWave { get; set; } = BaseWaveType.Triangle;
                        public override string ToString()
                        {
                            return GetPropertyValues(this);
                        }
                        public VibratoValue Clone()
                        {
                            VibratoValue clone = (VibratoValue)MemberwiseClone();
                            clone.Highest = Highest.Clone();
                            clone.Lowest = Lowest.Clone();
                            clone.Interval = Interval.Clone();
                            return clone;
                        }

                        public enum BaseWaveType
                        {
                            Sine, Triangle, Square, SawUp, SawDown
                        }
                        public class Parameter
                        {
                            public ValueMode Mode { get; set; } = ValueMode.Const;
                            public double Constant { get; set; } = -1;
                            public FunctionValue MovingValue { get; set; } = new();
                            public override string ToString()
                            {
                                return GetPropertyValues(this);
                            }
                            public Parameter Clone()
                            {
                                Parameter clone = (Parameter)MemberwiseClone();
                                clone.MovingValue = this.MovingValue.Clone();
                                return clone;
                            }

                            public enum ValueMode
                            {
                                Const, Moving
                            }
                        }
                    }
                    public class TableValue
                    {
                        public List<Parameter> Table { get; set; } = [];
                        public override string ToString()
                        {
                            return GetPropertyValues(this);
                        }
                        public TableValue Clone()
                        {
                            TableValue clone = new();
                            for (int i = 0; i < Table.Count; i++)
                            {
                                clone.Table.Add(Table[i].Clone());
                            }
                            return clone;
                        }

                        public class Parameter
                        {
                            public double ControlFrequencyFrom { get; set; } = -1;
                            public double CarrierFrequency { get; set; } = 1000;
                            public bool FreeRunStuckAtHere { get; set; } = false;
                            public override string ToString()
                            {
                                return GetPropertyValues(this);
                            }
                            public Parameter Clone()
                            {
                                Parameter clone = (Parameter)MemberwiseClone();
                                return clone;
                            }
                        }
                    }
                }
            }
            public class AmplitudeValue
            {
                public Parameter Default { get; set; } = new();
                public Parameter PowerOn { get; set; } = new();
                public Parameter PowerOff { get; set; } = new();
                public override string ToString()
                {
                    return GetPropertyValues(this);
                }
                public AmplitudeValue Clone()
                {
                    AmplitudeValue Cloned = (AmplitudeValue)MemberwiseClone();

                    //Deep copy
                    Cloned.Default = Default.Clone();
                    Cloned.PowerOn = PowerOn.Clone();
                    Cloned.PowerOff = PowerOff.Clone();

                    return Cloned;
                }

                public class Parameter
                {
                    public ValueMode Mode { get; set; } = ValueMode.Linear;
                    public double StartFrequency { get; set; } = -1;
                    public double StartAmplitude { get; set; } = -1;
                    public double EndFrequency { get; set; } = -1;
                    public double EndAmplitude { get; set; } = -1;
                    public double CurveChangeRate { get; set; } = 0;
                    public double CutOffAmplitude { get; set; } = -1;
                    public double MaxAmplitude { get; set; } = -1;
                    public bool DisableRangeLimit { get; set; } = false;
                    public double Polynomial { get; set; } = 0;
                    public bool AmplitudeTableInterpolation { get; set; } = false;
                    public (double Frequency, double Amplitude)[] AmplitudeTable { get; set; } = [];
                    public override string ToString()
                    {
                        return GetPropertyValues(this);
                    }
                    public Parameter Clone()
                    {
                        Parameter clone = (Parameter)MemberwiseClone();
                        return clone;
                    }

                    public enum ValueMode
                    {
                        Linear, LinearPolynomial, InverseProportional, Exponential, Sine, Table
                    }
                }
            }
        }
    }
}
