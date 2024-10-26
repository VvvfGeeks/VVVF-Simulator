using System;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.Generation
{
    public class GenerateBasic
    {
        public static class WaveForm
        {
            /// <summary>
            ///  Calculates one cycle of UVW.
            /// </summary>
            /// <param name="Control"></param>
            /// <param name="Sound"></param>
            /// <param name="Division"> Recommend : 120000 , Brief : 2000 </param>
            /// <param name="Precise"> True for more precise calculation when Freq < 1 </param>
            /// <returns> One cycle of UVW </returns>
            public static WaveValues[] GetUVWCycle(VvvfValues Control, YamlVvvfSoundData Sound, double InitialPhase, int Division, bool Precise)
            {
                PwmCalculateValues PreCalculate = YamlVvvfWave.CalculateYaml(Control, Sound);
                double _F = Control.GetSineFrequency();
                if (Control.GetSineFrequency() < PreCalculate.MinimumFrequency && Control.GetControlFrequency() > 0) _F = PreCalculate.MinimumFrequency;
                double _K = (_F > 0.01 && _F < 1) ? 1 / _F : 1;
                int Count = Precise ? (int)Math.Round(Division * _K) : Division;
                double InvDeltaT = Count * _F;

                Control.SetGenerationCurrentTime(0);
                Control.SetSineTime(0);
                Control.SetSawTime(0);

                return GetUVW(Control, Sound, InitialPhase, InvDeltaT, Count);
            }

            /// <summary>
            /// Calculates WaveForm of UVW in 1 sec.
            /// </summary>
            /// <param name="Control"></param>
            /// <param name="Sound"></param>
            /// <param name="InitialPhase"></param>
            /// <param name="Division"> Recommend : 120000 , Brief : 2000 </param>
            /// <param name="Precise"> True for more precise calculation when Freq < 1</param>
            /// <returns> WaveForm of UVW in 1 sec.</returns>
            public static WaveValues[] GetUVWSec(VvvfValues Control, YamlVvvfSoundData Sound, double InitialPhase, int Division, bool Precise)
            {
                PwmCalculateValues PreCalculate = YamlVvvfWave.CalculateYaml(Control, Sound);
                double _F = Control.GetSineFrequency();
                if (Control.GetSineFrequency() < PreCalculate.MinimumFrequency && Control.GetControlFrequency() > 0) _F = PreCalculate.MinimumFrequency;
                double _K = (_F > 0.01 && _F < 1) ? 1 / _F : 1;
                int Count = Precise ? (int)Math.Round(Division * _K) : Division;
                double InvDeltaT = Count;

                Control.SetGenerationCurrentTime(0);
                Control.SetSineTime(0);
                Control.SetSawTime(0);

                return GetUVW(Control, Sound, InitialPhase, InvDeltaT, Count);
            }

            public static WaveValues[] GetUVW(VvvfValues Control, YamlVvvfSoundData Sound, double InitialPhase, double InvDeltaT, int Count)
            {
                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(Control, Sound);
                WaveValues[] PWM_Array = new WaveValues[Count + 1];
                for (int i = 0; i <= Count; i++)
                {
                    Control.SetGenerationCurrentTime(i / InvDeltaT);
                    Control.SetSineTime(i / InvDeltaT);
                    Control.SetSawTime(i / InvDeltaT);
                    WaveValues value = Calculate.CalculatePhases(Control, calculated_Values, InitialPhase);
                    PWM_Array[i] = value;
                }
                return PWM_Array;
            }
        }
        public static class Fourier
        {
            public const double VoltageConvertFactor = 1.102657791;
            public static double GetFourier(ref WaveValues[] UVW, int N)
            {
                double integral = 0;
                double dt = 1.0 / (UVW.Length - 1);

                for (int i = 0; i < UVW.Length; i++)
                {
                    double iTime = MyMath.M_2PI * i / (UVW.Length - 1);
                    double sum = (UVW[i].U - UVW[i].V) * Math.Sin(N * iTime) * dt;
                    integral += sum;
                }
                double bn = integral;
                return bn;
            }

            public static double GetFourierFast(ref WaveValues[] UVW, int N)
            {
                double integral = 0;

                int Ft = 0;
                double Time = 0;

                for (int i = 0; i < UVW.Length; i++)
                {
                    int iFt = UVW[i].U - UVW[i].V;

                    if (i == 0)
                    {
                        Ft = iFt;
                        continue;
                    }

                    if (Ft == iFt) continue;
                    double iTime = MyMath.M_2PI * i / (UVW.Length - 1);
                    double sum = (-Math.Cos(N * iTime) + Math.Cos(N * Time)) * Ft / N;
                    integral += sum;

                    Time = iTime;
                    Ft = iFt;
                }
                double bn = integral / MyMath.M_2PI;
                return bn;
            }

            public static double[] GetFourierCoefficients(ref WaveValues[] UVW, int N)
            {
                double[] coefficients = new double[N];
                for (int n = 1; n <= N; n++)
                {
                    double result = GetFourierFast(ref UVW, n);
                    coefficients[n - 1] = result;
                }
                return coefficients;
            }

            /// <summary>
            /// Gets Fourier series coefficients
            /// </summary>
            /// <param name="Control">Make sure you put cloned data.</param>
            /// <param name="Sound"></param>
            /// <param name="Delta"></param>
            /// <param name="N"></param>
            /// <returns></returns>
            public static double[] GetFourierCoefficients(VvvfValues Control, YamlVvvfSoundData Sound, int Delta, int N)
            {
                Control.SetRandomFrequencyMoveAllowed(false);
                WaveValues[] PWM_Array = WaveForm.GetUVWCycle(Control, Sound, MyMath.M_PI_6, Delta, false);
                return GetFourierCoefficients(ref PWM_Array, N);
            }

            public static string GetDesmosFourierCoefficientsArray(ref double[] coefficients)
            {
                String array = "C = [";
                for (int i = 0; i < coefficients.Length; i++)
                {
                    array += (i == 0 ? "" : " ,") + coefficients[i];
                }
                array += "]";
                return array;
            }

            /// <summary>
            /// Do clone about control!
            /// </summary>
            /// <param name="Sound"></param>
            /// <param name="Control"></param>
            /// <returns></returns>
            public static double GetVoltageRate(VvvfValues Control, YamlVvvfSoundData Sound, bool Precise, bool FixSign = true)
            {
                WaveValues[] PWM_Array = WaveForm.GetUVWCycle(Control, Sound, MyMath.M_PI_6, 120000, Precise);
                return GetVoltageRate(ref PWM_Array, FixSign);
            }
            public static double GetVoltageRate(ref WaveValues[] UVW, bool FixSign = true)
            {
                double result = GetFourierFast(ref UVW, 1) / VoltageConvertFactor;
                if (FixSign) result = Math.Abs(result);
                return result;
            }
        }
    }
}
