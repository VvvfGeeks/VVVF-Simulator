using OpenCvSharp.Aruco;
using System;
using System.Collections.Generic;
using VvvfSimulator.GUI.Simulator.RealTime;
using VvvfSimulator.GUI.Simulator.RealTime.Setting;
using static VvvfSimulator.GUI.Simulator.RealTime.RealtimeDisplay.ControlStatus;
using static VvvfSimulator.GUI.Simulator.RealTime.RealtimeDisplay.Hexagon;
using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.VvvfStructs.PulseMode.DiscreteTimeConfiguration;
using static VvvfSimulator.VvvfStructs.PulseMode.PulseHarmonic;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze.YamlTrainSoundData.SoundFilter;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterDipolar;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlMovingValue;

namespace VvvfSimulator.GUI.Resource.Language
{
    public class FriendlyNameConverter
    {
        public static Dictionary<AmplitudeMode, string> GetAmplitudeModeNames()
        {
            Dictionary<AmplitudeMode, string> Names = [];
            foreach (AmplitudeMode type in (AmplitudeMode[])Enum.GetValues(typeof(AmplitudeMode)))
            {
                Names.Add(type, GetAmplitudeModeName(type));
            }
            return Names;
        }
        public static string GetAmplitudeModeName(AmplitudeMode mode)
        {
            return mode switch
            {
                AmplitudeMode.Linear => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.Linear"),
                AmplitudeMode.Wide_3_Pulse => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.Wide3Pulse"),
                AmplitudeMode.Inv_Proportional => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.InvProportional"),
                AmplitudeMode.Exponential => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.Exponential"),
                AmplitudeMode.Linear_Polynomial => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.LinearPolynomial"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.Sine"),
            };
        }

