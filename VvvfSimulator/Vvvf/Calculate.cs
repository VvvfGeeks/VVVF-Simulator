using System;
using System.Collections.Generic;
using static VvvfSimulator.Vvvf.MyMath;
using static VvvfSimulator.Vvvf.MyMath.Functions;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAmplitude;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAmplitude.AmplitudeParameter;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;


namespace VvvfSimulator.Vvvf
{
    public class Calculate
    {
        public static double GetModifiedSine(double x, int level)
        {
            double sine = Sine(x) * level;
            double value = Math.Round(sine) / level;
            return value;
        }

        public static double GetModifiedSaw(double x)
        {
            double sine = -Saw(x) * M_PI_2;
            int D = sine > 0 ? 1 : -1;
            if (Math.Abs(sine) > 0.5) sine = D;

            return sine;
        }

        public static double GetBaseWaveform(YamlPulseMode mode, double x, double amplitude, double T, double InitialPhase)
        {
            double BaseValue = mode.BaseWave switch
            {
                BaseWaveType.Sine => Sine(x),
                BaseWaveType.Saw => -Saw(x),
                BaseWaveType.Square => Square(x),
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
                    PulseHarmonic.PulseHarmonicType.Sine => Sine(harmonic_x),
                    PulseHarmonic.PulseHarmonicType.Saw => -Saw(harmonic_x),
                    PulseHarmonic.PulseHarmonicType.Square => Square(harmonic_x),
                    _ => throw new NotImplementedException(),
                };
                BaseValue += harmonic_value * harmonic.Amplitude * (harmonic.IsAmplitudeProportional ? amplitude : 1);
            }

            BaseValue = BaseValue > 1 ? 1 : BaseValue < -1 ? -1 : BaseValue;

            return BaseValue;
        }

        public static int ModulateSignal(double sin_value, double saw_value)
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
            double saw_value = -Saw(carrier * x);
            double modulated;
            if (fixed_x > totalSteps - 1) modulated = -1;
            else if (fixed_x > totalSteps / 2 + width) modulated = 1;
            else if (fixed_x > totalSteps / 2 - width) modulated = 2 * amplitude - 1;
            else if (fixed_x > 1) modulated = 1;
            else modulated = -1;
            if (x % M_2PI > M_PI) modulated = -modulated;

