using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VvvfSimulator.Generation;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf.Model;
using static VvvfSimulator.Vvvf.MyMath.EquationSolver;

namespace VvvfSimulator.Data
{
    public class Tool
    {
        public class AutoModulationIndexSolver
        {
            public class SolveConfiguration : ICloneable
            {
                public Data.Vvvf.Struct SoundData { get; set; }
                public Data.TrainAudio.Struct TrainData { get; set; }
                public double AccelEndFrequency { get; set; }
                public double AccelMaxVoltage { get; set; }
                public double BrakeEndFrequency { get; set; }
                public double BrakeMaxVoltage { get; set; }
                public EquationSolverType SolverType { get; set; } = EquationSolverType.Bisection;
                public int MaxEffort { get; set; }
                public double Precision { get; set; }
                public int TableDivision { get; set; }
                public bool IsTableDivisionPerHz { get; set; }

                public SolveConfiguration(Data.Vvvf.Struct soundData, Data.TrainAudio.Struct trainData, double accelEndFrequency, double accelMaxVoltage, double brakeEndFrequency, double brakeMaxVoltage, EquationSolverType solverType, int maxEffort, double precision, int tableDivision, bool isTableDivisionPerHz)
                {
                    SoundData = soundData;
                    TrainData = trainData;
                    AccelEndFrequency = accelEndFrequency;
                    AccelMaxVoltage = accelMaxVoltage;
                    BrakeEndFrequency = brakeEndFrequency;
                    BrakeMaxVoltage = brakeMaxVoltage;
                    SolverType = solverType;
                    MaxEffort = maxEffort;
                    Precision = precision;
                    TableDivision = tableDivision;
                    IsTableDivisionPerHz = isTableDivisionPerHz;
                }
                public object Clone()
                {
                    SolveConfiguration Cloned = (SolveConfiguration)MemberwiseClone();
                    Cloned.SoundData = Data.Vvvf.Manager.DeepClone(SoundData);
                    Cloned.TrainData = Data.TrainAudio.Manager.DeepClone(TrainData);
                    return Cloned;
                }
            }
            private class InternalFunctionParameter : ICloneable
            {
                public SolveConfiguration Configuration;
                public bool IsBrakePattern;
                public int Index;
                public double MaxFrequency;
                public double MaxVoltageRate;

                public InternalFunctionParameter(SolveConfiguration configuration, bool isBrakePattern, int index, double maxFrequency, double maxVoltageRate)
                {
                    Configuration = configuration;
                    IsBrakePattern = isBrakePattern;
                    Index = index;
                    MaxFrequency = maxFrequency;
                    MaxVoltageRate = maxVoltageRate;
                }

