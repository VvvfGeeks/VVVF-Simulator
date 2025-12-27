using System;
using System.Collections.Generic;

namespace VvvfSimulator.GUI.Resource.Language
{
    public class FriendlyNameConverter
    {
        public static Dictionary<Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode, string> GetAmplitudeModeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode>())
            {
                Names.Add(type, GetAmplitudeModeName(type));
            }
            return Names;
        }
        public static string GetAmplitudeModeName(Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode mode)
        {
            return mode switch
            {
                Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode.Linear => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.Linear"),
                Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode.InverseProportional => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.InverseProportional"),
                Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode.Exponential => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.Exponential"),
                Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode.LinearPolynomial => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.LinearPolynomial"),
                Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode.Sine => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.Sine"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.AmplitudeMode.Name.Table"),
            };
        }

        public static Dictionary<Data.TrainAudio.Struct.SoundFilter.FilterType, string> GetFrequencyFilterTypeNames()
        {
            Dictionary<Data.TrainAudio.Struct.SoundFilter.FilterType, string> Names = [];
            foreach (Data.TrainAudio.Struct.SoundFilter.FilterType type in Enum.GetValues<Data.TrainAudio.Struct.SoundFilter.FilterType>())
            {
                Names.Add(type, GetFrequencyFilterTypeName(type));
            }
            return Names;
        }
        public static string GetFrequencyFilterTypeName(Data.TrainAudio.Struct.SoundFilter.FilterType filterType)
        {
            return filterType switch
            {
                Data.TrainAudio.Struct.SoundFilter.FilterType.PeakingEQ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.FrequencyFilter.FilterType.PeakingEQ"),
                Data.TrainAudio.Struct.SoundFilter.FilterType.HighPassFilter => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.FrequencyFilter.FilterType.HighPassFilter"),
                Data.TrainAudio.Struct.SoundFilter.FilterType.LowPassFilter => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.FrequencyFilter.FilterType.LowPassFilter"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.FrequencyFilter.FilterType.NotchFilter")
            };
        }

        public static Dictionary<Simulator.RealTime.Setting.DeviceMode, string> GetMasconDeviceModeNames()
        {
            Dictionary<Simulator.RealTime.Setting.DeviceMode, string> Names = [];
            foreach (Simulator.RealTime.Setting.DeviceMode type in Enum.GetValues<Simulator.RealTime.Setting.DeviceMode>())
            {
                Names.Add(type, GetMasconDeviceModeName(type));
            }
            return Names;
        }
        public static string GetMasconDeviceModeName(Simulator.RealTime.Setting.DeviceMode mode)
        {
            return mode switch
            {
                Simulator.RealTime.Setting.DeviceMode.KeyBoard => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.MasconDevice.DeviceMode.KeyBoard"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.MasconDevice.DeviceMode.PicoMascon")
            };
        }
        public static Dictionary<Simulator.RealTime.Controller.ControllerStyle, string> GetRealTimeControllerStyleNames()
        {
            Dictionary<Simulator.RealTime.Controller.ControllerStyle, string> Names = [];
            foreach (Simulator.RealTime.Controller.ControllerStyle type in Enum.GetValues<Simulator.RealTime.Controller.ControllerStyle>())
            {
                Names.Add(type, GetRealTimeControllerStyleName(type));
            }
            return Names;
        }
        public static string GetRealTimeControllerStyleName(Simulator.RealTime.Controller.ControllerStyle design)
        {
            return LanguageManager.GetString("Resource.Language.FriendlyNameConverter.RealTime.Controller." + design.ToString());
        }
        public static Dictionary<Simulator.RealTime.RealtimeDisplay.ControlStatus.RealTimeControlStatStyle, string> GetRealTimeControlStatStyleNames()
        {
            Dictionary<Simulator.RealTime.RealtimeDisplay.ControlStatus.RealTimeControlStatStyle, string> Names = [];
            foreach (Simulator.RealTime.RealtimeDisplay.ControlStatus.RealTimeControlStatStyle type in Enum.GetValues<Simulator.RealTime.RealtimeDisplay.ControlStatus.RealTimeControlStatStyle>())
            {
                Names.Add(type, GetRealTimeControlStatStyleName(type));
            }
            return Names;
        }
        public static string GetRealTimeControlStatStyleName(Simulator.RealTime.RealtimeDisplay.ControlStatus.RealTimeControlStatStyle style)
        {
            return style switch
            {
                Simulator.RealTime.RealtimeDisplay.ControlStatus.RealTimeControlStatStyle.Original1 => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlStatus.Design.Original1"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlStatus.Design.Original2")
            };
        }

        public static Dictionary<Simulator.RealTime.RealtimeDisplay.Hexagon.RealTimeHexagonStyle, string> GetRealTimeHexagonStyleNames()
        {
            Dictionary<Simulator.RealTime.RealtimeDisplay.Hexagon.RealTimeHexagonStyle, string> Names = [];
            foreach (Simulator.RealTime.RealtimeDisplay.Hexagon.RealTimeHexagonStyle type in Enum.GetValues<Simulator.RealTime.RealtimeDisplay.Hexagon.RealTimeHexagonStyle>())
            {
                Names.Add(type, GetRealTimeHexagonStyleName(type));
            }
            return Names;
        }
        public static string GetRealTimeHexagonStyleName(Simulator.RealTime.RealtimeDisplay.Hexagon.RealTimeHexagonStyle style)
        {
            return style switch
            {
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.VoltageVector.Design.Original")
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseDataValue.PulseDataValueMode, string> GetPulseDataValueModeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseDataValue.PulseDataValueMode, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.Pulse.PulseDataValue.PulseDataValueMode type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.Pulse.PulseDataValue.PulseDataValueMode>())
            {
                Names.Add(type, GetPulseDataValueModeName(type));
            }
            return Names;
        }
        public static string GetPulseDataValueModeName(Data.Vvvf.Struct.PulseControl.Pulse.PulseDataValue.PulseDataValueMode Mode)
        {
            return Mode switch
            {
                Data.Vvvf.Struct.PulseControl.Pulse.PulseDataValue.PulseDataValueMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseDataSetting.Value.Mode.Name.Const"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseDataSetting.Value.Mode.Name.Moving")
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseDataKey, string> GetPulseDataKeyNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseDataKey, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.Pulse.PulseDataKey type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.Pulse.PulseDataKey>())
            {
                Names.Add(type, GetPulseDataKeyName(type));
            }
            return Names;
        }
        public static string GetPulseDataKeyName(Data.Vvvf.Struct.PulseControl.Pulse.PulseDataKey Key)
        {
            return LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseDataSetting.Key.Name." + Key.ToString());
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.FunctionValue.FunctionType, string> GetMovingValueTypeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.FunctionValue.FunctionType, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.FunctionValue.FunctionType type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.FunctionValue.FunctionType>())
            {
                Names.Add(type, GetMovingValueTypeName(type));
            }
            return Names;
        }
        public static string GetMovingValueTypeName(Data.Vvvf.Struct.PulseControl.FunctionValue.FunctionType Type)
        {
            return Type switch
            {
                Data.Vvvf.Struct.PulseControl.FunctionValue.FunctionType.Proportional => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlMovingSetting.MoveType.Name.Proportional"),
                Data.Vvvf.Struct.PulseControl.FunctionValue.FunctionType.Inv_Proportional => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlMovingSetting.MoveType.Name.InvProportional"),
                Data.Vvvf.Struct.PulseControl.FunctionValue.FunctionType.Pow2_Exponential => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlMovingSetting.MoveType.Name.Pow2Exponential"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ControlMovingSetting.MoveType.Name.Sine"),
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse, string> GetPulseModeNames(Data.Vvvf.Struct.PulseControl.Pulse[] Modes)
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.Pulse type in Modes)
            {
                Names.Add(type, GetPulseModeName(type));
            }
            return Names;
        }
        public static string GetPulseModeName(Data.Vvvf.Struct.PulseControl.Pulse Mode)
        {
            return Mode.PulseType switch
            {
                Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName.ASYNC => GetPulseTypeName(Mode.PulseType),
                Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName.ΔΣ => GetPulseTypeName(Mode.PulseType),
                _ => GetPulseTypeName(Mode.PulseType) + " " + Mode.PulseCount + " " + LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Pulse"),
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName, string> GetPulseTypeNames(Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName[]? Modes = null)
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName, string> Names = [];
            Modes ??= Enum.GetValues<Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName>();
            foreach (Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName type in Modes)
            {
                Names.Add(type, GetPulseTypeName(type));
            }
            return Names;
        }
        public static string GetPulseTypeName(Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName Name)
        {
            return Name switch
            {
                Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName.ASYNC => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Async"),
                Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName.CHM => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Chm"),
                Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName.SHE => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.She"),
                Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName.HO => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Ho"),
                Data.Vvvf.Struct.PulseControl.Pulse.PulseTypeName.ΔΣ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.DeltaSigma"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseModeName.Name.Sync"),
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

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative, string> GetPulseAltModeNames(Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative[]? Modes = null)
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative, string> Names = [];
            Modes ??= Enum.GetValues<Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative>();
            foreach (Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative type in Modes)
            {
                Names.Add(type, GetPulseAltModeName(type));
            }
            return Names;
        }
        public static string GetPulseAltModeName(Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative Mode)
        {
            return Mode switch
            {
                Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative.Default => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseAlternative.Name.Default"),
                Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative.CP => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseAlternative.Name.CP"),
                Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative.Square => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseAlternative.Name.Square"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseAlternative.Name.Alt") + ((int)Mode - (int)Data.Vvvf.Struct.PulseControl.Pulse.PulseAlternative.Alt1 + 1),
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.BaseWaveType, string> GetBaseWaveTypeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.BaseWaveType, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.Pulse.BaseWaveType type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.Pulse.BaseWaveType>())
            {
                Names.Add(type, GetBaseWaveTypeName(type));
            }
            return Names;
        }
        public static string GetBaseWaveTypeName(Data.Vvvf.Struct.PulseControl.Pulse.BaseWaveType Type)
        {
            return LanguageManager.GetString("Resource.Language.FriendlyNameConverter.BaseWaveType.Name." + Type.ToString());
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveType, string> GetCarrierWaveTypeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveType, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveType type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveType>())
            {
                Names.Add(type, GetCarrierWaveTypeName(type));
            }
            return Names;
        }
        public static string GetCarrierWaveTypeName(Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveType Type)
        {
            return LanguageManager.GetString("Resource.Language.FriendlyNameConverter.CarrierWave.Type.Name." + Type.ToString());
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveOption, string> GetCarrierWaveOptionNames(Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveOption[]? Options = null)
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveOption, string> Names = [];
            Options ??= Enum.GetValues<Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveOption>();
            foreach (Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveOption Option in Options)
            {
                Names.Add(Option, GetCarrierWaveOptionName(Option));
            }
            return Names;
        }
        public static string GetCarrierWaveOptionName(Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration.CarrierWaveOption Option)
        {
            return LanguageManager.GetString("Resource.Language.FriendlyNameConverter.CarrierWave.Option.Name." + Option.ToString());
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseHarmonic.PulseHarmonicType, string> GetPulseHarmonicTypeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.PulseHarmonic.PulseHarmonicType, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.Pulse.PulseHarmonic.PulseHarmonicType type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.Pulse.PulseHarmonic.PulseHarmonicType>())
            {
                Names.Add(type, GetPulseHarmonicTypeName(type));
            }
            return Names;
        }
        public static string GetPulseHarmonicTypeName(Data.Vvvf.Struct.PulseControl.Pulse.PulseHarmonic.PulseHarmonicType Type)
        {
            return Type switch
            {
                Data.Vvvf.Struct.PulseControl.Pulse.PulseHarmonic.PulseHarmonicType.Sine => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseHarmonicType.Name.Sine"),
                Data.Vvvf.Struct.PulseControl.Pulse.PulseHarmonic.PulseHarmonicType.Saw => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseHarmonicType.Name.Saw"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.PulseHarmonicType.Name.Square"),
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.DiscreteTimeConfiguration.DiscreteTimeMode, string> GetDiscreteTimeModeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.Pulse.DiscreteTimeConfiguration.DiscreteTimeMode, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.Pulse.DiscreteTimeConfiguration.DiscreteTimeMode type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.Pulse.DiscreteTimeConfiguration.DiscreteTimeMode>())
            {
                Names.Add(type, GetDiscreteTimeModeName(type));
            }
            return Names;
        }
        public static string GetDiscreteTimeModeName(Data.Vvvf.Struct.PulseControl.Pulse.DiscreteTimeConfiguration.DiscreteTimeMode Mode)
        {
            return Mode switch
            {
                Data.Vvvf.Struct.PulseControl.Pulse.DiscreteTimeConfiguration.DiscreteTimeMode.Left => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.DiscreteTimeMode.Name.Left"),
                Data.Vvvf.Struct.PulseControl.Pulse.DiscreteTimeConfiguration.DiscreteTimeMode.Middle => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.DiscreteTimeMode.Name.Middle"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.DiscreteTimeMode.Name.Right"),
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.AsyncControl.RandomModulation.Parameter.ValueMode, string> GetYamlAsyncParameterRandomValueModeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.AsyncControl.RandomModulation.Parameter.ValueMode, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.AsyncControl.RandomModulation.Parameter.ValueMode type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.AsyncControl.RandomModulation.Parameter.ValueMode>())
            {
                Names.Add(type, GetYamlAsyncParameterRandomValueModeName(type));
            }
            return Names;
        }
        public static string GetYamlAsyncParameterRandomValueModeName(Data.Vvvf.Struct.PulseControl.AsyncControl.RandomModulation.Parameter.ValueMode Mode)
        {
            return Mode switch
            {
                Data.Vvvf.Struct.PulseControl.AsyncControl.RandomModulation.Parameter.ValueMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterRandomValueMode.Name.Const"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterRandomValueMode.Name.Moving"),
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.ValueMode, string> GetYamlAsyncCarrierModeName()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.ValueMode, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.ValueMode type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.ValueMode>())
            {
                Names.Add(type, GetYamlAsyncCarrierModeName(type));
            }
            return Names;
        }
        public static string GetYamlAsyncCarrierModeName(Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.ValueMode Mode)
        {
            return Mode switch
            {
                Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.ValueMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Const"),
                Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.ValueMode.Moving => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Moving"),
                Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.ValueMode.Vibrato => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Vibrato"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncCarrierMode.Name.Table"),
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.Parameter.ValueMode, string> GetYamlAsyncParameterVibratoModeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.Parameter.ValueMode, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.Parameter.ValueMode type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.Parameter.ValueMode>())
            {
                Names.Add(type, GetYamlAsyncParameterVibratoModeName(type));
            }
            return Names;
        }
        public static string GetYamlAsyncParameterVibratoModeName(Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.Parameter.ValueMode Mode)
        {
            return Mode switch
            {
                Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.Parameter.ValueMode.Const => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterVibratoMode.Name.Const"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterVibratoMode.Name.Moving"),
            };
        }

        public static Dictionary<Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType, string> GetYamlAsyncParameterVibratoBaseWaveTypeNames()
        {
            Dictionary<Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType, string> Names = [];
            foreach (Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType type in Enum.GetValues<Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType>())
            {
                Names.Add(type, GetYamlAsyncParameterVibratoBaseWaveTypeName(type));
            }
            return Names;
        }
        public static string GetYamlAsyncParameterVibratoBaseWaveTypeName(Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType Type)
        {
            return Type switch
            {
                Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType.Sine => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterVibratoBaseWaveType.Name.Sine"),
                Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType.Triangle => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterVibratoBaseWaveType.Name.Triangle"),
                Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType.Square => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterVibratoBaseWaveType.Name.Square"),
                Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType.SawUp => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterVibratoBaseWaveType.Name.SawUp"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.YamlAsyncParameterVibratoBaseWaveType.Name.SawDown"),
            };
        }

        public static Dictionary<Vvvf.MyMath.EquationSolver.EquationSolverType, string> GetEquationSolverTypeNames()
        {
            Dictionary<Vvvf.MyMath.EquationSolver.EquationSolverType, string> Names = [];
            foreach (Vvvf.MyMath.EquationSolver.EquationSolverType type in Enum.GetValues<Vvvf.MyMath.EquationSolver.EquationSolverType>())
            {
                Names.Add(type, GetEquationSolverTypeName(type));
            }
            return Names;
        }
        public static string GetEquationSolverTypeName(Vvvf.MyMath.EquationSolver.EquationSolverType Mode)
        {
            return Mode switch
            {
                Vvvf.MyMath.EquationSolver.EquationSolverType.Newton => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.MyMath.EquationSolver.EquationSolverType.Name.Newton"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.MyMath.EquationSolver.EquationSolverType.Name.Bisection")
            };
        }

        public static Dictionary<bool, string> GetBoolNames()
        {
            Dictionary<bool, string> Names = [];
            Names.Add(false, GetBoolName(false));
            Names.Add(true, GetBoolName(true));
            return Names;
        }
        public static string GetBoolName(bool b)
        {
            return b switch
            {
                true => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.Bool.True"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.Bool.False"),
            };
        }

        public static Dictionary<Language, string> GetLanguageNames()
        {
            Dictionary<Language, string> Names = [];
            foreach (Language type in Enum.GetValues<Language>())
            {
                Names.Add(type, GetLanguageName(type));
            }
            return Names;
        }
        public static string GetLanguageName(Language language)
        {
            return LanguageManager.GetString("Resource.Language.FriendlyNameConverter.Language.Name." + language.ToString());
        }

        public static Dictionary<Theme.ColorTheme, string> GetColorThemeNames()
        {
            Dictionary<Theme.ColorTheme, string> Names = [];
            foreach (Theme.ColorTheme type in Enum.GetValues<Theme.ColorTheme>())
            {
                Names.Add(type, GetColorThemeName(type));
            }
            return Names;
        }
        public static string GetColorThemeName(Theme.ColorTheme colorTheme)
        {
            return colorTheme switch
            {
                Theme.ColorTheme.Light => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ColorTheme.Name.Light"),
                _ => LanguageManager.GetString("Resource.Language.FriendlyNameConverter.ColorTheme.Name.Dark"),
            };
        }
    }
}