        public static Dictionary<FilterType, string> GetFrequencyFilterTypeNames()
        {
            Dictionary<FilterType, string> Names = [];
            foreach (FilterType type in (FilterType[])Enum.GetValues(typeof(FilterType)))
            {
                Names.Add(type, GetFrequencyFilterTypeName(type));
            }
            return Names;
        }
        public static string GetFrequencyFilterTypeName(FilterType filterType)
        {
            return filterType switch
            {
                FilterType.PeakingEQ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.FrequencyFilter.FilterType.PeakingEQ"),
                FilterType.HighPassFilter => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.FrequencyFilter.FilterType.HighPassFilter"),
                FilterType.LowPassFilter => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.FrequencyFilter.FilterType.LowPassFilter"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.FrequencyFilter.FilterType.NotchFilter")
            };
        }

        public static Dictionary<DeviceMode, string> GetMasconDeviceModeNames()
        {
            Dictionary<DeviceMode, string> Names = [];
            foreach (DeviceMode type in (DeviceMode[])Enum.GetValues(typeof(DeviceMode)))
            {
                Names.Add(type, GetMasconDeviceModeName(type));
            }
            return Names;
        }
        public static string GetMasconDeviceModeName(DeviceMode mode)
        {
            return mode switch
            {
                DeviceMode.KeyBoard => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.MasconDevice.DeviceMode.KeyBoard"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.MasconDevice.DeviceMode.PicoMascon")
            };
        }

        public static Dictionary<RealTimeControlStatStyle, string> GetRealTimeControlStatStyleNames()
        {
            Dictionary<RealTimeControlStatStyle, string> Names = [];
            foreach (RealTimeControlStatStyle type in (RealTimeControlStatStyle[])Enum.GetValues(typeof(RealTimeControlStatStyle)))
            {
                Names.Add(type, GetRealTimeControlStatStyleName(type));
            }
            return Names;
        }
        public static string GetRealTimeControlStatStyleName(RealTimeControlStatStyle style)
        {
            return style switch
            {
                RealTimeControlStatStyle.Original1 => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlStatus.Design.Original1"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlStatus.Design.Original2")
            };
        }

        public static Dictionary<RealTimeHexagonStyle, string> GetRealTimeHexagonStyleNames()
        {
            Dictionary<RealTimeHexagonStyle, string> Names = [];
            foreach (RealTimeHexagonStyle type in (RealTimeHexagonStyle[])Enum.GetValues(typeof(RealTimeHexagonStyle)))
            {
                Names.Add(type, GetRealTimeHexagonStyleName(type));
            }
            return Names;
        }
        public static string GetRealTimeHexagonStyleName(RealTimeHexagonStyle style)
        {
            return style switch
            {
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.VoltageVector.Design.Original")
            };
        }

        public static Dictionary<YamlAsyncParameterDipolarMode, string> GetDipolarModeNames()
        {
            Dictionary<YamlAsyncParameterDipolarMode, string> Names = [];
            foreach (YamlAsyncParameterDipolarMode type in (YamlAsyncParameterDipolarMode[])Enum.GetValues(typeof(YamlAsyncParameterDipolarMode)))
            {
                Names.Add(type, GetDipolarModeName(type));
            }
            return Names;
        }
        public static string GetDipolarModeName(YamlAsyncParameterDipolarMode filterType)
        {
            return filterType switch
            {
                YamlAsyncParameterDipolarMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlDipolar.ParamType.Name.Const"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlDipolar.ParamType.Name.Moving")
            };
        }

        public static Dictionary<MovingValueType, string> GetMovingValueTypeNames()
        {
            Dictionary<MovingValueType, string> Names = [];
            foreach (MovingValueType type in (MovingValueType[])Enum.GetValues(typeof(MovingValueType)))
            {
                Names.Add(type, GetMovingValueTypeName(type));
            }
            return Names;
        }
        public static string GetMovingValueTypeName(MovingValueType Type)
        {
            return Type switch
            {
                MovingValueType.Proportional => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlMovingSetting.MoveType.Name.Proportional"),
                MovingValueType.Inv_Proportional => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlMovingSetting.MoveType.Name.InvProportional"),
                MovingValueType.Pow2_Exponential => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlMovingSetting.MoveType.Name.Pow2Exponential"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlMovingSetting.MoveType.Name.Sine"),
            };
        }

        public static Dictionary<BaseWaveType, string> GetBaseWaveTypeNames()
        {
            Dictionary<BaseWaveType, string> Names = [];
            foreach (BaseWaveType type in (BaseWaveType[])Enum.GetValues(typeof(BaseWaveType)))
            {
                Names.Add(type, GetBaseWaveTypeName(type));
            }
            return Names;
        }
        public static string GetBaseWaveTypeName(BaseWaveType Type)
        {
            return Type switch
            {
                BaseWaveType.Sine => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.BaseWaveType.Name.Sine"),
                BaseWaveType.Saw => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.BaseWaveType.Name.Saw"),
                BaseWaveType.Modified_Sine_1 => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.BaseWaveType.Name.ModifiedSine1"),
                BaseWaveType.Modified_Sine_2 => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.BaseWaveType.Name.ModifiedSine2"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.BaseWaveType.Name.ModifiedSaw1"),
            };
        }

        public static Dictionary<PulseAlternativeMode, string> GetPulseAltModeNames(PulseAlternativeMode[]? Modes)
        {
            Dictionary<PulseAlternativeMode, string> Names = [];
            Modes ??= (PulseAlternativeMode[])Enum.GetValues(typeof(PulseAlternativeMode));
            foreach (PulseAlternativeMode type in Modes)
            {
                Names.Add(type, GetPulseAltModeName(type));
            }
            return Names;
        }
        public static string GetPulseAltModeName(PulseAlternativeMode Mode)
        {
            return Mode switch
            {
                PulseAlternativeMode.Default => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseAlternativeMode.Name.Default"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseAlternativeMode.Name.Alt") + ((int)Mode),
            };
        }

        public static Dictionary<PulseModeName, string> GetPulseModeNames(PulseModeName[]? Modes)
        {
            Dictionary<PulseModeName, string> Names = [];
            Modes ??= (PulseModeName[])Enum.GetValues(typeof(PulseModeName));
            foreach (PulseModeName type in Modes)
            {
                Names.Add(type, GetPulseModeName(type));
            }
            return Names;
        }
        public static string GetPulseModeName(PulseModeName Name)
        {
            if (Name.Equals(PulseModeName.Async)) return LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Async");

            // Common Conversion
            string ConvertedName = "";

            string NameStr = Name.ToString();
            string[] NameParts = NameStr.Split("_");
            string SpecialPulseName = NameParts[0][..NameParts[0].LastIndexOf('P')];
            string PulseCount = NameParts[^1];

            ConvertedName += SpecialPulseName;
            ConvertedName += " ";

            for (int i = 1; i < NameParts.Length - 1; i++)
            {
                string LocalizedSpecificName = LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name." +  NameParts[i]);
                ConvertedName += LocalizedSpecificName;
            }
            
            ConvertedName += PulseCount;
            ConvertedName += LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Pulse");

            return ConvertedName;
        }

        public static Dictionary<PulseHarmonicType, string> GetPulseHarmonicTypeNames()
        {
            Dictionary<PulseHarmonicType, string> Names = [];
            foreach (PulseHarmonicType type in (PulseHarmonicType[])Enum.GetValues(typeof(PulseHarmonicType)))
            {
                Names.Add(type, GetPulseHarmonicTypeName(type));
            }
            return Names;
        }
        public static string GetPulseHarmonicTypeName(PulseHarmonicType Type)
        {
            return Type switch
            {
                PulseHarmonicType.Sine => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseHarmonicType.Name.Sine"),
                PulseHarmonicType.Saw => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseHarmonicType.Name.Saw"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseHarmonicType.Name.Square"),
            };
        }

        public static Dictionary<DiscreteTimeMode, string> GetDiscreteTimeModeNames()
        {
            Dictionary<DiscreteTimeMode, string> Names = [];
            foreach (DiscreteTimeMode type in (DiscreteTimeMode[])Enum.GetValues(typeof(DiscreteTimeMode)))
            {
                Names.Add(type, GetDiscreteTimeModeName(type));
            }
            return Names;
        }
        public static string GetDiscreteTimeModeName(DiscreteTimeMode Mode)
        {
            return Mode switch
            {
                DiscreteTimeMode.Left => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.DiscreteTimeMode.Name.Left"),
                DiscreteTimeMode.Middle => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.DiscreteTimeMode.Name.Middle"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.DiscreteTimeMode.Name.Right"),
            };
        }

        public static Dictionary<YamlAsyncParameterRandomValueMode, string> GetYamlAsyncParameterRandomValueModeNames()
        {
            Dictionary<YamlAsyncParameterRandomValueMode, string> Names = [];
            foreach (YamlAsyncParameterRandomValueMode type in (YamlAsyncParameterRandomValueMode[])Enum.GetValues(typeof(YamlAsyncParameterRandomValueMode)))
            {
                Names.Add(type, GetYamlAsyncParameterRandomValueModeName(type));
            }
            return Names;
        }
        public static string GetYamlAsyncParameterRandomValueModeName(YamlAsyncParameterRandomValueMode Mode)
        {
            return Mode switch
            {
                YamlAsyncParameterRandomValueMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterRandomValueMode.Name.Const"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterRandomValueMode.Name.Moving"),
            };
        }

        public static Dictionary<YamlAsyncCarrierMode, string> GetYamlAsyncCarrierModeName()
        {
            Dictionary<YamlAsyncCarrierMode, string> Names = [];
            foreach (YamlAsyncCarrierMode type in (YamlAsyncCarrierMode[])Enum.GetValues(typeof(YamlAsyncCarrierMode)))
            {
                Names.Add(type, GetYamlAsyncCarrierModeName(type));
            }
            return Names;
        }
        public static string GetYamlAsyncCarrierModeName(YamlAsyncCarrierMode Mode)
        {
            return Mode switch
            {
                YamlAsyncCarrierMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Const"),
                YamlAsyncCarrierMode.Moving => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Moving"),
                YamlAsyncCarrierMode.Vibrato => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Vibrato"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Table"),
            };
        }

        public static Dictionary<YamlAsyncParameterVibratoMode, string> GetYamlAsyncParameterVibratoModeNames()
        {
            Dictionary<YamlAsyncParameterVibratoMode, string> Names = [];
            foreach (YamlAsyncParameterVibratoMode type in (YamlAsyncParameterVibratoMode[])Enum.GetValues(typeof(YamlAsyncParameterVibratoMode)))
            {
                Names.Add(type, GetYamlAsyncParameterVibratoModeName(type));
            }
            return Names;
        }
        public static string GetYamlAsyncParameterVibratoModeName(YamlAsyncParameterVibratoMode Mode)
        {
            return Mode switch
            {
                YamlAsyncParameterVibratoMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterVibratoMode.Name.Const"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterVibratoMode.Name.Moving"),
            };
        }
    }
}
