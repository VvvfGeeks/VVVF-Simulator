using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;
using static VvvfSimulator.Vvvf.Calculate;

namespace VvvfSimulator.Yaml.VvvfSound
{
    public class YamlVvvfSoundData
    {
        private static string GetValueString(object? o)
        {
            if (o == null) return "null";
            if(o.GetType() == typeof(ArrayList))
            {
                string str = "[";
                ArrayList list = (ArrayList) o;
                for(int i = 0; i < list.Count; i++)
                {
                    str += GetValueString(list[i]) + (i + 1 == list.Count ? "]" : ", ");
                }
                return str;
            }

            string? value = o.ToString();
            if(value == null) return "null";
            return value;
        }

        public int Level { get; set; } = 2;
        public YamlMasconData MasconData { get; set; } = new();
        public YamlMinSineFrequency MinimumFrequency { get; set; } = new();
        public List<YamlControlData> AcceleratePattern { get; set; } = [];
        public void SortAcceleratePattern(bool Inverse)
        {
            AcceleratePattern.Sort((a, b) => Math.Sign(Inverse ? (a.ControlFrequencyFrom - b.ControlFrequencyFrom) : (b.ControlFrequencyFrom - a.ControlFrequencyFrom)));
        }
        public List<YamlControlData> BrakingPattern { get; set; } = [];
        public void SortBrakingPattern(bool Inverse)
        {
            BrakingPattern.Sort((a, b) => Math.Sign(Inverse ? (a.ControlFrequencyFrom - b.ControlFrequencyFrom) : (b.ControlFrequencyFrom - a.ControlFrequencyFrom)));
        }
        public override string ToString()
        {
            Type t = typeof(YamlVvvfSoundData);
            string final = "[\r\n";
            foreach (var f in t.GetFields())
            {
                final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
            }
            final += "]";
            return final;
        }

        public class YamlMasconData
        {
            public YamlMasconDataPattern Braking { get; set; } = new YamlMasconDataPattern();
            public YamlMasconDataPattern Accelerating { get; set; } = new YamlMasconDataPattern();
            public override string ToString()
            {
                Type t = typeof(YamlMasconData);
                string final = "[\r\n";
                foreach (var f in t.GetFields())
                {
                    final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                }
                final += "]";
                return final;
            }
            public class YamlMasconDataPattern
            {
                public YamlMasconDataPatternMode On { get; set; } = new YamlMasconDataPatternMode();
                public YamlMasconDataPatternMode Off { get; set; } = new YamlMasconDataPatternMode();
                public override string ToString()
                {
                    Type t = typeof(YamlMasconDataPattern);
                    string final = "[\r\n";
                    foreach (var f in t.GetFields())
                    {
                        final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                    }
                    final += "]";
                    return final;
                }
                public class YamlMasconDataPatternMode
                {
                    public double FrequencyChangeRate { get; set; } = 60;
                    public double MaxControlFrequency { get; set; } = 60;
                    public override string ToString()
                    {
                        Type t = typeof(YamlMasconDataPatternMode);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
                    }
                }
            }


        }

        public class YamlMinSineFrequency
        {
            public double Accelerating { get; set; } = -1.0;
            public double Braking { get; set; } = -1.0;
            public override string ToString()
            {
                Type t = typeof(YamlMinSineFrequency);
                string final = "[\r\n";
                foreach (var f in t.GetFields())
                {
                    final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                }
                final += "]";
                return final;
            }
        }

        public class YamlControlData
        {
            public double ControlFrequencyFrom { get; set; } = -1;
            public double RotateFrequencyFrom { get; set; } = -1;
            public double RotateFrequencyBelow { get; set; } = -1;
            public bool EnableFreeRunOn { get; set; } = true;
            public bool StuckFreeRunOn { get; set; } = false;
            public bool EnableFreeRunOff { get; set; } = true;
            public bool StuckFreeRunOff { get; set; } = false;
            public bool EnableNormal { get; set; } = true;

