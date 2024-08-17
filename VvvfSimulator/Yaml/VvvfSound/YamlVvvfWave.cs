using System;
using System.Collections.Generic;
using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlFreeRunCondition;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlMasconData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlControlDataAmplitudeControl;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqTable;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterRandom.YamlAsyncParameterRandomValue;

namespace VvvfSimulator.Yaml.VvvfSound
{
    public class YamlVvvfWave
    {
        public static double GetChangingValue(double x1, double y1, double x2, double y2, double x)
        {
            return y1 + (y2 - y1) / (x2 - x1) * (x - x1);
        }

        private static double YamlAmplitudeCalculate(YamlControlDataAmplitude amp_data, double x)
		{
			var amp_param = amp_data.Parameter;
			AmplitudeArgument aa = new(amp_param,x);
			double amp = GetAmplitude(amp_data.Mode, aa);
			if (amp_param.CutOffAmplitude > amp) amp = 0;
			if (amp_param.MaxAmplitude != -1 && amp_param.MaxAmplitude < amp) amp = amp_param.MaxAmplitude;
			return amp;
		}

		private static double GetMovingValue(YamlMovingValue moving_val, double current)
		{
			double val = 1000;
			if (moving_val.Type == YamlMovingValue.MovingValueType.Proportional)
				val = GetChangingValue(
					moving_val.Start,
					moving_val.StartValue,
					moving_val.End,
					moving_val.EndValue,
					current
				);
			else if (moving_val.Type == YamlMovingValue.MovingValueType.Pow2_Exponential)
				val = (Math.Pow(2, Math.Pow((current - moving_val.Start) / (moving_val.End - moving_val.Start), moving_val.Degree)) - 1) * (moving_val.EndValue - moving_val.StartValue) + moving_val.StartValue;
			else if(moving_val.Type == YamlMovingValue.MovingValueType.Inv_Proportional)
            {
				double x = GetChangingValue(
					moving_val.Start,
					1 / moving_val.StartValue,
					moving_val.End,
					1 / moving_val.EndValue,
					current
				);

				double c = -moving_val.CurveRate;
				double k = moving_val.EndValue;
				double l = moving_val.StartValue;
				double a = 1 / ((1 / l) - (1 / k)) * (1 / (l - c) - 1 / (k - c));
				double b = 1 / (1 - (1 / l) * k) * (1 / (l - c) - (1 / l) * k / (k - c));

				val = 1 / (a * x + b) + c;
			}
            else if (moving_val.Type == YamlMovingValue.MovingValueType.Sine)
			{
				double x = (MyMath.M_PI_2 - Math.Asin(moving_val.StartValue / moving_val.EndValue)) / (moving_val.End - moving_val.Start) * (current - moving_val.Start) + Math.Asin(moving_val.StartValue / moving_val.EndValue);
				val = Math.Sin(x) * moving_val.EndValue;
            }

            return val;

		}

