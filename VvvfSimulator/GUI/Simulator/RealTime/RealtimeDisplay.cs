using System;
using System.Drawing;
using System.Threading.Tasks;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.VvvfStructs;
using VvvfSimulator.GUI.Resource.Language;

namespace VvvfSimulator.GUI.Simulator.RealTime
{
    public class RealtimeDisplay
    {
        public class ControlStatus : BitmapViewerManager
        {
            private readonly RealTimeControlStatStyle _Style;
            private readonly RealTimeParameter _Paremter;
            private readonly bool _ControlPrecise;
            
            public ControlStatus(RealTimeParameter Parameter, RealTimeControlStatStyle Style, bool ControlPrecise)
            {
                _Style = Style;
                _Paremter = Parameter;
                _ControlPrecise = ControlPrecise;
            }
            public void RunTask()
            {
                Task.Run(() => {
                    while (!_Paremter.Quit)
                    {
                        UpdateControl();

                    }
                    Close();
                });
            }           
            private void UpdateControl()
            {
                Bitmap image;

                if (_Style == RealTimeControlStatStyle.Original1)
                {
                    VvvfValues control = _Paremter.Control.Clone();
                    image = Generation.Video.ControlInfo.GenerateControlOriginal.GetImage(
                        control,
                        _Paremter.Control.GetSineFrequency() == 0
                    );
                }
                else
                {
                    VvvfValues control = _Paremter.Control.Clone();
                    image = Generation.Video.ControlInfo.GenerateControlOriginal2.GetImage(
                        control,
                        _Paremter.VvvfSoundData,
                        _ControlPrecise
                    );
                }

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.ControlStatus.Title") + " (" + FriendlyNameConverter.GetRealTimeControlStatStyleName(_Style) + ")");
                image.Dispose();
            }

            public enum RealTimeControlStatStyle
            {
                Original1, Original2
            }
        }

        public class Fft : BitmapViewerManager
        {
            readonly RealTimeParameter _Parameter;
            public Fft(RealTimeParameter Parameter)
            {
                _Parameter = Parameter;
            }

            public void RunTask()
            {
                Task.Run(() => {
                    while (!_Parameter.Quit)
                    {
                        UpdateControl();
                    }
                    Close();
                });
            }
            private void UpdateControl()
            {
                VvvfValues control = _Parameter.Control.Clone();
                YamlVvvfSoundData ysd = _Parameter.VvvfSoundData;

                control.SetSineTime(0);
                control.SetSawTime(0);

                Bitmap image = Generation.Video.FFT.GenerateFFT.GetImage(control, ysd);

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.FFT.Title"));
                image.Dispose();
            }
        }

        public class Hexagon : BitmapViewerManager
        {
            private RealTimeHexagonStyle _Style;
            private RealTimeParameter _Parameter;
            private bool _ZeroVectorCircle;
            public Hexagon(RealTimeParameter Parameter, RealTimeHexagonStyle Style, bool ZeroVectorCircle)
            {
                _Parameter = Parameter;
                _Style = Style;
                _ZeroVectorCircle = ZeroVectorCircle;
            }

            public void RunTask()
            {
                Task.Run(() => {
                    while (!_Parameter.Quit)
                    {
                        UpdateControl();
                    }
                    Close();
                });
            }

            private void UpdateControl()
            {
                Bitmap image = new(100, 100);

                VvvfValues control = _Parameter.Control.Clone();
                YamlVvvfSoundData ysd = _Parameter.VvvfSoundData;

                control.SetSineTime(0);
                control.SetSawTime(0);

                if (_Style == RealTimeHexagonStyle.Original)
                {
                    int image_width = 1000;
                    int image_height = 1000;
                    int hex_div = 60000;
                    control.SetRandomFrequencyMoveAllowed(false);
                    image = Generation.Video.Hexagon.GenerateHexagonOriginal.GetImage(
                        control,
                        ysd,
                        image_width,
                        image_height,
                        hex_div,
                        2,
                        _ZeroVectorCircle,
                        false
                    );
                }

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.Hexagon.Title") + " (" + FriendlyNameConverter.GetRealTimeHexagonStyleName(_Style) + ")");
                image.Dispose();
            }

            public enum RealTimeHexagonStyle
            {
                Original
            }
        }
    
        public class WaveForm : BitmapViewerManager
        {
            private RealTimeParameter _Parameter;
            public WaveForm(RealTimeParameter Parameter)
            {
                _Parameter = Parameter;
            }

            public void RunTask()
            {
                Task.Run(() => {
                    while (!_Parameter.Quit)
                    {
                        UpdateControl();
                        System.Threading.Thread.Sleep(16);
                    }
                    Close();
                });
            }

            private void UpdateControl()
            {
                YamlVvvfSoundData Sound = _Parameter.VvvfSoundData;
                VvvfValues Control = _Parameter.Control.Clone();

                Control.SetSawTime(0);
                Control.SetSineTime(0);

                Control.SetRandomFrequencyMoveAllowed(false);

                int image_width = 1200;
                int image_height = 450;
                int calculate_div = 3;
                int wave_height = 100;

                VvvfStructs.ControlStatus cv = new()
                {
                    brake = Control.IsBraking(),
                    mascon_on = !Control.IsMasconOff(),
                    free_run = Control.IsFreeRun(),
                    wave_stat = Control.GetControlFrequency()
                };
                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(Control, cv, Sound);
                Bitmap image = Generation.Video.WaveForm.GenerateWaveFormUV.GetImage(Control, calculated_Values, image_width, image_height, wave_height, 2, calculate_div, 0);

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.WaveForm.Title"));
                image.Dispose();
            }
        }
    }
}
