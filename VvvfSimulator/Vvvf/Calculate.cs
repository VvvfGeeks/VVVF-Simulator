using System;
using System.Collections.Generic;
using static VvvfSimulator.Vvvf.MyMath;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAmplitude.YamlControlDataAmplitude;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;


namespace VvvfSimulator.Vvvf
{
    public class Calculate
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

        public static double GetSineValueWithHarmonic(YamlPulseMode mode, double x, double amplitude, double T, double InitialPhase)
        {
            double BaseValue = mode.BaseWave switch
            {
                BaseWaveType.Sine => GetSine(x),
                BaseWaveType.Saw => -GetSaw(x),
                BaseWaveType.ModifiedSine1 => GetModifiedSine(x, 1),
                BaseWaveType.ModifiedSine2 => GetModifiedSine(x, 2),
                BaseWaveType.ModifiedSaw1 => GetModifiedSaw(x),
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
            double seed = x % M_2PI * level / M_2PI;
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
            double fixed_x = x % M_PI / (M_PI / totalSteps);
            double saw_value = -GetSaw(carrier * x);
            double modulated;
            if (fixed_x > totalSteps - 1) modulated = -1;
            else if (fixed_x > totalSteps / 2 + width) modulated = 1;
            else if (fixed_x > totalSteps / 2 - width) modulated = 2 * amplitude - 1;
            else if (fixed_x > 1) modulated = 1;
            else modulated = -1;
            if (x % M_2PI > M_PI) modulated = -modulated;

            return ModulateSin(modulated, saw_value) * 2;
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
            public AmplitudeArgument(YamlControlDataAmplitudeParameter config, double current)
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
            if (data.Range == 0) return data.BaseFrequency;

            if (control.IsRandomFrequencyMoveAllowed())
            {
                double random_freq;
                if (control.GetRandomFrequencyPreviousTime() == 0 || control.GetPreviousSawRandomFrequency() == 0)
                {
                    Random rnd = new();
                    double diff_freq = rnd.NextDouble() * data.Range;
                    if (rnd.NextDouble() < 0.5) diff_freq = -diff_freq;
                    double silent_random_freq = data.BaseFrequency + diff_freq;
                    random_freq = silent_random_freq;
                    control.SetPreviousSawRandomFrequency(silent_random_freq);
                    control.SetRandomFrequencyPreviousTime(control.GetGenerationCurrentTime());
                }
                else
                {
                    random_freq = control.GetPreviousSawRandomFrequency();
                }

                if (control.GetRandomFrequencyPreviousTime() + data.Interval < control.GetGenerationCurrentTime())
                    control.SetRandomFrequencyPreviousTime(0);

                return random_freq;
            }
            else
            {
                return data.BaseFrequency;
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

            if (control.GetSineFrequency() < value.MinimumFrequency && control.GetControlFrequency() > 0) control.SetVideoSineFrequency(value.MinimumFrequency);
            else control.SetVideoSineFrequency(control.GetSineFrequency());

            if (value.None) return new WaveValues(0, 0, 0);

            control.SetVideoPulseMode(value.PulseMode);
            control.SetVideoSineAmplitude(value.Amplitude);
            if (value.Carrier != null) control.SetVideoCarrierFrequency(value.Carrier.Clone());
            control.SetVideoCalculatedPulseData(value.PulseData);

            int U = 0, V = 0, W = 0;

            for (int i = 0; i < 3; i++)
            {

                int val;
                double initial = M_2PI / 3.0 * i + add_initial;
                try
                {
                    if (value.Level == 2) val = CalculateTwoLevel(control, value, initial);
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

        public static int CalculateThreeLevel(VvvfValues Control, PwmCalculateValues Value, double InitialPhase)
        {
            double SineAngleFrequency = Control.GetSineAngleFrequency();
            double SineTime = Control.GetSineTime();
            if (SineAngleFrequency < Value.MinimumFrequency * M_2PI && Control.GetControlFrequency() > 0)
            {
                Control.SetSineTimeChangeAllowed(false);
                SineAngleFrequency = Value.MinimumFrequency * M_2PI;
            }
            else
                Control.SetSineTimeChangeAllowed(true);

            double SawAngleFrequency = Control.GetSawAngleFrequency();
            double SawTime = Control.GetSawTime();

            double Amplitude = Value.Amplitude;
            YamlPulseMode PulseMode = Value.PulseMode;
            PulseTypeName PulseType = PulseMode.PulseType;
            PulseAlternative Alternate = PulseMode.Alternative;
            Dictionary<PulseDataKey, double> PulseData = Value.PulseData;
            int PulseCount = PulseMode.PulseCount;
            CarrierFreq Carrier = Value.Carrier;

            double SineX = SineAngleFrequency * SineTime + InitialPhase;
            if (PulseMode.DiscreteTime.Enabled) SineX = DiscreteTimeLine(SineX, PulseMode.DiscreteTime.Steps, PulseMode.DiscreteTime.Mode);    

            switch (PulseType)
            {
                case PulseTypeName.ASYNC:
                    {
                        double NewSawAngleFrequency = Carrier.Range == 0 ? Carrier.BaseFrequency * M_2PI : GetRandomFrequency(Carrier, Control) * M_2PI;

                        if (NewSawAngleFrequency == 0)
                            SawTime = 0;
                        else
                            SawTime = SawAngleFrequency / NewSawAngleFrequency * SawTime;
                        SawAngleFrequency = NewSawAngleFrequency;

                        Control.SetSawAngleFrequency(SawAngleFrequency);
                        Control.SetSawTime(SawTime);

                        double SineVal = GetSineValueWithHarmonic(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                        double SawVal = GetSaw(Control.GetSawTime() * Control.GetSawAngleFrequency());
                        if (PulseMode.Shift)
                            SawVal = -SawVal;

                        double Dipolar = PulseData.GetValueOrDefault(PulseDataKey.Dipolar, -1);
                        SawVal *= (Dipolar != -1 ? Dipolar : 0.5);

                        return ModulateSin(SineVal, SawVal + 0.5) + ModulateSin(SineVal, SawVal - 0.5);
                    }
                case PulseTypeName.SYNC:
                    {
                        if (PulseCount == 1) // 1P ALTERNATE
                        {
                            if (Alternate == PulseAlternative.Alt1)
                            {
                                double SineVal = GetSine(SineX);
                                int D = SineVal > 0 ? 1 : -1;
                                double voltage_fix = D * (1 - Amplitude);

                                int gate = D * (SineVal - voltage_fix) > 0 ? D : 0;
                                gate += 1;
                                return gate;
                            }
                        }

                        if(PulseCount == 5)
                        {
                            if(Alternate == PulseAlternative.Alt1)
                            {
                                double Period = SineX % M_2PI;
                                int Orthant = (int)(Period / M_PI_2);
                                double Quater = Period % M_PI_2;

                                int _GetPwm(double t)
                                {
                                    double a = M_PI_2 - Value.Amplitude;
                                    double b = PulseData.GetValueOrDefault(PulseDataKey.L3P3Alt1Width, 0);
                                    if (t < a) return 1;
                                    if (t < a + b) return 2;
                                    if (t < a + 2 * b) return 1;
                                    return 2;
                                }

                                return Orthant switch
                                {
                                    0 => _GetPwm(Quater),
                                    1 => _GetPwm(M_PI_2 - Quater),
                                    2 => 2 - _GetPwm(Quater),
                                    _ => 2 - _GetPwm(M_PI_2 - Quater)
                                };
                            }
                        }

                        { // nP DEFAULT
                            double SineVal = GetSineValueWithHarmonic(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                            double SawVal = GetSaw(PulseCount * SineX);
                            if (PulseMode.Shift)
                                SawVal = -SawVal;

                            double Dipolar = PulseData.GetValueOrDefault(PulseDataKey.Dipolar, -1);
                            SawVal *= (Dipolar != -1 ? Dipolar : 0.5);

                            Control.SetSawAngleFrequency(SineAngleFrequency * PulseCount);
                            Control.SetSawTime(SineTime);

                            return ModulateSin(SineVal, SawVal + 0.5) + ModulateSin(SineVal, SawVal - 0.5);
                        }                        
                    }
                case PulseTypeName.SHE:
                    {
                        return PulseCount switch
                        {
                            21 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She21Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            19 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She19Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            17 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She17Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            15 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She15Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            13 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She13Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            11 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She11Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            9 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She9Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            7 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She7Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            5 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She5Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            3 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She3Default.GetPwm(Value.Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwm.L3She3Alt1.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            1 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L3She1Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            _ => 0,
                        };
                    }
                case PulseTypeName.CHM:
                    {
                        switch(PulseCount)
                        {
                            case 21:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm21Default.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm21Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm21Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L3Chm21Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L3Chm21Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L3Chm21Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L3Chm21Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L3Chm21Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L3Chm21Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L3Chm21Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L3Chm21Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L3Chm21Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L3Chm21Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L3Chm21Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwm.L3Chm21Alt14.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwm.L3Chm21Alt15.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwm.L3Chm21Alt16.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwm.L3Chm21Alt17.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwm.L3Chm21Alt18.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwm.L3Chm21Alt19.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt20 => CustomPwm.L3Chm21Alt20.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt21 => CustomPwm.L3Chm21Alt21.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt22 => CustomPwm.L3Chm21Alt22.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 19:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm19Default.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm19Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm19Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L3Chm19Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L3Chm19Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L3Chm19Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L3Chm19Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L3Chm19Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L3Chm19Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L3Chm19Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L3Chm19Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L3Chm19Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L3Chm19Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L3Chm19Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwm.L3Chm19Alt14.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwm.L3Chm19Alt15.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwm.L3Chm19Alt16.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwm.L3Chm19Alt17.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwm.L3Chm19Alt18.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwm.L3Chm19Alt19.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt20 => CustomPwm.L3Chm19Alt20.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt21 => CustomPwm.L3Chm19Alt21.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt22 => CustomPwm.L3Chm19Alt22.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt23 => CustomPwm.L3Chm19Alt23.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt24 => CustomPwm.L3Chm19Alt24.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt25 => CustomPwm.L3Chm19Alt25.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 17:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm17Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm17Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm17Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L3Chm17Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L3Chm17Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L3Chm17Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L3Chm17Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L3Chm17Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L3Chm17Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L3Chm17Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L3Chm17Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L3Chm17Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L3Chm17Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L3Chm17Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwm.L3Chm17Alt14.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwm.L3Chm17Alt15.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwm.L3Chm17Alt16.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwm.L3Chm17Alt17.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwm.L3Chm17Alt18.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwm.L3Chm17Alt19.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 15:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm15Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm15Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm15Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L3Chm15Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L3Chm15Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L3Chm15Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L3Chm15Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L3Chm15Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L3Chm15Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L3Chm15Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L3Chm15Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L3Chm15Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L3Chm15Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L3Chm15Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwm.L3Chm15Alt14.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwm.L3Chm15Alt15.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwm.L3Chm15Alt16.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwm.L3Chm15Alt17.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 13:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm13Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm13Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm13Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L3Chm13Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L3Chm13Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L3Chm13Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L3Chm13Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L3Chm13Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L3Chm13Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L3Chm13Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L3Chm13Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L3Chm13Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L3Chm13Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L3Chm13Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwm.L3Chm13Alt14.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 11:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm11Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm11Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm11Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L3Chm11Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L3Chm11Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L3Chm11Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L3Chm11Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L3Chm11Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L3Chm11Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L3Chm11Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L3Chm11Alt10.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 9:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm9Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm9Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm9Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L3Chm9Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L3Chm9Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L3Chm9Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L3Chm9Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L3Chm9Alt7.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 7:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm7Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm7Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm7Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L3Chm7Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L3Chm7Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L3Chm7Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L3Chm7Alt6.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 5:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm5Default.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm5Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm5Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L3Chm5Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L3Chm5Alt4.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 3:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm3Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L3Chm3Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L3Chm3Alt2.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 1:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L3Chm1Default.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                        }
                        return 0;
                    }
                default:
                    return 0;
            }
        }

        public static int CalculateTwoLevel(VvvfValues Control, PwmCalculateValues Value, double InitialPhase)
        {
            double SineAngleFrequency = Control.GetSineAngleFrequency();
            double SineTime = Control.GetSineTime();
            if (SineAngleFrequency < Value.MinimumFrequency * M_2PI && Control.GetControlFrequency() > 0)
            {
                Control.SetSineTimeChangeAllowed(false);
                SineAngleFrequency = Value.MinimumFrequency * M_2PI;
            }
            else
                Control.SetSineTimeChangeAllowed(true);

            double SawAngleFrequency = Control.GetSawAngleFrequency();
            double SawTime = Control.GetSawTime();

            double Amplitude = Value.Amplitude;
            YamlPulseMode PulseMode = Value.PulseMode;
            PulseTypeName PulseType = PulseMode.PulseType;
            PulseAlternative Alternate = PulseMode.Alternative;
            int PulseCount = PulseMode.PulseCount;
            CarrierFreq Carrier = Value.Carrier;

            double SineX = SineAngleFrequency * SineTime + InitialPhase;
            if (PulseMode.DiscreteTime.Enabled) SineX = DiscreteTimeLine(SineX, PulseMode.DiscreteTime.Steps, PulseMode.DiscreteTime.Mode);

            if (Value.None)
                return 0;

            switch (PulseType)
            {
                case PulseTypeName.ASYNC:
                    {
                        double desire_saw_angle_freq = Carrier.Range == 0 ? Carrier.BaseFrequency * M_2PI : GetRandomFrequency(Carrier, Control) * M_2PI;

                        if (desire_saw_angle_freq == 0)
                            SawTime = 0;
                        else
                            SawTime = SawAngleFrequency / desire_saw_angle_freq * SawTime;
                        SawAngleFrequency = desire_saw_angle_freq;

                        double SineVal = GetSineValueWithHarmonic(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                        double SawVal = GetSaw(SawTime * SawAngleFrequency);

                        Control.SetSawAngleFrequency(SawAngleFrequency);
                        Control.SetSawTime(SawTime);

                        return ModulateSin(SineVal, SawVal) * 2;
                    }
                case PulseTypeName.SYNC:
                    {
                        // SYNC 3 ALTERNATE 1
                        if(PulseCount == 3 && PulseMode.Alternative == PulseAlternative.Alt1)
                        {
                            double SineVal = GetSine(SineX);
                            double SawVal = GetSaw(SineX);
                            double Pwm = (SineVal - SawVal > 0 ? 1 : -1) * Amplitude;
                            double Negate = SawVal > 0 ? SawVal - 1 : SawVal + 1;
                            return ModulateSin(Pwm, Negate) * 2;
                        }

                        // SYNC 5 9 13 17 ALTERNATE 1
                        if((PulseCount == 5 || PulseCount == 9 || PulseCount == 13 || PulseCount == 17) && Alternate == PulseAlternative.Alt1)
                        {
                            double saw_value = GetSaw(27 * SineX);
                            double sin_value = GetSineValueWithHarmonic(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                            double FixedSineX = (int)(SineX / M_PI_2) % 2 == 1 ? M_PI_2 - SineX % M_PI_2 : SineX % M_PI_2;
                            Control.SetSawAngleFrequency(27 * SineAngleFrequency);
                            Control.SetSawTime(SineTime);
                            return (FixedSineX < M_PI * PulseMode.PulseCount / 54) ? ModulateSin(sin_value, saw_value) * 2 : (int)(SineX / M_PI_2) % 4 > 1 ? 0 : 2;
                        }

                        // SYNC N WITH SQUARE CONFIGURATION
                        if (PulseMode.Square && PulseModeConfiguration.IsPulseSquareAvail(PulseMode, 2))
                        {
                            PulseCount = PulseMode.PulseCount % 2 == 0 ? (int)(PulseMode.PulseCount * 1.5) : (int)((PulseMode.PulseCount - 1) * 1.5);
                            double CarrierPhase = PulseMode.PulseCount % 2 == 0 ? M_PI_2 : 0;
                            double CarrierVal = -GetSaw(PulseCount * SineX + CarrierPhase);
                            double SawVal = -GetSaw(SineX);
                            if (PulseMode.Shift)
                                SawVal = -SawVal;
                            return ModulateSin(SawVal > 0 ? Amplitude : -Amplitude, CarrierVal) * 2;
                        }

                        // SYNC N
                        {
                            double SineVal = GetSineValueWithHarmonic(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                            double SawVal = GetSaw(PulseMode.PulseCount * (SineAngleFrequency * SineTime + InitialPhase));
                            if (PulseMode.Shift)
                                SawVal = -SawVal;
                            Control.SetSawAngleFrequency(SineAngleFrequency * PulseMode.PulseCount);
                            Control.SetSawTime(SineTime);
                            return ModulateSin(SineVal, SawVal) * 2; ;
                        }
                    }
                case PulseTypeName.HO:
                    {
                        int[] Keys = PulseCount switch
                        {
                            5 => [9, 2, 13, 2, 17, 2, 21, 2, 25, 2, 29, 2, 33, 2, 37, 2,],
                            7 => [15, 4, 15, 3, 7, 1, 11, 2, 19, 4, 23, 4, 27, 4, 31, 4, 35, 4, 39, 4,],
                            9 => [21, 6, 13, 3, 17, 4, 25, 6, 29, 6, 33, 6, 37, 6],
                            11 => [27, 8, 19, 5, 23, 6, 31, 8, 35, 8, 39, 8],
                            13 => [25, 7, 29, 8, 33, 10, 37, 10],
                            15 => [31, 9, 35, 10, 39, 12],
                            17 => [37, 11],
                            _ => [0],
                        };
                        int Index = (int)PulseMode.Alternative + 1 > Keys.Length / 2 ? 0 : (int)PulseMode.Alternative;
                        return GetHopPulse(SineX, Amplitude, Keys[2 * Index], Keys[2 * Index + 1]);
                    }
                case PulseTypeName.SHE:
                    {
                        return PulseCount switch
                        {
                            3 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L2She3Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwm.L2She3Alt1.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            5 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L2She5Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwm.L2She5Alt1.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt2 => CustomPwm.L2She5Alt2.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            7 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L2She7Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwm.L2She7Alt1.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            9 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L2She9Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwm.L2She9Alt1.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt2 => CustomPwm.L2She9Alt2.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            11 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L2She11Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwm.L2She11Alt1.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            13 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L2She13Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwm.L2She13Alt1.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            15 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwm.L2She15Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwm.L2She15Alt1.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            _ => 0,
                        };
                    }
                case PulseTypeName.CHM:
                    {
                        switch (PulseCount)
                        {
                            case 25:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm25Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm25Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm25Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm25Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm25Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm25Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L2Chm25Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L2Chm25Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L2Chm25Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L2Chm25Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L2Chm25Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L2Chm25Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L2Chm25Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L2Chm25Alt13.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwm.L2Chm25Alt14.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwm.L2Chm25Alt15.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwm.L2Chm25Alt16.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwm.L2Chm25Alt17.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwm.L2Chm25Alt18.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwm.L2Chm25Alt19.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt20 => CustomPwm.L2Chm25Alt20.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 23:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm23Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm23Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm23Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm23Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm23Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm23Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L2Chm23Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L2Chm23Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L2Chm23Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L2Chm23Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L2Chm23Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L2Chm23Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L2Chm23Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L2Chm23Alt13.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwm.L2Chm23Alt14.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 21:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm21Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm21Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm21Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm21Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm21Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm21Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L2Chm21Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L2Chm21Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L2Chm21Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L2Chm21Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L2Chm21Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L2Chm21Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L2Chm21Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L2Chm21Alt13.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 19:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm19Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm19Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm19Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm19Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm19Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm19Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L2Chm19Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L2Chm19Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L2Chm19Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L2Chm19Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L2Chm19Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L2Chm19Alt11.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 17:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm17Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm17Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm17Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm17Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm17Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm17Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L2Chm17Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L2Chm17Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L2Chm17Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L2Chm17Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L2Chm17Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L2Chm17Alt11.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 15:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm15Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm15Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm15Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm15Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm15Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm15Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L2Chm15Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L2Chm15Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L2Chm15Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L2Chm15Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L2Chm15Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L2Chm15Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L2Chm15Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L2Chm15Alt13.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwm.L2Chm15Alt14.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwm.L2Chm15Alt15.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwm.L2Chm15Alt16.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwm.L2Chm15Alt17.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwm.L2Chm15Alt18.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwm.L2Chm15Alt19.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt20 => CustomPwm.L2Chm15Alt20.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt21 => CustomPwm.L2Chm15Alt21.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt22 => CustomPwm.L2Chm15Alt22.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt23 => CustomPwm.L2Chm15Alt23.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 13:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm13Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm13Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm13Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm13Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm13Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm13Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L2Chm13Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L2Chm13Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L2Chm13Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L2Chm13Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L2Chm13Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L2Chm13Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwm.L2Chm13Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwm.L2Chm13Alt13.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 11:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm11Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm11Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm11Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm11Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm11Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm11Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L2Chm11Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L2Chm11Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L2Chm11Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwm.L2Chm11Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwm.L2Chm11Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwm.L2Chm11Alt11.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 9:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm9Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm9Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm9Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm9Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm9Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm9Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwm.L2Chm9Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwm.L2Chm9Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwm.L2Chm9Alt8.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 7:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm7Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm7Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm7Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm7Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwm.L2Chm7Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwm.L2Chm7Alt5.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 5:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm5Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm5Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwm.L2Chm5Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwm.L2Chm5Alt3.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 3:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwm.L2Chm3Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwm.L2Chm3Alt1.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            default:
                                return 0;
                        }
                    }
                default:
                    return 0;
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
