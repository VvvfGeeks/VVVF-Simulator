using System.IO.Ports;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.Generation.Audio
{
    public class RealTime
    {

        // ---------- COMMON ---------------
        public class Parameter(Data.Vvvf.Struct VvvfSound, Data.TrainAudio.Struct TrainSound)
        {
            public double FrequencyChangeRate { get; set; } = 0;
            public bool IsBraking { get; set; } = false;
            public bool Quit { get; set; } = false;
            public bool IsFreeRunning { get; set; } = false;
            public Domain Control { get; } = new(TrainSound.MotorSpec);
            public Data.Vvvf.Struct VvvfSoundData { get; } = VvvfSound;
            public Data.TrainAudio.Struct TrainSoundData { get; } = TrainSound;
            public string AudioDeviceId { get; set; } = new NAudio.CoreAudioApi.MMDeviceEnumerator().GetDefaultAudioEndpoint(NAudio.CoreAudioApi.DataFlow.Render, NAudio.CoreAudioApi.Role.Multimedia).ID;
        }
        public class VvvfSoundParameter(Data.Vvvf.Struct VvvfSound, Data.TrainAudio.Struct TrainSound) : Parameter(VvvfSound, TrainSound)
        {
            public Mode OutputMode { get; set; } = Mode.Line;
            public SerialPort? Port { get; set; }
            public enum Mode
            {
                Line, Phase, PhaseCurrent
            }
        }
        public class TrainSoundParameter(Data.Vvvf.Struct VvvfSound, Data.TrainAudio.Struct TrainSound) : Parameter(VvvfSound, TrainSound)
        {
        }

        public static int RealTimeFrequencyControl(Domain Control, Parameter Param, double dt)
        {
            Control.SetBraking(Param.IsBraking);
            Control.SetPowerOff(Param.IsFreeRunning);

            Vvvf.Modulation.BaseWave baseWave = Control.GetBaseWaveInstance();

            double angleFrequency = baseWave.AngleFrequency;
            angleFrequency += Param.FrequencyChangeRate * dt;
            if (angleFrequency < 0) angleFrequency = 0;

            if (!Control.IsFreeRun())
            {
                if (Control.IsBaseWaveTimeChangeAllowed())
                {
                    if (angleFrequency != 0)
                    {
                        double amp = baseWave.AngleFrequency / angleFrequency;
                        baseWave.Time *= amp;
                    }
                }

                baseWave.AngleFrequency = angleFrequency;
                Control.SetControlFrequency(baseWave.Frequency);
            }


            if (Param.Quit) return 0;

            Control.ProcessControlParameter(dt, Param.VvvfSoundData.JerkSetting);

            return -1;
        }
    }

}