            public YamlPulseMode PulseMode { get; set; } = new();
            public YamlAmplitude Amplitude { get; set; } = new YamlAmplitude();
            public YamlAsync AsyncModulationData { get; set; } = new YamlAsync();

            public YamlControlData Clone()
            {
                YamlControlData clone = (YamlControlData)MemberwiseClone();

                //Deep copy
                clone.Amplitude = Amplitude.Clone();
                clone.AsyncModulationData = AsyncModulationData.Clone();
                clone.PulseMode = PulseMode.Clone();

                return clone;
            }
            public override string ToString()
            {
                Type t = typeof(YamlControlData);
                string final = "[\r\n";
                foreach (var f in t.GetFields())
                {
                    final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                }
                final += "]";
                return final;
            }

            public class YamlMovingValue
            {
                public MovingValueType Type { get; set; } = MovingValueType.Proportional;
                public double Start { get; set; } = 0;
                public double StartValue { get; set; } = 0;
                public double End { get; set; } = 1;
                public double EndValue { get; set; } = 100;
                public double Degree { get; set; } = 2;
                public double CurveRate { get; set; } = 0;

                public enum MovingValueType
                {
                    Proportional, Inv_Proportional, Pow2_Exponential, Sine
                }

                public override string ToString()
                {
                    Type t = typeof(YamlMovingValue);
                    string final = "[\r\n";
                    foreach (var f in t.GetFields())
                    {
                        final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                    }
                    final += "]";
                    return final;
                }

                public YamlMovingValue Clone()
                {
                    YamlMovingValue clone = (YamlMovingValue)MemberwiseClone();
                    return clone;
                }
            }

            public class YamlPulseMode
            {
                public YamlPulseMode Clone()
                {
                    var x = (YamlPulseMode)MemberwiseClone();
                    List<PulseHarmonic> clone_pulse_harmonics = [];
                    for (int i = 0; i < PulseHarmonics.Count; i++)
                    {
                        clone_pulse_harmonics.Add(PulseHarmonics[i].Clone());
                    }
                    Dictionary<PulseDataKey, PulseDataValue> ClonePulseData = [];
                    for(int i = 0; i < PulseData.Count; i++)
                    {
                        ClonePulseData.Add(PulseData.Keys.ToArray()[i], PulseData.Values.ToArray()[i].Clone());
                    }
                    x.PulseData = ClonePulseData;
                    x.PulseHarmonics = clone_pulse_harmonics;
                    x.DiscreteTime = DiscreteTime.Clone();
                    return x;

                }

                //
                // Fundamental Configuration
                //
                public PulseTypeName PulseType { get; set; }
                public int PulseCount { get; set; } = 1;
                public enum PulseTypeName
                {
                    ASYNC, SYNC, CHM, SHE, HO,
                };

                //
                // Alternative Modes
                //
                public PulseAlternative Alternative { get; set; } = PulseAlternative.Default;
                public enum PulseAlternative
                {
                    Default,
                    Alt1, Alt2, Alt3, Alt4, Alt5, Alt6, Alt7, Alt8, Alt9, Alt10,
                    Alt11, Alt12, Alt13, Alt14, Alt15, Alt16, Alt17, Alt18, Alt19, Alt20,
                    Alt21, Alt22, Alt23, Alt24, Alt25, Alt26, Alt27, Alt28, Alt29, Alt30,
                }

                //
                // Flat Configurations
                //
                public bool Shift { get; set; } = false;
                public bool Square { get; set; } = false;

                //
                // Discrete Time Configuration
                //
                public DiscreteTimeConfiguration DiscreteTime { get; set; } = new();
                public class DiscreteTimeConfiguration
                {
                    public bool Enabled { get; set; } = false;
                    public int Steps { get; set; } = 2;
                    public DiscreteTimeMode Mode { get; set; } = DiscreteTimeMode.Middle;

                    public enum DiscreteTimeMode
                    {
                        Left, Middle, Right
                    }

                    public DiscreteTimeConfiguration Clone()
                    {
                        return (DiscreteTimeConfiguration)MemberwiseClone();
                    }
                }

