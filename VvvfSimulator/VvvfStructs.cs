using System;
using System.Collections.Generic;
using VvvfSimulator.GUI.Create.Settings;
using static OpenCvSharp.Stitcher;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;

namespace VvvfSimulator
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
        private PulseMode VideoPulseMode { get; set; } = new();
        private double VideoSineAmplitude { get; set; }
        private CarrierFreq VideoCarrierFrequency { get; set; } = new CarrierFreq(0, 0, 0.0005);
        private double VideoDipolarValue { get; set; }
        private double VideoSineFrequency { get; set; }

        public void SetVideoPulseMode(PulseMode p) { VideoPulseMode = p; }
        public PulseMode GetVideoPulseMode() { return VideoPulseMode; }

        public void SetVideoSineAmplitude(double d) { VideoSineAmplitude = d; }
        public double GetVideoSineAmplitude() { return VideoSineAmplitude; }

        public void SetVideoCarrierFrequency(CarrierFreq c) { VideoCarrierFrequency = c; }
        public CarrierFreq GetVideoCarrierFrequency() { return VideoCarrierFrequency; }

        public void SetVideoDipolar(double d) { VideoDipolarValue = d; }
        public double GetVideoDipolar() { return VideoDipolarValue; }
        public void SetVideoSineFrequency(double d) { VideoSineFrequency = d; }
        public double GetVideoSineFrequency() { return VideoSineFrequency; }

        // Values for Check mascon
        private double GenerationCurrentTime { get; set; } = 0;
        public void SetGenerationCurrentTime(double d) { GenerationCurrentTime = d; }
        public double GetGenerationCurrentTime() { return GenerationCurrentTime; }
        public void AddGenerationCurrentTime(double d) { GenerationCurrentTime += d; }

    }

    public static class VvvfStructs
    {
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

        public class ControlStatus
        {
            public bool brake;
            public bool mascon_on;
            public bool free_run;
            public double wave_stat;
        }

        public class CarrierFreq(double base_freq_a, double range_b, double interval_c)
        {
            public CarrierFreq Clone()
            {
                return (CarrierFreq)MemberwiseClone();
            }

            public double base_freq = base_freq_a;
            public double range = range_b;
            public double interval = interval_c;
        }

        public class PwmCalculateValues
        {
            public PulseMode pulse_mode = new();
            public CarrierFreq carrier_freq = new(100, 0, 0.0005);

            public double dipolar;
            public int level;
            public bool none;

            public double amplitude;
            public double min_sine_freq;

            public PwmCalculateValues Clone()
            {
                var clone = (PwmCalculateValues)MemberwiseClone();
                clone.carrier_freq = carrier_freq.Clone();
                clone.pulse_mode = pulse_mode.Clone();

                return clone;
            }
        }

        //
        // Pulse Mode Struct
        //
        public class PulseMode
        {
            public PulseMode Clone()
            {
                var x = (PulseMode)MemberwiseClone();
                List<PulseHarmonic> clone_pulse_harmonics = [];
                for (int i = 0; i < PulseHarmonics.Count; i++)
                {
                    clone_pulse_harmonics.Add(PulseHarmonics[i].Clone());
                }
                x.PulseHarmonics = clone_pulse_harmonics;
                x.DiscreteTime = DiscreteTime.Clone();
                return x;

            }

            //
            // Flat Configurations
            //
            public bool Shift { get; set; } = false;
            public bool Square { get; set; } = false;

            //
            // Discrete Time Configuration
            //
            public DiscreteTimeConfiguration DiscreteTime { get; set; } = new();
            public class DiscreteTimeConfiguration
            {
                public bool Enabled { get; set; } = false;
                public int Steps { get; set; } = 2;
                public DiscreteTimeMode Mode { get; set; } = DiscreteTimeMode.Middle;

                public enum DiscreteTimeMode
                {
                    Left, Middle, Right
                }

                public DiscreteTimeConfiguration Clone()
                {
                    return (DiscreteTimeConfiguration)MemberwiseClone();
                }
            }

            //
            // Pulse Mode Name
            //
            public PulseModeName PulseName { get; set; }
            public enum PulseModeName
            {
                Async, P_Wide_3,

                P_1, P_2, P_3, P_4, P_5, P_6, P_7, P_8, P_9, P_10,
                P_11, P_12, P_13, P_14, P_15, P_16, P_17, P_18, P_19, P_20,
                P_21, P_22, P_23, P_24, P_25, P_26, P_27, P_28, P_29, P_30,
                P_31, P_32, P_33, P_34, P_35, P_36, P_37, P_38, P_39, P_40,
                P_41, P_42, P_43, P_44, P_45, P_46, P_47, P_48, P_49, P_50,
                P_51, P_52, P_53, P_54, P_55, P_56, P_57, P_58, P_59, P_60,
                P_61,

                // Current harmonic minimum Pulse width modulation
                CHMP_3, CHMP_Wide_3, CHMP_5, CHMP_Wide_5, CHMP_7, CHMP_Wide_7,
                CHMP_9, CHMP_Wide_9, CHMP_11, CHMP_Wide_11, CHMP_13,
                CHMP_Wide_13, CHMP_15, CHMP_Wide_15,

                // Selective harmonic elimination Pulse width modulation
                SHEP_3, SHEP_5, SHEP_7, SHEP_9, SHEP_11, SHEP_13, SHEP_15,

                HOP_5, HOP_7, HOP_9, HOP_11, HOP_13, HOP_15, HOP_17,
            };

            //
            // Compare Base Wave
            //
            public BaseWaveType BaseWave { get; set; } = BaseWaveType.Sine;
            public enum BaseWaveType
            {
                Sine, Saw, Modified_Sine_1, Modified_Sine_2, Modified_Saw_1
            }

            //
            // Compare Wave Harmonics
            //
            public List<PulseHarmonic> PulseHarmonics { get; set; } = new();
            public class PulseHarmonic
            {
                public double Harmonic { get; set; } = 3;
                public bool IsHarmonicProportional { get; set; } = true;
                public double Amplitude { get; set; } = 0.2;
                public bool IsAmplitudeProportional { get; set; } = true;
                public double InitialPhase { get; set; } = 0;
                public PulseHarmonicType Type { get; set; } = PulseHarmonicType.Sine;

                public enum PulseHarmonicType
                {
                    Sine, Saw, Square
                }

                public PulseHarmonic Clone()
                {
                    return (PulseHarmonic)MemberwiseClone();
                }
            }

            //
            // Alternative Modes
            //
            public PulseAlternativeMode AltMode { get; set; } = PulseAlternativeMode.Default;
            public enum PulseAlternativeMode
            {
                Default, Alt1, Alt2, Alt3, Alt4, Alt5, Alt6, Alt7, Alt8, Alt9, Alt10, Alt11,
            }
        }

        // "Pulse Mode" Configuration
        public class PulseModeConfiguration
        {
            public static PulseModeName[] ValidPulseModeNames(int level)
            {
                if (level == 2) return (PulseModeName[])Enum.GetValues(typeof(PulseModeName));
                if (level == 3) return [
                    PulseModeName.Async,
                    PulseModeName.P_1, PulseModeName.P_2, PulseModeName.P_3, PulseModeName.P_4, PulseModeName.P_5, PulseModeName.P_6, PulseModeName.P_7, PulseModeName.P_8, PulseModeName.P_9, PulseModeName.P_10,
                    PulseModeName.P_11, PulseModeName.P_12, PulseModeName.P_13, PulseModeName.P_14, PulseModeName.P_15, PulseModeName.P_16, PulseModeName.P_17, PulseModeName.P_18, PulseModeName.P_19, PulseModeName.P_20,
                    PulseModeName.P_21, PulseModeName.P_22, PulseModeName.P_23, PulseModeName.P_24, PulseModeName.P_25, PulseModeName.P_26, PulseModeName.P_27, PulseModeName.P_28, PulseModeName.P_29, PulseModeName.P_30,
                    PulseModeName.P_31, PulseModeName.P_32, PulseModeName.P_33, PulseModeName.P_34, PulseModeName.P_35, PulseModeName.P_36, PulseModeName.P_37, PulseModeName.P_38, PulseModeName.P_39, PulseModeName.P_40,
                    PulseModeName.P_41, PulseModeName.P_42, PulseModeName.P_43, PulseModeName.P_44, PulseModeName.P_45, PulseModeName.P_46, PulseModeName.P_47, PulseModeName.P_48, PulseModeName.P_49, PulseModeName.P_50,
                    PulseModeName.P_51, PulseModeName.P_52, PulseModeName.P_53, PulseModeName.P_54, PulseModeName.P_55, PulseModeName.P_56, PulseModeName.P_57, PulseModeName.P_58, PulseModeName.P_59, PulseModeName.P_60,
                    PulseModeName.P_61,
                    ];
                return [PulseModeName.P_1];
            }
            public static bool IsDiscreteTimeValid(PulseMode mode, int level)
            {
                PulseModeName pulseName = mode.PulseName;
                if (level == 3) return true;

                {
                    List<PulseModeName> deny = [
                        PulseModeName.P_Wide_3,
                        PulseModeName.CHMP_15, PulseModeName.CHMP_Wide_15,PulseModeName.CHMP_13,PulseModeName.CHMP_Wide_13,
                        PulseModeName.CHMP_11,PulseModeName.CHMP_Wide_11,PulseModeName.CHMP_9,PulseModeName.CHMP_Wide_9,
                        PulseModeName.CHMP_7,PulseModeName.CHMP_Wide_7,PulseModeName.CHMP_5,PulseModeName.CHMP_Wide_5,
                        PulseModeName.CHMP_3,PulseModeName.CHMP_Wide_3,PulseModeName.SHEP_3,
                        PulseModeName.SHEP_5,PulseModeName.SHEP_7,PulseModeName.SHEP_11,PulseModeName.SHEP_13,PulseModeName.SHEP_15,
                        PulseModeName.HOP_5,PulseModeName.HOP_7,PulseModeName.HOP_9,
                        PulseModeName.HOP_11,PulseModeName.HOP_13,PulseModeName.HOP_15,PulseModeName.HOP_17
                    ];
                    if (deny.Contains(pulseName)) return false;
                }
                if (mode.Square && IsPulseSquareAvail(mode, 2)) return false;
                if ((pulseName == PulseModeName.P_17 ||
                    pulseName == PulseModeName.P_13 ||
                    pulseName == PulseModeName.P_9 ||
                    pulseName == PulseModeName.P_5) && (mode.AltMode == PulseAlternativeMode.Alt1)) return false;

                return true;
            }
            private static int GetPulseNameNum(PulseModeName mode)
            {
                int[] pulse_list =
                [
                0, 0,

                1,2,3,4,5,6,7,8,9,10,
                11,12,13,14,15,16,17,18,19,20,
                21,22,23,24,25,26,27,28,29,30,
                31,32,33,34,35,36,37,38,39,40,
                41,42,43,44,45,46,47,48,49,50,
                51,52,53,54,55,56,57,58,59,60,
                61,

                // Current harmonic minimum Pulse width modulation
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,

                // Selective harmonic elimination Pulse width modulation
                0, 0, 0, 0, 0, 0, 0
                ];
                return pulse_list[(int)mode];
            }

            public static bool IsPulseHarmonicBaseWaveChangeAvailable(PulseMode mode, int level)
            {
                if (level == 3) return true;

                if (IsPulseSquareAvail(mode, level) && mode.Square) return false;

                bool[] pulse_list =
                [
                true, false,

                false,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,true,true,true,true,true,true,true,true,true,
                true,

                // Current harmonic minimum Pulse width modulation
                false,false,false,false,false,false,
                false,false,false,false,false,false,
                false,false,

                // Selective harmonic elimination Pulse width modulation
                false,false,false,false,false,false,false,

                // HOP
                false,false,false,false,false,false,false,
                ];
                return pulse_list[(int)mode.PulseName];
            }

            public static bool IsPulseShiftedAvailable(PulseMode mode, int level)
            {
                if (level == 3) return true;

                int id = (int)mode.PulseName;
                bool stat_1 = (id > (int)PulseModeName.P_1 && id <= (int)PulseModeName.P_61);

                return stat_1;
            }

            public static bool IsPulseSquareAvail(PulseMode mode, int level)
            {
                if (level == 3) return false;

                int id = (int)mode.PulseName;
                bool stat_1 = (id > (int)PulseModeName.P_1 && id <= (int)PulseModeName.P_61);

                return stat_1;
            }

            public static int GetPulseNum(PulseMode mode, int level)
            {
                int pulses = GetPulseNameNum(mode.PulseName);
                if (level == 3) return pulses;

                if (mode.Square)
                {
                    if (pulses % 2 == 0) pulses = (int)(pulses * 1.5);
                    else pulses = (int)((pulses - 1) * 1.5);
                }

                return pulses;
            }
            public static double GetPulseInitial(PulseMode mode, int level)
            {
                if (level == 3) return 0;

                if (mode.Square)
                {
                    if (GetPulseNameNum(mode.PulseName) % 2 == 0) return MyMath.M_PI_2;
                    else return 0;
                }

                return 0;
            }

            public static PulseAlternativeMode[] GetPulseAltModes(PulseMode pulse_Mode, int level)
            {
                if (level == 3) // level 3
                {
                    if (pulse_Mode.PulseName == PulseModeName.P_1)
                        return [PulseAlternativeMode.Default, PulseAlternativeMode.Alt1];
                }
                else // level 2
                {
                    if (pulse_Mode.PulseName == PulseModeName.CHMP_11)
                        return [PulseAlternativeMode.Default, PulseAlternativeMode.Alt1];
                    if (pulse_Mode.PulseName == PulseModeName.CHMP_13)
                        return [PulseAlternativeMode.Default, PulseAlternativeMode.Alt1];
                    if (pulse_Mode.PulseName == PulseModeName.CHMP_15)
                        return [PulseAlternativeMode.Default, PulseAlternativeMode.Alt1];
                    if (pulse_Mode.PulseName == PulseModeName.P_17)
                        return [PulseAlternativeMode.Default, PulseAlternativeMode.Alt1];
                    if (pulse_Mode.PulseName == PulseModeName.P_13)
                        return [PulseAlternativeMode.Default, PulseAlternativeMode.Alt1];
                    if (pulse_Mode.PulseName == PulseModeName.P_9)
                        return [PulseAlternativeMode.Default, PulseAlternativeMode.Alt1];
                    if (pulse_Mode.PulseName == PulseModeName.P_5)
                        return [PulseAlternativeMode.Default, PulseAlternativeMode.Alt1];
                    if (pulse_Mode.PulseName == PulseModeName.HOP_5)
                        return [
                            PulseAlternativeMode.Default, PulseAlternativeMode.Alt1, PulseAlternativeMode.Alt2,PulseAlternativeMode.Alt3,
                            PulseAlternativeMode.Alt4, PulseAlternativeMode.Alt5,PulseAlternativeMode.Alt6, PulseAlternativeMode.Alt7
                        ];
                    if (pulse_Mode.PulseName == PulseModeName.HOP_7)
                        return [
                            PulseAlternativeMode.Default, PulseAlternativeMode.Alt1, PulseAlternativeMode.Alt2, PulseAlternativeMode.Alt3,
                            PulseAlternativeMode.Alt4, PulseAlternativeMode.Alt5, PulseAlternativeMode.Alt6, PulseAlternativeMode.Alt7,
                            PulseAlternativeMode.Alt8, PulseAlternativeMode.Alt9,
                        ];
                    if (pulse_Mode.PulseName == PulseModeName.HOP_9)
                        return [
                            PulseAlternativeMode.Default, PulseAlternativeMode.Alt1, PulseAlternativeMode.Alt2, PulseAlternativeMode.Alt3,
                            PulseAlternativeMode.Alt4, PulseAlternativeMode.Alt5,PulseAlternativeMode.Alt6
                        ];
                    if (pulse_Mode.PulseName == PulseModeName.HOP_11)
                        return [
                            PulseAlternativeMode.Default, PulseAlternativeMode.Alt1, PulseAlternativeMode.Alt2,PulseAlternativeMode.Alt3,
                            PulseAlternativeMode.Alt4, PulseAlternativeMode.Alt5
                        ];
                    if (pulse_Mode.PulseName == PulseModeName.HOP_13)
                        return [
                            PulseAlternativeMode.Default, PulseAlternativeMode.Alt1, PulseAlternativeMode.Alt2,PulseAlternativeMode.Alt3
                        ];
                    if (pulse_Mode.PulseName == PulseModeName.HOP_15)
                        return [
                            PulseAlternativeMode.Default, PulseAlternativeMode.Alt1, PulseAlternativeMode.Alt2
                        ];
                }

                return [PulseAlternativeMode.Default];
            }
        }
    }
}
