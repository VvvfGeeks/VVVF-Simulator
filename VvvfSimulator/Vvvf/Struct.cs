using System.Collections.Generic;
using System.Linq;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;

namespace VvvfSimulator.Vvvf
{
    public class Struct
    {
        public class VvvfValues
        {
            public VvvfValues Clone()
            {
                VvvfValues clone = (VvvfValues)MemberwiseClone();

                //Deep copy
                clone.SetVideoCarrierFrequency(clone.GetVideoCarrierFrequency().Clone());
                clone.SetVideoPulseMode(clone.GetVideoPulseMode().Clone());

                return clone;
            }



            // variables for controlling parameters
            private bool brake = false;
            private bool free_run = false;
            private double wave_stat = 0;
            private bool mascon_off = false;
            private double free_freq_change = 0.0;

            private bool allow_sine_time_change = true;
            private bool allow_random_freq_move = true;

            public void ResetControlValues()
            {
                brake = false;
                free_run = false;
                wave_stat = 0;
                mascon_off = false;
                allow_sine_time_change = true;
                allow_random_freq_move = true;
                free_freq_change = 1.0;
            }

            public double GetControlFrequency() { return wave_stat; }
            public void SetControlFrequency(double b) { wave_stat = b; }
            public void AddControlFrequency(double b) { wave_stat += b; }

            public bool IsMasconOff() { return mascon_off; }
            public void SetMasconOff(bool b) { mascon_off = b; }

            public bool IsFreeRun() { return free_run; }
            public void SetFreeRun(bool b) { free_run = b; }

            public bool IsBraking() { return brake; }
            public void SetBraking(bool b) { brake = b; }

            public bool IsSineTimeChangeAllowed() { return allow_sine_time_change; }
            public void SetSineTimeChangeAllowed(bool b) { allow_sine_time_change = b; }

            public bool IsRandomFrequencyMoveAllowed() { return allow_random_freq_move; }
            public void SetRandomFrequencyMoveAllowed(bool b) { allow_random_freq_move = b; }

            public double GetFreeFrequencyChange() { return free_freq_change; }
            public void SetFreeFrequencyChange(double d) { free_freq_change = d; }


            //--- from vvvf wave calculate
            //sin value definitions
            private double sin_angle_freq = 0;
            private double sin_time = 0;
            //saw value definitions
            private double saw_angle_freq = 1050;
            private double saw_time = 0;
            private double pre_saw_random_freq = 0;
            private double random_freq_pre_time = 0;
            private double vibrato_freq_pre_time = 0;

            public void SetSineAngleFrequency(double b) { sin_angle_freq = b; }
            public double GetSineAngleFrequency() { return sin_angle_freq; }
            public void AddSineAngleFrequency(double b) { sin_angle_freq += b; }

            // Util for sine angle freq
            public double GetSineFrequency() { return sin_angle_freq * MyMath.M_1_2PI; }

            public void SetSineTime(double t) { sin_time = t; }
            public double GetSineTime() { return sin_time; }
            public void AddSineTime(double t) { sin_time += t; }
            public void MultiplySineTime(double x) { sin_time *= x; }


            public void SetSawAngleFrequency(double f) { saw_angle_freq = f; }
            public double GetSawAngleFrequency() { return saw_angle_freq; }
            public void AddSawAngleFrequency(double f) { saw_angle_freq += f; }

            public void SetSawTime(double t) { saw_time = t; }
            public double GetSawTime() { return saw_time; }
            public void AddSawTime(double t) { saw_time += t; }
            public void MultiplySawTime(double x) { saw_time *= x; }

            public void SetPreviousSawRandomFrequency(double f) { pre_saw_random_freq = f; }
            public double GetPreviousSawRandomFrequency() { return pre_saw_random_freq; }


            public void SetRandomFrequencyPreviousTime(double i) { random_freq_pre_time = i; }
            public double GetRandomFrequencyPreviousTime() { return random_freq_pre_time; }
            public void AddRandomFrequencyPreviousTime(double x) { random_freq_pre_time += x; }

