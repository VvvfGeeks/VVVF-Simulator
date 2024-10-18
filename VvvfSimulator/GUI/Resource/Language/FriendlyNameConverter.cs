using System;
using System.Collections.Generic;
using VvvfSimulator.GUI.Resource.Theme;
using VvvfSimulator.GUI.Simulator.RealTime.Setting;
using static VvvfSimulator.GUI.Simulator.RealTime.RealtimeDisplay.ControlStatus;
using static VvvfSimulator.GUI.Simulator.RealTime.RealtimeDisplay.Hexagon;
using static VvvfSimulator.Vvvf.Calculate;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze.YamlTrainSoundData.SoundFilter;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsync.CarrierFrequency;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsync.CarrierFrequency.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsync.RandomModulation.YamlAsyncParameterRandomValue;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlMovingValue;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode.DiscreteTimeConfiguration;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode.PulseDataValue;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode.PulseHarmonic;

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

        public static Dictionary<PulseDataValueMode, string> GetPulseDataValueModeNames()
        {
            Dictionary<PulseDataValueMode, string> Names = [];
            foreach (PulseDataValueMode type in (PulseDataValueMode[])Enum.GetValues(typeof(PulseDataValueMode)))
            {
                Names.Add(type, GetPulseDataValueModeName(type));
            }
            return Names;
        }
        public static string GetPulseDataValueModeName(PulseDataValueMode Mode)
        {
            return Mode switch
            {
                PulseDataValueMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseDataSetting.Value.Mode.Name.Const"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseDataSetting.Value.Mode.Name.Moving")
            };
        }

        public static Dictionary<PulseDataKey, string> GetPulseDataKeyNames()
        {
            Dictionary<PulseDataKey, string> Names = [];
            foreach (PulseDataKey type in (PulseDataKey[])Enum.GetValues(typeof(PulseDataKey)))
            {
                Names.Add(type, GetPulseDataKeyName(type));
            }
            return Names;
        }
        public static string GetPulseDataKeyName(PulseDataKey Key)
        {
            return LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseDataSetting.Key.Name." + Key.ToString());
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
                BaseWaveType.ModifiedSine1 => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.BaseWaveType.Name.ModifiedSine1"),
                BaseWaveType.ModifiedSine2 => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.BaseWaveType.Name.ModifiedSine2"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.BaseWaveType.Name.ModifiedSaw1"),
            };
        }

        public static Dictionary<PulseAlternative, string> GetPulseAltModeNames(PulseAlternative[]? Modes)
        {
            Dictionary<PulseAlternative, string> Names = [];
            Modes ??= (PulseAlternative[])Enum.GetValues(typeof(PulseAlternative));
            foreach (PulseAlternative type in Modes)
            {
                Names.Add(type, GetPulseAltModeName(type));
            }
            return Names;
        }
        public static string GetPulseAltModeName(PulseAlternative Mode)
        {
            return Mode switch
            {
                PulseAlternative.Default => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseAlternative.Name.Default"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseAlternative.Name.Alt") + ((int)Mode),
            };
        }

        public static Dictionary<PulseTypeName, string> GetPulseTypeNames(PulseTypeName[]? Modes)
        {
            Dictionary<PulseTypeName, string> Names = [];
            Modes ??= (PulseTypeName[])Enum.GetValues(typeof(PulseTypeName));
            foreach (PulseTypeName type in Modes)
            {
                Names.Add(type, GetPulseTypeName(type));
            }
            return Names;
        }
        public static string GetPulseTypeName(PulseTypeName Name)
        {
            return Name switch
            {
                PulseTypeName.ASYNC => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Async"),
                PulseTypeName.CHM => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Chm"),
                PulseTypeName.SHE => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.She"),
                PulseTypeName.HO => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Ho"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Sync"),
            };
        }

        public static Dictionary<YamlPulseMode, string> GetPulseModeNames(YamlPulseMode[] Modes)
        {
            Dictionary<YamlPulseMode, string> Names = [];
            foreach (YamlPulseMode type in Modes)
            {
                Names.Add(type, GetPulseModeName(type));
            }
            return Names;
        }

        public static string GetPulseModeName(YamlPulseMode Mode)
        {
            return Mode.PulseType switch
            {
                PulseTypeName.ASYNC => GetPulseTypeName(Mode.PulseType),
                _ => GetPulseTypeName(Mode.PulseType) + " " + Mode.PulseCount + " " + LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Pulse"),
            };
        }

        public static Dictionary<int, string> GetPulseCountNames(int[] Modes)
        {
            Dictionary<int, string> Names = [];
            foreach (int type in Modes)
            {
                Names.Add(type, GetPulseCountName(type));
            }
            return Names;
        }

        public static string GetPulseCountName(int PulseCount)
        {
            return PulseCount + " " + LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Pulse");
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

        public static Dictionary<CarrierFrequencyValueMode, string> GetYamlAsyncCarrierModeName()
        {
            Dictionary<CarrierFrequencyValueMode, string> Names = [];
            foreach (CarrierFrequencyValueMode type in (CarrierFrequencyValueMode[])Enum.GetValues(typeof(CarrierFrequencyValueMode)))
            {
                Names.Add(type, GetYamlAsyncCarrierModeName(type));
            }
            return Names;
        }
        public static string GetYamlAsyncCarrierModeName(CarrierFrequencyValueMode Mode)
        {
            return Mode switch
            {
                CarrierFrequencyValueMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Const"),
                CarrierFrequencyValueMode.Moving => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Moving"),
                CarrierFrequencyValueMode.Vibrato => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Vibrato"),
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

        public static Dictionary<Language, string> GetLanguageNames()
        {
            Dictionary<Language, string> Names = [];
            foreach (Language type in (Language[])Enum.GetValues(typeof(Language)))
            {
                Names.Add(type, GetLanguageName(type));
            }
            return Names;
        }
        public static string GetLanguageName(Language language)
        {
            return LanguageManager.GetString("Resource.Language.FriendlyNameConverter.Language.Name." + language.ToString());
        }

        public static Dictionary<ColorTheme, string> GetColorThemeNames()
        {
            Dictionary<ColorTheme, string> Names = [];
            foreach (ColorTheme type in (ColorTheme[])Enum.GetValues(typeof(ColorTheme)))
            {
                Names.Add(type, GetColorThemeName(type));
            }
            return Names;
        }
        public static string GetColorThemeName(ColorTheme colorTheme) {
            return colorTheme switch
            {
                ColorTheme.Light => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ColorTheme.Name.Light"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ColorTheme.Name.Dark"),
            };
        }
    }
}
