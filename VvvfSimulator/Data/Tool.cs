using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VvvfSimulator.Generation;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.TaskViewer;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf.Model;
using static VvvfSimulator.Vvvf.MyMath.EquationSolver;

namespace VvvfSimulator.Data
{
    public class Tool
    {
        public class AutoModulationIndexSolver
        {
            public class SolveConfiguration
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
                public SolveConfiguration Clone()
                {
                    SolveConfiguration Cloned = (SolveConfiguration)MemberwiseClone();
                    Cloned.SoundData = Data.Vvvf.Manager.DeepClone(SoundData);
                    Cloned.TrainData = Data.TrainAudio.Manager.DeepClone(TrainData);
                    return Cloned;
                }
            }
            private class TaskParameter
            {
                public TaskProgress Progress;
                public SolveConfiguration Configuration;
                public bool IsBrakePattern;
                public int Index;
                public double MaxFrequency { 
                    get
                    {
                        return IsBrakePattern ? Configuration.BrakeEndFrequency : Configuration.AccelEndFrequency;
                    } 
                }
                public double MaxVoltageRate
                {
                    get
                    {
                        return (IsBrakePattern ? Configuration.BrakeMaxVoltage : Configuration.AccelMaxVoltage) * 0.01;
                    }
                }

                public TaskParameter(TaskProgress progress, SolveConfiguration configuration, bool isBrakePattern, int index)
                {
                    Progress = progress;
                    Configuration = configuration;
                    IsBrakePattern = isBrakePattern;
                    Index = index;
                }

