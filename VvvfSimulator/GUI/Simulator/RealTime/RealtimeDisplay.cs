using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.Generation;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Util;
using static VvvfSimulator.Generation.Audio.RealTime;
using static VvvfSimulator.Vvvf.Model.Struct;

namespace VvvfSimulator.GUI.Simulator.RealTime
{
    public class RealtimeDisplay
    {
        public class WaveFormLine : BitmapViewerManager, IRealtimeDisplay
        {
            private readonly Parameter Parameter;
            private int ImageWidth = 1200;
            private int ImageHeight = 450;
            private int WaveHeight = 100;
            private int Thikness = 2;
            private int Spacing = 0;
            private bool Baseline = false;
            private int Division = 3;
            public WaveFormLine(Parameter Parameter) : base(true)
            {
                this.Parameter = Parameter;
                Viewer.SettingMenuClicked += (_, _) =>
                {

                    List<DialogInputWindow.InputContext> Inputs =
                            [
                                new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Line.Width"), DialogInputWindow.InputContextMode.TextBox, ImageWidth, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Line.Height"), DialogInputWindow.InputContextMode.TextBox, ImageHeight, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Line.WaveHeight"), DialogInputWindow.InputContextMode.TextBox, WaveHeight, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Line.Thikness"), DialogInputWindow.InputContextMode.TextBox, Thikness, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Line.Margin"), DialogInputWindow.InputContextMode.TextBox, Spacing, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Line.Baseline"), DialogInputWindow.InputContextMode.CheckBox, Baseline, typeof(bool)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Line.Division"), DialogInputWindow.InputContextMode.TextBox, Division, typeof(int)),
                        ];
                    DialogInputWindow InputDialog = new(
                        Viewer,
                        LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Line.Title"),
                        Inputs
                    );

                    InputDialog.ShowDialog();
                    if (InputDialog.Contexts == null) return;