            public void SetVibratoFrequencyPreviousTime(double i) { vibrato_freq_pre_time = i; }
            public double GetVibratoFrequencyPreviousTime() { return vibrato_freq_pre_time; }

            public void ResetMathematicValues()
            {
                sin_angle_freq = 0;
                sin_time = 0;

                saw_angle_freq = 1050;
                saw_time = 0;

                random_freq_pre_time = 0;
                random_freq_pre_time = 0;

                GenerationCurrentTime = 0;
            }

            // Values for Video Generation.
            private YamlPulseMode VideoPulseMode { get; set; } = new();
            private double VideoSineAmplitude { get; set; }
            private CarrierFreq VideoCarrierFrequency { get; set; } = new CarrierFreq(0, 0, 0.0005);
            private Dictionary<PulseDataKey, double> VideoPulseData { get; set; } = [];
            private double VideoSineFrequency { get; set; }

            public void SetVideoPulseMode(YamlPulseMode p) { VideoPulseMode = p; }
            public YamlPulseMode GetVideoPulseMode() { return VideoPulseMode; }

            public void SetVideoSineAmplitude(double d) { VideoSineAmplitude = d; }
            public double GetVideoSineAmplitude() { return VideoSineAmplitude; }

            public void SetVideoCarrierFrequency(CarrierFreq c) { VideoCarrierFrequency = c; }
            public CarrierFreq GetVideoCarrierFrequency() { return VideoCarrierFrequency; }

            public void SetVideoCalculatedPulseData(Dictionary<PulseDataKey, double> Data) { VideoPulseData = Data; }
            public Dictionary<PulseDataKey, double> GetVideoCalculatedPulseData() { return VideoPulseData; }

            public void SetVideoSineFrequency(double d) { VideoSineFrequency = d; }
            public double GetVideoSineFrequency() { return VideoSineFrequency; }

            // Values for Check mascon
            private double GenerationCurrentTime { get; set; } = 0;
            public void SetGenerationCurrentTime(double d) { GenerationCurrentTime = d; }
            public double GetGenerationCurrentTime() { return GenerationCurrentTime; }
            public void AddGenerationCurrentTime(double d) { GenerationCurrentTime += d; }

        }
        public class WaveValues(int u, int v, int w)
        {
            public int U = u;
            public int V = v;
            public int W = w;

            public WaveValues Clone()
            {
                return (WaveValues)MemberwiseClone();
            }
        };

        public class CarrierFreq(double BaseFrequency, double Range, double Interval)
        {
            public CarrierFreq Clone()
            {
                return (CarrierFreq)MemberwiseClone();
            }

            public double BaseFrequency = BaseFrequency;
            public double Range = Range;
            public double Interval = Interval;
        }

        public class PwmCalculateValues
        {
            public YamlPulseMode PulseMode = new();
            public CarrierFreq Carrier = new(100, 0, 0.0005);

            public Dictionary<PulseDataKey, double> PulseData { get; set; } = [];
            public int Level;
            public bool None;

            public double Amplitude;
            public double MinimumFrequency;

            public PwmCalculateValues Clone()
            {
                var clone = (PwmCalculateValues)MemberwiseClone();

                clone.PulseData = new Dictionary<PulseDataKey, double>(PulseData);
                clone.Carrier = Carrier.Clone();
                clone.PulseMode = PulseMode.Clone();

                return clone;
            }
        }