                public TaskParameter Clone()
                {
                    TaskParameter Cloned = (TaskParameter)MemberwiseClone();
                    Cloned.Configuration = Configuration.Clone();
                    return Cloned;
                }
            }
            private static void SolveModulationIndex(
                TaskParameter Param,
                double TargetFrequency,
                Action<double> OnComplete)
            {
                try
                {
                    if (Param.Progress.Cancel)
                        return;

                    double DesireVoltageRate = TargetFrequency / Param.MaxFrequency * Param.MaxVoltageRate;
                    DesireVoltageRate = DesireVoltageRate > 1 ? 1 : DesireVoltageRate;

                    Struct.Domain Domain = new(Param.Configuration.TrainData.MotorSpec);
                    Domain.SetPowerOff(false);
                    Domain.SetFreeRun(false);
                    Domain.SetBraking(Param.IsBrakePattern);
                    Domain.GetCarrierInstance().UseSimpleFrequency = true;
                    Domain.SetControlFrequency(TargetFrequency);
                    Domain.GetBaseWaveInstance().Frequency = TargetFrequency;
                    Data.Vvvf.Analyze.Calculate(Domain, Param.Configuration.SoundData);

                    double SolveFunction(double Amplitude)
                    {
                        if (Param.Progress.Cancel)
                            return 0;

                        Domain.ElectricalState = new Struct.ElectricalParameter(
                            Domain.ElectricalState.IsNone,
                            Domain.ElectricalState.IsZeroOutput,
                            Domain.ElectricalState.PwmLevel,
                            Domain.ElectricalState.PulsePattern,
                            Domain.ElectricalState.CarrierFrequency,
                            Domain.ElectricalState.PulseData,
                            Domain.ElectricalState.BaseWaveFrequency,
                            Amplitude
                        );
                        double difference = GenerateBasic.Fourier.GetVoltageRate(Domain, false, FixSign: false) - DesireVoltageRate;
                        return difference * 100;
                    }

                    double ProperAmplitude = Param.Configuration.SolverType switch
                    {
                        EquationSolverType.Bisection => new BisectionMethod(SolveFunction).Calculate(-10, 10, Param.Configuration.Precision, Param.Configuration.MaxEffort),
                        EquationSolverType.Newton => new NewtonMethod(SolveFunction, 0.05).Calculate(DesireVoltageRate, Param.Configuration.Precision, Param.Configuration.MaxEffort),
                        _ => 0,
                    };
                    
                    OnComplete(ProperAmplitude);
                }
                catch (Exception ex)
                {
                    string message = string.Format(LanguageManager.GetStringWithNewLine("MainWindow.Dialog.Tools.AutoVoltage.Message.Error"), Param.Index, FriendlyNameConverter.GetBoolName(Param.IsBrakePattern), TargetFrequency, ex.Message);
                    DialogBox.Show(message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                }
            }
            private static void AutoModulationIndexTask(TaskParameter TaskParam, List<Task> TaskList)
            {
                List<Data.Vvvf.Struct.PulseControl> ysd = TaskParam.IsBrakePattern ? TaskParam.Configuration.SoundData.BrakingPattern : TaskParam.Configuration.SoundData.AcceleratePattern;
                var Parameter = ysd[TaskParam.Index].Amplitude.Default;
                var ParameterPowerOn = ysd[TaskParam.Index].Amplitude.PowerOn;
                var ParameterPowerOff = ysd[TaskParam.Index].Amplitude.PowerOff;

                Parameter.DisableRangeLimit = false;
                Parameter.MaxAmplitude = -1;
                Parameter.CutOffAmplitude = 0;
                ParameterPowerOn.DisableRangeLimit = false;
                ParameterPowerOn.MaxAmplitude = -1;
                ParameterPowerOn.CutOffAmplitude = 0;
                ParameterPowerOff.DisableRangeLimit = false;
                ParameterPowerOff.MaxAmplitude = -1;
                ParameterPowerOff.CutOffAmplitude = 0;
                Parameter.StartFrequency = (ysd[TaskParam.Index].ControlFrequencyFrom <= 0 ? 0.01 : ysd[TaskParam.Index].ControlFrequencyFrom);
                ParameterPowerOn.StartFrequency = Parameter.StartFrequency;
                ParameterPowerOff.StartFrequency = Parameter.StartFrequency;
                Parameter.EndFrequency = (TaskParam.Index + 1) == ysd.Count ? TaskParam.MaxFrequency + (ysd[TaskParam.Index].ControlFrequencyFrom == TaskParam.MaxFrequency ? 0.1 : 0) : (ysd[TaskParam.Index + 1].ControlFrequencyFrom - 0.1);
                ParameterPowerOn.EndFrequency = Parameter.EndFrequency;
                ParameterPowerOff.EndFrequency = Parameter.EndFrequency;

                if (Parameter.Mode == Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter.ValueMode.Table)
                {
                    List<(double Frequency, double M)> ModulationIndexList = [];
                    Parameter.StartAmplitude = 0;
                    if (TaskParam.Configuration.IsTableDivisionPerHz)
                    {
                        for (double _Freq = Parameter.StartFrequency; _Freq <= Parameter.EndFrequency; _Freq += 1.0 / TaskParam.Configuration.TableDivision)
                            ModulationIndexList.Add((_Freq, 0));
                        ModulationIndexList.Add((Parameter.EndFrequency, 0));
                    }
                    else
                        for (int i = 0; i <= TaskParam.Configuration.TableDivision; i++) ModulationIndexList.Add(((Parameter.EndFrequency - Parameter.StartFrequency) / TaskParam.Configuration.TableDivision * i + Parameter.StartFrequency, 0));

                    for (int Index = 0; Index < ModulationIndexList.Count; Index++)
                    {
                        double Frequency = ModulationIndexList[Index].Frequency;
                        int localIndex = Index;
                        Parameter.AmplitudeTable = [.. ModulationIndexList];
                        TaskParameter Tester = TaskParam.Clone();
                        TaskList.Add(Task.Run(() =>
                        {
                            SolveModulationIndex(Tester, Frequency, 
                                (double Amplitude) =>
                                {
                                    if (TaskParam.Progress.Cancel) return;
                                    TaskParam.Progress.Progress++;
                                    ModulationIndexList[localIndex] = (Frequency, Amplitude);
                                    Parameter.AmplitudeTable = [.. ModulationIndexList];
                                }
                            );
                        }));
                    }
                }
                else
                {
                    TaskList.Add(Task.Run(() => 
                    {
                        void Setter(double Amplitude)
                        {
                            if (TaskParam.Progress.Cancel) return;
                            TaskParam.Progress.Progress++;

                            Parameter.StartAmplitude = Amplitude;
                            ParameterPowerOn.StartAmplitude = Amplitude;
                            ParameterPowerOff.StartAmplitude = Amplitude;
                        }
                        SolveModulationIndex(TaskParam, Parameter.StartFrequency, Setter);
                    }));
                    TaskList.Add(Task.Run(() => 
                    {
                        void Setter(double Amplitude)
                        {
                            if (TaskParam.Progress.Cancel) return;
                            TaskParam.Progress.Progress++;

                            Parameter.EndAmplitude = Amplitude;
                            ParameterPowerOn.EndAmplitude = Amplitude;
                            ParameterPowerOff.EndAmplitude = Amplitude;
                        }
                        SolveModulationIndex(TaskParam, Parameter.EndFrequency, Setter);
                    }));
                }
            }
            public static bool Run(TaskProgress Progress, SolveConfiguration Configuration)
            {
                if (Configuration.SoundData == null) return false;
                if (Configuration.SoundData.AcceleratePattern.Count == 0) return false;
                if (Configuration.SoundData.BrakingPattern.Count == 0) return false;

                for (int i = 0; i < Configuration.SoundData.AcceleratePattern.Count; i++)
                    if (Configuration.SoundData.AcceleratePattern[i].ControlFrequencyFrom < 0) return false;

                for (int i = 0; i < Configuration.SoundData.BrakingPattern.Count; i++)
                    if (Configuration.SoundData.BrakingPattern[i].ControlFrequencyFrom < 0) return false;

                Configuration.SoundData.SortAcceleratePattern(true);
                Configuration.SoundData.SortBrakingPattern(true);

                List<Task> TaskList = [];
                for (int i = 0; i < Configuration.SoundData.AcceleratePattern.Count; i++)
                    AutoModulationIndexTask(new(Progress, Configuration, false, i), TaskList);
                for (int i = 0; i < Configuration.SoundData.BrakingPattern.Count; i++)
                    AutoModulationIndexTask(new(Progress, Configuration, true, i), TaskList);

                Progress.Total = TaskList.Count;
                Task.WaitAll([.. TaskList]);

                Configuration.SoundData.SortAcceleratePattern(false);
                Configuration.SoundData.SortBrakingPattern(false);

                return true;
            }
        }
    }
}
