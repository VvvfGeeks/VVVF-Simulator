using System;
using System.Windows;
using static VvvfSimulator.MyMath;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;

namespace VvvfSimulator
{
    public class VvvfCalculate
    {
        //
        // Basic Calculation
        //
        public static double GetSaw(double x)
        {
            double val;
            double fixed_x = x - (double)((int)(x * M_1_2PI) * M_2PI);
            if (0 <= fixed_x && fixed_x < M_PI_2)
                val = M_2_PI * fixed_x;
            else if (M_PI_2 <= fixed_x && fixed_x < 3.0 * M_PI_2)
                val = -M_2_PI * fixed_x + 2;
            else
                val = M_2_PI * fixed_x - 4;

            return -val;
        }

        public static double GetSine(double x)
        {
            return Math.Sin(x);
        }

        public static double GetSquare(double x)
        {
            double fixed_x = x - (double)((int)(x * M_1_2PI) * M_2PI);
            if (fixed_x / M_PI > 1) return -1;
            return 1;
        }

        public static double GetModifiedSine(double x, int level)
        {
            double sine = GetSine(x) * level;
            double value = Math.Round(sine) / level;
            return value;
        }

        public static double GetModifiedSaw(double x)
        {
            double sine = -GetSaw(x) * M_PI_2;
            int D = sine > 0 ? 1 : -1;
            if (Math.Abs(sine) > 0.5) sine = D;

            return sine;
        }

        public static double GetSineValueWithHarmonic(PulseMode mode, double x, double amplitude, double T, double InitialPhase)
        {
            double BaseValue = mode.BaseWave switch
            {
                BaseWaveType.Sine => GetSine(x),
                BaseWaveType.Saw => -GetSaw(x),
                BaseWaveType.Modified_Sine_1 => GetModifiedSine(x, 1),
                BaseWaveType.Modified_Sine_2 => GetModifiedSine(x, 2),
                BaseWaveType.Modified_Saw_1 => GetModifiedSaw(x),
                _ => throw new NotImplementedException(),
            };
            BaseValue *= amplitude;

            for (int i = 0; i < mode.PulseHarmonics.Count; i++)
            {
                PulseHarmonic harmonic = mode.PulseHarmonics[i];
                double harmonic_x = harmonic.IsHarmonicProportional switch
                {
                    true => harmonic.Harmonic * (x + harmonic.InitialPhase),
                    false => M_2PI * harmonic.Harmonic * (T + InitialPhase)
                };
                double harmonic_value = harmonic.Type switch
                {
                    PulseHarmonic.PulseHarmonicType.Sine => GetSine(harmonic_x),
                    PulseHarmonic.PulseHarmonicType.Saw => -GetSaw(harmonic_x),
                    PulseHarmonic.PulseHarmonicType.Square => GetSquare(harmonic_x),
                    _ => throw new NotImplementedException(),
                };
                BaseValue += harmonic_value * harmonic.Amplitude * (harmonic.IsAmplitudeProportional ? amplitude : 1);
            }

            BaseValue = BaseValue > 1 ? 1 : BaseValue < -1 ? -1 : BaseValue;
            
            return BaseValue;
        }

        public static int ModulateSin(double sin_value, double saw_value)
        {
            return sin_value > saw_value ? 1 : 0;
        }

        //
        // Discrete Time
        //
        public static double DiscreteTimeLine(double x, int level, DiscreteTimeConfiguration.DiscreteTimeMode mode)
        {
            //int level = (discrete.ProportionToPulse ? sawCarrier : 1) * discrete.Steps;
            double seed = (x % M_2PI) * level / M_2PI;
            double time = mode switch
            {
                DiscreteTimeConfiguration.DiscreteTimeMode.Left => Math.Ceiling(seed),
                DiscreteTimeConfiguration.DiscreteTimeMode.Middle => Math.Round(seed),
                DiscreteTimeConfiguration.DiscreteTimeMode.Right => Math.Floor(seed),
                _ => throw new NotImplementedException(),
            };
            return time * M_2PI / level;
        }

