using System;
using System.Collections.Generic;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Vvvf.Model;
using static VvvfSimulator.Vvvf.Model.Struct;
using static VvvfSimulator.Data.Vvvf.Struct;
using static VvvfSimulator.Data.Vvvf.Struct.JerkSettings;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;

namespace VvvfSimulator.Data.Vvvf
{
    public class Analyze
    {
        private static double GetChangingValue(double x1, double y1, double x2, double y2, double x)
        {
            return y1 + (y2 - y1) / (x2 - x1) * (x - x1);
        }
        private static double GetMovingValue(FunctionValue Data, double Current)
        {
            double val = 1000;
            if (Data.Type == FunctionValue.FunctionType.Proportional)
                val = GetChangingValue(
                    Data.Start,
                    Data.StartValue,
                    Data.End,
                    Data.EndValue,
                    Current
                );
            else if (Data.Type == FunctionValue.FunctionType.Pow2_Exponential)
                val = (Math.Pow(2, Math.Pow((Current - Data.Start) / (Data.End - Data.Start), Data.Degree)) - 1) * (Data.EndValue - Data.StartValue) + Data.StartValue;
            else if (Data.Type == FunctionValue.FunctionType.Inv_Proportional)
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
            else if (Data.Type == FunctionValue.FunctionType.Sine)
            {
                double x = (MyMath.M_PI_2 - Math.Asin(Data.StartValue / Data.EndValue)) / (Data.End - Data.Start) * (Current - Data.Start) + Math.Asin(Data.StartValue / Data.EndValue);
                val = Math.Sin(x) * Data.EndValue;
            }

            return val;

        }
        private static double GetAmplitude(AmplitudeValue.Parameter Param, double Current)
        {
            double Amplitude = 0;

            if (Param.EndAmplitude == Param.StartAmplitude) Amplitude = Param.StartAmplitude;
            else if (Param.Mode == AmplitudeValue.Parameter.ValueMode.Linear)
            {
                if (!Param.DisableRangeLimit)
                {
                    if (Current < Param.StartFrequency) Current = Param.StartFrequency;
                    if (Current > Param.EndFrequency) Current = Param.EndFrequency;
                }
                Amplitude = (Param.EndAmplitude - Param.StartAmplitude) / (Param.EndFrequency - Param.StartFrequency) * (Current - Param.StartFrequency) + Param.StartAmplitude;
            }
            else if (Param.Mode == AmplitudeValue.Parameter.ValueMode.InverseProportional)
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
            else if (Param.Mode == AmplitudeValue.Parameter.ValueMode.Exponential)
            {
                if (!Param.DisableRangeLimit)
                {
                    if (Current > Param.EndFrequency) Current = Param.EndFrequency;
                }

                double t = 1 / Param.EndFrequency * Math.Log(Param.EndAmplitude + 1);

                Amplitude = Math.Pow(Math.E, t * Current) - 1;
            }
            else if (Param.Mode == AmplitudeValue.Parameter.ValueMode.LinearPolynomial)
            {
                if (!Param.DisableRangeLimit)
                {
                    if (Current > Param.EndFrequency) Current = Param.EndFrequency;
                }
                Amplitude = Math.Pow(Current, Param.Polynomial) / Math.Pow(Param.EndFrequency, Param.Polynomial) * Param.EndAmplitude;
            }
            else if (Param.Mode == AmplitudeValue.Parameter.ValueMode.Sine)
            {
                if (!Param.DisableRangeLimit)
                {
                    if (Current > Param.EndFrequency) Current = Param.EndFrequency;
                }

                double x = Math.PI * Current / (2.0 * Param.EndFrequency);

                Amplitude = Math.Sin(x) * Param.EndAmplitude;
            }
            else if (Param.Mode == AmplitudeValue.Parameter.ValueMode.Table)
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
        private static bool IsMatching(Domain Control, PulseControl ysd)
        {
            bool enable_free_run_condition = Control.IsFreeRun() && ((!Control.IsPowerOff() && ysd.EnableFreeRunOn) || (Control.IsPowerOff() && ysd.EnableFreeRunOff));
            bool enable_normal_condition = ysd.EnableNormal && !Control.IsFreeRun();
            if (!(enable_free_run_condition || enable_normal_condition)) return false;

            bool Condition1 = ysd.ControlFrequencyFrom <= Control.GetControlFrequency();
            bool Condition2 = ysd.RotateFrequencyFrom == -1 || ysd.RotateFrequencyFrom <= Control.GetBaseWaveFrequency();
            bool Condition3 = ysd.RotateFrequencyBelow == -1 || ysd.RotateFrequencyBelow > Control.GetBaseWaveFrequency();

            if (!Condition2) return false;
            if (!Condition3) return false;

            if (!Control.IsFreeRun() && Condition1) return true;
            if (!Control.IsFreeRun() && !Condition1) return false;

            if (Condition1) return true;

            if (
                (ysd.StuckFreeRunOn && Control.IsFreeRun() && !Control.IsPowerOff()) ||
                (ysd.StuckFreeRunOff && Control.IsFreeRun() && Control.IsPowerOff())
            )
            {
                if (Control.GetBaseWaveFrequency() > ysd.ControlFrequencyFrom) return true;
                return false;
            }

            return false;

        }
        public static void Calculate(Domain Domain, Struct Data)
        {
            // Minimum Frequency Solve
            double minBaseFrequency;
            if (Domain.IsBraking()) minBaseFrequency = Data.MinimumFrequency.Braking;
            else minBaseFrequency = Data.MinimumFrequency.Accelerating;
            if (0 < Domain.GetControlFrequency() && Domain.GetControlFrequency() < minBaseFrequency && !Domain.IsFreeRun()) Domain.SetControlFrequency(minBaseFrequency);
            Domain.SetBaseWaveTimeChangeAllowed(!(Domain.GetBaseWaveFrequency() < minBaseFrequency && Domain.GetControlFrequency() > 0));
            double solvedElectricBaseWaveFrequency = Domain.IsBaseWaveTimeChangeAllowed() ? Domain.GetBaseWaveFrequency() : minBaseFrequency;

            // Pattern Solve
            int solveIndex = -1;
            List<PulseControl> patternList = [.. Domain.IsBraking() ? Data.BrakingPattern : Data.AcceleratePattern];
            patternList.Sort((a, b) => b.ControlFrequencyFrom.CompareTo(a.ControlFrequencyFrom));
            for (int index = 0; index < patternList.Count; index++)
            {
                PulseControl ysd = patternList[index];
                if (!IsMatching(Domain, ysd)) continue;
                solveIndex = index;
                break;
            }
            if (solveIndex == -1)
            {
                if (Domain.IsFreeRun())
                {
                    if (Domain.IsPowerOff())
                        Domain.SetControlFrequency(0);
                    else
                        Domain.SetControlFrequency(Domain.GetBaseWaveFrequency());
                }
                Domain.ElectricalState = new(Data.Level, solvedElectricBaseWaveFrequency);
                return;
            }

            // Target Pattern
            PulseControl solvePattern = patternList[solveIndex];
            Pulse solvePulse = solvePattern.PulseMode;

            // Solve Async Carrier Frequency
            ElectricalParameter.CarrierParameter? solvedCarrierFrequency = null;
            if (solvePulse.PulseType == PulseTypeName.ASYNC)
            {
                ElectricalParameter.CarrierParameter.RandomFrequency randomFrequency = new(
                    solvePattern.AsyncModulationData.RandomData.Range.Mode switch
                    {
                        AsyncControl.RandomModulation.Parameter.ValueMode.Moving => GetMovingValue(solvePattern.AsyncModulationData.RandomData.Range.MovingValue, Domain.GetControlFrequency()),
                        _ => solvePattern.AsyncModulationData.RandomData.Range.Constant,
                    },
                    solvePattern.AsyncModulationData.RandomData.Interval.Mode switch
                    {
                        AsyncControl.RandomModulation.Parameter.ValueMode.Moving => GetMovingValue(solvePattern.AsyncModulationData.RandomData.Interval.MovingValue, Domain.GetControlFrequency()),
                        _ => solvePattern.AsyncModulationData.RandomData.Interval.Constant,
                    }
                );

                ElectricalParameter.CarrierParameter.VibratoFrequency SolveVibratoParameter() { 
                    double SolveVibratoValue(AsyncControl.CarrierFrequency.VibratoValue.Parameter ValueData)
                    {
                        return ValueData.Mode switch
                        {
                            AsyncControl.CarrierFrequency.VibratoValue.Parameter.ValueMode.Moving => GetMovingValue(ValueData.MovingValue, Domain.GetControlFrequency()),
                            _ => ValueData.Constant
                        };
                    }
                    return new ElectricalParameter.CarrierParameter.VibratoFrequency(
                        SolveVibratoValue(solvePattern.AsyncModulationData.CarrierWaveData.VibratoData.Highest),
                        SolveVibratoValue(solvePattern.AsyncModulationData.CarrierWaveData.VibratoData.Lowest),
                        SolveVibratoValue(solvePattern.AsyncModulationData.CarrierWaveData.VibratoData.Interval)
                    );
                }

                ElectricalParameter.CarrierParameter.ConstantFrequency SolveTableParameter()
                {
                    List<AsyncControl.CarrierFrequency.TableValue.Parameter> Table = [.. solvePattern.AsyncModulationData.CarrierWaveData.CarrierFrequencyTable.Table];
                    Table.Sort((a, b) => Math.Sign(b.ControlFrequencyFrom - a.ControlFrequencyFrom));
                    int target = 0;
                    for (int i = 0; i < Table.Count; i++)
                    {
                        var carrier = Table[i];
                        bool flag1 = carrier.FreeRunStuckAtHere && (Domain.GetBaseWaveFrequency() >= carrier.ControlFrequencyFrom) && Domain.IsFreeRun();
                        bool flag2 = Domain.GetControlFrequency() > carrier.ControlFrequencyFrom;
                        if (!flag1 && !flag2) continue;
                        target = i;
                        break;
                    }
                    return new ElectricalParameter.CarrierParameter.ConstantFrequency(Table[target].CarrierFrequency);
                }

                object baseFrequency = solvePattern.AsyncModulationData.CarrierWaveData.Mode switch
                {
                    AsyncControl.CarrierFrequency.ValueMode.Vibrato => SolveVibratoParameter(),
                    AsyncControl.CarrierFrequency.ValueMode.Table => SolveTableParameter(),
                    AsyncControl.CarrierFrequency.ValueMode.Moving => new ElectricalParameter.CarrierParameter.ConstantFrequency(GetMovingValue(solvePattern.AsyncModulationData.CarrierWaveData.MovingValue, Domain.GetControlFrequency())),
                    _ => new ElectricalParameter.CarrierParameter.ConstantFrequency(solvePattern.AsyncModulationData.CarrierWaveData.Constant),
                };

                solvedCarrierFrequency = new ElectricalParameter.CarrierParameter(randomFrequency, baseFrequency);
            }

            // Solve Amplitude
            double solvedAmplitude = 0;
            if (Domain.IsFreeRun())
            {

                Jerk baseFrequencyInfo = Domain.IsBraking() ? Data.JerkSetting.Braking : Data.JerkSetting.Accelerating;
                AmplitudeValue.Parameter Param = (Domain.IsPowerOff() ? solvePattern.Amplitude.PowerOff : solvePattern.Amplitude.PowerOn).Clone();
                double MaxControlFrequency = !Domain.IsPowerOff() ? baseFrequencyInfo.On.MaxControlFrequency : baseFrequencyInfo.Off.MaxControlFrequency;

                if (Param.EndFrequency == -1)
                {
                    if (solvePattern.Amplitude.Default.DisableRangeLimit) Param.EndFrequency = Domain.GetBaseWaveFrequency();
                    else
                    {
                        Param.EndFrequency = (Domain.GetBaseWaveFrequency() > MaxControlFrequency) ? MaxControlFrequency : Domain.GetBaseWaveFrequency();
                        Param.EndFrequency = (Param.EndFrequency > solvePattern.Amplitude.Default.EndFrequency) ? solvePattern.Amplitude.Default.EndFrequency : Param.EndFrequency;
                    }
                }

                if (Param.EndAmplitude == -1) Param.EndAmplitude = GetAmplitude(solvePattern.Amplitude.Default, Domain.GetBaseWaveFrequency());
                if (Param.StartAmplitude == -1) Param.StartAmplitude = GetAmplitude(solvePattern.Amplitude.Default, Domain.GetBaseWaveFrequency());
                solvedAmplitude = GetAmplitude(Param, Domain.GetControlFrequency());
            }
            else
                solvedAmplitude = GetAmplitude(solvePattern.Amplitude.Default, Domain.GetControlFrequency());

            // Solve PulseData
            Dictionary<PulseDataKey, double> solvedPulseData = [];
            PulseDataKey[] PulseDataKeys = Config.GetAvailablePulseDataKey(solvePulse, Data.Level);
            for (int i = 0; i < PulseDataKeys.Length; i++)
            {
                PulseDataValue? Value = solvePulse.PulseData.GetValueOrDefault(PulseDataKeys[i]);
                double Val = Config.GetPulseDataKeyDefaultConstant(PulseDataKeys[i]);
                if (Value != null)
                {
                    Val = Value.Mode switch
                    {
                        PulseDataValue.PulseDataValueMode.Moving => GetMovingValue(Value.MovingValue, Domain.GetControlFrequency()),
                        _ => Value.Constant,
                    };
                }

                solvedPulseData.Add(PulseDataKeys[i], Val);
            }

            if (Domain.IsPowerOff() && solvedAmplitude == 0) Domain.SetControlFrequency(0);

            Domain.ElectricalState = new ElectricalParameter(
                false,
                Domain.ElectricalState.BaseWaveAmplitude == 0 || Domain.GetControlFrequency() == 0,
                Data.Level, 
                solvePattern.Clone(), 
                solvedCarrierFrequency, 
                solvedPulseData, 
                solvedElectricBaseWaveFrequency, 
                solvedAmplitude
            );
        }
    }
}