        // "Pulse Mode" Configuration
        public class PulseModeConfiguration
        {
            public static int[] GetAvailablePulseCount(PulseTypeName PulseType, int Level)
            {
                if(Level == 2)
                {
                    return PulseType switch
                    {
                        PulseTypeName.SYNC => [-1],
                        PulseTypeName.HO => [5,7,9,11,13,15,17],
                        PulseTypeName.SHE => [3,5,7,9,11,13,15],
                        PulseTypeName.CHM => [3,5,7,9,11,13,15,17,19,21,23,25],
                        _ => [],
                    };
                }

                if(Level == 3)
                {
                    return PulseType switch
                    {
                        PulseTypeName.SYNC => [-1],
                        PulseTypeName.SHE => [1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21],
                        PulseTypeName.CHM => [1, 3, 5, 7, 9, 11, 13, 15, 17, 19, 21],
                        _ => [],
                    };
                }

                return [];
            }
            public static PulseTypeName[] GetAvailablePuseType(int Level)
            {
                return Level switch
                {
                    2 => [PulseTypeName.ASYNC, PulseTypeName.SYNC, PulseTypeName.SHE, PulseTypeName.CHM, PulseTypeName.HO],
                    3 => [PulseTypeName.ASYNC, PulseTypeName.SYNC, PulseTypeName.SHE, PulseTypeName.CHM],
                    _ => []
                };
            }
            public static bool IsPulseSquareAvail(YamlPulseMode PulseMode, int Level)
            {
                if (Level == 2 && PulseMode.PulseType == PulseTypeName.SYNC)
                {
                    if (PulseMode.PulseCount == 1) return false;
                    if (PulseMode.Alternative > PulseAlternative.Default) return false;
                    return true;
                }
                return false;
            }
            public static bool IsCompareWaveEditable(YamlPulseMode PulseMode, int Level)
            {
                if (Level == 2)
                {
                    if (IsPulseSquareAvail(PulseMode, Level) && PulseMode.Square) return false;
                    if (PulseMode.Alternative > PulseAlternative.Default) return false;
                    if (PulseMode.PulseType == PulseTypeName.SYNC)
                    {
                        if (PulseMode.PulseCount == 1) return false;
                        return true;
                    }
                    if (PulseMode.PulseType == PulseTypeName.ASYNC) return true;
                    return false;
                }

                if (Level == 3)
                {
                    if (PulseMode.Alternative > PulseAlternative.Default) return false;
                    if (PulseMode.PulseType == PulseTypeName.SYNC) return true;
                    if (PulseMode.PulseType == PulseTypeName.ASYNC) return true;
                    return false;
                }
                return false;
            }
            public static bool IsPulseShiftedAvailable(YamlPulseMode PulseMode, int Level)
            {
                if (Level == 2)
                {
                    if (PulseMode.Alternative > PulseAlternative.Default) return false;
                    if (PulseMode.PulseType == PulseTypeName.SYNC)
                    {
                        if (PulseMode.PulseCount == 1) return false;
                        return true;
                    }
                    if (PulseMode.PulseType == PulseTypeName.ASYNC) return true;
                    return false;
                }

                if (Level == 3)
                {
                    if (PulseMode.Alternative > PulseAlternative.Default) return false;
                    if (PulseMode.PulseType == PulseTypeName.SYNC) return true;
                    if (PulseMode.PulseType == PulseTypeName.ASYNC) return true;
                    return false;
                }
                return false;
            }
            public static PulseAlternative[] GetPulseAlternatives(YamlPulseMode PulseMode, int Level)
            {
                return GetPulseAlternatives(PulseMode.PulseType, PulseMode.PulseCount, Level);
            }
            public static PulseAlternative[] GetPulseAlternatives(PulseTypeName PulseType, int PulseCount, int Level)
            {
                static PulseAlternative[] AlternativesDefaultToX(int X)
                {
                    PulseAlternative[] Alternatives = new PulseAlternative[X + 1];
                    Alternatives[0] = PulseAlternative.Default;
                    for (int i = 0; i < X; i++)
                    {
                        Alternatives[i + 1] = (PulseAlternative)((int)PulseAlternative.Default + i + 1);
                    }
                    return Alternatives;
                }

                if (Level == 3)
                {
                    if(PulseType == PulseTypeName.SYNC)
                    {
                        return PulseCount switch
                        {
                            1 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            5 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            _ => [PulseAlternative.Default],
                        };
                    }

                    if (PulseType == PulseTypeName.CHM)
                    {
                        return PulseCount switch
                        {
                            3 => AlternativesDefaultToX(2),
                            5 => AlternativesDefaultToX(4),
                            7 => AlternativesDefaultToX(6),
                            9 => AlternativesDefaultToX(7),
                            11 => AlternativesDefaultToX(10),
                            13 => AlternativesDefaultToX(14),
                            15 => AlternativesDefaultToX(17),
                            17 => AlternativesDefaultToX(19),
                            19 => AlternativesDefaultToX(25),
                            21 => AlternativesDefaultToX(22),
                            _ => [PulseAlternative.Default],
                        };
                    }

                    if (PulseType == PulseTypeName.SHE)
                    {
                        return PulseCount switch
                        {
                            3 => AlternativesDefaultToX(1),
                            _ => [PulseAlternative.Default],
                        };
                    }

                    return [PulseAlternative.Default];
                }
                
                if (Level == 2)
                {
                    if (PulseType == PulseTypeName.SYNC)
                    {
                        return PulseCount switch
                        {
                            3 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            5 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            6 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            8 => AlternativesDefaultToX(1),
                            9 => AlternativesDefaultToX(1),
                            13 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            17 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            _ => [PulseAlternative.Default],
                        };
                    }

                    if (PulseType == PulseTypeName.CHM)
                    {
                        return PulseCount switch
                        {
                            3 => AlternativesDefaultToX(1),
                            5 => AlternativesDefaultToX(3),
                            7 => AlternativesDefaultToX(5),
                            9 => AlternativesDefaultToX(8),
                            11 => AlternativesDefaultToX(12),
                            13 => AlternativesDefaultToX(13),
                            15 => AlternativesDefaultToX(23),
                            17 => AlternativesDefaultToX(11),
                            19 => AlternativesDefaultToX(11),
                            21 => AlternativesDefaultToX(13),
                            23 => AlternativesDefaultToX(14),
                            25 => AlternativesDefaultToX(20),

                            _ => [PulseAlternative.Default],
                        };
                    }

                    if (PulseType == PulseTypeName.SHE)
                    {
                        return PulseCount switch
                        {
                            3 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            5 => [PulseAlternative.Default, PulseAlternative.Alt1, PulseAlternative.Alt2],
                            7 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            9 => [PulseAlternative.Default, PulseAlternative.Alt1, PulseAlternative.Alt2],
                            11 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            13 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            15 => [PulseAlternative.Default, PulseAlternative.Alt1],
                            _ => [PulseAlternative.Default],
                        };
                    }

                    if (PulseType == PulseTypeName.HO)
                    {
                        return PulseCount switch
                        {
                            5 => AlternativesDefaultToX(7),
                            7 => AlternativesDefaultToX(9),
                            9 => AlternativesDefaultToX(6),
                            11 => AlternativesDefaultToX(5),
                            13 => AlternativesDefaultToX(3),
                            15 => AlternativesDefaultToX(2),
                            _ => [PulseAlternative.Default],
                        };
                    }

                    return [PulseAlternative.Default];
                }

                return [PulseAlternative.Default];
            }
            public static PulseDataKey[] GetAvailablePulseDataKey(YamlPulseMode PulseMode, int Level)
            {

                if(Level == 2)
                {
                    return PulseMode.PulseType switch
                    {
                        PulseTypeName.SYNC => PulseMode.PulseCount switch
                        {
                            6 => PulseMode.Alternative switch { 
                                PulseAlternative.Alt1 => [PulseDataKey.PulseWidth],
                                _ => [],
                            },
                            8 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Alt1 => [PulseDataKey.PulseWidth],
                                _ => [],
                            },
                            _ => []
                        },
                        _ => []
                    };
                }

                if(Level == 3)
                {
                    return PulseMode.PulseType switch
                    {
                        PulseTypeName.SYNC => PulseMode.PulseCount switch
                        {
                            5 => PulseMode.Alternative switch
                            {
                                PulseAlternative.Alt1 => [PulseDataKey.PulseWidth],
                                _ => [],
                            },
                            _ => []
                        },
                        PulseTypeName.ASYNC => [PulseDataKey.Dipolar],
                        _ => []
                    };
                }

                return [];
            }
        }
    }
}
