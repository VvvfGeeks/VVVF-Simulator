using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.Yaml.VvvfSound
{
    public class YamlVvvfUtil
    {
        public class NewtonMethod(NewtonMethod.Function function, double dx)
        {
            public delegate double Function(double x);
            private readonly Function function = function;

            public double Calculate(double begin, double tolerance, int n)
            {
                double x = begin;
                for (int i = 0; i < n; i++)
                {
                    double pre_x = x;
                    x = GetZeroIntersect(x);
                    if (pre_x == x || double.IsNaN(x) || double.IsInfinity(x)) x = pre_x + dx;
                    double fx = Math.Abs(function(x));
                    if (fx < tolerance) return x;
                }
                return x;
            }

            private double GetDerivative(double x)
            {
                double Fxdx = function(x + dx);
                double Fx = function(x);
                double Dy = Fxdx - Fx;
                double Dx = dx;
                double Derivative = Dy / Dx;
                return Derivative;
            }

            private double GetZeroIntersect(double x)
            {
                double zeroX = -function(x) / GetDerivative(x) + x;
                return zeroX;
            }
        }

        private static void AutoModulationIndexTask(YamlVvvfSoundData SoundData,bool IsBrakePattern, bool IsEnd,int Index, double MaxFrequency, double MaxVoltageRate, double Presicion, int N)
        {
            List<YamlVvvfSoundData.YamlControlData> ysd = IsBrakePattern ? SoundData.BrakingPattern : SoundData.AcceleratePattern;
            var parameter = ysd[Index].Amplitude.DefaultAmplitude.Parameter;
            var parameter_freerun_on = ysd[Index].Amplitude.FreeRunAmplitude.On.Parameter;
            var parameter_freerun_off = ysd[Index].Amplitude.FreeRunAmplitude.Off.Parameter;

            parameter.DisableRangeLimit = false;
            parameter.MaxAmplitude = -1;
            parameter.CutOffAmplitude = 0;
            parameter_freerun_on.DisableRangeLimit = false;
            parameter_freerun_on.MaxAmplitude = -1;
            parameter_freerun_on.CutOffAmplitude = 0;
            parameter_freerun_off.DisableRangeLimit = false;
            parameter_freerun_off.MaxAmplitude = -1;
            parameter_freerun_off.CutOffAmplitude = 0;

            if (!IsEnd)
            {
                parameter.StartFrequency = ysd[Index].ControlFrequencyFrom;
                parameter_freerun_on.StartFrequency = parameter.StartFrequency;
                parameter_freerun_off.StartFrequency = parameter.StartFrequency;
            }
            else
            {
                parameter.EndFrequency = (Index + 1) == ysd.Count ? MaxFrequency + (ysd[Index].ControlFrequencyFrom == MaxFrequency ? 0.1 : 0) : (ysd[Index + 1].ControlFrequencyFrom - 0.1);
                parameter_freerun_on.EndFrequency = parameter.EndFrequency;
                parameter_freerun_off.EndFrequency = parameter.EndFrequency;
            }
            double TargetFrequency = IsEnd ? parameter.EndFrequency : parameter.StartFrequency;
            double DesireVoltageRate = TargetFrequency / MaxFrequency * MaxVoltageRate;
            DesireVoltageRate = DesireVoltageRate > 1 ? 1 : DesireVoltageRate;

            VvvfValues control = new();
            control.ResetMathematicValues();
            control.ResetControlValues();
            control.SetSineAngleFrequency(TargetFrequency * Math.PI * 2);
            control.SetControlFrequency(TargetFrequency);
            control.SetMasconOff(false);
            control.SetFreeRun(false);
            control.SetBraking(IsBrakePattern);
            control.SetRandomFrequencyMoveAllowed(false);

            NewtonMethod.Function CalculateVoltageDifference = delegate (double amplitude)
            {
                if (IsEnd) parameter.EndAmplitude = amplitude;
                else parameter.StartAmplitude = amplitude;
                double difference = Generation.Video.ControlInfo.GenerateControlCommon.GetVoltageRate(control, SoundData, true) - DesireVoltageRate;
                return difference * 100;
            };

            NewtonMethod Calculator = new(CalculateVoltageDifference, 0.05);
            double ProperAmplitude = 0;
            try
            {
                ProperAmplitude = Calculator.Calculate(DesireVoltageRate, Presicion, N);
            }
            catch (Exception ex) {
                MessageBox.Show($"Auto Modulation Index set failed on item,\r\nIndex:{Index}\r\nIsBrake:{IsBrakePattern}\r\nIsEnd:{IsEnd}\r\nStackTrace:{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (IsEnd)
            {
                parameter.EndAmplitude = ProperAmplitude;
                parameter_freerun_on.EndAmplitude = ProperAmplitude;
                parameter_freerun_off.EndAmplitude = ProperAmplitude;
            }
            else {
                parameter.StartAmplitude = ProperAmplitude;
                parameter_freerun_on.StartAmplitude = ProperAmplitude;
                parameter_freerun_off.StartAmplitude = ProperAmplitude;
            }
        }

        public class AutoModulationIndexConfiguration
        {
            public YamlVvvfSoundData? Data { get; set; }
            public double AccelEndFrequency { get; set; }
            public double AccelMaxVoltage { get; set; }
            public double BrakeEndFrequency { get; set; }
            public double BrakeMaxVoltage { get; set; }
            public int MaxEffort { get; set; }
            public double Precision { get; set; }
        }
        public static bool AutoModulationIndex(AutoModulationIndexConfiguration Configuration)
        {
            if(Configuration.Data == null) return false;
            if(Configuration.Data.AcceleratePattern.Count == 0) return false;
            if(Configuration.Data.BrakingPattern.Count == 0) return false;
            
            List<YamlVvvfSoundData.YamlControlData> accel = Configuration.Data.AcceleratePattern;
            List<YamlVvvfSoundData.YamlControlData> brake = Configuration.Data.BrakingPattern;

            for (int i = 0; i < Configuration.Data.AcceleratePattern.Count; i++)
            {
                if (Configuration.Data.AcceleratePattern[i].ControlFrequencyFrom < 0) return false;
            }
            for (int i = 0; i < Configuration.Data.BrakingPattern.Count; i++)
            {
                if (Configuration.Data.BrakingPattern[i].ControlFrequencyFrom < 0) return false;
            }

            Configuration.Data.SortAcceleratePattern(true);
            Configuration.Data.SortBrakingPattern(true);

            List <Task> tasks = [];
            for (int i = 0; i < accel.Count; i++)
            {
                int _i = i;
                tasks.Add(Task.Run(() => AutoModulationIndexTask(Configuration.Data, false, false, _i, 
                    Configuration.AccelEndFrequency, Configuration.AccelMaxVoltage / 100, Configuration.Precision, Configuration.MaxEffort)));
                tasks.Add(Task.Run(() => AutoModulationIndexTask(Configuration.Data, false, true, _i, 
                    Configuration.AccelEndFrequency, Configuration.AccelMaxVoltage / 100, Configuration.Precision, Configuration.MaxEffort)));
            }
            for (int i = 0; i < brake.Count; i++)
            {
                int _i = i;
                tasks.Add(Task.Run(() => AutoModulationIndexTask(Configuration.Data, true, false, _i, 
                    Configuration.BrakeEndFrequency, Configuration.BrakeMaxVoltage / 100, Configuration.Precision, Configuration.MaxEffort)));
                tasks.Add(Task.Run(() => AutoModulationIndexTask(Configuration.Data, true, true, _i, 
                    Configuration.BrakeEndFrequency, Configuration.BrakeMaxVoltage / 100, Configuration.Precision, Configuration.MaxEffort)));
            }
            Task.WaitAll([.. tasks]);

            Configuration.Data.SortAcceleratePattern(false);
            Configuration.Data.SortBrakingPattern(false);

            return true;
        }

        public static bool SetFreeRunModulationIndexToZero(YamlVvvfSoundData data)
        {
            var accel = data.AcceleratePattern;
            for(int i = 0; i < accel.Count; i++)
            {
                accel[i].Amplitude.FreeRunAmplitude.Off.Parameter.StartAmplitude = 0;
                accel[i].Amplitude.FreeRunAmplitude.Off.Parameter.StartFrequency = 0;
                accel[i].Amplitude.FreeRunAmplitude.On.Parameter.StartAmplitude = 0;
                accel[i].Amplitude.FreeRunAmplitude.On.Parameter.StartFrequency = 0;
            }

            var brake = data.BrakingPattern;
            for (int i = 0; i < brake.Count; i++)
            {
                brake[i].Amplitude.FreeRunAmplitude.Off.Parameter.StartAmplitude = 0;
                brake[i].Amplitude.FreeRunAmplitude.Off.Parameter.StartFrequency = 0;
                brake[i].Amplitude.FreeRunAmplitude.On.Parameter.StartAmplitude = 0;
                brake[i].Amplitude.FreeRunAmplitude.On.Parameter.StartFrequency = 0;
            }

            return true;
        }

        public static bool SetFreeRunEndAmplitudeContinuous(YamlVvvfSoundData data)
        {
            var accel = data.AcceleratePattern;
            for (int i = 0; i < accel.Count; i++)
            {
                accel[i].Amplitude.FreeRunAmplitude.Off.Parameter.EndAmplitude = -1;
                accel[i].Amplitude.FreeRunAmplitude.Off.Parameter.EndFrequency = -1;
                accel[i].Amplitude.FreeRunAmplitude.On.Parameter.EndAmplitude = -1;
                accel[i].Amplitude.FreeRunAmplitude.On.Parameter.EndFrequency = -1;
            }

            var brake = data.BrakingPattern;
            for (int i = 0; i < brake.Count; i++)
            {
                brake[i].Amplitude.FreeRunAmplitude.Off.Parameter.EndAmplitude = -1;
                brake[i].Amplitude.FreeRunAmplitude.Off.Parameter.EndFrequency = -1;
                brake[i].Amplitude.FreeRunAmplitude.On.Parameter.EndAmplitude = -1;
                brake[i].Amplitude.FreeRunAmplitude.On.Parameter.EndFrequency = -1;
            }

            return true;
        }
    }
}
