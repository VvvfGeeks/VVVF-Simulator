namespace VvvfSimulator.Data.Vvvf
{
    public class Util
    {
        public static bool SetFreeRunModulationIndexToZero(Struct data)
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
        public static bool SetFreeRunEndAmplitudeContinuous(Struct data)
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
