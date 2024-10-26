using System;
using System.Collections.Generic;
using VvvfSimulator.Vvvf;
using static VvvfSimulator.Vvvf.Calculate;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAmplitude;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsync.CarrierFrequency.YamlAsyncParameterCarrierFreqTable;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsync.RandomModulation.YamlAsyncParameterRandomValue;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlMasconData;

namespace VvvfSimulator.Yaml.VvvfSound
{
    public class YamlVvvfWave
    {
        public static double GetChangingValue(double x1, double y1, double x2, double y2, double x)
        {
            return y1 + (y2 - y1) / (x2 - x1) * (x - x1);
        }
        private static double GetMovingValue(YamlMovingValue Data, double Current)
        {
            double val = 1000;
            if (Data.Type == YamlMovingValue.MovingValueType.Proportional)
                val = GetChangingValue(
                    Data.Start,
                    Data.StartValue,
                    Data.End,
                    Data.EndValue,
                    Current
                );
            else if (Data.Type == YamlMovingValue.MovingValueType.Pow2_Exponential)
                val = (Math.Pow(2, Math.Pow((Current - Data.Start) / (Data.End - Data.Start), Data.Degree)) - 1) * (Data.EndValue - Data.StartValue) + Data.StartValue;
            else if (Data.Type == YamlMovingValue.MovingValueType.Inv_Proportional)
            {
                double x = GetChangingValue(
                    Data.Start,
                    1 / Data.StartValue,
                    Data.End,
                    1 / Data.EndValue,
                    Current
                );

                double c = -Data.CurveRate;
                double k = Data.EndValue;
                double l = Data.StartValue;
                double a = 1 / ((1 / l) - (1 / k)) * (1 / (l - c) - 1 / (k - c));
                double b = 1 / (1 - (1 / l) * k) * (1 / (l - c) - (1 / l) * k / (k - c));

                val = 1 / (a * x + b) + c;
            }
            else if (Data.Type == YamlMovingValue.MovingValueType.Sine)
            {
                double x = (MyMath.M_PI_2 - Math.Asin(Data.StartValue / Data.EndValue)) / (Data.End - Data.Start) * (Current - Data.Start) + Math.Asin(Data.StartValue / Data.EndValue);
                val = Math.Sin(x) * Data.EndValue;
            }

            return val;

        }

