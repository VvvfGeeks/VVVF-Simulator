using NAudio.Dsp;
using System;
using System.Collections.Generic;
using static VvvfSimulator.Data.Util;

namespace VvvfSimulator.Data.TrainAudio
{
    public class Struct
    {
        public List<HarmonicData> GearSound { get; set; } = [];
        public List<HarmonicData> HarmonicSound { get; set; } = [];
        public bool UseFilters { get; set; } = false;
        public List<SoundFilter> Filters { get; set; } = [];
        public bool UseConvolutionFilter { get; set; } = true;
        public int ImpulseResponseSampleRate { get; set; } = 192000;
        public float[] ImpulseResponse { get; set; } = [];
        public MotorSpecification MotorSpec { get; set; } = new MotorSpecification();
        public double MotorVolumeDb { get; set; } = -2.0;
        public double TotalVolumeDb { get; set; } = 0.0;
        public Struct() {
            Generation.Audio.TrainSound.AudioResourceManager.ReadResourceAudioFileSample(Generation.Audio.TrainSound.AudioResourceManager.SampleIrPath, out float[] Data, out int SampleRate);
            ImpulseResponse = Data;
            ImpulseResponseSampleRate = SampleRate;
        }
        public override string ToString()
        {
            return GetPropertyValues(this);
        }
        public Struct Clone()
        {
            Struct Clone = (Struct)MemberwiseClone();

            {
                List<HarmonicData> CloneGearSound = [];
                for (int i = 0; i < GearSound.Count; i++)
                {
                    CloneGearSound.Add(GearSound[i].Clone());
                }
                Clone.GearSound = CloneGearSound;
            }
            {
                List<HarmonicData> CloneHarmonicSound = [];
                for (int i = 0; i < HarmonicSound.Count; i++)
                {
                    CloneHarmonicSound.Add(HarmonicSound[i].Clone());
                }
                Clone.HarmonicSound = CloneHarmonicSound;
            }
            {
                List<SoundFilter> CloneFilters = [];
                for (int i = 0; i < Filters.Count; i++)
                {
                    CloneFilters.Add(Filters[i].Clone());
                }
                Clone.Filters = CloneFilters;
            }
            Clone.ImpulseResponse = [.. ImpulseResponse];
            Clone.MotorSpec = MotorSpec.Clone();

            return Clone;
        }
        public BiQuadFilter[,] GetFilteres(int SampleFreq)
        {
            BiQuadFilter[,] nFilteres = new BiQuadFilter[1, Filters.Count];
            for (int i = 0; i < Filters.Count; i++)
            {
                SoundFilter sf = Filters[i];
                BiQuadFilter bqf;
                switch (sf.Type)
                {
                    case SoundFilter.FilterType.PeakingEQ:
                        {
                            bqf = BiQuadFilter.PeakingEQ(SampleFreq, sf.Frequency, sf.Q, sf.Gain);
                            break;
                        }
                    case SoundFilter.FilterType.HighPassFilter:
                        {
                            bqf = BiQuadFilter.HighPassFilter(SampleFreq, sf.Frequency, sf.Q);
                            break;
                        }
                    case SoundFilter.FilterType.LowPassFilter:
                        {
                            bqf = BiQuadFilter.LowPassFilter(SampleFreq, sf.Frequency, sf.Q);
                            break;
                        }
                    default: //case SoundFilter.FilterType.NotchFilter:
                        {
                            bqf = BiQuadFilter.NotchFilter(SampleFreq, sf.Frequency, sf.Q);
                            break;
                        }
                }
                nFilteres[0, i] = bqf;
            }
            return nFilteres;
        }
        public float[] GetImpulseResponse(int SampleRate)
        {
            if (ImpulseResponseSampleRate == SampleRate)
                return ImpulseResponse;
            return Generation.Audio.TrainSound.AudioResourceManager.Resample(ImpulseResponse, ImpulseResponseSampleRate, SampleRate);
        }
        public void SetCalculatedGearHarmonic(int Gear1, int Gear2)
        {
            List<HarmonicData> GearHarmonicsList = [];
            double Rotation = 120 / Math.Pow(2, MotorSpec.Np) / 60.0;

            double[] Harmonic = [9.0 * 2 * Gear1 / Gear2 * 189.0 / 225, 9.0 * 2 * Gear1 / Gear2, 9.0, 1.0];
            for (int i = 0; i < Harmonic.Length; i++)
            {
                HarmonicData.HarmonicAmplitude amplitude = new()
                {
                    Start = 0,
                    StartValue = 0x0,
                    End = 40,
                    EndValue = 0.1 * Math.Pow(1.4, -i),
                    MinimumValue = 0,
                    MaximumValue = 0.1
                };
                GearHarmonicsList.Add(new HarmonicData { Harmonic = Rotation * Gear1 * Harmonic[i], Amplitude = amplitude, Disappear = -1 });
            }

            GearSound = [.. GearHarmonicsList];
        }
        public class SoundFilter
        {
            public FilterType Type { get; set; }
            public float Gain { get; set; }
            public float Frequency { get; set; }
            public float Q { get; set; }
            public SoundFilter() { }
            public SoundFilter(FilterType filterType, float gain, float frequency, float q)
            {
                this.Type = filterType;
                this.Gain = gain;
                this.Frequency = frequency;
                this.Q = q;
            }
            public override string ToString()
            {
                return GetPropertyValues(this);
            }
            public SoundFilter Clone()
            {
                return (SoundFilter)MemberwiseClone();
            }

