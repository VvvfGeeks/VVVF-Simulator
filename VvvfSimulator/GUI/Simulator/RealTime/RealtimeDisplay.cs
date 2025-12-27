using System.Drawing;
using System.Threading.Tasks;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Data.Vvvf;
using static VvvfSimulator.Generation.Audio.RealTime;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.GUI.Simulator.RealTime
{
    public class RealtimeDisplay
    {
        public class ControlStatus(Parameter Parameter, ControlStatus.RealTimeControlStatStyle Style, bool ControlPrecise) : BitmapViewerManager, IRealtimeDisplay
        {
            public void Start()
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
                    image = Generation.Video.ControlInfo.Design1.GetImage(
                        Parameter.Control.Clone(),
                        Parameter.Control.GetBaseWaveFrequency() == 0
                    );
                }
                else
                {
                    image = Generation.Video.ControlInfo.Design2.GetImage(
                        Parameter.Control.Clone(),
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

        public class Fft (Parameter Parameter) : BitmapViewerManager, IRealtimeDisplay
        {
            public void Start()
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
                Bitmap image = Generation.Video.FFT.GenerateFFT.GetImage(Parameter.Control.Clone());
                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.FFT.Title"));
                image.Dispose();
            }
        }

        public class Hexagon(Parameter Parameter, Hexagon.RealTimeHexagonStyle Style, bool ZeroVectorCircle) : BitmapViewerManager, IRealtimeDisplay
        {
            public void Start()
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

                if (Style == RealTimeHexagonStyle.Original)
                {
                    int image_width = 1000;
                    int image_height = 1000;
                    int hex_div = 65536;

                    Domain Domain = Parameter.Control.Clone();
                    Domain.GetCarrierInstance().UseSimpleFrequency = true;
                    image = Generation.Video.Hexagon.Design1.GetImage(
                        Domain,
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
    
        public class WaveFormLine(Parameter Parameter) : BitmapViewerManager, IRealtimeDisplay
        {
            public void Start()
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
                Domain Control = Parameter.Control.Clone();
                Control.GetCarrierInstance().UseSimpleFrequency = true;

                int image_width = 1200;
                int image_height = 450;
                int calculate_div = 3;
                int wave_height = 100;

                Bitmap image = Generation.Video.WaveForm.GenerateWaveFormUV.GetImage(Control, image_width, image_height, wave_height, 2, calculate_div, 0);

                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.WaveForm.Title"));
                image.Dispose();
            }
        }

        public class WaveFormPhase(Parameter Parameter) : BitmapViewerManager, IRealtimeDisplay
        {
            public void Start()
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
                Domain Control = Parameter.Control.Clone();
                Control.GetCarrierInstance().UseSimpleFrequency = true;
                Bitmap image = Generation.Video.WaveForm.GenerateWaveFormUVW.GetImage(Control);
                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.WaveForm.Title"));
                image.Dispose();
            }
        }
    }
}