        //
        // Pulse Calculation
        //
        public static int GetHopPulse(double x, double amplitude, int carrier, int width)
        {
            int totalSteps = carrier * 2;
            double fixed_x = (x % M_PI) / (M_PI / (totalSteps));
            double saw_value = -GetSaw(carrier * x);
            double modulated;
            if (fixed_x > (totalSteps - 1)) modulated = -1;
            else if (fixed_x > (totalSteps / 2) + width) modulated = 1;
            else if (fixed_x > (totalSteps / 2) - width) modulated = 2 * amplitude - 1;
            else if (fixed_x > 1) modulated = 1;
            else modulated = -1;
            if ((x % M_2PI) > M_PI) modulated = -modulated;

            return ModulateSin(modulated, saw_value) * 2;
        }
        public static int GetWideP3(double time, double angle_frequency, double initial_phase, double voltage, bool saw_oppose)
        {
            double sin = GetSine(time * angle_frequency + initial_phase);
            double saw = GetSaw(time * angle_frequency + initial_phase);
            if (saw_oppose)
                saw = -saw;
            double pwm = ((sin - saw > 0) ? 1 : -1) * voltage;
            double nega_saw = (saw > 0) ? saw - 1 : saw + 1;
            int gate = ModulateSin(pwm, nega_saw) * 2;
            return gate;
        }
        public static int GetPulseWithSaw(double x, double carrier_initial_phase, double voltage, double carrier_mul, bool saw_oppose)
        {
            double carrier_saw = -GetSaw(carrier_mul * x + carrier_initial_phase);
            double saw = -GetSaw(x);
            if (saw_oppose)
                saw = -saw;
            double pwm = (saw > 0) ? voltage : -voltage;
            int gate = ModulateSin(pwm, carrier_saw) * 2;
            return gate;
        }
        public static int GetPulseWithSwitchAngle(
            double alpha1,
            double alpha2,
            double alpha3,
            double alpha4,
            double alpha5,
            double alpha6,
            double alpha7,
            int flag,
            double time, double sin_angle_frequency, double initial_phase)
        {
            double theta = (initial_phase + time * sin_angle_frequency) - (double)((int)((initial_phase + time * sin_angle_frequency) * M_1_2PI) * M_2PI);

            int PWM_OUT = (((((theta <= alpha2) && (theta >= alpha1)) || ((theta <= alpha4) && (theta >= alpha3)) || ((theta <= alpha6) && (theta >= alpha5)) || ((theta <= M_PI - alpha1) && (theta >= M_PI - alpha2)) || ((theta <= M_PI - alpha3) && (theta >= M_PI - alpha4)) || ((theta <= M_PI - alpha5) && (theta >= M_PI - alpha6))) && ((theta <= M_PI) && (theta >= 0))) || (((theta <= M_PI - alpha7) && (theta >= alpha7)) && ((theta <= M_PI) && (theta >= 0)))) || ((!(((theta <= alpha2 + M_PI) && (theta >= alpha1 + M_PI)) || ((theta <= alpha4 + M_PI) && (theta >= alpha3 + M_PI)) || ((theta <= alpha6 + M_PI) && (theta >= alpha5 + M_PI)) || ((theta <= M_2PI - alpha1) && (theta >= M_2PI - alpha2)) || ((theta <= M_2PI - alpha3) && (theta >= M_2PI - alpha4)) || ((theta <= M_2PI - alpha5) && (theta >= M_2PI - alpha6))) && ((theta <= M_2PI) && (theta >= M_PI))) && !((theta <= M_2PI - alpha7) && (theta >= M_PI + alpha7)) && (theta <= M_2PI) && (theta >= M_PI)) ? 1 : -1;

            int gate = flag == 'A' ? -PWM_OUT + 1 : PWM_OUT + 1;
            return gate;

        }

        //
        // Amplitude Calculation
        //
        public enum AmplitudeMode
        {
            Linear, Linear_Polynomial, Inv_Proportional, Exponential, Sine, Wide_3_Pulse
        }
        public class AmplitudeArgument
        {
            public double min_freq = 0;
            public double min_amp = 0;
            public double max_freq = 0;
            public double max_amp = 0;

            public bool disable_range_limit = false;
            public double change_const = 0.43;
            public double polynomial = 1.0;

            public double current = 0;

            public AmplitudeArgument() { }
            public AmplitudeArgument(YamlControlDataAmplitudeControl.YamlControlDataAmplitude.YamlControlDataAmplitudeParameter config, double current)
            {
                change_const = config.CurveChangeRate;
                this.current = current;
                disable_range_limit = config.DisableRangeLimit;
                polynomial = config.Polynomial;

                max_amp = config.EndAmplitude;
                max_freq = config.EndFrequency;
                min_amp = config.StartAmplitude;
                min_freq = config.StartFrequency;
            }
        }

        public static double GetAmplitude(AmplitudeMode mode, AmplitudeArgument arg)
        {
            if (arg.max_amp == arg.min_amp) return arg.min_amp;

            switch (mode)
            {
                case AmplitudeMode.Linear:
                    if (!arg.disable_range_limit)
                    {
                        if (arg.current < arg.min_freq) arg.current = arg.min_freq;
                        if (arg.current > arg.max_freq) arg.current = arg.max_freq;
                    }
                    return (arg.max_amp - arg.min_amp) / (arg.max_freq - arg.min_freq) * (arg.current - arg.min_freq) + arg.min_amp;
                case AmplitudeMode.Wide_3_Pulse:
                    if (!arg.disable_range_limit)
                    {
                        if (arg.current < arg.min_freq) arg.current = arg.min_freq;
                        if (arg.current > arg.max_freq) arg.current = arg.max_freq;
                    }
                    return 0.2 * ((arg.current - arg.min_freq) * ((arg.max_amp - arg.min_amp) / (arg.max_freq - arg.min_freq)) + arg.min_amp) + 0.8;
                case AmplitudeMode.Inv_Proportional:
                    {
                        if (!arg.disable_range_limit)
                        {
                            if (arg.current < arg.min_freq) arg.current = arg.min_freq;
                            if (arg.current > arg.max_freq) arg.current = arg.max_freq;
                        }

                        double x = GetAmplitude(AmplitudeMode.Linear, new AmplitudeArgument()
                        {
                            min_freq = arg.min_freq,
                            min_amp = 1 / arg.min_amp,
                            max_freq = arg.max_freq,
                            max_amp = 1 / arg.max_amp,
                            current = arg.current,
                            disable_range_limit = arg.disable_range_limit
                        });

                        double c = -arg.change_const;
                        double k = arg.max_amp;
                        double l = arg.min_amp;
                        double a = 1 / (1 / l - 1 / k) * (1 / (l - c) - 1 / (k - c));
                        double b = 1 / (1 - 1 / l * k) * (1 / (l - c) - 1 / l * k / (k - c));

                        return 1 / (a * x + b) + c;
                    }

                case AmplitudeMode.Exponential:
                    {

                        if (!arg.disable_range_limit)
                        {
                            if (arg.current > arg.max_freq) arg.current = arg.max_freq;
                        }

                        double t = 1 / arg.max_freq * Math.Log(arg.max_amp + 1);

                        return Math.Pow(Math.E, t * arg.current) - 1;
                    }

                case AmplitudeMode.Linear_Polynomial:
                    if (!arg.disable_range_limit)
                    {
                        if (arg.current > arg.max_freq) arg.current = arg.max_freq;
                    }
                    return Math.Pow(arg.current, arg.polynomial) / Math.Pow(arg.max_freq, arg.polynomial) * arg.max_amp;
                case AmplitudeMode.Sine:
                    {
                        if (!arg.disable_range_limit)
                        {
                            if (arg.current > arg.max_freq) arg.current = arg.max_freq;
                        }

                        double x = Math.PI * arg.current / (2.0 * arg.max_freq);

                        return Math.Sin(x) * arg.max_amp;
                    }
                default:
                    return 0;
            }
        }

