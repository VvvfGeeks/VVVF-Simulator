using System;
using VvvfSimulator.Vvvf.Modulation;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;
using static VvvfSimulator.Vvvf.Model.Struct;
using static VvvfSimulator.Vvvf.MyMath;

namespace VvvfSimulator.Vvvf.Calculation
{
    public class L2
    {
        private static PhaseState Async(Domain Domain, double InitialPhase)
        {
            Domain.GetCarrierInstance().ProcessCarrierFrequency(Domain.GetTime(), Domain.ElectricalState);
            double CarrierVal = Common.GetCarrierWaveform(Domain, Domain.GetCarrierInstance().Phase);
            return new(
                Common.ModulateSignal(Common.GetBaseWaveform(Domain, 0, InitialPhase), CarrierVal) * 2,
                Common.ModulateSignal(Common.GetBaseWaveform(Domain, 1, InitialPhase), CarrierVal) * 2,
                Common.ModulateSignal(Common.GetBaseWaveform(Domain, 2, InitialPhase), CarrierVal) * 2
            );
        }

        private static int Sync(Domain Domain, double InitialPhase, int Phase)
        {
            if (Domain.ElectricalState.IsNone) return 0;

            (double X, double RawX) = Common.GetBaseWaveParameter(Domain, Phase, InitialPhase);
            // SYNC 1 ALTERNATE 1 2
            if (Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 1 && (Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt1 || Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt2))
            {
                int Sign = Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt1 ? 1 : -1;
                double AmpAbs = (double)(Domain.ElectricalState.BaseWaveAmplitude < 0 ? -Domain.ElectricalState.BaseWaveAmplitude : Domain.ElectricalState.BaseWaveAmplitude);
                int AmpSign = Domain.ElectricalState.BaseWaveAmplitude < 0 ? -1 : 1;
                double SineVal = -Functions.Triangle(X) + Sign * (1 - Functions.ArcSine(Math.Clamp(AmpAbs * M_PI_4, 0, 1)) * M_2_PI);
                return SineVal > 0 ? -AmpSign + 1 : AmpSign + 1;
            }

            // SYNC 3 ALTERNATE 1
            if (Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 3 && Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt1)
            {
                double SineVal = Functions.Sine(RawX);
                double SawVal = -Functions.Triangle(RawX - Common.GetPulseDataValue(Domain.ElectricalState.PulseData, PulseDataKey.Phase) / 180.0 * M_PI);
                double Pwm = (double)((SineVal > 0 ? 1 : -1) * (Domain.ElectricalState.BaseWaveAmplitude * 2 / 3.0 + 1 / 3.0));
                double Negate = SawVal > 0 ? SawVal - 1 : SawVal + 1;
                return Common.ModulateSignal(Pwm, Negate) * 2;
            }

            // SYNC 5 9 13 17 ALTERNATE 1
            if ((Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 5 || Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 9 || Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 13 || Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 17) && Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt1)
            {
                double SineVal = Common.GetBaseWaveform(Domain, Phase, InitialPhase);
                double SawValue = -Functions.Triangle(27 * RawX);
                double FixedX = (int)(RawX / M_PI_2) % 2 == 1 ? M_PI_2 - RawX % M_PI_2 : RawX % M_PI_2;
                Domain.GetCarrierInstance().AngleFrequency = Domain.ElectricalState.BaseWaveAngleFrequency;
                Domain.GetCarrierInstance().Time = Domain.GetBaseWaveTime();
                return (FixedX < M_PI * Domain.ElectricalState.PulsePattern.PulseMode.PulseCount / 54) ? Common.ModulateSignal(SineVal, SawValue) * 2 : (int)(RawX / M_PI_2) % 4 > 1 ? 0 : 2;
            }

            // SYNC 11 ALTERNATE 1
            if (Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 11 && Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt1)
            {
                (double, byte)[] Alpha = [
                    ((double)(M_PI / 15 - (1 + Math.Sqrt(5)) / (10 * Math.Sqrt(3)) * Domain.ElectricalState.BaseWaveAmplitude - 2 * Functions.Sine(M_PI/30) / (5 * Math.Sqrt(3)) * Domain.ElectricalState.BaseWaveAmplitude), 2),
                                ((double)(M_PI / 15 + (Math.Sqrt(5) - 1) / (10 * Math.Sqrt(3)) * Domain.ElectricalState.BaseWaveAmplitude + 2 * Functions.Sine(M_PI*7.0/30.0) / (5 * Math.Sqrt(3)) * Domain.ElectricalState.BaseWaveAmplitude), 0),
                                ((double)(M_PI / 6 - 1 / (5 * Math.Sqrt(3)) * Domain.ElectricalState.BaseWaveAmplitude), 2),
                                ((double)(M_PI * 2.0 / 5 - 2 * Functions.Sine(M_PI/30) / (5 * Math.Sqrt(3)) * Domain.ElectricalState.BaseWaveAmplitude), 0),
                                ((double)(M_PI * 2.0 / 5 + (Math.Sqrt(5) - 1) / (10 * Math.Sqrt(3)) * Domain.ElectricalState.BaseWaveAmplitude), 2),
                            ];

                if (Domain.ElectricalState.BaseWaveAmplitude >= 0.9927) Alpha[0] = (0, 2);
                if (Domain.ElectricalState.BaseWaveAmplitude >= 0.9203069589)
                {
                    Alpha[1] = (0.417331, 0);
                    Alpha[2] = (0.417331, 2);
                    Alpha[3] = (1.23442104526 + 0.278769982056 * (double)(Domain.ElectricalState.BaseWaveAmplitude - 0.9203069589), 0);
                    Alpha[4] = (1.32231416347 - 0.824126360283 * (double)(Domain.ElectricalState.BaseWaveAmplitude - 0.9203069589), 2);
                }

                return Modulation.CustomPwm.GetPwm(ref Alpha, X, 0);
            }

            // SYNC 6 8 ALTERNATE 1
            if ((Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 6 || Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 8) && Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt1)
            {
                int C = Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 6 ? 6 : 9;
                double SawVal = -Functions.Triangle(C * RawX + M_PI_2);
                int Orthant = (int)((RawX % M_2PI) / M_PI_2);
                double FixX = Orthant % 2 == 1 ? M_PI_2 - (RawX % M_PI_2) : (RawX % M_PI_2);
                double Sig = Orthant > 1 ? 1 : -1;
                if (FixX > Common.GetPulseDataValue(Domain.ElectricalState.PulseData, PulseDataKey.PulseWidth)) Sig = Orthant > 1 ? -1 : 1;
                Sig *= (double)Domain.ElectricalState.BaseWaveAmplitude;
                return Common.ModulateSignal(Sig, SawVal) * 2;
            }

            // SYNC N WITH CP CONFIGURATION
            if (Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.CP)
            {
                double SineVal = Common.GetBaseWaveform(Domain, Phase, InitialPhase);

                int CarrierFrequency = Domain.ElectricalState.PulsePattern.PulseMode.PulseCount / 2 * 6;
                double SawVal = CarrierFrequency == 0 ? 0 : (-Functions.Triangle(CarrierFrequency * RawX + M_PI_2) * ((Domain.ElectricalState.PulsePattern.PulseMode.PulseCount % 2 == 1) ? 0.5 : -0.5) + 0.5);
                double CycleX = RawX % M_2PI;
                int Orthant = (int)((RawX % M_PI) / M_PI_3);
                if (CycleX >= M_PI) SawVal = -SawVal;
                if (Orthant != 1) SawVal = 0;

                return Common.ModulateSignal(SineVal, SawVal) * 2;
            }

            // SYNC N WITH SQUARE CONFIGURATION
            if (Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Square)
            {
                int PulseCount = Domain.ElectricalState.PulsePattern.PulseMode.PulseCount;
                PulseCount += PulseCount % 2 == 0 ? 0 : -1;
                double CarrierVal = 0.5 * ((Domain.ElectricalState.PulsePattern.PulseMode.PulseCount % 2 == 0 ? 1 : -1) * Functions.Triangle(3 * PulseCount * RawX + M_PI_2) + 1);
                return Common.ModulateSignal((double)(X % M_2PI < M_PI ? Domain.ElectricalState.BaseWaveAmplitude : -Domain.ElectricalState.BaseWaveAmplitude), CarrierVal) * 2;
            }

            // SYNC N
            {
                double SineVal = Common.GetBaseWaveform(Domain, Phase, InitialPhase);
                double CarrierVal = Common.GetCarrierWaveform(Domain, Domain.ElectricalState.PulsePattern.PulseMode.PulseCount * RawX);
                Domain.GetCarrierInstance().AngleFrequency = Domain.ElectricalState.BaseWaveAngleFrequency;
                Domain.GetCarrierInstance().Time = Domain.GetBaseWaveTime();
                return Common.ModulateSignal(SineVal, CarrierVal) * 2;
            }
        }
        private static PhaseState Sync(Domain Domain, double InitialPhase)
        {
            return new(
                Sync(Domain, InitialPhase, 0),
                Sync(Domain, InitialPhase, 1),
                Sync(Domain, InitialPhase, 2)
            );
        }