                public object Clone()
                {
                    InternalFunctionParameter Cloned = (InternalFunctionParameter)MemberwiseClone();
                    Cloned.Configuration = (SolveConfiguration)Configuration.Clone();
                    return Cloned;
                }
            }
            private static void SolveModulationIndex(
                double TargetFrequency,
                InternalFunctionParameter Param,
                Action<double> TestAmplitudeSetter,
                Action<double> AmplitudeSetter)
            {
                try
                {
                    double DesireVoltageRate = TargetFrequency / Param.MaxFrequency * Param.MaxVoltageRate;
                    DesireVoltageRate = DesireVoltageRate > 1 ? 1 : DesireVoltageRate;

                    Struct.Domain Domain = new(Param.Configuration.TrainData.MotorSpec);
                    Domain.SetBaseWaveAngleFrequency(TargetFrequency * Math.PI * 2);
                    Domain.SetControlFrequency(TargetFrequency);
                    Domain.SetPowerOff(false);
                    Domain.SetFreeRun(false);
                    Domain.SetBraking(Param.IsBrakePattern);
                    Domain.GetCarrierInstance().UseSimpleFrequency = true;
                    Data.Vvvf.Analyze.Calculate(Domain, Param.Configuration.SoundData);

                    double SolveFunction(double Amplitude)
                    {
                        TestAmplitudeSetter(Amplitude);
                        double difference = GenerateBasic.Fourier.GetVoltageRate(Domain, true, FixSign: false) - DesireVoltageRate;
                        return difference * 100;
                    }
                    double ProperAmplitude = Param.Configuration.SolverType switch
                    {
                        EquationSolverType.Bisection => new BisectionMethod(SolveFunction).Calculate(-10, 10, Param.Configuration.Precision, Param.Configuration.MaxEffort),
                        EquationSolverType.Newton => new NewtonMethod(SolveFunction, 0.05).Calculate(DesireVoltageRate, Param.Configuration.Precision, Param.Configuration.MaxEffort),
                        _ => 0,
                    };
                    AmplitudeSetter(ProperAmplitude);
                }
                catch (Exception ex)
                {
                    string message = string.Format(LanguageManager.GetStringWithNewLine("MainWindow.Dialog.Tools.AutoVoltage.Message.Error"), Param.Index, FriendlyNameConverter.GetBoolName(Param.IsBrakePattern), TargetFrequency, ex.Message);
                    DialogBox.Show(message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                }
            }
            private static void AutoModulationIndexTask(List<Task> TaskList, InternalFunctionParameter FunctionParameter)
            {
                List<Data.Vvvf.Struct.PulseControl> ysd = FunctionParameter.IsBrakePattern ? FunctionParameter.Configuration.SoundData.BrakingPattern : FunctionParameter.Configuration.SoundData.AcceleratePattern;
                var Parameter = ysd[FunctionParameter.Index].Amplitude.Default;
                var ParameterPowerOn = ysd[FunctionParameter.Index].Amplitude.PowerOn;
                var ParameterPowerOff = ysd[FunctionParameter.Index].Amplitude.PowerOff;

                Parameter.DisableRangeLimit = false;
                Parameter.MaxAmplitude = -1;
                Parameter.CutOffAmplitude = 0;
                ParameterPowerOn.DisableRangeLimit = false;
                ParameterPowerOn.MaxAmplitude = -1;
                ParameterPowerOn.CutOffAmplitude = 0;
                ParameterPowerOff.DisableRangeLimit = false;
                ParameterPowerOff.MaxAmplitude = -1;
                ParameterPowerOff.CutOffAmplitude = 0;
                Parameter.StartFrequency = (ysd[FunctionParameter.Index].ControlFrequencyFrom <= 0 ? 0.01 : ysd[FunctionParameter.Index].ControlFrequencyFrom);
                ParameterPowerOn.StartFrequency = Parameter.StartFrequency;
                ParameterPowerOff.StartFrequency = Parameter.StartFrequency;
                Parameter.EndFrequency = (FunctionParameter.Index + 1) == ysd.Count ? FunctionParameter.MaxFrequency + (ysd[FunctionParameter.Index].ControlFrequencyFrom == FunctionParameter.MaxFrequency ? 0.1 : 0) : (ysd[FunctionParameter.Index + 1].ControlFrequencyFrom - 0.1);
                ParameterPowerOn.EndFrequency = Parameter.EndFrequency;
                ParameterPowerOff.EndFrequency = Parameter.EndFrequency;

                if (Parameter.Mode == Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode.Table)
                {
                    List<(double Frequency, double M)> ModulationIndexList = [];
                    Parameter.StartAmplitude = 0;
                    if (FunctionParameter.Configuration.IsTableDivisionPerHz)
                    {
                        for (double _Freq = Parameter.StartFrequency; _Freq <= Parameter.EndFrequency; _Freq += 1.0 / FunctionParameter.Configuration.TableDivision)
                        {
                            ModulationIndexList.Add((_Freq, 0));
                        }
                        ModulationIndexList.Add((Parameter.EndFrequency, 0));
                    }
                    else
                        for (int i = 0; i <= FunctionParameter.Configuration.TableDivision; i++) ModulationIndexList.Add(((Parameter.EndFrequency - Parameter.StartFrequency) / FunctionParameter.Configuration.TableDivision * i + Parameter.StartFrequency, 0));

                    for (int i = 0; i < ModulationIndexList.Count; i++)
                    {
                        int Index = i;
                        double Frequency = ModulationIndexList[Index].Frequency;
                        Parameter.AmplitudeTable = [.. ModulationIndexList];
                        InternalFunctionParameter Tester = (InternalFunctionParameter)FunctionParameter.Clone();
                        TaskList.Add(Task.Run(() =>
                        {
                            SolveModulationIndex(Frequency, Tester,
                            (double Amplitude) =>
                            {
                                Data.Vvvf.Struct.PulseControl Target = (Tester.IsBrakePattern ? Tester.Configuration.SoundData.BrakingPattern : Tester.Configuration.SoundData.AcceleratePattern)[Tester.Index];
                                Target.Amplitude.Default.AmplitudeTable = [(Frequency, Amplitude)];
                            },
                            (double Amplitude) =>
                            {
                                ModulationIndexList[Index] = (Frequency, Amplitude);
                                Parameter.AmplitudeTable = [.. ModulationIndexList];
                            });
                        }));
                    }
                }
                else
                {
                    TaskList.Add(Task.Run(() => {
                        void Setter(double Amplitude)
                        {
                            Parameter.StartAmplitude = Amplitude;
                            ParameterPowerOn.StartAmplitude = Amplitude;
                            ParameterPowerOff.StartAmplitude = Amplitude;
                        }
                        SolveModulationIndex(Parameter.StartFrequency, FunctionParameter, Setter, Setter);
                    }));
                    TaskList.Add(Task.Run(() => {
                        void Setter(double Amplitude)
                        {
                            Parameter.EndAmplitude = Amplitude;
                            ParameterPowerOn.EndAmplitude = Amplitude;
                            ParameterPowerOff.EndAmplitude = Amplitude;
                        }
                        SolveModulationIndex(Parameter.EndFrequency, FunctionParameter, Setter, Setter);
                    }));
                }
            }
            public static bool Run(SolveConfiguration Configuration)
            {
                if (Configuration.SoundData == null) return false;
                if (Configuration.SoundData.AcceleratePattern.Count == 0) return false;
                if (Configuration.SoundData.BrakingPattern.Count == 0) return false;

                List<Data.Vvvf.Struct.PulseControl> accel = Configuration.SoundData.AcceleratePattern;
                List<Data.Vvvf.Struct.PulseControl> brake = Configuration.SoundData.BrakingPattern;

                for (int i = 0; i < Configuration.SoundData.AcceleratePattern.Count; i++)
                {
                    if (Configuration.SoundData.AcceleratePattern[i].ControlFrequencyFrom < 0) return false;
                }
                for (int i = 0; i < Configuration.SoundData.BrakingPattern.Count; i++)
                {
                    if (Configuration.SoundData.BrakingPattern[i].ControlFrequencyFrom < 0) return false;
                }

                Configuration.SoundData.SortAcceleratePattern(true);
                Configuration.SoundData.SortBrakingPattern(true);

                List<Task> TaskList = [];
                for (int i = 0; i < accel.Count; i++)
                {
                    InternalFunctionParameter Pram = new(
                        Configuration,
                        false,
                        i,
                        Configuration.AccelEndFrequency,
                        Configuration.AccelMaxVoltage / 100.0
                    );
                    AutoModulationIndexTask(TaskList, Pram);
                }
                for (int i = 0; i < brake.Count; i++)
                {
                    InternalFunctionParameter Pram = new(
                        Configuration,
                        true,
                        i,
                        Configuration.BrakeEndFrequency,
                        Configuration.BrakeMaxVoltage / 100.0
                    );
                    AutoModulationIndexTask(TaskList, Pram);
                }
                Task.WaitAll([.. TaskList]);

                Configuration.SoundData.SortAcceleratePattern(false);
                Configuration.SoundData.SortBrakingPattern(false);

                return true;
            }
        }
    }
}