        public static bool IsMatching(VvvfValues Control, YamlControlData ysd)
        {
            bool enable_free_run_condition = Control.IsFreeRun() && ((!Control.IsMasconOff() && ysd.EnableFreeRunOn) || (Control.IsMasconOff() && ysd.EnableFreeRunOff));
            bool enable_normal_condition = ysd.EnableNormal && !Control.IsFreeRun();
            if (!(enable_free_run_condition || enable_normal_condition)) return false;

            bool Condition1 = ysd.ControlFrequencyFrom <= Control.GetControlFrequency();
            bool Condition2 = ysd.RotateFrequencyFrom == -1 || ysd.RotateFrequencyFrom <= Control.GetSineFrequency();
            bool Condition3 = ysd.RotateFrequencyBelow == -1 || ysd.RotateFrequencyBelow > Control.GetSineFrequency();

            if (!Condition2) return false;
            if (!Condition3) return false;

            if (!Control.IsFreeRun() && Condition1) return true;
            if (!Control.IsFreeRun() && !Condition1) return false;

            if (Condition1) return true;

            if (
                (ysd.StuckFreeRunOn && Control.IsFreeRun() && !Control.IsMasconOff()) ||
                (ysd.StuckFreeRunOff && Control.IsFreeRun() && Control.IsMasconOff())
            )
            {
                if (Control.GetSineFrequency() > ysd.ControlFrequencyFrom) return true;
                return false;
            }

            return false;

        }
        public static PwmCalculateValues CalculateYaml(VvvfValues Control, YamlVvvfSoundData yvs)
        {
            YamlPulseMode pulse_mode;
            CarrierFreq carrier_freq = new(0, 0, 0.0005);
            double amplitude = 0;

            //
            // mascon off solve
            //
            double max_voltage_freq;
            YamlMasconDataPattern BaseFrequencyInfo;
            if (Control.IsBraking()) BaseFrequencyInfo = yvs.MasconData.Braking;
            else BaseFrequencyInfo = yvs.MasconData.Accelerating;

            if (!Control.IsMasconOff())
            {
                Control.SetFreeFrequencyChange(BaseFrequencyInfo.On.FrequencyChangeRate);
                max_voltage_freq = BaseFrequencyInfo.On.MaxControlFrequency;
                if (Control.IsFreeRun())
                {
                    if (Control.GetControlFrequency() > max_voltage_freq)
                    {
                        double rolling_freq = Control.GetSineFrequency();
                        Control.SetControlFrequency(rolling_freq);
                    }
                }
            }
            else
            {
                Control.SetFreeFrequencyChange(BaseFrequencyInfo.Off.FrequencyChangeRate);
                max_voltage_freq = BaseFrequencyInfo.Off.MaxControlFrequency;
                if (Control.IsFreeRun())
                {
                    if (Control.GetControlFrequency() > max_voltage_freq)
                    {
                        Control.SetControlFrequency(max_voltage_freq);
                    }
                }
            }

            //if (cv.free_run)
            //{
            //	if (cv.mascon_on)
            //	{
            //		double x_ref = control.get_Sine_Freq() > max_voltage_freq ? max_voltage_freq : control.get_Sine_Freq();
            //                 double x = cv.wave_stat / x_ref;
            //		double k;
            //		if(x < 0.3) k = Math.Pow(0.3, 0.18) / 0.3 * x;
            //		else k = Math.Pow(x, 0.18);
            //                 cv.wave_stat = x_ref * k;
            //             }
            //}
            //
            // control stat solve
            //
            List<YamlControlData> control_list = new(Control.IsBraking() ? yvs.BrakingPattern : yvs.AcceleratePattern);
            control_list.Sort((a, b) => b.ControlFrequencyFrom.CompareTo(a.ControlFrequencyFrom));

            //determine what control data to solve
            int solve = -1;
            for (int x = 0; x < control_list.Count; x++)
            {
                YamlControlData ysd = control_list[x];
                bool match = IsMatching(Control, ysd);
                if (match)
                {
                    solve = x;
                    break;
                }

            }

            if (solve == -1)
            {
                if (Control.IsFreeRun())
                {
                    if (Control.IsMasconOff())
                    {
                        Control.SetControlFrequency(0);
                        return new PwmCalculateValues() { None = true };
                    }
                    else
                    {
                        Control.SetControlFrequency(Control.GetSineFrequency());
                        return new PwmCalculateValues() { None = true };
                    }
                }
                else
                    return new PwmCalculateValues() { None = true };
            }

            //
            // min sine freq solve
            //
            double minimum_sine_freq, original_wave_stat = Control.GetControlFrequency();
            if (Control.IsBraking()) minimum_sine_freq = yvs.MinimumFrequency.Braking;
            else minimum_sine_freq = yvs.MinimumFrequency.Accelerating;
            if (0 < Control.GetControlFrequency() && Control.GetControlFrequency() < minimum_sine_freq && !Control.IsFreeRun()) Control.SetControlFrequency(minimum_sine_freq);

            YamlControlData SolvePattern = control_list[solve];
            pulse_mode = SolvePattern.PulseMode;

            if (pulse_mode.PulseType == YamlPulseMode.PulseTypeName.ASYNC)
            {
                var async_data = SolvePattern.AsyncModulationData;

                //
                //carrier freq solve
                //
                var carrier_data = async_data.CarrierWaveData;
                var carrier_freq_mode = carrier_data.Mode;
                double carrier_freq_val = 100;
                if (carrier_freq_mode == YamlAsync.CarrierFrequency.CarrierFrequencyValueMode.Const)
                    carrier_freq_val = carrier_data.Constant;
                else if (carrier_freq_mode == YamlAsync.CarrierFrequency.CarrierFrequencyValueMode.Moving)
                    carrier_freq_val = GetMovingValue(carrier_data.MovingValue, original_wave_stat);
                else if (carrier_freq_mode == YamlAsync.CarrierFrequency.CarrierFrequencyValueMode.Table)
                {
                    var table_data = carrier_data.CarrierFrequencyTable;

                    //Solve from high.
                    List<YamlAsyncParameterCarrierFreqTableValue> async_carrier_freq_table = new(table_data.CarrierFrequencyTableValues);
                    async_carrier_freq_table.Sort((a, b) => Math.Sign(b.ControlFrequencyFrom - a.ControlFrequencyFrom));

                    for (int i = 0; i < async_carrier_freq_table.Count; i++)
                    {
                        var carrier = async_carrier_freq_table[i];
                        bool condition_1 = carrier.FreeRunStuckAtHere && (Control.GetSineFrequency() < carrier.ControlFrequencyFrom) && Control.IsFreeRun();
                        bool condition_2 = original_wave_stat > carrier.ControlFrequencyFrom;
                        if (!condition_1 && !condition_2) continue;

                        carrier_freq_val = carrier.CarrierFrequency;
                        break;

                    }

                }
                else if (carrier_freq_mode == YamlAsync.CarrierFrequency.CarrierFrequencyValueMode.Vibrato)
                {
                    var vibrato_data = carrier_data.VibratoData;

                    double highest, lowest;
                    if (vibrato_data.Highest.Mode == YamlAsync.CarrierFrequency.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue.YamlAsyncParameterVibratoMode.Const)
                        highest = vibrato_data.Highest.Constant;
                    else
                    {
                        var moving_val = vibrato_data.Highest.MovingValue;
                        highest = GetMovingValue(moving_val, original_wave_stat);
                    }

                    if (vibrato_data.Lowest.Mode == YamlAsync.CarrierFrequency.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue.YamlAsyncParameterVibratoMode.Const)
                        lowest = vibrato_data.Lowest.Constant;
                    else
                    {
                        var moving_val = vibrato_data.Lowest.MovingValue;
                        lowest = GetMovingValue(moving_val, original_wave_stat);
                    }

                    double interval;
                    if (vibrato_data.Interval.Mode == YamlAsync.CarrierFrequency.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue.YamlAsyncParameterVibratoMode.Const)
                        interval = vibrato_data.Interval.Constant;
                    else
                        interval = GetMovingValue(vibrato_data.Interval.MovingValue, original_wave_stat);

                    carrier_freq_val = GetVibratoFrequency(lowest, highest, interval, vibrato_data.Continuous, Control);
                }

                //
                //random range solve
                //
                double random_range = 0, random_interval = 0;
                if (async_data.RandomData.Range.Mode == YamlAsyncParameterRandomValueMode.Const) random_range = async_data.RandomData.Range.Constant;
                else random_range = GetMovingValue(async_data.RandomData.Range.MovingValue, original_wave_stat);

                if (async_data.RandomData.Interval.Mode == YamlAsyncParameterRandomValueMode.Const) random_interval = async_data.RandomData.Interval.Constant;
                else random_interval = GetMovingValue(async_data.RandomData.Interval.MovingValue, original_wave_stat);

                carrier_freq = new CarrierFreq(carrier_freq_val, random_range, random_interval);
            }

            //
            // Amplitude (Modulation Index)
            //
            if (Control.IsFreeRun())
            {
                AmplitudeParameter Param = (Control.IsMasconOff() ? SolvePattern.Amplitude.PowerOff : SolvePattern.Amplitude.PowerOn).Clone();
                double MaxControlFrequency = !Control.IsMasconOff() ? BaseFrequencyInfo.On.MaxControlFrequency : BaseFrequencyInfo.Off.MaxControlFrequency;

                if (Param.EndFrequency == -1)
                {
                    if (SolvePattern.Amplitude.Default.DisableRangeLimit) Param.EndFrequency = Control.GetSineFrequency();
                    else
                    {
                        Param.EndFrequency = (Control.GetSineFrequency() > MaxControlFrequency) ? MaxControlFrequency : Control.GetSineFrequency();
                        Param.EndFrequency = (Param.EndFrequency > SolvePattern.Amplitude.Default.EndFrequency) ? SolvePattern.Amplitude.Default.EndFrequency : Param.EndFrequency;
                    }
                }

                if (Param.EndAmplitude == -1) Param.EndAmplitude = GetAmplitude(SolvePattern.Amplitude.Default, Control.GetSineFrequency());
                if (Param.StartAmplitude == -1) Param.StartAmplitude = GetAmplitude(SolvePattern.Amplitude.Default, Control.GetSineFrequency());
                amplitude = GetAmplitude(Param, Control.GetControlFrequency());
            }
            else 
                amplitude = GetAmplitude(SolvePattern.Amplitude.Default, Control.GetControlFrequency());

            Dictionary<PulseDataKey, double> CalculatedPulseData = [];
            PulseDataKey[] PulseDataKeys = PulseModeConfiguration.GetAvailablePulseDataKey(pulse_mode, yvs.Level);
            for (int i = 0; i < PulseDataKeys.Length; i++)
            {
                PulseDataValue? Value = pulse_mode.PulseData.GetValueOrDefault(PulseDataKeys[i]);
                double Val = 0;
                if (Value != null)
                {
                    Val = Value.Mode switch
                    {
                        PulseDataValue.PulseDataValueMode.Moving => GetMovingValue(Value.MovingValue, original_wave_stat),
                        _ => Value.Constant,
                    };
                }

                CalculatedPulseData.Add(PulseDataKeys[i], Val);
            }

            if (Control.IsMasconOff() && amplitude == 0) Control.SetControlFrequency(0);
            if (Control.GetControlFrequency() == 0) return new PwmCalculateValues() { None = true };
            if (amplitude == 0) return new PwmCalculateValues() { None = true };

            PwmCalculateValues values = new()
            {
                None = false,
                Level = yvs.Level,
                PulseMode = pulse_mode,

                Carrier = carrier_freq,
                PulseData = CalculatedPulseData,

                MinimumFrequency = minimum_sine_freq,
                Amplitude = amplitude,
            };
            return values;

        }
    }
}
