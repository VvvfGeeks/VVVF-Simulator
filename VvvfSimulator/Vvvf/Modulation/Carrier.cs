using System;
using VvvfSimulator.Vvvf.Model;

namespace VvvfSimulator.Vvvf.Modulation
{
    public class Carrier
    {
        public double AngleFrequency { get; set; } = 0;
        public double Frequency
        {
            get
            {
                return AngleFrequency / MyMath.M_2PI;
            }
            set
            {
                AngleFrequency = MyMath.M_2PI * value;
            }
        }
        public double Time { get; set; } = 0;
        public double AsyncAngleFrequency
        {
            set
            {
                if (value == 0)
                    Time = 0;
                else
                    Time = AngleFrequency / value * Time;
                AngleFrequency = value;
            }
        }
        public double AsyncFrequency
        {
            set
            {
                if (value == 0)
                    Time = 0;
                else
                    Time = Frequency / value * Time;
                Frequency = value;
            }
        }
        public double Phase
        {
            get
            {
                return Time * AngleFrequency;
            }
        }
        public bool UseSimpleFrequency { get; set; } = false;
        private RandomFrequency RandomInstance;
        private VibratoFrequency VibratoInstance;

        public Carrier()
        {
            RandomInstance = new();
            VibratoInstance = new();
        }

        public Carrier Clone()
        {
            Carrier Copy = (Carrier)MemberwiseClone();
            Copy.RandomInstance = RandomInstance.Clone();
            Copy.VibratoInstance = VibratoInstance.Clone();
            return Copy;
        }

        public double CalculateBaseCarrierFrequency(double Time, Struct.ElectricalParameter ElectricalState)
        {
            if (ElectricalState.IsNone)
                return 0;

            object BaseCarrierFrequencyParameter = ElectricalState.CarrierFrequency.BaseFrequency;
            double BaseCarrierFrequency = 0;

            if (BaseCarrierFrequencyParameter is Struct.ElectricalParameter.CarrierParameter.ConstantFrequency Constant)
                BaseCarrierFrequency = Constant.Value;
            else if (BaseCarrierFrequencyParameter is Struct.ElectricalParameter.CarrierParameter.VibratoFrequency Vibrato)
            {
                VibratoInstance.SetState(UseSimpleFrequency, Time);
                VibratoInstance.SetCustomParameter(Vibrato, ElectricalState.PulsePattern.AsyncModulationData.CarrierWaveData.VibratoData.BaseWave);
                BaseCarrierFrequency = VibratoInstance.Calculate();
            }
            return BaseCarrierFrequency;
        }
        public double CalculateCarrierFrequency(double Time, Struct.ElectricalParameter ElectricalState)
        {
            if (ElectricalState.IsNone)
                return 0;

            double FinalCarrierFrequency;
            double BaseCarrierFrequency = CalculateBaseCarrierFrequency(Time, ElectricalState);

            RandomInstance.SetState(UseSimpleFrequency, Time);
            RandomInstance.SetCustomParameter(ElectricalState.CarrierFrequency.RandomRange, BaseCarrierFrequency);

            FinalCarrierFrequency = RandomInstance.Calculate();
            return FinalCarrierFrequency;
        }
        public void ProcessCarrierFrequency(double Time, Struct.ElectricalParameter ElectricalState)
        {
            AsyncFrequency = CalculateCarrierFrequency(Time, ElectricalState);
        }
        public void ResetIFrequencyTime(double Time)
        {
            RandomInstance.ResetTime(Time);
            VibratoInstance.ResetTime(Time);
        }

        public interface IFrequency
        {
            public void SetState(bool Simple, double Time);
            public double Calculate();
            public void ResetTime(double Time);
        }
        public class RandomFrequency : IFrequency
        {
            private bool Simple = false;
            private double Time = 0;

            private Struct.ElectricalParameter.CarrierParameter.RandomFrequency? Parameter = null;
            private double BaseFrequency = 0;

