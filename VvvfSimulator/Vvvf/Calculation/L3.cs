using VvvfSimulator.Vvvf.Modulation;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;
using static VvvfSimulator.Vvvf.Model.Struct;
using static VvvfSimulator.Vvvf.MyMath;

namespace VvvfSimulator.Vvvf.Calculation
{
    public class L3
    {
        private static PhaseState Async(Domain Domain, double InitialPhase)
        {
            if (Domain.ElectricalState.IsNone) return PhaseState.Zero();

            static int Modulate(double BaseWave, double Carrier) => Common.ModulateSignal(BaseWave, Carrier + 0.5) + Common.ModulateSignal(BaseWave, Carrier - 0.5);
            
            Domain.GetCarrierInstance().ProcessCarrierFrequency(Domain.GetTime(), Domain.ElectricalState);
            double CarrierVal = Common.GetCarrierWaveform(Domain, Domain.GetCarrierInstance().Phase);
            double Dipolar = Common.GetPulseDataValue(Domain.ElectricalState.PulseData, PulseDataKey.Dipolar);
            CarrierVal *= (Dipolar != -1 ? Dipolar : 0.5);

            return new(
                Modulate(Common.GetBaseWaveform(Domain, 0, InitialPhase), CarrierVal),
                Modulate(Common.GetBaseWaveform(Domain, 1, InitialPhase), CarrierVal),
                Modulate(Common.GetBaseWaveform(Domain, 2, InitialPhase), CarrierVal)
            );
        }
        
        private static int Sync(Domain Domain, double InitialPhase, int Phase)
        {
            if (Domain.ElectricalState.IsNone) return 0;

            (double X, double RawX) = Common.GetBaseWaveParameter(Domain, Phase, InitialPhase);

            if (Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 1 && Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt1)
            {
                double SineVal = Functions.Sine(X);
                int D = SineVal > 0 ? 1 : -1;
                double voltage_fix = (double)(D * (1 - Domain.ElectricalState.BaseWaveAmplitude));

                int gate = D * (SineVal - voltage_fix) > 0 ? D : 0;
                gate += 1;
                return gate;
            }

            if (Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 5 && Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt1)
            {
                double Period = X % M_2PI;
                int Orthant = (int)(Period / M_PI_2);
                double Quater = Period % M_PI_2;

                int _GetPwm(double t)
                {
                    double a = (double)(M_PI_2 - Domain.ElectricalState.BaseWaveAmplitude);
                    double b = Common.GetPulseDataValue(Domain.ElectricalState.PulseData, PulseDataKey.PulseWidth) / 180 * M_PI;
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

            if (Domain.ElectricalState.PulsePattern.PulseMode.PulseCount == 5 && Domain.ElectricalState.PulsePattern.PulseMode.Alternative == PulseAlternative.Alt2)
            {
                double SineVal = (double)Domain.ElectricalState.BaseWaveAmplitude * Functions.Sine(RawX);
                double beta = Common.GetPulseDataValue(Domain.ElectricalState.PulseData, PulseDataKey.PulseWidth) / 180 * M_PI;
                double x = RawX % M_2PI;
                int Orthant = (int)(x / M_PI_2) % 4;

                double _GetCarrier(double x, double beta)
                {
                    if (0 <= x && x < beta)
                        return -(1 / beta) * x + 1;
                    else if (beta <= x && x < M_PI_2)
                        return -1 / (M_PI_2 - beta) * (x - M_PI_2);
                    else
                        return 0;
                }

                double SawVal = Orthant switch
                {
                    0 => _GetCarrier(x, beta),
                    1 => _GetCarrier(M_PI - x, beta),
                    2 => -_GetCarrier(x - M_PI, beta),
                    _ => -_GetCarrier(M_2PI - x, beta)
                };

                return Orthant <= 1 ? Common.ModulateSignal(SineVal, SawVal) + 1 : -Common.ModulateSignal(SawVal, SineVal) + 1;
            }

            { // nP DEFAULT
                Domain.GetCarrierInstance().AngleFrequency = Domain.ElectricalState.BaseWaveAngleFrequency;
                Domain.GetCarrierInstance().Time = Domain.GetBaseWaveInstance().Time;

                double SineVal = Common.GetBaseWaveform(Domain, Phase, InitialPhase);
                double CarrierVal = Common.GetCarrierWaveform(Domain, Domain.ElectricalState.PulsePattern.PulseMode.PulseCount * RawX);

                double Dipolar = Common.GetPulseDataValue(Domain.ElectricalState.PulseData, PulseDataKey.Dipolar);
                CarrierVal *= (Dipolar != -1 ? Dipolar : 0.5);

                return Common.ModulateSignal(SineVal, CarrierVal + 0.5) + Common.ModulateSignal(SineVal, CarrierVal - 0.5);
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
        public static Common.PhaseStateCalculator GetCalculator(PulseTypeName PulseType)
        {
            return PulseType switch
            {
                PulseTypeName.ASYNC => Async,
                PulseTypeName.SYNC => Sync,
                _ => FromCustomPwm
            };
        }
    }
}
