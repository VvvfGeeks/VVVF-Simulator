using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using VvvfSimulator.Generation;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Vvvf.MyMath.EquationSolver;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;

namespace VvvfSimulator.Yaml.VvvfSound
{
    public class YamlVvvfUtil
    {
        public static class AutoModulationIndexSolver
        {
            public class SolveConfiguration : ICloneable
            {
                public YamlVvvfSoundData SoundData { get; set; } = new();
                public double AccelEndFrequency { get; set; }
                public double AccelMaxVoltage { get; set; }
                public double BrakeEndFrequency { get; set; }
                public double BrakeMaxVoltage { get; set; }
                public EquationSolverType SolverType { get; set; } = EquationSolverType.Bisection;
                public int MaxEffort { get; set; }
                public double Precision { get; set; }
                public int TableDivision { get; set; }
                public bool IsTableDivisionPerHz { get; set; }
                public object Clone()
                {
                    SolveConfiguration Cloned = (SolveConfiguration)MemberwiseClone();
                    if (SoundData != null) Cloned.SoundData = YamlVvvfManage.DeepClone(SoundData);
                    return Cloned;
                }
            }
            private class InternalFunctionParameter : ICloneable
            {
                public SolveConfiguration Configuration = new();
                public bool IsBrakePattern;
                public int Index;
                public double MaxFrequency;
                public double MaxVoltageRate;

                public object Clone()
                {
                    InternalFunctionParameter Cloned = (InternalFunctionParameter)MemberwiseClone();
                    if (Configuration != null) Cloned.Configuration = (SolveConfiguration)Configuration.Clone();
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

                    VvvfValues Control = new();
                    Control.ResetMathematicValues();
                    Control.ResetControlValues();
                    Control.SetSineAngleFrequency(TargetFrequency * Math.PI * 2);
                    Control.SetControlFrequency(TargetFrequency);
                    Control.SetMasconOff(false);
                    Control.SetFreeRun(false);
                    Control.SetBraking(Param.IsBrakePattern);
                    Control.SetRandomFrequencyMoveAllowed(false);

                    double SolveFunction(double Amplitude)
                    {
                        TestAmplitudeSetter(Amplitude);
                        double difference = GenerateBasic.Fourier.GetVoltageRate(Control, Param.Configuration.SoundData, true, FixSign: false) - DesireVoltageRate;
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
                    MessageBox.Show(message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            private static void AutoModulationIndexTask(List<Task> TaskList, InternalFunctionParameter FunctionParameter)
            {
                List<YamlControlData> ysd = FunctionParameter.IsBrakePattern ? FunctionParameter.Configuration.SoundData.BrakingPattern : FunctionParameter.Configuration.SoundData.AcceleratePattern;
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

                if(Parameter.Mode == YamlControlData.YamlAmplitude.AmplitudeParameter.AmplitudeMode.Table)
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
                                YamlControlData Target = (Tester.IsBrakePattern ? Tester.Configuration.SoundData.BrakingPattern : Tester.Configuration.SoundData.AcceleratePattern)[Tester.Index];
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

                List<YamlControlData> accel = Configuration.SoundData.AcceleratePattern;
                List<YamlControlData> brake = Configuration.SoundData.BrakingPattern;

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
                    InternalFunctionParameter Pram = new()
                    {
                        Configuration = Configuration,
                        IsBrakePattern = false,
                        Index = i,
                        MaxFrequency = Configuration.AccelEndFrequency,
                        MaxVoltageRate = Configuration.AccelMaxVoltage / 100.0,
                    };
                    AutoModulationIndexTask(TaskList, Pram);
                }
                for (int i = 0; i < brake.Count; i++)
                {
                    InternalFunctionParameter Pram = new()
                    {
                        Configuration = Configuration,
                        IsBrakePattern = true,
                        Index = i,
                        MaxFrequency = Configuration.BrakeEndFrequency,
                        MaxVoltageRate = Configuration.BrakeMaxVoltage / 100.0,
                    };
                    AutoModulationIndexTask(TaskList, Pram);
                }
                Task.WaitAll([.. TaskList]);

                Configuration.SoundData.SortAcceleratePattern(false);
                Configuration.SoundData.SortBrakingPattern(false);

                return true;
            }
        }

        public static bool SetFreeRunModulationIndexToZero(YamlVvvfSoundData data)
        {
            var accel = data.AcceleratePattern;
            for(int i = 0; i < accel.Count; i++)
            {
                accel[i].Amplitude.PowerOff.StartAmplitude = 0;
                accel[i].Amplitude.PowerOff.StartFrequency = 0;
                accel[i].Amplitude.PowerOn.StartAmplitude = 0;
                accel[i].Amplitude.PowerOn.StartFrequency = 0;
            }

            var brake = data.BrakingPattern;
            for (int i = 0; i < brake.Count; i++)
            {
                brake[i].Amplitude.PowerOff.StartAmplitude = 0;
                brake[i].Amplitude.PowerOff.StartFrequency = 0;
                brake[i].Amplitude.PowerOn.StartAmplitude = 0;
                brake[i].Amplitude.PowerOn.StartFrequency = 0;
            }

            return true;
        }

        public static bool SetFreeRunEndAmplitudeContinuous(YamlVvvfSoundData data)
        {
            var accel = data.AcceleratePattern;
            for (int i = 0; i < accel.Count; i++)
            {
                accel[i].Amplitude.PowerOff.EndAmplitude = -1;
                accel[i].Amplitude.PowerOff.EndFrequency = -1;
                accel[i].Amplitude.PowerOn.EndAmplitude = -1;
                accel[i].Amplitude.PowerOn.EndFrequency = -1;
            }

            var brake = data.BrakingPattern;
            for (int i = 0; i < brake.Count; i++)
            {
                brake[i].Amplitude.PowerOff.EndAmplitude = -1;
                brake[i].Amplitude.PowerOff.EndFrequency = -1;
                brake[i].Amplitude.PowerOn.EndAmplitude = -1;
                brake[i].Amplitude.PowerOn.EndFrequency = -1;
            }

            return true;
        }
    }
}
