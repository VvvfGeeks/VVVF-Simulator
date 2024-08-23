using System.Drawing;
using System.Threading.Tasks;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Yaml.VvvfSound;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.Vvvf.Struct;

namespace VvvfSimulator.GUI.Simulator.RealTime
{
    public class RealtimeDisplay
    {
        public class ControlStatus(RealTimeParameter Parameter, ControlStatus.RealTimeControlStatStyle Style, bool ControlPrecise) : BitmapViewerManager
        {
            public void RunTask()
            {
                Task.Run(() => {
                    while (!Parameter.Quit)
                    {
                        UpdateControl();

                    }
                    Close();
                });
            }           
            private void UpdateControl()
            {
                Bitmap image;

                if (Style == RealTimeControlStatStyle.Original1)
                {
                    VvvfValues control = Parameter.Control.Clone();
                    image = Generation.Video.ControlInfo.GenerateControlOriginal.GetImage(
                        control,
                        Parameter.Control.GetSineFrequency() == 0
                    );
                }
                else
                {
                    VvvfValues control = Parameter.Control.Clone();
                    image = Generation.Video.ControlInfo.GenerateControlOriginal2.GetImage(
                        control,
                        Parameter.VvvfSoundData,
                        ControlPrecise
                    );
                }

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.ControlStatus.Title") + " (" + FriendlyNameConverter.GetRealTimeControlStatStyleName(Style) + ")");
                image.Dispose();
            }

            public enum RealTimeControlStatStyle
            {
                Original1, Original2
            }
        }

        public class Fft (RealTimeParameter Parameter) : BitmapViewerManager 
        {
            public void RunTask()
            {
                Task.Run(() => {
                    while (!Parameter.Quit)
                    {
                        UpdateControl();
                    }
                    Close();
                });
            }
            private void UpdateControl()
            {
                VvvfValues control = Parameter.Control.Clone();
                YamlVvvfSoundData ysd = Parameter.VvvfSoundData;

                control.SetSineTime(0);
                control.SetSawTime(0);

                Bitmap image = Generation.Video.FFT.GenerateFFT.GetImage(control, ysd);

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.FFT.Title"));
                image.Dispose();
            }
        }

        public class Hexagon(RealTimeParameter Parameter, Hexagon.RealTimeHexagonStyle Style, bool ZeroVectorCircle) : BitmapViewerManager
        {
            public void RunTask()
            {
                Task.Run(() => {
                    while (!Parameter.Quit)
                    {
                        UpdateControl();
                    }
                    Close();
                });
            }

            private void UpdateControl()
            {
                Bitmap image = new(100, 100);

                VvvfValues control = Parameter.Control.Clone();
                YamlVvvfSoundData ysd = Parameter.VvvfSoundData;

                control.SetSineTime(0);
                control.SetSawTime(0);

                if (Style == RealTimeHexagonStyle.Original)
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
                        ZeroVectorCircle,
                        false
                    );
                }

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.Hexagon.Title") + " (" + FriendlyNameConverter.GetRealTimeHexagonStyleName(Style) + ")");
                image.Dispose();
            }

            public enum RealTimeHexagonStyle
            {
                Original
            }
        }
    
        public class WaveFormLine(RealTimeParameter Parameter) : BitmapViewerManager
        {
            public void RunTask()
            {
                Task.Run(() => {
                    while (!Parameter.Quit)
                    {
                        UpdateControl();
                        System.Threading.Thread.Sleep(16);
                    }
                    Close();
                });
            }

            private void UpdateControl()
            {
                YamlVvvfSoundData Sound = Parameter.VvvfSoundData;
                VvvfValues Control = Parameter.Control.Clone();

                Control.SetSawTime(0);
                Control.SetSineTime(0);

                Control.SetRandomFrequencyMoveAllowed(false);

                int image_width = 1200;
                int image_height = 450;
                int calculate_div = 3;
                int wave_height = 100;

                PwmCalculateValues calculated_Values = YamlVvvfWave.CalculateYaml(Control, Sound);
                Bitmap image = Generation.Video.WaveForm.GenerateWaveFormUV.GetImage(Control, calculated_Values, image_width, image_height, wave_height, 2, calculate_div, 0);

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.WaveForm.Title"));
                image.Dispose();
            }
        }

        public class WaveFormPhase(RealTimeParameter Parameter) : BitmapViewerManager
        {
            public void RunTask()
            {
                Task.Run(() => {
                    while (!Parameter.Quit)
                    {
                        UpdateControl();
                        System.Threading.Thread.Sleep(16);
                    }
                    Close();
                });
            }

            private void UpdateControl()
            {
                YamlVvvfSoundData Sound = Parameter.VvvfSoundData;
                VvvfValues Control = Parameter.Control.Clone();

                Control.SetSawTime(0);
                Control.SetSineTime(0);

                Control.SetRandomFrequencyMoveAllowed(false);

                Bitmap image = Generation.Video.WaveForm.GenerateWaveFormUVW.GetImage(Control, Sound);

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.WaveForm.Title"));
                image.Dispose();
            }
        }
    }
}
