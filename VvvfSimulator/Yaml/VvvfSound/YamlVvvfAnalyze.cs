using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.VvvfStructs;

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
        public List<YamlControlData> AcceleratePattern { get; set; } = new();
        public void SortAcceleratePattern(bool Inverse)
        {
            AcceleratePattern.Sort((a, b) => Math.Sign(Inverse ? (a.ControlFrequencyFrom - b.ControlFrequencyFrom) : (b.ControlFrequencyFrom - a.ControlFrequencyFrom)));
        }
        public List<YamlControlData> BrakingPattern { get; set; } = new();
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
            public bool EnableFreeRunOff { get; set; } = true;
            public bool EnableNormal { get; set; } = true;

            // Null check !
            public PulseMode PulseMode { get; set; } = new();
            public YamlFreeRunCondition FreeRunCondition { get; set; } = new YamlFreeRunCondition();
            public YamlControlDataAmplitudeControl Amplitude { get; set; } = new YamlControlDataAmplitudeControl();
            public YamlAsyncParameter AsyncModulationData { get; set; } = new YamlAsyncParameter();

            public YamlControlData Clone()
            {
                YamlControlData clone = (YamlControlData)MemberwiseClone();

                //Deep copy
                clone.FreeRunCondition = FreeRunCondition.Clone();
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
            public class YamlFreeRunCondition
            {
                public YamlFreeRunConditionSingle On { get; set; } = new YamlFreeRunConditionSingle();
                public YamlFreeRunConditionSingle Off { get; set; } = new YamlFreeRunConditionSingle();

                public override string ToString()
                {
                    Type t = typeof(YamlFreeRunCondition);
                    string final = "[\r\n";
                    foreach (var f in t.GetFields())
                    {
                        final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                    }
                    final += "]";
                    return final;
                }

                public YamlFreeRunCondition Clone()
                {
                    YamlFreeRunCondition clone = (YamlFreeRunCondition)MemberwiseClone();
                    clone.On = On.Clone();
                    clone.Off = Off.Clone();
                    return clone;
                }

                public class YamlFreeRunConditionSingle
                {
                    public bool Skip { get; set; } = false;
                    public bool StuckAtHere { get; set; } = false;
                    public override string ToString()
                    {
                        Type t = typeof(YamlFreeRunConditionSingle);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
                    }

                    public YamlFreeRunConditionSingle Clone()
                    {
                        YamlFreeRunConditionSingle clone = (YamlFreeRunConditionSingle)MemberwiseClone();
                        return clone;
                    }
                }

            }

            public class YamlAsyncParameter
            {
                public YamlAsyncParameterRandom RandomData { get; set; } = new();
                public YamlAsyncParameterCarrierFreq CarrierWaveData { get; set; } = new();
                public YamlAsyncParameterDipolar DipolarData { get; set; } = new();
                public override string ToString()
                {
                    Type t = typeof(YamlAsyncParameter);
                    string final = "[\r\n";
                    foreach (var f in t.GetFields())
                    {
                        final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                    }
                    final += "]";
                    return final;
                }

                public YamlAsyncParameter Clone()
                {
                    YamlAsyncParameter clone = (YamlAsyncParameter)MemberwiseClone();
                    clone.RandomData = RandomData.Clone();
                    clone.CarrierWaveData = CarrierWaveData.Clone();
                    clone.DipolarData = DipolarData.Clone();
                    return clone;
                }

                public class YamlAsyncParameterRandom
                {
                    public YamlAsyncParameterRandomValue Range { get; set; } = new();
                    public YamlAsyncParameterRandomValue Interval { get; set; } = new();
                    public override string ToString()
                    {
                        Type t = typeof(YamlAsyncParameterRandom);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
                    }
                    public YamlAsyncParameterRandom Clone()
                    {
                        YamlAsyncParameterRandom clone = (YamlAsyncParameterRandom)MemberwiseClone();
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
                public class YamlAsyncParameterCarrierFreq
                {
                    public YamlAsyncCarrierMode Mode { get; set; }
                    public double Constant { get; set; } = -1.0;
                    public YamlMovingValue MovingValue { get; set; } = new YamlMovingValue();
                    public YamlAsyncParameterCarrierFreqVibrato VibratoData { get; set; } = new YamlAsyncParameterCarrierFreqVibrato();
                    public YamlAsyncParameterCarrierFreqTable CarrierFrequencyTable { get; set; } = new YamlAsyncParameterCarrierFreqTable();

                    public override string ToString()
                    {
                        Type t = typeof(YamlAsyncParameterCarrierFreq);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
                    }


                    public YamlAsyncParameterCarrierFreq Clone()
                    {
                        YamlAsyncParameterCarrierFreq clone = (YamlAsyncParameterCarrierFreq)MemberwiseClone();
                        clone.MovingValue = MovingValue.Clone();
                        clone.VibratoData = VibratoData.Clone();
                        clone.CarrierFrequencyTable = CarrierFrequencyTable.Clone();
                        return clone;
                    }

                    public enum YamlAsyncCarrierMode
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
                        public List<YamlAsyncParameterCarrierFreqTableValue> CarrierFrequencyTableValues { get; set; } = new List<YamlAsyncParameterCarrierFreqTableValue>();
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
                public class YamlAsyncParameterDipolar
                {
                    public YamlAsyncParameterDipolarMode Mode { get; set; } = YamlAsyncParameterDipolarMode.Const;
                    public double Constant { get; set; } = -1;
                    public YamlMovingValue MovingValue { get; set; } = new YamlMovingValue();
                    public override string ToString()
                    {
                        Type t = typeof(YamlAsyncParameterDipolar);
                        string final = "[\r\n";
                        foreach (var f in t.GetFields())
                        {
                            final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                        }
                        final += "]";
                        return final;
                    }
                    public YamlAsyncParameterDipolar Clone()
                    {
                        YamlAsyncParameterDipolar clone = (YamlAsyncParameterDipolar)MemberwiseClone();
                        clone.MovingValue = this.MovingValue.Clone();
                        return clone;
                    }

                    public enum YamlAsyncParameterDipolarMode
                    {
                        Const, Moving
                    }


                }

            }

            public class YamlControlDataAmplitudeControl
            {
                public YamlControlDataAmplitude DefaultAmplitude { get; set; } = new YamlControlDataAmplitude();
                public YamlControlDataAmplitudeFreeRun FreeRunAmplitude { get; set; } = new YamlControlDataAmplitudeFreeRun();
                public override string ToString()
                {
                    Type t = typeof(YamlControlDataAmplitudeControl);
                    string final = "[\r\n";
                    foreach (var f in t.GetFields())
                    {
                        final += string.Format("{0} : {1}", f.Name, GetValueString(f.GetValue(t))) + "\r\n";
                    }
                    final += "]";
                    return final;
                }

                public YamlControlDataAmplitudeControl Clone()
                {
                    YamlControlDataAmplitudeControl clone = (YamlControlDataAmplitudeControl)MemberwiseClone();

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
                using TextWriter writer = File.CreateText(path);
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