                //
                // Compare Base Wave
                //
                public BaseWaveType BaseWave { get; set; } = BaseWaveType.Sine;
                public enum BaseWaveType
                {
                    Sine, Saw, ModifiedSine1, ModifiedSine2, ModifiedSaw1
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

                    public enum PulseHarmonicType
                    {
                        Sine, Saw, Square
                    }

                    public PulseHarmonic Clone()
                    {
                        return (PulseHarmonic)MemberwiseClone();
                    }
                }


                //
                // Pulse Custom Variable
                //
                public Dictionary<PulseDataKey, PulseDataValue> PulseData { get; set; } = [];
                public enum PulseDataKey
                {
                    Dipolar, L3P3Alt1Width
                }
                public class PulseDataValue
                {
                    public PulseDataValueMode Mode { get; set; } = PulseDataValueMode.Const;
                    public double Constant { get; set; } = -1;
                    public YamlMovingValue MovingValue { get; set; } = new YamlMovingValue();
                    public override string ToString()
                    {
                        Type t = typeof(PulseDataValue);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
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

            public class YamlAsync
            {
                public RandomModulation RandomData { get; set; } = new();
                public CarrierFrequency CarrierWaveData { get; set; } = new();
                public override string ToString()
                {
                    Type t = typeof(YamlAsync);
                    string final = "[\r\n";
                    foreach (var f in t.GetFields())
                    {
                        final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                    }
                    final += "]";
                    return final;
                }
                public YamlAsync Clone()
                {
                    YamlAsync clone = (YamlAsync)MemberwiseClone();
                    clone.RandomData = RandomData.Clone();
                    clone.CarrierWaveData = CarrierWaveData.Clone();
                    return clone;
                }
                public class RandomModulation
                {
                    public YamlAsyncParameterRandomValue Range { get; set; } = new();
                    public YamlAsyncParameterRandomValue Interval { get; set; } = new();
                    public override string ToString()
                    {
                        Type t = typeof(RandomModulation);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
                    }
                    public RandomModulation Clone()
                    {
                        RandomModulation clone = (RandomModulation)MemberwiseClone();
                        clone.Range = Range.Clone();
                        clone.Interval = Interval.Clone();
                        return clone;
                    }
                    public class YamlAsyncParameterRandomValue
                    {
                        public YamlAsyncParameterRandomValueMode Mode { get; set; }
                        public double Constant { get; set; } = 0;
                        public YamlMovingValue MovingValue { get; set; } = new YamlMovingValue();
                        public override string ToString()
                        {
                            Type t = typeof(YamlAsyncParameterRandomValue);
                            string final = "[\r\n";
                            foreach (var f in t.GetFields())
                            {
                                final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                            }
                            final += "]";
                            return final;
                        }

                        public YamlAsyncParameterRandomValue Clone()
                        {
                            YamlAsyncParameterRandomValue clone = (YamlAsyncParameterRandomValue)MemberwiseClone();
                            clone.MovingValue = this.MovingValue.Clone();
                            return clone;
                        }

                        public enum YamlAsyncParameterRandomValueMode
                        {
                            Const, Moving
                        }
                    }


                }
                public class CarrierFrequency
                {
                    public CarrierFrequencyValueMode Mode { get; set; }
                    public double Constant { get; set; } = -1.0;
                    public YamlMovingValue MovingValue { get; set; } = new YamlMovingValue();
                    public YamlAsyncParameterCarrierFreqVibrato VibratoData { get; set; } = new YamlAsyncParameterCarrierFreqVibrato();
                    public YamlAsyncParameterCarrierFreqTable CarrierFrequencyTable { get; set; } = new YamlAsyncParameterCarrierFreqTable();

                    public override string ToString()
                    {
                        Type t = typeof(CarrierFrequency);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
                    }


                    public CarrierFrequency Clone()
                    {
                        CarrierFrequency clone = (CarrierFrequency)MemberwiseClone();
                        clone.MovingValue = MovingValue.Clone();
                        clone.VibratoData = VibratoData.Clone();
                        clone.CarrierFrequencyTable = CarrierFrequencyTable.Clone();
                        return clone;
                    }

                    public enum CarrierFrequencyValueMode
                    {
                        Const, Moving, Vibrato, Table
                    }

                    public class YamlAsyncParameterCarrierFreqVibrato
                    {
                        public YamlAsyncParameterVibratoValue Highest { get; set; } = new();
                        public YamlAsyncParameterVibratoValue Lowest { get; set; } = new();
                        public YamlAsyncParameterVibratoValue Interval { get; set; } = new();
                        public bool Continuous { get; set; } = true;
                        public override string ToString()
                        {
                            Type t = typeof(YamlAsyncParameterCarrierFreqVibrato);
                            string final = "[\r\n";
                            foreach (var f in t.GetFields())
                            {
                                final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                            }
                            final += "]";
                            return final;
                        }
                        public YamlAsyncParameterCarrierFreqVibrato Clone()
                        {
                            YamlAsyncParameterCarrierFreqVibrato clone = (YamlAsyncParameterCarrierFreqVibrato)MemberwiseClone();
                            clone.Highest = Highest.Clone();
                            clone.Lowest = Lowest.Clone();
                            clone.Interval = Interval.Clone();
                            return clone;
                        }
                        public class YamlAsyncParameterVibratoValue
                        {
                            public YamlAsyncParameterVibratoMode Mode { get; set; } = YamlAsyncParameterVibratoMode.Const;
                            public double Constant { get; set; } = -1;
                            public YamlMovingValue MovingValue { get; set; } = new();
                            public override string ToString()
                            {
                                Type t = typeof(YamlAsyncParameterVibratoValue);
                                string final = "[\r\n";
                                foreach (var f in t.GetFields())
                                {
                                    final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                                }
                                final += "]";
                                return final;
                            }
                            public YamlAsyncParameterVibratoValue Clone()
                            {
                                YamlAsyncParameterVibratoValue clone = (YamlAsyncParameterVibratoValue)MemberwiseClone();
                                clone.MovingValue = this.MovingValue.Clone();
                                return clone;
                            }

                            public enum YamlAsyncParameterVibratoMode
                            {
                                Const, Moving
                            }
                        }

                    }

                    public class YamlAsyncParameterCarrierFreqTable
                    {
                        public List<YamlAsyncParameterCarrierFreqTableValue> CarrierFrequencyTableValues { get; set; } = [];
                        public class YamlAsyncParameterCarrierFreqTableValue
                        {
                            public double ControlFrequencyFrom { get; set; } = -1;
                            public double CarrierFrequency { get; set; } = 1000;
                            public bool FreeRunStuckAtHere { get; set; } = false;
                            public override string ToString()
                            {
                                Type t = typeof(YamlAsyncParameterCarrierFreqTableValue);
                                string final = "[\r\n";
                                foreach (var f in t.GetFields())
                                {
                                    final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                                }
                                final += "]";
                                return final;
                            }

                            public YamlAsyncParameterCarrierFreqTableValue Clone()
                            {
                                YamlAsyncParameterCarrierFreqTableValue clone = (YamlAsyncParameterCarrierFreqTableValue)MemberwiseClone();
                                return clone;
                            }
                        }
                        public override string ToString()
                        {
                            Type t = typeof(YamlAsyncParameterCarrierFreqTable);
                            string final = "[\r\n";
                            foreach (var f in t.GetFields())
                            {
                                final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                            }
                            final += "]";
                            return final;
                        }
                        public YamlAsyncParameterCarrierFreqTable Clone()
                        {
                            YamlAsyncParameterCarrierFreqTable clone = new();
                            for (int i = 0; i < CarrierFrequencyTableValues.Count; i++)
                            {
                                clone.CarrierFrequencyTableValues.Add(CarrierFrequencyTableValues[i].Clone());
                            }
                            return clone;
                        }
                    }
                }
            }

            public class YamlAmplitude
            {
                public YamlControlDataAmplitude DefaultAmplitude { get; set; } = new YamlControlDataAmplitude();
                public YamlControlDataAmplitudeFreeRun FreeRunAmplitude { get; set; } = new YamlControlDataAmplitudeFreeRun();
                public override string ToString()
                {
                    Type t = typeof(YamlAmplitude);
                    string final = "[\r\n";
                    foreach (var f in t.GetFields())
                    {
                        final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                    }
                    final += "]";
                    return final;
                }

                public YamlAmplitude Clone()
                {
                    YamlAmplitude clone = (YamlAmplitude)MemberwiseClone();

                    //Deep copy
                    clone.DefaultAmplitude = DefaultAmplitude.Clone();
                    clone.FreeRunAmplitude = FreeRunAmplitude.Clone();

                    return clone;
                }

                public class YamlControlDataAmplitudeFreeRun
                {
                    public YamlControlDataAmplitude On { get; set; } = new YamlControlDataAmplitude();
                    public YamlControlDataAmplitude Off { get; set; } = new YamlControlDataAmplitude();
                    public override string ToString()
                    {
                        Type t = typeof(YamlControlDataAmplitudeFreeRun);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
                    }
                    public YamlControlDataAmplitudeFreeRun Clone()
                    {
                        YamlControlDataAmplitudeFreeRun clone = (YamlControlDataAmplitudeFreeRun)MemberwiseClone();
                        clone.On = On.Clone();
                        clone.Off = Off.Clone();
                        return clone;
                    }

                }
                public class YamlControlDataAmplitude
                {
                    public AmplitudeMode Mode { get; set; } = AmplitudeMode.Linear;
                    public YamlControlDataAmplitudeParameter Parameter { get; set; } = new YamlControlDataAmplitudeParameter();
                    public override string ToString()
                    {
                        Type t = typeof(YamlControlDataAmplitude);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
                    }
                    public YamlControlDataAmplitude Clone()
                    {
                        YamlControlDataAmplitude clone = (YamlControlDataAmplitude)MemberwiseClone();
                        clone.Parameter = Parameter.Clone();
                        return clone;
                    }
                    public class YamlControlDataAmplitudeParameter
                    {
                        public double StartFrequency { get; set; } = -1;
                        public double StartAmplitude { get; set; } = -1;
                        public double EndFrequency { get; set; } = -1;
                        public double EndAmplitude { get; set; } = -1;
                        public double CurveChangeRate { get; set; } = 0;
                        public double CutOffAmplitude { get; set; } = -1;
                        public double MaxAmplitude { get; set; } = -1;
                        public bool DisableRangeLimit { get; set; } = false;
                        public double Polynomial { get; set; } = 0;
                        public override string ToString()
                        {
                            Type t = typeof(YamlControlDataAmplitudeParameter);
                            string final = "[\r\n";
                            foreach (var f in t.GetFields())
                            {
                                final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                            }
                            final += "]";
                            return final;
                        }
                        public YamlControlDataAmplitudeParameter Clone()
                        {
                            YamlControlDataAmplitudeParameter clone = (YamlControlDataAmplitudeParameter)MemberwiseClone();
                            return clone;
                        }
                    }
                }
            }
        }
    }
    public static class YamlVvvfManage
    {
        public static YamlVvvfSoundData CurrentData = new();

        public static bool Save(String path)
        {
            try
            {
                using StreamWriter writer = File.CreateText(path);
                var serializer = new Serializer();
                serializer.Serialize(writer, CurrentData);
                writer.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public static bool Load(String path)
        {
            var input = new StreamReader(path, Encoding.UTF8);
            var deserializer = new Deserializer();
            YamlVvvfSoundData deserializeObject = deserializer.Deserialize<YamlVvvfSoundData>(input);
            YamlVvvfManage.CurrentData = deserializeObject;
            input.Close();
            return true;
        }
        public static YamlVvvfSoundData DeepClone(YamlVvvfSoundData src)
        {
            YamlVvvfSoundData deserializeObject = new Deserializer().Deserialize<YamlVvvfSoundData>(new Serializer().Serialize(src));
            return deserializeObject;
        }
    }
}