        private static int Ho(Domain Domain, double InitialPhase, int Phase)
        {
            if (Domain.ElectricalState.IsNone) return 0;

            static int GetHo(double x, double amplitude, int carrier, int width)
            {
                int totalSteps = carrier * 2;
                double fixed_x = x % M_PI / (M_PI / totalSteps);
                double saw_value = Functions.Triangle(carrier * x);
                double modulated;
                if (fixed_x > totalSteps - 1) modulated = -1;
                else if (fixed_x > totalSteps / 2 + width) modulated = 1;
                else if (fixed_x > totalSteps / 2 - width) modulated = 2 * amplitude - 1;
                else if (fixed_x > 1) modulated = 1;
                else modulated = -1;
                if (x % M_2PI > M_PI) modulated = -modulated;
                return Common.ModulateSignal(modulated, saw_value);
            }

            (double SineX, _) = Common.GetBaseWaveParameter(Domain, Phase, InitialPhase);
            int[] Keys = Domain.ElectricalState.PulsePattern.PulseMode.PulseCount switch
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
            int Index = (Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Default || Domain.ElectricalState.PulsePattern.PulseMode.Alternative - PulseAlternative.Alt1 + 1 >= Keys.Length / 2) ?
                0 : (Domain.ElectricalState.PulsePattern.PulseMode.Alternative - PulseAlternative.Alt1 + 1);
            return GetHo(SineX, (double)Domain.ElectricalState.BaseWaveAmplitude, Keys[2 * Index], Keys[2 * Index + 1]) * 2;
        }
        private static PhaseState Ho(Domain Domain, double InitialPhase)
        {
            return new(
                Ho(Domain, InitialPhase, 0),
                Ho(Domain, InitialPhase, 1),
                Ho(Domain, InitialPhase, 2)
            );
        }
        
