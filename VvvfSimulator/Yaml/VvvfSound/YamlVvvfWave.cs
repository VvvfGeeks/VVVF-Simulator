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
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlMasconData;

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

		public static bool IsMatching(VvvfValues Control,YamlControlData ysd)
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
			double dipolar = -1;

			//
			// mascon off solve
			//
			double max_voltage_freq;
			YamlMasconDataPattern mascon_on_off_check_data;
			if (Control.IsBraking()) mascon_on_off_check_data = yvs.MasconData.Braking;
			else mascon_on_off_check_data = yvs.MasconData.Accelerating;

			if (!Control.IsMasconOff())
			{
				Control.SetFreeFrequencyChange(mascon_on_off_check_data.On.FrequencyChangeRate);
				max_voltage_freq = mascon_on_off_check_data.On.MaxControlFrequency;
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
				Control.SetFreeFrequencyChange(mascon_on_off_check_data.Off.FrequencyChangeRate);
				max_voltage_freq = mascon_on_off_check_data.Off.MaxControlFrequency;
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

			YamlControlData solve_data = control_list[solve];
			pulse_mode = solve_data.PulseMode;

			if (pulse_mode.PulseType == YamlPulseMode.PulseTypeName.ASYNC)
			{
				var async_data = solve_data.AsyncModulationData;

				//
				//carrier freq solve
				//
				var carrier_data = async_data.CarrierWaveData;
				var carrier_freq_mode = carrier_data.Mode;
				double carrier_freq_val = 100;
				if (carrier_freq_mode == YamlAsync.CarrierFrequency.YamlAsyncCarrierMode.Const)
					carrier_freq_val = carrier_data.Constant;
				else if (carrier_freq_mode == YamlAsync.CarrierFrequency.YamlAsyncCarrierMode.Moving)
					carrier_freq_val = GetMovingValue(carrier_data.MovingValue, original_wave_stat);
				else if (carrier_freq_mode == YamlAsync.CarrierFrequency.YamlAsyncCarrierMode.Table)
				{
					var table_data = carrier_data.CarrierFrequencyTable;

					//Solve from high.
					List<YamlAsyncParameterCarrierFreqTableValue> async_carrier_freq_table = new(table_data.CarrierFrequencyTableValues);
					async_carrier_freq_table.Sort((a, b) => Math.Sign(b.ControlFrequencyFrom - a.ControlFrequencyFrom));
	
					for(int i = 0; i < async_carrier_freq_table.Count; i++)
                    {
						var carrier = async_carrier_freq_table[i];
						bool condition_1 = carrier.FreeRunStuckAtHere && (Control.GetSineFrequency() < carrier.ControlFrequencyFrom) && Control.IsFreeRun();
						bool condition_2 = original_wave_stat > carrier.ControlFrequencyFrom;
						if (!condition_1 && !condition_2) continue;

						carrier_freq_val = carrier.CarrierFrequency;
						break;

					}
					
				}
				else if(carrier_freq_mode == YamlAsync.CarrierFrequency.YamlAsyncCarrierMode.Vibrato)
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

				//
				// dipolar solve
				//
				var dipolar_data = async_data.DipolarData;
				if (dipolar_data.Mode == YamlAsync.Dipolar.YamlAsyncParameterDipolarMode.Const)
					dipolar = dipolar_data.Constant;
				else
				{
					var moving_val = dipolar_data.MovingValue;
					dipolar = GetMovingValue(moving_val, original_wave_stat);
				}



			}

			amplitude = YamlAmplitudeCalculate(solve_data.Amplitude.DefaultAmplitude, Control.GetControlFrequency());

			if (Control.IsFreeRun() && solve_data.Amplitude.FreeRunAmplitude != null)
			{
				var free_run_data = solve_data.Amplitude.FreeRunAmplitude;
				var free_run_amp_data = (!Control.IsMasconOff()) ? free_run_data.On : free_run_data.Off;
				var free_run_amp_param = free_run_amp_data.Parameter;

				double max_control_freq = !Control.IsMasconOff() ? mascon_on_off_check_data.On.MaxControlFrequency : mascon_on_off_check_data.Off.MaxControlFrequency;

				double target_freq = free_run_amp_param.EndFrequency;
				if (free_run_amp_param.EndFrequency == -1)
				{
					if (solve_data.Amplitude.DefaultAmplitude.Parameter.DisableRangeLimit) target_freq = Control.GetSineFrequency();
					else
					{
                        target_freq = (Control.GetSineFrequency() > max_control_freq) ? max_control_freq : Control.GetSineFrequency();
						target_freq = (target_freq > solve_data.Amplitude.DefaultAmplitude.Parameter.EndFrequency) ? solve_data.Amplitude.DefaultAmplitude.Parameter.EndFrequency : target_freq;
                    }
                    
                }
					

				double target_amp = free_run_amp_param.EndAmplitude;
				if (free_run_amp_param.EndAmplitude == -1)
					target_amp = YamlAmplitudeCalculate(solve_data.Amplitude.DefaultAmplitude, Control.GetSineFrequency());

				double start_amp = free_run_amp_param.StartAmplitude;
				if(start_amp == -1) 
					start_amp = YamlAmplitudeCalculate(solve_data.Amplitude.DefaultAmplitude, Control.GetSineFrequency());


				AmplitudeArgument aa = new()
				{
					min_freq = free_run_amp_param.StartFrequency,
					min_amp = start_amp,
					max_freq = target_freq,
					max_amp = target_amp,

					current = Control.GetControlFrequency(),
					disable_range_limit = free_run_amp_param.DisableRangeLimit,
					polynomial = free_run_amp_param.Polynomial,
					change_const = free_run_amp_param.CurveChangeRate
				};
				
				amplitude = GetAmplitude(free_run_amp_data.Mode, aa);

				if (free_run_amp_param.CutOffAmplitude > amplitude) amplitude = 0;
				if (free_run_amp_param.MaxAmplitude != -1 && amplitude > free_run_amp_param.MaxAmplitude) amplitude = free_run_amp_param.MaxAmplitude;
				if (Control.IsMasconOff() && amplitude == 0) Control.SetControlFrequency(0);
			}

			if (Control.GetControlFrequency() == 0) return new PwmCalculateValues() { None = true };
			if (amplitude == 0) return new PwmCalculateValues() { None = true };

			PwmCalculateValues values = new()
			{
				None = false,
				Carrier = carrier_freq,
				Pulse = pulse_mode,
				Level = yvs.Level,
				Dipolar = dipolar,

				MinimumFrequency = minimum_sine_freq,
				Amplitude = amplitude,
			};
			return values;
			
		}
	}
}