		public static bool IsMatching(VvvfValues control, ControlStatus cv,YamlControlData ysd, bool compare_with_sine)
        {
			YamlFreeRunConditionSingle free_run_data;
			if (cv.mascon_on) free_run_data = ysd.FreeRunCondition.On;
			else free_run_data = ysd.FreeRunCondition.Off;

			bool enable_free_run_condition = cv.free_run && ((cv.mascon_on && ysd.EnableFreeRunOn) || (!cv.mascon_on && ysd.EnableFreeRunOff));
			bool enable_normal_condition = ysd.EnableNormal && !cv.free_run;
			if (!(enable_free_run_condition || enable_normal_condition)) return false;

			bool over_from = ysd.ControlFrequencyFrom <= (compare_with_sine ? control.GetSineFrequency() : cv.wave_stat);
			bool is_sine_from = ysd.RotateFrequencyFrom == -1 || ysd.RotateFrequencyFrom <= control.GetSineFrequency();
			bool is_sine_below = ysd.RotateFrequencyBelow == -1 || ysd.RotateFrequencyBelow > control.GetSineFrequency();

			if (!is_sine_from) return false;
			if (!is_sine_below) return false;

			if (!cv.free_run && over_from) return true;
			if (!cv.free_run && !over_from) return false;

			if (free_run_data.Skip) return false;

			if (over_from) return true;

			if (free_run_data.StuckAtHere)
			{
				if (control.GetSineFrequency() > ysd.ControlFrequencyFrom) return true;
				return false;
			}

			return false;

		}
		public static PwmCalculateValues CalculateYaml(VvvfValues control , ControlStatus cv, YamlVvvfSoundData yvs)
		{
			PulseMode pulse_mode;
			CarrierFreq carrier_freq = new(0, 0, 0.0005);
			double amplitude = 0;
			double dipolar = -1;

			//
			// mascon off solve
			//
			double max_voltage_freq;
			YamlMasconDataPattern mascon_on_off_check_data;
			if (cv.brake) mascon_on_off_check_data = yvs.MasconData.Braking;
			else mascon_on_off_check_data = yvs.MasconData.Accelerating;

			if (cv.mascon_on)
			{
				control.SetFreeFrequencyChange(mascon_on_off_check_data.On.FrequencyChangeRate);
				max_voltage_freq = mascon_on_off_check_data.On.MaxControlFrequency;
				if (cv.free_run)
				{
					if (cv.wave_stat > max_voltage_freq)
					{
						double rolling_freq = control.GetSineFrequency();
						control.SetControlFrequency(rolling_freq);
						cv.wave_stat = rolling_freq;
					}
				}
			}
			else
			{
				control.SetFreeFrequencyChange(mascon_on_off_check_data.Off.FrequencyChangeRate);
				max_voltage_freq = mascon_on_off_check_data.Off.MaxControlFrequency;
				if (cv.free_run)
				{
					if (cv.wave_stat > max_voltage_freq)
					{
						control.SetControlFrequency(max_voltage_freq);
						cv.wave_stat = max_voltage_freq;
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
			List<YamlControlData> control_list = new(cv.brake ? yvs.BrakingPattern : yvs.AcceleratePattern);
			control_list.Sort((a, b) => b.ControlFrequencyFrom.CompareTo(a.ControlFrequencyFrom));

			//determine what control data to solve
			int solve = -1;
			for (int x = 0; x < control_list.Count; x++)
			{
				YamlControlData ysd = control_list[x];
				bool match = IsMatching(control, cv, ysd , false);
                if (match)
                {
					solve = x;
					break;
                }

			}

			if (solve == -1)
			{
                if (cv.free_run)
                {
                    if (!cv.mascon_on)
                    {
						control.SetControlFrequency(0);
						return new PwmCalculateValues() { none = true };
					}
					else
					{
						control.SetControlFrequency(control.GetSineFrequency());
						return new PwmCalculateValues() { none = true };
					}
				}
				else
					return new PwmCalculateValues() { none = true };
			}

			//
			// min sine freq solve
			//
			double minimum_sine_freq, original_wave_stat = cv.wave_stat;
			if (cv.brake) minimum_sine_freq = yvs.MinimumFrequency.Braking;
			else minimum_sine_freq = yvs.MinimumFrequency.Accelerating;
			if (0 < cv.wave_stat && cv.wave_stat < minimum_sine_freq && !cv.free_run) cv.wave_stat = minimum_sine_freq;

			YamlControlData solve_data = control_list[solve];
			pulse_mode = solve_data.PulseMode;

			if (pulse_mode.PulseName == PulseModeName.Async)
			{
				var async_data = solve_data.AsyncModulationData;

				//
				//carrier freq solve
				//
				var carrier_data = async_data.CarrierWaveData;
				var carrier_freq_mode = carrier_data.Mode;
				double carrier_freq_val = 100;
				if (carrier_freq_mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Const)
					carrier_freq_val = carrier_data.Constant;
				else if (carrier_freq_mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Moving)
					carrier_freq_val = GetMovingValue(carrier_data.MovingValue, original_wave_stat);
				else if (carrier_freq_mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Table)
				{
					var table_data = carrier_data.CarrierFrequencyTable;

					//Solve from high.
					List<YamlAsyncParameterCarrierFreqTableValue> async_carrier_freq_table = new(table_data.CarrierFrequencyTableValues);
					async_carrier_freq_table.Sort((a, b) => Math.Sign(b.ControlFrequencyFrom - a.ControlFrequencyFrom));
	
					for(int i = 0; i < async_carrier_freq_table.Count; i++)
                    {
						var carrier = async_carrier_freq_table[i];
						bool condition_1 = carrier.FreeRunStuckAtHere && (control.GetSineFrequency() < carrier.ControlFrequencyFrom) && cv.free_run;
						bool condition_2 = original_wave_stat > carrier.ControlFrequencyFrom;
						if (!condition_1 && !condition_2) continue;

						carrier_freq_val = carrier.CarrierFrequency;
						break;

					}
					
				}
				else if(carrier_freq_mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncCarrierMode.Vibrato)
				{
					var vibrato_data = carrier_data.VibratoData;

					double highest, lowest;
					if (vibrato_data.Highest.Mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue.YamlAsyncParameterVibratoMode.Const)
						highest = vibrato_data.Highest.Constant;
					else
					{
						var moving_val = vibrato_data.Highest.MovingValue;
						highest = GetMovingValue(moving_val, original_wave_stat);
					}

					if (vibrato_data.Lowest.Mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue.YamlAsyncParameterVibratoMode.Const)
						lowest = vibrato_data.Lowest.Constant;
					else
					{
						var moving_val = vibrato_data.Lowest.MovingValue;
						lowest = GetMovingValue(moving_val, original_wave_stat);
					}

					double interval;
					if (vibrato_data.Interval.Mode == YamlAsyncParameter.YamlAsyncParameterCarrierFreq.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue.YamlAsyncParameterVibratoMode.Const)
						interval = vibrato_data.Interval.Constant;
					else
						interval = GetMovingValue(vibrato_data.Interval.MovingValue, original_wave_stat);

					carrier_freq_val = GetVibratoFrequency(lowest, highest, interval, vibrato_data.Continuous, control);
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

				//
				// dipolar solve
				//
				var dipolar_data = async_data.DipolarData;
				if (dipolar_data.Mode == YamlAsyncParameter.YamlAsyncParameterDipolar.YamlAsyncParameterDipolarMode.Const)
					dipolar = dipolar_data.Constant;
				else
				{
					var moving_val = dipolar_data.MovingValue;
					dipolar = GetMovingValue(moving_val, original_wave_stat);
				}



			}

			amplitude = YamlAmplitudeCalculate(solve_data.Amplitude.DefaultAmplitude, cv.wave_stat);

			if (cv.free_run && solve_data.Amplitude.FreeRunAmplitude != null)
			{
				var free_run_data = solve_data.Amplitude.FreeRunAmplitude;
				var free_run_amp_data = (cv.mascon_on) ? free_run_data.On : free_run_data.Off;
				var free_run_amp_param = free_run_amp_data.Parameter;

				double max_control_freq = cv.mascon_on ? mascon_on_off_check_data.On.MaxControlFrequency : mascon_on_off_check_data.Off.MaxControlFrequency;

				double target_freq = free_run_amp_param.EndFrequency;
				if (free_run_amp_param.EndFrequency == -1)
				{
					if (solve_data.Amplitude.DefaultAmplitude.Parameter.DisableRangeLimit) target_freq = control.GetSineFrequency();
					else
					{
                        target_freq = (control.GetSineFrequency() > max_control_freq) ? max_control_freq : control.GetSineFrequency();
						target_freq = (target_freq > solve_data.Amplitude.DefaultAmplitude.Parameter.EndFrequency) ? solve_data.Amplitude.DefaultAmplitude.Parameter.EndFrequency : target_freq;
                    }
                    
                }
					

				double target_amp = free_run_amp_param.EndAmplitude;
				if (free_run_amp_param.EndAmplitude == -1)
					target_amp = YamlAmplitudeCalculate(solve_data.Amplitude.DefaultAmplitude, control.GetSineFrequency());

				double start_amp = free_run_amp_param.StartAmplitude;
				if(start_amp == -1) 
					start_amp = YamlAmplitudeCalculate(solve_data.Amplitude.DefaultAmplitude, control.GetSineFrequency());


				AmplitudeArgument aa = new()
				{
					min_freq = free_run_amp_param.StartFrequency,
					min_amp = start_amp,
					max_freq = target_freq,
					max_amp = target_amp,

					current = cv.wave_stat,
					disable_range_limit = free_run_amp_param.DisableRangeLimit,
					polynomial = free_run_amp_param.Polynomial,
					change_const = free_run_amp_param.CurveChangeRate
				};
				
				amplitude = GetAmplitude(free_run_amp_data.Mode, aa);

				if (free_run_amp_param.CutOffAmplitude > amplitude) amplitude = 0;
				if (free_run_amp_param.MaxAmplitude != -1 && amplitude > free_run_amp_param.MaxAmplitude) amplitude = free_run_amp_param.MaxAmplitude;
				if (!cv.mascon_on && amplitude == 0) control.SetControlFrequency(0);
			}

			if (cv.wave_stat == 0) return new PwmCalculateValues() { none = true };
			if (amplitude == 0) return new PwmCalculateValues() { none = true };

			PwmCalculateValues values = new()
			{
				none = false,
				carrier_freq = carrier_freq,
				pulse_mode = pulse_mode,
				level = yvs.Level,
				dipolar = dipolar,

				min_sine_freq = minimum_sine_freq,
				amplitude = amplitude,
			};
			return values;
			
		}
	}
}