            private double LastRange = 0;
            private double LastUpdateTime = 0;
            private readonly Random RandomInstance = new();

            public RandomFrequency Clone()
            {
                RandomFrequency Copy = (RandomFrequency)MemberwiseClone();
                Copy.Parameter = Parameter?.Clone();
                return Copy;
            }
            public void SetState(bool Simple, double Time)
            {
                this.Simple = Simple;
                this.Time = Time;
            }
            public void SetCustomParameter(Struct.ElectricalParameter.CarrierParameter.RandomFrequency Parameter, double BaseFrequency)
            {
                this.Parameter = Parameter;
                this.BaseFrequency = BaseFrequency;
            }
            public double Calculate()
            {
                if (Parameter == null) return double.NaN;
                if (Simple) return BaseFrequency;

                if (LastUpdateTime + Parameter.Interval < Time)
                {
                    double Range = RandomInstance.NextDouble() * Parameter.Range;
                    if (RandomInstance.NextDouble() < 0.5) Range = -Range;
                    LastRange = Range;
                    LastUpdateTime = Time;
                }
                return BaseFrequency + LastRange;
            }
            public void ResetTime(double Time)
            {
                this.Time = Time;
                LastUpdateTime = Time;
            }
        }
        public class VibratoFrequency : IFrequency
        {
            private bool Simple = false;
            private double Time = 0;

            private Struct.ElectricalParameter.CarrierParameter.VibratoFrequency? Parameter = null;
            private Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType? BaseWaveType = null;

            private double LastInterval = 0;
            private double LastTime = 0;

            public VibratoFrequency Clone()
            {
                VibratoFrequency Copy = (VibratoFrequency)MemberwiseClone();
                Copy.Parameter = Parameter?.Clone();
                return Copy;
            }
            public void SetState(bool Simple, double Time)
            {
                this.Simple = Simple;
                this.Time = Time;
            }
            public void SetCustomParameter(Struct.ElectricalParameter.CarrierParameter.VibratoFrequency Parameter, Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType BaseWaveType)
            {
                this.Parameter = Parameter;
                this.BaseWaveType = BaseWaveType;
            }
            public double Calculate()
            {
                if (Parameter == null || BaseWaveType == null) return double.NaN;
                if (Simple) return (Parameter.Highest + Parameter.Lowest) / 2.0;

                if (LastInterval != Parameter.Interval)
                {
                    LastTime = Time - (Time - LastTime) * (LastInterval == 0 ? 1 : Parameter.Interval / LastInterval);
                    LastInterval = Parameter.Interval;
                    LastTime = Time;
                }

                if (Time - LastTime >= Parameter.Interval)
                    LastTime = Time;

                double Phase = Parameter.Interval > 0 ? (Time - LastTime) / Parameter.Interval * MyMath.M_2PI : 0;
                return BaseWaveType switch
                {
                    Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType.Sine => (Parameter.Highest - Parameter.Lowest) / 2 * (MyMath.Functions.Sine(Phase) + 1) + Parameter.Lowest,
                    Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType.Triangle => (Parameter.Highest - Parameter.Lowest) / 2 * (MyMath.Functions.Triangle(Phase) + 1) + Parameter.Lowest,
                    Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType.Square => (Parameter.Highest - Parameter.Lowest) / 2 * (MyMath.Functions.Square(Phase) + 1) + Parameter.Lowest,
                    Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency.VibratoValue.BaseWaveType.SawUp => (Parameter.Highest - Parameter.Lowest) / 2 * (MyMath.Functions.Saw(Phase) + 1) + Parameter.Lowest,
                    _ => (Parameter.Highest - Parameter.Lowest) / 2 * (-MyMath.Functions.Saw(Phase) + 1) + Parameter.Lowest,
                };
            }
            public void ResetTime(double Time)
            {
                this.Time = Time;
                LastTime = Time;
            }
        }
    }
}
