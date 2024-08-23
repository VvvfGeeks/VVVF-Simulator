using System;
using VvvfSimulator.Vvvf;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.Generation
{
    public class GenerateBasic
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
            double _F = Control.GetSineFrequency();
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
            double _F = Control.GetSineFrequency();
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
}