        //
        // Carrier Freq Calculation
        //
        private static double GetRandomFrequency(CarrierFreq data, VvvfValues control)
        {
            if (data.range == 0) return data.base_freq;

            if (control.IsRandomFrequencyMoveAllowed())
            {
                double random_freq;
                if (control.GetRandomFrequencyPreviousTime() == 0 || control.GetPreviousSawRandomFrequency() == 0)
                {
                    Random rnd = new();
                    double diff_freq = rnd.NextDouble() * data.range;
                    if (rnd.NextDouble() < 0.5) diff_freq = -diff_freq;
                    double silent_random_freq = data.base_freq + diff_freq;
                    random_freq = silent_random_freq;
                    control.SetPreviousSawRandomFrequency(silent_random_freq);
                    control.SetRandomFrequencyPreviousTime(control.GetGenerationCurrentTime());
                }
                else
                {
                    random_freq = control.GetPreviousSawRandomFrequency();
                }

                if (control.GetRandomFrequencyPreviousTime() + data.interval < control.GetGenerationCurrentTime())
                    control.SetRandomFrequencyPreviousTime(0);

                return random_freq;
            }
            else
            {
                return data.base_freq;
            }
        }
        public static double GetVibratoFrequency(double lowest, double highest, double interval_time, bool continuous, VvvfValues control)
        {

            if (!control.IsRandomFrequencyMoveAllowed())
                return (highest + lowest) / 2.0;

            double random_freq;
            double current_t = control.GetGenerationCurrentTime();
            double solve_t = control.GetVibratoFrequencyPreviousTime();

            if (continuous)
            {
                if (interval_time / 2.0 > current_t - solve_t)
                    random_freq = lowest + (highest - lowest) / (interval_time / 2.0) * (current_t - solve_t);
                else
                    random_freq = highest + (lowest - highest) / (interval_time / 2.0) * (current_t - solve_t - interval_time / 2.0);
            }
            else
            {
                if (interval_time / 2.0 > current_t - solve_t)
                    random_freq = highest;
                else
                    random_freq = lowest;
            }

            if (current_t - solve_t > interval_time)
                control.SetVibratoFrequencyPreviousTime(current_t);
            return random_freq;
        }

        //
        // VVVF Calculation
        //
        public static WaveValues CalculatePhases(VvvfValues control, PwmCalculateValues value, double add_initial)
        {

            if (control.GetSineFrequency() < value.min_sine_freq && control.GetControlFrequency() > 0) control.SetVideoSineFrequency(value.min_sine_freq);
            else control.SetVideoSineFrequency(control.GetSineFrequency());

            if (value.none) return new WaveValues(0, 0, 0);

            control.SetVideoPulseMode(value.pulse_mode);
            control.SetVideoSineAmplitude(value.amplitude);
            if (value.carrier_freq != null) control.SetVideoCarrierFrequency(value.carrier_freq.Clone());
            control.SetVideoDipolar(value.dipolar);

            int U = 0, V = 0, W = 0;

            for (int i = 0; i < 3; i++)
            {

                int val;
                double initial = M_2PI / 3.0 * i + add_initial;
                try
                {
                    if (value.level == 2) val = CalculateTwoLevel(control, value, initial);
                    else val = CalculateThreeLevel(control, value, initial);
                }
                catch
                {
                    val = 0;
                }
                if (i == 0) U = val;
                else if (i == 1) V = val;
                else W = val;

            }

            return new(U, V, W);
        }

