using System;
using System.Collections.Generic;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;
using static VvvfSimulator.Vvvf.Model.Struct;
using static VvvfSimulator.Vvvf.MyMath;
using static VvvfSimulator.Vvvf.MyMath.Functions;

namespace VvvfSimulator.Vvvf.Calculation
{
    public class Common
    {
        #region Other
        public static int ModulateSignal(double Signal, double Carrier)
        {
            return Signal > Carrier ? 1 : 0;
        }
        public static double GetPulseDataValue(Dictionary<PulseDataKey, double> PulseData, PulseDataKey Key)
        {
            return PulseData.GetValueOrDefault(Key, Model.Config.GetPulseDataKeyDefaultConstant(Key));
        }
        #endregion

        #region BaseWave
        public static double DiscreteTimeLine(double x, int level, DiscreteTimeConfiguration.DiscreteTimeMode mode)
        {
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
        public static (double X, double RawX) GetBaseWaveParameter(Domain Control, int Phase, double InitialPhase)
        {
            if (Control.ElectricalState.IsNone) return (0, 0);

            double SineTime = Control.GetBaseWaveTime();
            double RawX = Control.ElectricalState.BaseWaveAngleFrequency * SineTime + M_2PI_3 * Phase + InitialPhase;
            double SineX;
            if (Control.ElectricalState.PulsePattern.PulseMode.DiscreteTime.Enabled) SineX = DiscreteTimeLine(RawX, Control.ElectricalState.PulsePattern.PulseMode.DiscreteTime.Steps, Control.ElectricalState.PulsePattern.PulseMode.DiscreteTime.Mode);
            else SineX = RawX;

            return (SineX, RawX);
        }
        public static double GetBaseWaveform(Domain Control, int Phase, double InitialPhase)
        {
            if (Control.ElectricalState.IsNone) return 0;

            if (Control.ElectricalState.PulsePattern.PulseMode.BaseWave == BaseWaveType.SV)
            {
                Modulation.SVM.Vabc Vabc = new()
                {
                    U = (double)(Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, 0, InitialPhase).X)),
                    V = (double)(Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, 1, InitialPhase).X)),
                    W = (double)(Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, 2, InitialPhase).X))
                };
                Modulation.SVM.Valbe VAlBe = Vabc.Clark();
                int Sector = VAlBe.EstimateSector();
                Modulation.SVM.FunctionTime Ft = VAlBe.GetFunctionTime(Sector);
                Modulation.SVM.Vabc Vsv = Ft.GetVabc(Sector);
                return Phase switch
                {
                    0 => Vsv.U,
                    1 => Vsv.V,
                    _ => Vsv.W
                } * 2 - 1;
            }
            else if (Control.ElectricalState.PulsePattern.PulseMode.BaseWave == BaseWaveType.DPWM30)
            {
                double X = GetBaseWaveParameter(Control, Phase, InitialPhase).X;
                int Sector = (int)(X % M_2PI / M_PI_6);
                return (double)(Sector switch 
                {
                    0 or 9 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X)),
                    1 or 4 => 1,
                    2 or 11 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X)),
                    3 or 6 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X)),
                    5 or 8 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X)),
                    _ => -1
                });
            }
            else if (Control.ElectricalState.PulsePattern.PulseMode.BaseWave == BaseWaveType.DPWM60C)
            {
                double X = GetBaseWaveParameter(Control, Phase, InitialPhase).X;
                int Sector = (int)(X % M_2PI / M_PI_3);
                return (double)(Sector switch
                {
                    0 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X)),
                    1 => 1,
                    2 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X)),
                    3 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X)),
                    4 => -1,
                    _ => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X))
                });
            }
            else if (Control.ElectricalState.PulsePattern.PulseMode.BaseWave == BaseWaveType.DPWM60P)
            {
                double X = GetBaseWaveParameter(Control, Phase, InitialPhase).X;
                int Sector = (int)(X % M_2PI / M_PI_6);
                return (double)(Sector switch
                {
                    1 or 2 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X)),
                    3 or 4 => 1,
                    5 or 6 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X)),
                    7 or 8 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X)),
                    9 or 10 => -1,
                    _ => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X))
                });
            }
            else if (Control.ElectricalState.PulsePattern.PulseMode.BaseWave == BaseWaveType.DPWM60N)
            {
                double X = GetBaseWaveParameter(Control, Phase, InitialPhase).X;
                int Sector = (int)(X % M_2PI / M_PI_6);
                return (double)(Sector switch
                {
                    1 or 2 => 1,
                    3 or 4 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X)),
                    5 or 6 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X)),
                    7 or 8 => -1,
                    9 or 10 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X)),
                    _ => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X))
                });
            }
            else if (Control.ElectricalState.PulsePattern.PulseMode.BaseWave == BaseWaveType.DPWM120P)
            {
                double X = GetBaseWaveParameter(Control, Phase, InitialPhase).X;
                int Sector = (int)(X % M_2PI / M_PI_6);
                return (double)(Sector switch
                {
                    >= 1 and <= 4 => 1,
                    >= 5 and <= 8 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X)),
                    _ => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X)),
                });
            }
            else if (Control.ElectricalState.PulsePattern.PulseMode.BaseWave == BaseWaveType.DPWM120N)
            {
                double X = GetBaseWaveParameter(Control, Phase, InitialPhase).X;
                int Sector = (int)(X % M_2PI / M_PI_6);
                return (double)(Sector switch
                {
                    >= 3 and <= 6 => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase + M_2PI_3).X)),
                    >= 7 and <= 10 => -1,
                    _ => Control.ElectricalState.BaseWaveAmplitude * Sine(X) + (-1 - Control.ElectricalState.BaseWaveAmplitude * Sine(GetBaseWaveParameter(Control, Phase, InitialPhase - M_2PI_3).X)),
                });
            }
            else
            {
                double X = GetBaseWaveParameter(Control, Phase, InitialPhase).X;
                double GetModifiedSine(double X, int Level) => Math.Round(Sine(X) * Level) / Level;
                double GetModifiedSaw(double X)
                {
                    double Y = Triangle(X) * M_PI_2;
                    if (Math.Abs(Y) > 0.5) Y = Y > 0 ? 1 : -1;
                    return Y;
                }
                double BaseWave = (double)(Control.ElectricalState.BaseWaveAmplitude * Control.ElectricalState.PulsePattern.PulseMode.BaseWave switch
                {
                    BaseWaveType.Sine => Sine(X),
                    BaseWaveType.Saw => Triangle(X),
                    BaseWaveType.Square => Square(X),
                    BaseWaveType.ModifiedSine1 => GetModifiedSine(X, 1),
                    BaseWaveType.ModifiedSine2 => GetModifiedSine(X, 2),
                    BaseWaveType.ModifiedSaw1 => GetModifiedSaw(X),
                    _ => 0
                });

                double HarmonicWave = 0.0;
                for (int i = 0; i < Control.ElectricalState.PulsePattern.PulseMode.PulseHarmonics.Count; i++)
                {
                    PulseHarmonic HarmonicData = Control.ElectricalState.PulsePattern.PulseMode.PulseHarmonics[i];
                    double HarmonicX = HarmonicData.IsHarmonicProportional switch
                    {
                        true => HarmonicData.Harmonic * (X + HarmonicData.InitialPhase),
                        false => M_2PI * HarmonicData.Harmonic * (Control.GetTime() + InitialPhase)
                    };
                    HarmonicWave += (double)(HarmonicData.Type switch
                    {
                        PulseHarmonic.PulseHarmonicType.Sine => Sine(HarmonicX),
                        PulseHarmonic.PulseHarmonicType.Saw => Triangle(HarmonicX),
                        PulseHarmonic.PulseHarmonicType.Square => Square(HarmonicX),
                        _ => 0,
                    } * HarmonicData.Amplitude * (HarmonicData.IsAmplitudeProportional ? Control.ElectricalState.BaseWaveAmplitude : 1));
                }

                double Wave = BaseWave + HarmonicWave;
                Wave = Wave > 1 ? 1 : Wave < -1 ? -1 : Wave;
                return Wave;
            }
        }
        #endregion

        #region CarrierWave
        public static double GetCarrierWaveform(Domain Control, double Time)
        {
            if (Control.ElectricalState.IsNone) return 0;

            return Control.ElectricalState.PulsePattern.PulseMode.CarrierWave.Type switch
            {
                CarrierWaveConfiguration.CarrierWaveType.Triangle => Control.ElectricalState.PulsePattern.PulseMode.CarrierWave.Option switch
                {
                    CarrierWaveConfiguration.CarrierWaveOption.RaiseStart => Triangle(Time),
                    CarrierWaveConfiguration.CarrierWaveOption.FallStart => -Triangle(Time),
                    CarrierWaveConfiguration.CarrierWaveOption.TopStart => Triangle(Time + M_PI_2),
                    _ => -Triangle(Time + M_PI_2)
                },
                CarrierWaveConfiguration.CarrierWaveType.Saw => Control.ElectricalState.PulsePattern.PulseMode.CarrierWave.Option switch
                {
                    CarrierWaveConfiguration.CarrierWaveOption.RaiseStart => Saw(Time),
                    CarrierWaveConfiguration.CarrierWaveOption.FallStart => -Saw(Time),
                    CarrierWaveConfiguration.CarrierWaveOption.TopStart => -Saw(Time + M_PI_2),
                    _ => Saw(Time + M_PI_2)
                },
                CarrierWaveConfiguration.CarrierWaveType.Sine => Control.ElectricalState.PulsePattern.PulseMode.CarrierWave.Option switch
                {
                    CarrierWaveConfiguration.CarrierWaveOption.RaiseStart => Sine(Time),
                    CarrierWaveConfiguration.CarrierWaveOption.FallStart => -Sine(Time),
                    CarrierWaveConfiguration.CarrierWaveOption.TopStart => Sine(Time + M_PI_2),
                    _ => -Sine(Time + M_PI_2)
                },
                _ => 0
            };
        }
        #endregion

        public delegate PhaseState PhaseStateCalculator(Domain Control, double InitialPhase);
        public static PhaseStateCalculator GetCalculator(int PwmLevel, PulseTypeName PulseType)
        {
            return PwmLevel switch
            {
                2 => L2.GetCalculator(PulseType),
                _ => L3.GetCalculator(PulseType),
            };
        }
        public static PhaseState CalculatePhsaseState(Domain Control, double InitialPhase)
        {
            PhaseState State = new(0, 0, 0);
            if(!Control.ElectricalState.IsNone && !Control.ElectricalState.IsZeroOutput)
                State = GetCalculator(Control.ElectricalState.PwmLevel, Control.ElectricalState.PulsePattern.PulseMode.PulseType)(Control, InitialPhase);
            Control.Motor.Process(Control.GetDeltaTime(), Control.ElectricalState.BaseWaveAngleFrequency, State);
            return State;
        }        
    }
}