                    ImageWidth = InputDialog.GetValue<int>(0);
                    ImageHeight = InputDialog.GetValue<int>(1);
                    WaveHeight = InputDialog.GetValue<int>(2);
                    Thikness = InputDialog.GetValue<int>(3);
                    Spacing = InputDialog.GetValue<int>(4);
                    Baseline = InputDialog.GetValue<bool>(5);
                    Division = InputDialog.GetValue<int>(6);
                };
            }
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
                Bitmap image = Generation.Video.WaveForm.GenerateWaveFormUV.GetImage(Control, ImageWidth, ImageHeight, WaveHeight, Thikness, Spacing, Baseline, Division);
                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.WaveForm.Title"));
                image.Dispose();
            }
        }
        public class WaveFormPhase : BitmapViewerManager, IRealtimeDisplay
        {
            private readonly Parameter Parameter;
            private int ImageWidth = 1500;
            private int ImageHeight = 1000;
            private int WaveHeight = 100;
            private int Thikness = 1;
            private int Division = 10;
            public WaveFormPhase(Parameter Parameter) : base(true)
            {
                this.Parameter = Parameter;
                Viewer.SettingMenuClicked += (_, _) =>
                {
                    List<DialogInputWindow.InputContext> Inputs =
                        [
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Phase.Width"), DialogInputWindow.InputContextMode.TextBox, ImageWidth, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Phase.Height"), DialogInputWindow.InputContextMode.TextBox, ImageHeight, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Phase.WaveHeight"), DialogInputWindow.InputContextMode.TextBox, WaveHeight, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Phase.Thikness"), DialogInputWindow.InputContextMode.TextBox, Thikness, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Phase.Division"), DialogInputWindow.InputContextMode.TextBox, Division, typeof(int)),
                        ];
                    DialogInputWindow InputDialog = new(
                        Viewer,
                        LanguageManager.GetString("Simulator.Generation.Dialog.Waveform.Phase.Title"),
                        Inputs
                    );
                    InputDialog.ShowDialog();
                    if (InputDialog.Contexts == null) return;

                    ImageWidth = InputDialog.GetValue<int>(0);
                    ImageHeight = InputDialog.GetValue<int>(1);
                    WaveHeight = InputDialog.GetValue<int>(2);
                    Thikness = InputDialog.GetValue<int>(3);
                    Division = InputDialog.GetValue<int>(4);
                };
            }
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
                Bitmap image = Generation.Video.WaveForm.GenerateWaveFormUVW.GetImage(Control, ImageWidth, ImageHeight, WaveHeight, Thikness, Division);
                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.WaveForm.Title"));
                image.Dispose();
            }
        }
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
                        Parameter.Control.GetBaseWaveInstance().IsZeroFrequency()
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
        public class Hexagon : BitmapViewerManager, IRealtimeDisplay
        {
            private readonly Parameter Parameter;
            private int ImageSize = 1000;
            private double DrawSize = 0.9;
            private int Thikness = 2;
            private double CircleSize = -1;
            private int Division = 65536;
            private bool Precise = false;
            public Hexagon(Parameter Parameter) : base(true)
            {
                this.Parameter = Parameter;
                Viewer.SettingMenuClicked += (_, _) =>
                {
                    List<DialogInputWindow.InputContext> Inputs =
                        [
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Hexagon.Design1.ImageSize"), DialogInputWindow.InputContextMode.TextBox, ImageSize, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Hexagon.Design1.DrawSize"), DialogInputWindow.InputContextMode.TextBox, DrawSize, typeof(double)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Hexagon.Design1.Thikness"), DialogInputWindow.InputContextMode.TextBox, Thikness, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Hexagon.Design1.ZeroVectorSize"), DialogInputWindow.InputContextMode.TextBox, CircleSize, typeof(double)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Hexagon.Design1.Division"), DialogInputWindow.InputContextMode.TextBox, Division, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.Hexagon.Design1.Precise"), DialogInputWindow.InputContextMode.CheckBox, Precise, typeof(bool)),
                        ];
                    DialogInputWindow InputDialog = new(
                        Viewer,
                        LanguageManager.GetString("Simulator.Generation.Dialog.Hexagon.Design1.Title"),
                        Inputs
                    );

                    InputDialog.ShowDialog();
                    if (InputDialog.Contexts == null) return;

                    ImageSize = InputDialog.GetValue<int>(0);
                    DrawSize = InputDialog.GetValue<double>(1);
                    Thikness = InputDialog.GetValue<int>(2);
                    CircleSize = InputDialog.GetValue<double>(3);
                    Division = InputDialog.GetValue<int>(4);
                    Precise = InputDialog.GetValue<bool>(5);
                };
            }
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
                Domain Domain = Parameter.Control.Clone();
                Domain.GetCarrierInstance().UseSimpleFrequency = true;
                Bitmap image = Generation.Video.Hexagon.Design1.GetImage(
                    Domain,
                    ImageSize,
                    DrawSize,
                    Thikness,
                    CircleSize,
                    Division,
                    Precise
                );
                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.Hexagon.Title"));
                image.Dispose();
            }
        }
        public class Fft : BitmapViewerManager, IRealtimeDisplay
        {
            private readonly Parameter Parameter;
            private int Width = 1000;
            private int Height = 1000;
            private int Amplitude = 2000;
            private int Thikness = 2;
            private int Size = 15;
            private int RangeBegin = 0;
            private int RangeEnd = 3141;
            public Fft(Parameter Parameter) : base(true)
            {
                this.Parameter = Parameter;
                Viewer.SettingMenuClicked += (_, _) =>
                {
                    List<DialogInputWindow.InputContext> Inputs =
                        [
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FFT.Design1.Width"), DialogInputWindow.InputContextMode.TextBox, Width, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FFT.Design1.Height"), DialogInputWindow.InputContextMode.TextBox, Height, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FFT.Design1.Amplitude"), DialogInputWindow.InputContextMode.TextBox, Amplitude, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FFT.Design1.Thikness"), DialogInputWindow.InputContextMode.TextBox, Thikness, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FFT.Design1.Size"), DialogInputWindow.InputContextMode.TextBox, Size, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FFT.Design1.Range.Begin"), DialogInputWindow.InputContextMode.TextBox, RangeBegin, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FFT.Design1.Range.End"), DialogInputWindow.InputContextMode.TextBox, RangeEnd, typeof(int)),
                        ];
                    DialogInputWindow InputDialog = new(
                        Viewer,
                        LanguageManager.GetString("Simulator.Generation.Dialog.FFT.Design1.Title"),
                        Inputs
                    );
                    InputDialog.ShowDialog();
                    if (InputDialog.Contexts == null) return;

                    Width = InputDialog.GetValue<int>(0);
                    Height = InputDialog.GetValue<int>(1);
                    Amplitude = InputDialog.GetValue<int>(2);
                    Thikness = InputDialog.GetValue<int>(3);
                    Size = InputDialog.GetValue<int>(4);
                    RangeBegin = InputDialog.GetValue<int>(5);
                    RangeEnd = InputDialog.GetValue<int>(6);
                };
            }
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
                Bitmap image = Generation.Video.FFT.Design1.GetImage(Parameter.Control.Clone(), Width, Height, Amplitude, Thikness, Size, RangeBegin, RangeEnd);
                SetImage(image, LanguageManager.GetString("Simulator.RealTime.RealtimeWindows.FFT.Title"));
                image.Dispose();
            }
        }
        public class Fs : BitmapViewerManager, IRealtimeDisplay
        {
            private readonly Parameter Parameter;
            private int ImageWidth = 1000;
            private int ImageHeight = 1000;
            private (int, int) N = (1, 100);
            private int Division = 10000;
            private bool Precise = false;
            private string StrCoefficients = "C = [0]";
            public Fs(Parameter Parameter) : base(true)
            {
                this.Parameter = Parameter;

                MenuItem CopyButton = new()
                {
                    Header = LanguageManager.GetString("Simulator.RealTime.UniqueWindow.Fs.CopyCoefficients")
                };
                CopyButton.Click += (_, _) =>
                {
                    Clipboard.SetText(StrCoefficients);
                };

                Viewer.ContextMenu.Items.Insert(0, CopyButton);

                Viewer.SettingMenuClicked += (_, _) =>
                {
                    List<DialogInputWindow.InputContext> Inputs =
                        [
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FS.Design1.Width"), DialogInputWindow.InputContextMode.TextBox, ImageWidth, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FS.Design1.Height"), DialogInputWindow.InputContextMode.TextBox, ImageHeight, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FS.Design1.N.Begin"), DialogInputWindow.InputContextMode.TextBox, N.Item1, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FS.Design1.N.End"), DialogInputWindow.InputContextMode.TextBox, N.Item2, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FS.Design1.Division"), DialogInputWindow.InputContextMode.TextBox, Division, typeof(int)),
                            new (LanguageManager.GetString("Simulator.Generation.Dialog.FS.Design1.Precise"), DialogInputWindow.InputContextMode.CheckBox, Precise, typeof(bool)),
                        ];
                    DialogInputWindow InputDialog = new(
                        Viewer,
                        LanguageManager.GetString("Simulator.Generation.Dialog.FS.Design1.Title"),
                        Inputs
                    );

                    InputDialog.ShowDialog();
                    if (InputDialog.Contexts == null) return;

                    ImageWidth = InputDialog.GetValue<int>(0);
                    ImageHeight = InputDialog.GetValue<int>(1);
                    N = (InputDialog.GetValue<int>(2), InputDialog.GetValue<int>(3));
                    Division = InputDialog.GetValue<int>(4);
                    Precise = InputDialog.GetValue<bool>(5);
                };
            }
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
                Vvvf.Model.Struct.Domain Domain = Parameter.Control.Clone();
                Data.Vvvf.Struct ysd = Parameter.VvvfSoundData;
                double[] Coefficients = GenerateBasic.Fourier.GetFourierCoefficients(Domain, Division, Precise, GenerateBasic.Fourier.PrimitiveOfSine, N);
                StrCoefficients = GenerateBasic.Fourier.GetDesmosFourierCoefficientsArray(ref Coefficients);
                Bitmap image = Generation.Video.FS.Design1.GetImage(ref Coefficients, ImageWidth, ImageHeight);
                SetImage(image, LanguageManager.GetString("Simulator.RealTime.UniqueWindow.Fs.Title"));
                image.Dispose();
            }
        }
    }
}