            return ModulateSignal(modulated, saw_value) * 2;
        }

        public static double GetAmplitude(AmplitudeParameter Param, double Current)
        {
            double Amplitude = 0;

            if (Param.EndAmplitude == Param.StartAmplitude) Amplitude = Param.StartAmplitude;
            else if (Param.Mode == AmplitudeMode.Linear)
            {
                if (!Param.DisableRangeLimit)
                {
                    if (Current < Param.StartFrequency) Current = Param.StartFrequency;
                    if (Current > Param.EndFrequency) Current = Param.EndFrequency;
                }
                Amplitude = (Param.EndAmplitude - Param.StartAmplitude) / (Param.EndFrequency - Param.StartFrequency) * (Current - Param.StartFrequency) + Param.StartAmplitude;
            }
            else if (Param.Mode == AmplitudeMode.InverseProportional)
            {
                if (!Param.DisableRangeLimit)
                {
                    if (Current < Param.StartFrequency) Current = Param.StartFrequency;
                    if (Current > Param.EndFrequency) Current = Param.EndFrequency;
                }

                double x = (1.0 / Param.EndAmplitude - 1.0 / Param.StartAmplitude) / (Param.EndFrequency - Param.StartFrequency) * (Current - Param.StartFrequency) + 1.0 / Param.StartAmplitude;

                double c = -Param.CurveChangeRate;
                double k = Param.EndAmplitude;
                double l = Param.StartAmplitude;
                double a = 1 / (1 / l - 1 / k) * (1 / (l - c) - 1 / (k - c));
                double b = 1 / (1 - 1 / l * k) * (1 / (l - c) - 1 / l * k / (k - c));

                Amplitude = 1 / (a * x + b) + c;
            }
            else if (Param.Mode == AmplitudeMode.Exponential)
            {
                if (!Param.DisableRangeLimit)
                {
                    if (Current > Param.EndFrequency) Current = Param.EndFrequency;
                }

                double t = 1 / Param.EndFrequency * Math.Log(Param.EndAmplitude + 1);

                Amplitude = Math.Pow(Math.E, t * Current) - 1;
            }
            else if (Param.Mode == AmplitudeMode.LinearPolynomial)
            {
                if (!Param.DisableRangeLimit)
                {
                    if (Current > Param.EndFrequency) Current = Param.EndFrequency;
                }
                Amplitude = Math.Pow(Current, Param.Polynomial) / Math.Pow(Param.EndFrequency, Param.Polynomial) * Param.EndAmplitude;
            }
            else if (Param.Mode == AmplitudeMode.Sine)
            {
                if (!Param.DisableRangeLimit)
                {
                    if (Current > Param.EndFrequency) Current = Param.EndFrequency;
                }

                double x = Math.PI * Current / (2.0 * Param.EndFrequency);

                Amplitude = Math.Sin(x) * Param.EndAmplitude;
            }
            else if (Param.Mode == AmplitudeMode.Table)
            {
                int TargetIndex = 0;
                for (int i = 0; i < Param.AmplitudeTable.Length; i++)
                {
                    if (Param.AmplitudeTable[i].Frequency > Current) break;
                    TargetIndex = i;
                }

                if (Param.AmplitudeTableInterpolation && (TargetIndex + 1 < Param.AmplitudeTable.Length))
                {
                    (double FrequencyStart, double AmplitudeStart) = Param.AmplitudeTable[TargetIndex];
                    (double FrequencyEnd, double AmplitudeEnd) = Param.AmplitudeTable[TargetIndex + 1];
                    Amplitude = (AmplitudeEnd - AmplitudeStart) / (FrequencyEnd - FrequencyStart) * (Current - FrequencyStart) + AmplitudeStart;
                }
                else
                {
                    Amplitude = Param.AmplitudeTable[TargetIndex].Amplitude;
                }
            }

            if (Param.CutOffAmplitude > Amplitude) Amplitude = 0;
            if (Param.MaxAmplitude != -1 && Param.MaxAmplitude < Amplitude) Amplitude = Param.MaxAmplitude;

            return Amplitude;
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

            if (Value.None)
                return 0;

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

                        double SineVal = GetBaseWaveform(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                        double SawVal = Saw(Control.GetSawTime() * Control.GetSawAngleFrequency());
                        
                        if (PulseMode.Alternative == PulseAlternative.Shifted)
                            SawVal = -SawVal;

                        double Dipolar = PulseData.GetValueOrDefault(PulseDataKey.Dipolar, -1);
                        SawVal *= (Dipolar != -1 ? Dipolar : 0.5);

                        return ModulateSignal(SineVal, SawVal + 0.5) + ModulateSignal(SineVal, SawVal - 0.5);
                    }
                case PulseTypeName.SYNC:
                    {
                        if (PulseCount == 1) // 1P ALTERNATE
                        {
                            if (Alternate == PulseAlternative.Alt1)
                            {
                                double SineVal = Sine(SineX);
                                int D = SineVal > 0 ? 1 : -1;
                                double voltage_fix = D * (1 - Amplitude);

                                int gate = D * (SineVal - voltage_fix) > 0 ? D : 0;
                                gate += 1;
                                return gate;
                            }
                        }

                        if(PulseCount == 5)
                        {
                            // SYNC 5 ALTERNATE 1
                            if(Alternate == PulseAlternative.Alt1)
                            {
                                double Period = SineX % M_2PI;
                                int Orthant = (int)(Period / M_PI_2);
                                double Quater = Period % M_PI_2;

                                int _GetPwm(double t)
                                {
                                    double a = M_PI_2 - Value.Amplitude;
                                    double b = PulseData.GetValueOrDefault(PulseDataKey.PulseWidth, 0);
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
                            double SineVal = GetBaseWaveform(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                            double SawVal = Saw(PulseCount * SineX);
                            if (PulseMode.Alternative == PulseAlternative.Shifted)
                                SawVal = -SawVal;

                            double Dipolar = PulseData.GetValueOrDefault(PulseDataKey.Dipolar, -1);
                            SawVal *= (Dipolar != -1 ? Dipolar : 0.5);

                            Control.SetSawAngleFrequency(SineAngleFrequency * PulseCount);
                            Control.SetSawTime(SineTime);

                            return ModulateSignal(SineVal, SawVal + 0.5) + ModulateSignal(SineVal, SawVal - 0.5);
                        }                        
                    }
                case PulseTypeName.SHE:
                    {
                        return PulseCount switch
                        {
                            21 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She21Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            19 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She19Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            17 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She17Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            15 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She15Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            13 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She13Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            11 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She11Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            9 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She9Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            7 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She7Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            5 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She5Default.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            3 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She3Default.GetPwm(Value.Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwmPresets.L3She3Alt1.GetPwm(Value.Amplitude, SineX),
                                _ => 0
                            },
                            1 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L3She1Default.GetPwm(Value.Amplitude, SineX),
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
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm21Default.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm21Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm21Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L3Chm21Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L3Chm21Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L3Chm21Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L3Chm21Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L3Chm21Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L3Chm21Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L3Chm21Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L3Chm21Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L3Chm21Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L3Chm21Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L3Chm21Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwmPresets.L3Chm21Alt14.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwmPresets.L3Chm21Alt15.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwmPresets.L3Chm21Alt16.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwmPresets.L3Chm21Alt17.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwmPresets.L3Chm21Alt18.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwmPresets.L3Chm21Alt19.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt20 => CustomPwmPresets.L3Chm21Alt20.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt21 => CustomPwmPresets.L3Chm21Alt21.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt22 => CustomPwmPresets.L3Chm21Alt22.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 19:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm19Default.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm19Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm19Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L3Chm19Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L3Chm19Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L3Chm19Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L3Chm19Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L3Chm19Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L3Chm19Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L3Chm19Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L3Chm19Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L3Chm19Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L3Chm19Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L3Chm19Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwmPresets.L3Chm19Alt14.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwmPresets.L3Chm19Alt15.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwmPresets.L3Chm19Alt16.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwmPresets.L3Chm19Alt17.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwmPresets.L3Chm19Alt18.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwmPresets.L3Chm19Alt19.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt20 => CustomPwmPresets.L3Chm19Alt20.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt21 => CustomPwmPresets.L3Chm19Alt21.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt22 => CustomPwmPresets.L3Chm19Alt22.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt23 => CustomPwmPresets.L3Chm19Alt23.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt24 => CustomPwmPresets.L3Chm19Alt24.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt25 => CustomPwmPresets.L3Chm19Alt25.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 17:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm17Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm17Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm17Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L3Chm17Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L3Chm17Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L3Chm17Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L3Chm17Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L3Chm17Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L3Chm17Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L3Chm17Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L3Chm17Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L3Chm17Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L3Chm17Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L3Chm17Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwmPresets.L3Chm17Alt14.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwmPresets.L3Chm17Alt15.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwmPresets.L3Chm17Alt16.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwmPresets.L3Chm17Alt17.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwmPresets.L3Chm17Alt18.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwmPresets.L3Chm17Alt19.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 15:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm15Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm15Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm15Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L3Chm15Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L3Chm15Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L3Chm15Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L3Chm15Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L3Chm15Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L3Chm15Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L3Chm15Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L3Chm15Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L3Chm15Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L3Chm15Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L3Chm15Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwmPresets.L3Chm15Alt14.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwmPresets.L3Chm15Alt15.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwmPresets.L3Chm15Alt16.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwmPresets.L3Chm15Alt17.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 13:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm13Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm13Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm13Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L3Chm13Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L3Chm13Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L3Chm13Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L3Chm13Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L3Chm13Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L3Chm13Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L3Chm13Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L3Chm13Alt10.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L3Chm13Alt11.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L3Chm13Alt12.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L3Chm13Alt13.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwmPresets.L3Chm13Alt14.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 11:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm11Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm11Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm11Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L3Chm11Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L3Chm11Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L3Chm11Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L3Chm11Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L3Chm11Alt7.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L3Chm11Alt8.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L3Chm11Alt9.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L3Chm11Alt10.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 9:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm9Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm9Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm9Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L3Chm9Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L3Chm9Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L3Chm9Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L3Chm9Alt6.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L3Chm9Alt7.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 7:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm7Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm7Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm7Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L3Chm7Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L3Chm7Alt4.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L3Chm7Alt5.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L3Chm7Alt6.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 5:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm5Default.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm5Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm5Alt2.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L3Chm5Alt3.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L3Chm5Alt4.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 3:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm3Default.GetPwm(Value.Amplitude,SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L3Chm3Alt1.GetPwm(Value.Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L3Chm3Alt2.GetPwm(Value.Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 1:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L3Chm1Default.GetPwm(Value.Amplitude, SineX),
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

                        double SineVal = GetBaseWaveform(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                        double SawVal = Saw(SawTime * SawAngleFrequency);

                        if (PulseMode.Alternative == PulseAlternative.Shifted)
                            SawVal = -SawVal;

                        Control.SetSawAngleFrequency(SawAngleFrequency);
                        Control.SetSawTime(SawTime);

                        return ModulateSignal(SineVal, SawVal) * 2;
                    }
                case PulseTypeName.SYNC:
                    {
                        // SYNC 3 ALTERNATE 1
                        if(PulseCount == 3 && PulseMode.Alternative == PulseAlternative.Alt1)
                        {
                            double SineVal = Sine(SineX);
                            double SawVal = Saw(SineX - Value.PulseData.GetValueOrDefault(PulseDataKey.Phase, 0) / 180.0 * M_PI);
                            double Pwm = (SineVal > 0 ? 1 : -1) * (Amplitude * 2 / 3.0 + 1 / 3.0);
                            double Negate = SawVal > 0 ? SawVal - 1 : SawVal + 1;
                            return ModulateSignal(Pwm, Negate) * 2;
                        }

                        // SYNC 5 9 13 17 ALTERNATE 1
                        if((PulseCount == 5 || PulseCount == 9 || PulseCount == 13 || PulseCount == 17) && Alternate == PulseAlternative.Alt1)
                        {
                            double SawValue = Saw(27 * SineX);
                            double SineVal = GetBaseWaveform(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                            double FixedSineX = (int)(SineX / M_PI_2) % 2 == 1 ? M_PI_2 - SineX % M_PI_2 : SineX % M_PI_2;
                            Control.SetSawAngleFrequency(27 * SineAngleFrequency);
                            Control.SetSawTime(SineTime);
                            return (FixedSineX < M_PI * PulseMode.PulseCount / 54) ? ModulateSignal(SineVal, SawValue) * 2 : (int)(SineX / M_PI_2) % 4 > 1 ? 0 : 2;
                        }

                        // SYNC 11 ALTERNATE 1
                        if (PulseCount == 11 && PulseMode.Alternative == PulseAlternative.Alt1)
                        {
                            (double, int)[] Alpha = [
                                (M_PI / 15 - (1 + Math.Sqrt(5)) / (10 * Math.Sqrt(3)) * Amplitude - 2 * Sine(M_PI/30) / (5 * Math.Sqrt(3)) * Amplitude, 2),
                                (M_PI / 15 + (Math.Sqrt(5) - 1) / (10 * Math.Sqrt(3)) * Amplitude + 2 * Sine(M_PI*7.0/30.0) / (5 * Math.Sqrt(3)) * Amplitude, 0),
                                (M_PI / 6 - 1 / (5 * Math.Sqrt(3)) * Amplitude, 2),
                                (M_PI * 2.0 / 5 - 2 * Sine(M_PI/30) / (5 * Math.Sqrt(3)) * Amplitude, 0),
                                (M_PI * 2.0 / 5 + (Math.Sqrt(5) - 1) / (10 * Math.Sqrt(3)) * Amplitude, 2),
                            ];

                            if (Amplitude >= 0.9927) Alpha[0] = (0, 2);
                            if(Amplitude >= 0.9203069589)
                            {
                                Alpha[1] = (0.417331, 0);
                                Alpha[2] = (0.417331, 2);
                                Alpha[3] = (1.23442104526 + 0.278769982056 * (Amplitude - 0.9203069589), 0);
                                Alpha[4] = (1.32231416347 - 0.824126360283 * (Amplitude - 0.9203069589), 2);
                            }

                            return CustomPwm.GetPwm(ref Alpha, SineX, false);
                        }

                        // SYNC 6 8 ALTERNATE 1
                        if ((PulseCount == 6 || PulseCount == 8) && Alternate == PulseAlternative.Alt1)
                        {
                            int C = PulseCount == 6 ? 6 : 9;
                            double SawVal = Saw(C * SineX + M_PI_2);
                            int Orthant = (int)((SineX % M_2PI) / M_PI_2);
                            double FixX = Orthant % 2 == 1 ? M_PI_2 - (SineX % M_PI_2) : (SineX % M_PI_2);
                            double Sig = Orthant > 1 ? 1 : -1;
                            if (FixX > Value.PulseData.GetValueOrDefault(PulseDataKey.PulseWidth, 0)) Sig = Orthant > 1 ? -1 : 1;
                            Sig *= Amplitude;
                            return ModulateSignal(Sig, SawVal) * 2;
                        }

                        // SYNC N WITH CP CONFIGURATION
                        if(Alternate == PulseAlternative.CP)
                        {
                            double SineVal = GetBaseWaveform(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                            
                            int CarrierFrequency = PulseMode.PulseCount / 2 * 6;
                            double SawVal = CarrierFrequency == 0 ? 0 : (Saw(CarrierFrequency * SineX + M_PI_2) * ((PulseMode.PulseCount % 2 == 1) ? 0.5 : -0.5) + 0.5);
                            double X = SineAngleFrequency * SineTime + InitialPhase;
                            double CycleX = X % M_2PI;
                            int Orthant = (int)((X % M_PI) / M_PI_3);
                            if (CycleX >= M_PI) SawVal = -SawVal;
                            if (Orthant != 1) SawVal = 0;
                            
                            return ModulateSignal(SineVal, SawVal) * 2;
                        }

                        // SYNC N WITH SQUARE CONFIGURATION
                        if (Alternate == PulseAlternative.Square)
                        {
                            PulseCount = PulseMode.PulseCount;
                            PulseCount += PulseMode.PulseCount % 2 == 0 ? 0 : -1;
                            double CarrierVal = 0.5 * ((PulseMode.PulseCount % 2 == 0 ? -1 : 1) * Saw(3 * PulseCount * SineX + M_PI_2) + 1);
                            return ModulateSignal(SineX % M_2PI < M_PI ? Amplitude : -Amplitude, CarrierVal) * 2;
                        }

                        // SYNC N
                        {
                            double SineVal = GetBaseWaveform(PulseMode.Clone(), SineX, Amplitude, Control.GetGenerationCurrentTime(), InitialPhase);
                            double SawVal = Saw(PulseMode.PulseCount * (SineAngleFrequency * SineTime + InitialPhase));
                            if (PulseMode.Alternative == PulseAlternative.Shifted)
                                SawVal = -SawVal;
                            Control.SetSawAngleFrequency(SineAngleFrequency * PulseMode.PulseCount);
                            Control.SetSawTime(SineTime);
                            return ModulateSignal(SineVal, SawVal) * 2;
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
                                PulseAlternative.Default => CustomPwmPresets.L2She3Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwmPresets.L2She3Alt1.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            5 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L2She5Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwmPresets.L2She5Alt1.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt2 => CustomPwmPresets.L2She5Alt2.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            7 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L2She7Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwmPresets.L2She7Alt1.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            9 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L2She9Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwmPresets.L2She9Alt1.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt2 => CustomPwmPresets.L2She9Alt2.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            11 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L2She11Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwmPresets.L2She11Alt1.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            13 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L2She13Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwmPresets.L2She13Alt1.GetPwm(Amplitude, SineX),
                                _ => 0,
                            },
                            15 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Default => CustomPwmPresets.L2She15Default.GetPwm(Amplitude, SineX),
                                PulseAlternative.Alt1 => CustomPwmPresets.L2She15Alt1.GetPwm(Amplitude, SineX),
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
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm25Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm25Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm25Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm25Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm25Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm25Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L2Chm25Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L2Chm25Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L2Chm25Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L2Chm25Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L2Chm25Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L2Chm25Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L2Chm25Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L2Chm25Alt13.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwmPresets.L2Chm25Alt14.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwmPresets.L2Chm25Alt15.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwmPresets.L2Chm25Alt16.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwmPresets.L2Chm25Alt17.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwmPresets.L2Chm25Alt18.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwmPresets.L2Chm25Alt19.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt20 => CustomPwmPresets.L2Chm25Alt20.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 23:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm23Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm23Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm23Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm23Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm23Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm23Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L2Chm23Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L2Chm23Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L2Chm23Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L2Chm23Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L2Chm23Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L2Chm23Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L2Chm23Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L2Chm23Alt13.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwmPresets.L2Chm23Alt14.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 21:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm21Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm21Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm21Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm21Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm21Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm21Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L2Chm21Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L2Chm21Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L2Chm21Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L2Chm21Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L2Chm21Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L2Chm21Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L2Chm21Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L2Chm21Alt13.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 19:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm19Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm19Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm19Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm19Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm19Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm19Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L2Chm19Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L2Chm19Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L2Chm19Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L2Chm19Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L2Chm19Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L2Chm19Alt11.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 17:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm17Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm17Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm17Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm17Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm17Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm17Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L2Chm17Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L2Chm17Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L2Chm17Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L2Chm17Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L2Chm17Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L2Chm17Alt11.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 15:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm15Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm15Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm15Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm15Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm15Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm15Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L2Chm15Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L2Chm15Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L2Chm15Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L2Chm15Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L2Chm15Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L2Chm15Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L2Chm15Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L2Chm15Alt13.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt14 => CustomPwmPresets.L2Chm15Alt14.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt15 => CustomPwmPresets.L2Chm15Alt15.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt16 => CustomPwmPresets.L2Chm15Alt16.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt17 => CustomPwmPresets.L2Chm15Alt17.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt18 => CustomPwmPresets.L2Chm15Alt18.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt19 => CustomPwmPresets.L2Chm15Alt19.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt20 => CustomPwmPresets.L2Chm15Alt20.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt21 => CustomPwmPresets.L2Chm15Alt21.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt22 => CustomPwmPresets.L2Chm15Alt22.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt23 => CustomPwmPresets.L2Chm15Alt23.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 13:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm13Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm13Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm13Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm13Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm13Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm13Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L2Chm13Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L2Chm13Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L2Chm13Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L2Chm13Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L2Chm13Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L2Chm13Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L2Chm13Alt12.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt13 => CustomPwmPresets.L2Chm13Alt13.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 11:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm11Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm11Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm11Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm11Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm11Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm11Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L2Chm11Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L2Chm11Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L2Chm11Alt8.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt9 => CustomPwmPresets.L2Chm11Alt9.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt10 => CustomPwmPresets.L2Chm11Alt10.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt11 => CustomPwmPresets.L2Chm11Alt11.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt12 => CustomPwmPresets.L2Chm11Alt12.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 9:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm9Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm9Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm9Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm9Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm9Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm9Alt5.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt6 => CustomPwmPresets.L2Chm9Alt6.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt7 => CustomPwmPresets.L2Chm9Alt7.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt8 => CustomPwmPresets.L2Chm9Alt8.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 7:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm7Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm7Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm7Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm7Alt3.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt4 => CustomPwmPresets.L2Chm7Alt4.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt5 => CustomPwmPresets.L2Chm7Alt5.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 5:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm5Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm5Alt1.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt2 => CustomPwmPresets.L2Chm5Alt2.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt3 => CustomPwmPresets.L2Chm5Alt3.GetPwm(Amplitude, SineX),
                                        _ => 0
                                    };
                                }
                            case 3:
                                {
                                    return PulseMode.Alternative switch
                                    {
                                        PulseAlternative.Default => CustomPwmPresets.L2Chm3Default.GetPwm(Amplitude, SineX),
                                        PulseAlternative.Alt1 => CustomPwmPresets.L2Chm3Alt1.GetPwm(Amplitude, SineX),
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
            public static implicit operator Vabc(WaveValues uvw)
            {
                return new Vabc()
                {
                    Ua = uvw.U,
                    Ub = uvw.V,
                    Uc = uvw.W
                };
            }

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

            public static implicit operator PointD(Valbe Uab0)
            {
                return new PointD(Uab0.Ualpha, Uab0.Ubeta);
            }
        };
        public static int EstimateSector(Valbe U)
        {
            int A = U.Ubeta > 0.0 ? 0 : 1;
            int B = U.Ubeta - M_SQRT3 * U.Ualpha > 0.0 ? 0 : 1;
            int C = U.Ubeta + M_SQRT3 * U.Ualpha > 0.0 ? 0 : 1;
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
                        ft.T1 = M_SQRT3_2 * Vin.Ualpha - 0.5 * Vin.Ubeta;
                        ft.T2 = Vin.Ubeta;
                    }
                    break;
                case 2:
                    {
                        ft.T1 = M_SQRT3_2 * Vin.Ualpha + 0.5 * Vin.Ubeta;
                        ft.T2 = 0.5 * Vin.Ubeta - M_SQRT3_2 * Vin.Ualpha;
                    }
                    break;
                case 3:
                    {
                        ft.T1 = Vin.Ubeta;
                        ft.T2 = -(M_SQRT3_2 * Vin.Ualpha + 0.5 * Vin.Ubeta);
                    }
                    break;
                case 4:
                    {
                        ft.T1 = 0.5 * Vin.Ubeta - M_SQRT3_2 * Vin.Ualpha;
                        ft.T2 = -Vin.Ubeta;
                    }
                    break;
                case 5:
                    {
                        ft.T1 = -(M_SQRT3_2 * Vin.Ualpha + 0.5 * Vin.Ubeta);
                        ft.T2 = M_SQRT3_2 * Vin.Ualpha - 0.5 * Vin.Ubeta;
                    }
                    break;
                case 6:
                    {
                        ft.T1 = -Vin.Ubeta;
                        ft.T2 = M_SQRT3_2 * Vin.Ualpha + 0.5 * Vin.Ubeta;
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
                Ubeta = 0.66666666666666666666666666666667 * (M_SQRT3_2 * (V0.Ub - V0.Uc))
            };
            return Vin;
        }

    }
}