            public enum FilterType
            {
                PeakingEQ, HighPassFilter, LowPassFilter, NotchFilter
            }
        }
        public class HarmonicData
        {
            public double Harmonic { get; set; } = 0;
            public HarmonicAmplitude Amplitude { get; set; } = new();
            public HarmonicDataRange Range { get; set; } = new();
            public double Disappear { get; set; } = 0;
            public override string ToString()
            {
                return GetPropertyValues(this);
            }
            public HarmonicData Clone()
            {
                var cloned = (HarmonicData)MemberwiseClone();

                cloned.Amplitude = Amplitude.Clone();
                cloned.Range = Range.Clone();

                return cloned;
            }

            public class HarmonicAmplitude
            {
                public double Start { get; set; } = 0;
                public double StartValue { get; set; } = 0;
                public double End { get; set; } = 0;
                public double EndValue { get; set; } = 0;
                public double MinimumValue { get; set; } = 0;
                public double MaximumValue { get; set; } = 0x60;
                public override string ToString()
                {
                    return GetPropertyValues(this);
                }
                public HarmonicAmplitude Clone()
                {
                    return (HarmonicAmplitude)MemberwiseClone();
                }
            }
            public class HarmonicDataRange
            {
                public double Start { get; set; } = 0;
                public double End { get; set; } = -1;
                public override string ToString()
                {
                    return GetPropertyValues(this);
                }
                public HarmonicDataRange Clone()
                {
                    return (HarmonicDataRange)MemberwiseClone();
                }
            }
        }
        public class MotorSpecification
        {
            public double V { get; set; } = 220;        // Source voltage (V) 
            public double Rs { get; set; } = 0.45;      /*stator resistance(ohm)*/
            public double Rr { get; set; } = 0.38;      /*Rotor resistance(ohm)*/
            public double Ls { get; set; } = 0.012;     /*Stator inductance(H)*/
            public double Lr { get; set; } = 0.012;     /*Rotor inductance(H)*/
            public double Lm { get; set; } = 0.011;     /*Mutual inductance(H)*/
            public double Np { get; set; } = 2;         /*Number of pole pairs*/
            public double Damping { get; set; } = 0.02; /*General damping coefficient*/
            public double Inertia { get; set; } = 0.2;  /*Rotational inertia (kg·m²)*/
            public double Fd { get; set; } = 0.001;     // Viscous (dynamic) friction coeff
            public double Fc { get; set; } = 0.001;     // Coulomb (sliding) friction torque [N*m]
            public double Fs { get; set; } = 0.01;      // static friction torque [N*m]
            public double StribeckOmega { get; set; } = 2.0; // Stribeck scale [rad/s]
            public double FricSmoothK { get; set; } = 50.0;  // Smoothing gain for tanh sign

            public override string ToString()
            {
                return GetPropertyValues(this);
            }
            public MotorSpecification Clone()
            {
                return (MotorSpecification)MemberwiseClone();
            }
        }
    }
}