        private static PhaseState FromCustomPwm(Domain Domain, double InitialPhase)
        {
            if (Domain.ElectricalState.IsNone) return PhaseState.Zero();

            CustomPwm? Preset = CustomPwmPresets.GetCustomPwm(
                Domain.ElectricalState.PwmLevel,
                Domain.ElectricalState.PulsePattern.PulseMode.PulseType,
                Domain.ElectricalState.PulsePattern.PulseMode.PulseCount,
                Domain.ElectricalState.PulsePattern.PulseMode.Alternative
            );

            if (Preset == null) return PhaseState.Zero();

            return new(
                Preset.GetPwm((double)Domain.ElectricalState.BaseWaveAmplitude, Common.GetBaseWaveParameter(Domain, 0, InitialPhase).X),
                Preset.GetPwm((double)Domain.ElectricalState.BaseWaveAmplitude, Common.GetBaseWaveParameter(Domain, 1, InitialPhase).X),
                Preset.GetPwm((double)Domain.ElectricalState.BaseWaveAmplitude, Common.GetBaseWaveParameter(Domain, 2, InitialPhase).X)
            );
        }

        private static int DeltaSigma(Domain Domain, double InitialPhase, int Phase)
        {
            if (Domain.ElectricalState.IsNone) return 0;
            DeltaSigma deltaSigma = Domain.GetDeltaSigmaInstance(Phase);
            deltaSigma.ResetIfLastTime(Domain.GetLastTime());
            deltaSigma.FeedbackInterval = 1.0 / Common.GetPulseDataValue(Domain.ElectricalState.PulseData, PulseDataKey.UpdateFrequency);
            return deltaSigma.Process(Common.GetBaseWaveform(Domain, Phase, InitialPhase), Domain.GetTime()) * 2;
        }
        private static PhaseState DeltaSigma(Domain Domain, double InitialPhase)
        {
            return new(
                DeltaSigma(Domain, InitialPhase, 0),
                DeltaSigma(Domain, InitialPhase, 1),
                DeltaSigma(Domain, InitialPhase, 2)
            );
        }
        public static Common.PhaseStateCalculator GetCalculator(PulseTypeName PulseType)
        {
            return PulseType switch
            {
                PulseTypeName.ASYNC => Async,
                PulseTypeName.SYNC => Sync,
                PulseTypeName.HO => Ho,
                PulseTypeName.ΔΣ => DeltaSigma,
                _ => FromCustomPwm
            };
        }
    }
}