        public static int CalculateThreeLevel(VvvfValues control, PwmCalculateValues calculate_values, double initial_phase)
        {
            double sine_angle_freq = control.GetSineAngleFrequency();
            double sine_time = control.GetSineTime();
            double min_sine_angle_freq = calculate_values.min_sine_freq * M_2PI;
            PulseMode pulse_mode = calculate_values.pulse_mode;
            CarrierFreq freq_data = calculate_values.carrier_freq;
            double dipolar = calculate_values.dipolar;

            if (sine_angle_freq < min_sine_angle_freq && control.GetControlFrequency() > 0)
            {
                control.SetSineTimeChangeAllowed(false);
                sine_angle_freq = min_sine_angle_freq;
            }
            else
                control.SetSineTimeChangeAllowed(true);

            if (pulse_mode.PulseName == PulseModeName.Async)
            {

                double desire_saw_angle_freq = (freq_data.range == 0) ? freq_data.base_freq * M_2PI : GetRandomFrequency(freq_data, control) * M_2PI;

                double saw_time = control.GetSawTime();
                double saw_angle_freq = control.GetSawAngleFrequency();

                if (desire_saw_angle_freq == 0)
                    saw_time = 0;
                else
                    saw_time = saw_angle_freq / desire_saw_angle_freq * saw_time;
                saw_angle_freq = desire_saw_angle_freq;

                control.SetSawAngleFrequency(saw_angle_freq);
                control.SetSawTime(saw_time);

                double sine_x = sine_time * sine_angle_freq + initial_phase;
                if (pulse_mode.DiscreteTime.Enabled) sine_x = DiscreteTimeLine(sine_x, pulse_mode.DiscreteTime.Steps, pulse_mode.DiscreteTime.Mode);
                double sin_value = GetSineValueWithHarmonic(pulse_mode.Clone(), sine_x, calculate_values.amplitude, control.GetGenerationCurrentTime(), initial_phase);

                double saw_value = GetSaw(control.GetSawTime() * control.GetSawAngleFrequency());
                if (pulse_mode.Shift)
                    saw_value = -saw_value;

                double changed_saw = ((dipolar != -1) ? dipolar : 0.5) * saw_value;
                int pwm_value = ModulateSin(sin_value, changed_saw + 0.5) + ModulateSin(sin_value, changed_saw - 0.5);

                return pwm_value;



            }
            else
            {
                int pulses = PulseModeConfiguration.GetPulseNum(pulse_mode, 3);
                double sine_x = sine_time * sine_angle_freq + initial_phase;
                if (pulse_mode.DiscreteTime.Enabled) sine_x = DiscreteTimeLine(sine_x, pulse_mode.DiscreteTime.Steps, pulse_mode.DiscreteTime.Mode);
                double saw_x = pulses * (sine_angle_freq * sine_time + initial_phase);

                if (pulse_mode.PulseName == PulseModeName.P_1)
                {
                    if (pulse_mode.AltMode == PulseAlternativeMode.Alt1)
                    {
                        double sine = GetSine(sine_x);
                        int D = sine > 0 ? 1 : -1;
                        double voltage_fix = D * (1 - calculate_values.amplitude);

                        int gate = (D * (sine - voltage_fix) > 0) ? D : 0;
                        gate += 1;
                        return gate;
                    }
                }

                double saw_value = GetSaw(saw_x);
                if (pulse_mode.Shift)
                    saw_value = -saw_value;

                double sin_value = GetSineValueWithHarmonic(pulse_mode.Clone(), sine_x, calculate_values.amplitude, control.GetGenerationCurrentTime(), initial_phase);

                double changed_saw = ((dipolar != -1) ? dipolar : 0.5) * saw_value;
                int pwm_value = ModulateSin(sin_value, changed_saw + 0.5) + ModulateSin(sin_value, changed_saw - 0.5);

                control.SetSawAngleFrequency(sine_angle_freq * pulses);
                control.SetSawTime(sine_time);

                return pwm_value;
            }


        }
        public static int CalculateTwoLevel(VvvfValues control, PwmCalculateValues calculate_values, double initial_phase)
        {
            double sin_angle_freq = control.GetSineAngleFrequency();
            double sin_time = control.GetSineTime();
            double min_sine_angle_freq = calculate_values.min_sine_freq * M_2PI;
            if (sin_angle_freq < min_sine_angle_freq && control.GetControlFrequency() > 0)
            {
                control.SetSineTimeChangeAllowed(false);
                sin_angle_freq = min_sine_angle_freq;
            }
            else
                control.SetSineTimeChangeAllowed(true);

            double saw_time = control.GetSawTime();
            double saw_angle_freq = control.GetSawAngleFrequency();

            double amplitude = calculate_values.amplitude;
            PulseMode pulse_mode = calculate_values.pulse_mode;
            PulseModeName pulse_name = pulse_mode.PulseName;
            CarrierFreq carrier_freq_data = calculate_values.carrier_freq;

            if (calculate_values.none)
                return 0;


            // Async CHM SHE
            switch (pulse_name)
            {

                case PulseModeName.P_Wide_3: return GetWideP3(sin_time, sin_angle_freq, initial_phase, amplitude, false);

                case PulseModeName.Async:
                    {
                        double desire_saw_angle_freq = (carrier_freq_data.range == 0) ? carrier_freq_data.base_freq * M_2PI : GetRandomFrequency(carrier_freq_data, control) * M_2PI;

                        if (desire_saw_angle_freq == 0)
                            saw_time = 0;
                        else
                            saw_time = saw_angle_freq / desire_saw_angle_freq * saw_time;
                        saw_angle_freq = desire_saw_angle_freq;

                        double sine_x = sin_time * sin_angle_freq + initial_phase;
                        if (pulse_mode.DiscreteTime.Enabled) sine_x = DiscreteTimeLine(sine_x, pulse_mode.DiscreteTime.Steps, pulse_mode.DiscreteTime.Mode);
                        double sin_value = GetSineValueWithHarmonic(pulse_mode.Clone(), sine_x, amplitude, control.GetGenerationCurrentTime(), initial_phase);

                        double saw_value = GetSaw(saw_time * saw_angle_freq);
                        int pwm_value = ModulateSin(sin_value, saw_value) * 2;

                        control.SetSawAngleFrequency(saw_angle_freq);
                        control.SetSawTime(saw_time);

                        return pwm_value;
                    }

                case PulseModeName.CHMP_15:
                    {
                        if (pulse_mode.AltMode == PulseAlternativeMode.Default)
                        {
                            return GetPulseWithSwitchAngle(
                                SwitchAngles._7Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                                SwitchAngles._7Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                                SwitchAngles._7Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                                SwitchAngles._7Alpha[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
                                SwitchAngles._7Alpha[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
                                SwitchAngles._7Alpha[(int)(1000 * amplitude) + 1, 5] * M_PI_180,
                                SwitchAngles._7Alpha[(int)(1000 * amplitude) + 1, 6] * M_PI_180,
                                SwitchAngles._7Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
                            );
                        }
                        else if (pulse_mode.AltMode == PulseAlternativeMode.Alt1)
                        {
                            return GetPulseWithSwitchAngle(
                                SwitchAngles._7Alpha_Old[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                                SwitchAngles._7Alpha_Old[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                                SwitchAngles._7Alpha_Old[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                                SwitchAngles._7Alpha_Old[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
                                SwitchAngles._7Alpha_Old[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
                                SwitchAngles._7Alpha_Old[(int)(1000 * amplitude) + 1, 5] * M_PI_180,
                                SwitchAngles._7Alpha_Old[(int)(1000 * amplitude) + 1, 6] * M_PI_180,
                                SwitchAngles._7OldAlpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
                            );
                        }
                        break;
                    }

                case PulseModeName.CHMP_Wide_15:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._7WideAlpha[(int)(1000 * amplitude) - 999, 0] * M_PI_180,
                           SwitchAngles._7WideAlpha[(int)(1000 * amplitude) - 999, 1] * M_PI_180,
                           SwitchAngles._7WideAlpha[(int)(1000 * amplitude) - 999, 2] * M_PI_180,
                           SwitchAngles._7WideAlpha[(int)(1000 * amplitude) - 999, 3] * M_PI_180,
                           SwitchAngles._7WideAlpha[(int)(1000 * amplitude) - 999, 4] * M_PI_180,
                           SwitchAngles._7WideAlpha[(int)(1000 * amplitude) - 999, 5] * M_PI_180,
                           SwitchAngles._7WideAlpha[(int)(1000 * amplitude) - 999, 6] * M_PI_180,
                           'B', sin_time, sin_angle_freq, initial_phase);
                    }

                case PulseModeName.CHMP_13:
                    {
                        if (pulse_mode.AltMode == PulseAlternativeMode.Default)
                        {
                            return GetPulseWithSwitchAngle(
                                SwitchAngles._6Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                                SwitchAngles._6Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                                SwitchAngles._6Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                                SwitchAngles._6Alpha[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
                                SwitchAngles._6Alpha[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
                                SwitchAngles._6Alpha[(int)(1000 * amplitude) + 1, 5] * M_PI_180,
                                M_PI_2,
                                SwitchAngles._6Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
                            );
                        }
                        else if (pulse_mode.AltMode == PulseAlternativeMode.Alt1)
                        {
                            return GetPulseWithSwitchAngle(
                                SwitchAngles._6Alpha_Old[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                                SwitchAngles._6Alpha_Old[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                                SwitchAngles._6Alpha_Old[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                                SwitchAngles._6Alpha_Old[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
                                SwitchAngles._6Alpha_Old[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
                                SwitchAngles._6Alpha_Old[(int)(1000 * amplitude) + 1, 5] * M_PI_180,
                                M_PI_2,
                                SwitchAngles._6OldAlpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
                            );
                        }
                        break;
                    }

                case PulseModeName.CHMP_Wide_13:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._6WideAlpha[(int)(1000 * amplitude) - 999, 0] * M_PI_180,
                           SwitchAngles._6WideAlpha[(int)(1000 * amplitude) - 999, 1] * M_PI_180,
                           SwitchAngles._6WideAlpha[(int)(1000 * amplitude) - 999, 2] * M_PI_180,
                           SwitchAngles._6WideAlpha[(int)(1000 * amplitude) - 999, 3] * M_PI_180,
                           SwitchAngles._6WideAlpha[(int)(1000 * amplitude) - 999, 4] * M_PI_180,
                           SwitchAngles._6WideAlpha[(int)(1000 * amplitude) - 999, 5] * M_PI_180,
                           M_PI_2,
                           'A', sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.CHMP_11:
                    {
                        if (pulse_mode.AltMode == PulseAlternativeMode.Default)
                        {
                            return GetPulseWithSwitchAngle(
                                SwitchAngles._5Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                                SwitchAngles._5Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                                SwitchAngles._5Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                                SwitchAngles._5Alpha[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
                                SwitchAngles._5Alpha[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
                                M_PI_2,
                                M_PI_2,
                                SwitchAngles._5Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
                            );
                        }
                        else if (pulse_mode.AltMode == PulseAlternativeMode.Alt1)
                        {
                            return GetPulseWithSwitchAngle(
                                SwitchAngles._5Alpha_Old[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                                SwitchAngles._5Alpha_Old[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                                SwitchAngles._5Alpha_Old[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                                SwitchAngles._5Alpha_Old[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
                                SwitchAngles._5Alpha_Old[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
                                M_PI_2,
                                M_PI_2,
                                SwitchAngles._5OldAlpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase
                            );
                        }
                        break;
                    }

                case PulseModeName.CHMP_Wide_11:
                    {
                        return GetPulseWithSwitchAngle(
                            SwitchAngles._5WideAlpha[(int)(1000 * amplitude) - 999, 0] * M_PI_180,
                            SwitchAngles._5WideAlpha[(int)(1000 * amplitude) - 999, 1] * M_PI_180,
                            SwitchAngles._5WideAlpha[(int)(1000 * amplitude) - 999, 2] * M_PI_180,
                            SwitchAngles._5WideAlpha[(int)(1000 * amplitude) - 999, 3] * M_PI_180,
                            SwitchAngles._5WideAlpha[(int)(1000 * amplitude) - 999, 4] * M_PI_180,
                            M_PI_2,
                            M_PI_2,
                            'B', sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.CHMP_9:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._4Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                           SwitchAngles._4Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                           SwitchAngles._4Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                           SwitchAngles._4Alpha[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           SwitchAngles._4Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.CHMP_Wide_9:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._4WideAlpha[(int)(1000 * amplitude) - 799, 0] * M_PI_180,
                           SwitchAngles._4WideAlpha[(int)(1000 * amplitude) - 799, 1] * M_PI_180,
                           SwitchAngles._4WideAlpha[(int)(1000 * amplitude) - 799, 2] * M_PI_180,
                           SwitchAngles._4WideAlpha[(int)(1000 * amplitude) - 799, 3] * M_PI_180,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           'A', sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.CHMP_7:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._3Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                           SwitchAngles._3Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                           SwitchAngles._3Alpha[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           SwitchAngles._3Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.CHMP_Wide_7:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._3WideAlpha[(int)(1000 * amplitude) - 799, 0] * M_PI_180,
                           SwitchAngles._3WideAlpha[(int)(1000 * amplitude) - 799, 1] * M_PI_180,
                           SwitchAngles._3WideAlpha[(int)(1000 * amplitude) - 799, 2] * M_PI_180,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           'B', sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.CHMP_5:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._2Alpha[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                           SwitchAngles._2Alpha[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           SwitchAngles._2Alpha_Polary[(int)(1000 * amplitude) + 1], sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.CHMP_Wide_5:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._2WideAlpha[(int)(1000 * amplitude) - 799, 0] * M_PI_180,
                           SwitchAngles._2WideAlpha[(int)(1000 * amplitude) - 799, 1] * M_PI_180,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           'A', sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.CHMP_Wide_3:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._WideAlpha[(int)(500 * amplitude) + 1] * M_PI_180,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           'B', sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.SHEP_3:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._1Alpha_SHE[(int)(1000 * amplitude) + 1] * M_PI_180,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           'B', sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.SHEP_5:
                    {
                        return GetPulseWithSwitchAngle(
                           SwitchAngles._2Alpha_SHE[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                           SwitchAngles._2Alpha_SHE[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           M_PI_2,
                           'A', sin_time, sin_angle_freq, initial_phase);
                    }
                case PulseModeName.SHEP_7:
                    {
                        return GetPulseWithSwitchAngle(
                              SwitchAngles._3Alpha_SHE[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                              SwitchAngles._3Alpha_SHE[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                              SwitchAngles._3Alpha_SHE[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                              M_PI_2,
                              M_PI_2,
                              M_PI_2,
                              M_PI_2,
                              'B', sin_time, sin_angle_freq, initial_phase);

                    }
                case PulseModeName.SHEP_11:
                    {
                        return GetPulseWithSwitchAngle(
                              SwitchAngles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 0] * M_PI_180,
                              SwitchAngles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 1] * M_PI_180,
                              SwitchAngles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 2] * M_PI_180,
                              SwitchAngles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 3] * M_PI_180,
                              SwitchAngles._5Alpha_SHE[(int)(1000 * amplitude) + 1, 4] * M_PI_180,
                              M_PI_2,
                              M_PI_2,
                              'A', sin_time, sin_angle_freq, initial_phase);
                    }
                default: break;
            }

            // HOP
            if (pulse_name == PulseModeName.HOP_5 || pulse_name == PulseModeName.HOP_7 || pulse_name == PulseModeName.HOP_9 ||
                pulse_name == PulseModeName.HOP_11 || pulse_name == PulseModeName.HOP_13 || pulse_name == PulseModeName.HOP_15 ||
                pulse_name == PulseModeName.HOP_17)
            {
                double sine_x = sin_angle_freq * sin_time + initial_phase;
                int[] keys = [];
                if(pulse_name == PulseModeName.HOP_5) keys = [9, 2, 13, 2, 17, 2, 21, 2, 25, 2, 29, 2, 33, 2, 37, 2,];
                else if(pulse_name == PulseModeName.HOP_7) keys = [15, 4, 15, 3, 7, 1, 11, 2, 19, 4, 23, 4, 27, 4, 31, 4, 35, 4, 39, 4,];
                else if (pulse_name == PulseModeName.HOP_9) keys = [21, 6, 13, 3, 17, 4, 25, 6, 29, 6, 33, 6, 37, 6];
                else if (pulse_name == PulseModeName.HOP_11) keys = [27, 8, 19, 5, 23, 6, 31, 8, 35, 8, 39, 8];
                else if (pulse_name == PulseModeName.HOP_13) keys = [25, 7, 29, 8, 33, 10, 37, 10];
                else if (pulse_name == PulseModeName.HOP_15) keys = [31, 9, 35, 10, 39, 12];
                else if (pulse_name == PulseModeName.HOP_17) keys = [37, 11];
                int alt = ((int)pulse_mode.AltMode + 1) > (keys.Length / 2) ? 0 : (int)pulse_mode.AltMode;
                return GetHopPulse(sine_x, amplitude, keys[2 * alt], keys[2 * alt + 1]);
            }
                

            if (
                pulse_name == PulseModeName.CHMP_3 ||
                pulse_mode.Square && PulseModeConfiguration.IsPulseSquareAvail(pulse_mode, 2)
            )
            {
                bool is_shift = pulse_mode.Shift;
                int pulse_num = PulseModeConfiguration.GetPulseNum(pulse_mode, 2);
                double pulse_initial_phase = PulseModeConfiguration.GetPulseInitial(pulse_mode, 2);
                return GetPulseWithSaw(sin_angle_freq * sin_time + initial_phase, pulse_initial_phase, amplitude, pulse_num, is_shift);
            }

            if (pulse_name == PulseModeName.P_17 || pulse_name == PulseModeName.P_13 || pulse_name == PulseModeName.P_9 || pulse_name == PulseModeName.P_5)
            {
                if (pulse_mode.AltMode == PulseAlternativeMode.Alt1)
                {
                    int pulse_num = 27;
                    double x = sin_angle_freq * sin_time + initial_phase;
                    double saw_value = GetSaw(pulse_num * x);
                    double sin_value = GetSineValueWithHarmonic(pulse_mode.Clone(), x, amplitude, control.GetGenerationCurrentTime(), initial_phase);
                    int pwm_value;
                    double fixed_x = (int)(x / M_PI_2) % 2 == 1 ? M_PI_2 - x % M_PI_2 : x % M_PI_2;
                    control.SetSawAngleFrequency(sin_angle_freq * pulse_num);
                    control.SetSawTime(sin_time);
                    if (fixed_x < M_PI * PulseModeConfiguration.GetPulseNum(pulse_mode, 2) / 54)
                    {
                        pwm_value = ModulateSin(sin_value, saw_value) * 2;
                    }
                    else
                    {
                        pwm_value = (int)(x / M_PI_2) % 4 > 1 ? 0 : 2;
                    }
                    return pwm_value;

                }
            }

            //sync mode but no the above.
            {
                int pulse_num = PulseModeConfiguration.GetPulseNum(pulse_mode, 2);
                double sine_x = sin_angle_freq * sin_time + initial_phase;
                if (pulse_mode.DiscreteTime.Enabled) sine_x = DiscreteTimeLine(sine_x, pulse_mode.DiscreteTime.Steps, pulse_mode.DiscreteTime.Mode);
                double saw_x = pulse_num * (sin_angle_freq * sin_time + initial_phase);
                double saw_value = GetSaw(saw_x);
                double sin_value = GetSineValueWithHarmonic(pulse_mode.Clone(), sine_x, amplitude, control.GetGenerationCurrentTime(), initial_phase);

                if (pulse_mode.Shift)
                    saw_value = -saw_value;

                int pwm_value = ModulateSin(sin_value, saw_value) * 2;

                control.SetSawAngleFrequency(sin_angle_freq * pulse_num);
                control.SetSawTime(sin_time);
                //Console.WriteLine(pwm_value);
                return pwm_value;
            }




        }
    }

    public class SVM
    {
        private static readonly double SQRT3 = Math.Sqrt(3);
        private static readonly double SQRT3_2 = Math.Sqrt(3) / 2.0;
        //private double __2PI_3 = Math.PI * 2 / 3.0;
        public class FunctionTime
        {
            public double T0;
            public double T1;
            public double T2;

            public static FunctionTime operator *(FunctionTime a, double d) => new()
            {
                T0 = a.T0 * d,
                T1 = a.T1 * d,
                T2 = a.T2 * d
            };

            public static FunctionTime operator *(double d, FunctionTime a) => new()
            {
                T0 = a.T0 * d,
                T1 = a.T1 * d,
                T2 = a.T2 * d
            };
        };

        public class Vabc
        {
            public double Ua;
            public double Ub;
            public double Uc;

            public static Vabc operator +(Vabc a, double d) => new()
            {
                Ua = a.Ua + d,
                Ub = a.Ub + d,
                Uc = a.Uc + d
            };

            public static Vabc operator *(Vabc a, double d) => new()
            {
                Ua = a.Ua * d,
                Ub = a.Ub * d,
                Uc = a.Uc * d
            };

            public static Vabc operator -(Vabc a) => new()
            {
                Ua = -a.Ua,
                Ub = -a.Ub,
                Uc = -a.Uc,
            };

            public static Vabc operator -(Vabc a, Vabc b) => new()
            {
                Ua = a.Ua - b.Ua,
                Ub = a.Ub - b.Ub,
                Uc = a.Uc - b.Uc
            };
        };

        public class Valbe
        {
            public double Ualpha;
            public double Ubeta;
        };
        public static int EstimateSector(Valbe U)
        {
            int A = U.Ubeta > 0.0 ? 0 : 1;
            int B = U.Ubeta - SQRT3 * U.Ualpha > 0.0 ? 0 : 1;
            int C = U.Ubeta + SQRT3 * U.Ualpha > 0.0 ? 0 : 1;
            return (4 * A + 2 * B + C) switch
            {
                0 => 2,
                1 => 3,
                2 => 1,
                3 => 0,
                4 => 0,
                5 => 4,
                6 => 6,
                7 => 5,
                _ => 2,
            };
        }
        public static FunctionTime GetFunctionTime(Valbe Vin, int sector)
        {
            FunctionTime ft = new();
            switch (sector)
            {
                case 1:
                    {
                        ft.T1 = SQRT3_2 * Vin.Ualpha - 0.5 * Vin.Ubeta;
                        ft.T2 = Vin.Ubeta;
                    }
                    break;
                case 2:
                    {
                        ft.T1 = SQRT3_2 * Vin.Ualpha + 0.5 * Vin.Ubeta;
                        ft.T2 = 0.5 * Vin.Ubeta - SQRT3_2 * Vin.Ualpha;
                    }
                    break;
                case 3:
                    {
                        ft.T1 = Vin.Ubeta;
                        ft.T2 = -(SQRT3_2 * Vin.Ualpha + 0.5 * Vin.Ubeta);
                    }
                    break;
                case 4:
                    {
                        ft.T1 = 0.5 * Vin.Ubeta - SQRT3_2 * Vin.Ualpha;
                        ft.T2 = -Vin.Ubeta;
                    }
                    break;
                case 5:
                    {
                        ft.T1 = -(SQRT3_2 * Vin.Ualpha + 0.5 * Vin.Ubeta);
                        ft.T2 = SQRT3_2 * Vin.Ualpha - 0.5 * Vin.Ubeta;
                    }
                    break;
                case 6:
                    {
                        ft.T1 = -Vin.Ubeta;
                        ft.T2 = SQRT3_2 * Vin.Ualpha + 0.5 * Vin.Ubeta;
                    }
                    break;
            }
            ft.T0 = 1.0 - ft.T1 - ft.T2;
            return ft;
        }
        public static Vabc GetAbcVoltage(FunctionTime Tin, int sector)
        {
            Vabc v_out = new();
            switch (sector)
            {
                case 1:
                    {
                        v_out.Ua = 0.5 * Tin.T0;
                        v_out.Ub = Tin.T1 + 0.5 * Tin.T0;
                        v_out.Uc = Tin.T1 + Tin.T2 + 0.5 * Tin.T0;
                    }
                    break;
                case 2:
                    {
                        v_out.Ua = Tin.T2 + 0.5 * Tin.T0;
                        v_out.Ub = 0.5 * Tin.T0;
                        v_out.Uc = Tin.T1 + Tin.T2 + 0.5 * Tin.T0;
                    }
                    break;
                case 3:
                    {
                        v_out.Ua = Tin.T1 + Tin.T2 + 0.5 * Tin.T0;
                        v_out.Ub = 0.5 * Tin.T0;
                        v_out.Uc = Tin.T1 + 0.5 * Tin.T0;
                    }
                    break;
                case 4:
                    {
                        v_out.Ua = Tin.T1 + Tin.T2 + 0.5 * Tin.T0;
                        v_out.Ub = Tin.T2 + 0.5 * Tin.T0;
                        v_out.Uc = 0.5 * Tin.T0;
                    }
                    break;
                case 5:
                    {
                        v_out.Ua = Tin.T1 + 0.5 * Tin.T0;
                        v_out.Ub = Tin.T1 + Tin.T2 + 0.5 * Tin.T0;
                        v_out.Uc = 0.5 * Tin.T0;
                    }
                    break;
                case 6:
                    {
                        v_out.Ua = 0.5 * Tin.T0;
                        v_out.Ub = Tin.T1 + Tin.T2 + 0.5 * Tin.T0;
                        v_out.Uc = Tin.T2 + 0.5 * Tin.T0;
                    }
                    break;
            }
            return v_out;
        }
        public static Valbe Clark(Vabc V0)
        {
            Valbe Vin = new()
            {
                Ualpha = 0.66666666666666666666666666666667 * (V0.Ua - 0.5 * V0.Ub - 0.5 * V0.Uc),
                Ubeta = 0.66666666666666666666666666666667 * (SQRT3_2 * (V0.Ub - V0.Uc))
            };
            return Vin;
        }

    }
}
