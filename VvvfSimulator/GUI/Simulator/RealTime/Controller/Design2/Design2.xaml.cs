using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VvvfSimulator.Generation;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Resource.Theme;
using VvvfSimulator.GUI.Simulator.RealTime.Setting;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Vvvf;
using static VvvfSimulator.Generation.Audio.RealTime;
using static VvvfSimulator.Vvvf.Model.Struct;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Key = System.Windows.Input.Key;

namespace VvvfSimulator.GUI.Simulator.RealTime.Controller.Design2
{
    /// <summary>
    /// Mascon.xaml の相互作用ロジック
    /// </summary>
    public partial class Design2 : Window, IController
    {
        private readonly Parameter Param;
        private readonly ViewModel Model = new();
        private int CalculatePrecision = 10000;
        public int MasconPosition = 0;
        public DeviceMode CurrentMode = DeviceMode.KeyBoard;
        public string MasconComPort = "";
        public SerialPort serialPort = new();
        public partial class ViewModel : ViewModelBase
        {
            private Brush _B4 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush B4 { get { return _B4; } set { _B4 = value; RaisePropertyChanged(nameof(B4)); } }


            private Brush _B3 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush B3 { get { return _B3; } set { _B3 = value; RaisePropertyChanged(nameof(B3)); } }


            private Brush _B2 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush B2 { get { return _B2; } set { _B2 = value; RaisePropertyChanged(nameof(B2)); } }


            private Brush _B1 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush B1 { get { return _B1; } set { _B1 = value; RaisePropertyChanged(nameof(B1)); } }

            private Brush _B0 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush B0 { get { return _B0; } set { _B0 = value; RaisePropertyChanged(nameof(B0)); } }


            private Brush _N = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush N { get { return _N; } set { _N = value; RaisePropertyChanged(nameof(N)); } }

            private Brush _P0 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush P0 { get { return _P0; } set { _P0 = value; RaisePropertyChanged(nameof(P0)); } }

            private Brush _P1 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush P1 { get { return _P1; } set { _P1 = value; RaisePropertyChanged(nameof(P1)); } }

            private Brush _P2 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush P2 { get { return _P2; } set { _P2 = value; RaisePropertyChanged(nameof(P2)); } }

            private Brush _P3 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush P3 { get { return _P3; } set { _P3 = value; RaisePropertyChanged(nameof(P3)); } }


            private Brush _P4 = new SolidColorBrush(Color.FromRgb(0xA0, 0xA0, 0xA0));
            public Brush P4 { get { return _P4; } set { _P4 = value; RaisePropertyChanged(nameof(P4)); } }


            private string _SineFrequency = string.Empty;
            public string SineFrequency { get { return _SineFrequency; } set { _SineFrequency = value; RaisePropertyChanged(nameof(SineFrequency)); } }

            private string _Voltage = string.Empty;
            public string Voltage { get { return _Voltage; } set { _Voltage = value; RaisePropertyChanged(nameof(Voltage)); } }

            private string _PulseState = string.Empty;
            public string PulseState { get { return _PulseState; } set { _PulseState = value; RaisePropertyChanged(nameof(PulseState)); } }
        };
        public Design2(Parameter parameter)
        {
            Param = parameter;
            CalculatePrecision = (Param is VvvfSoundParameter) ? Properties.Settings.Default.RealTime_VVVF_Controller_Design2_CalculatePrecision :
                Properties.Settings.Default.RealTime_Train_Controller_Design2_CalculatePrecision;

            InitializeComponent();
            SetState(0);
            DataContext = Model;
            SetWindowContentVisibility();
        }
        public void StartTask()
        {

            Task.Run(() => {
                while (!Param.Quit)
                {
                    Domain Control = Param.Control.Clone();
                    Control.GetCarrierInstance().UseSimpleFrequency = true;

                    Model.SineFrequency = Control.ElectricalState.BaseWaveFrequency.ToString("F1");

                    if (Control.ElectricalState.IsNone)
                        Model.PulseState = LanguageManager.GetString("Simulator.RealTime.Controller.PulseState.None");
                    else
                    {
                        int PulseCount = Control.ElectricalState.PulsePattern.PulseMode.PulseCount;
                        Model.PulseState = Control.ElectricalState.PulsePattern.PulseMode.PulseType switch
                        {
                            PulseTypeName.ASYNC => Control.GetCarrierInstance().CalculateCarrierFrequency(Control.GetTime(), Control.ElectricalState).ToString("F2"),
                            PulseTypeName.SYNC => PulseCount.ToString(),
                            PulseTypeName.CHM => "CHM " + PulseCount.ToString(),
                            PulseTypeName.SHE => "SHE " + PulseCount.ToString(),
                            PulseTypeName.HO => "HO " + PulseCount.ToString(),
                            PulseTypeName.ΔΣ => Control.ElectricalState.PulseData.GetValueOrDefault(PulseDataKey.UpdateFrequency).ToString("F2"),
                            _ => PulseCount.ToString(),
                        };
                    }
                }
            });
            Task.Run(() => {
                while (!Param.Quit)
                {
                    System.Threading.Thread.Sleep(15);

                    Domain Control = Param.Control.Clone();
                    Control.GetCarrierInstance().UseSimpleFrequency = true;

                    double InitialPhase = MyMath.M_PI_6;
                    {
                        if (Param is VvvfSoundParameter VvvfParam)
                        {
                            InitialPhase = VvvfParam.OutputMode switch
                            {
                                VvvfSoundParameter.Mode.Phase => 0,
                                VvvfSoundParameter.Mode.PhaseCurrent => 0,
                                _ => MyMath.M_PI_6,
                            };
                        }
                    }

                    PhaseState[] Pwm = GenerateBasic.WaveForm.GetUVWCycle(Control, InitialPhase, CalculatePrecision, false);

                    double voltage = GenerateBasic.Fourier.GetVoltageRate(ref Pwm) * 100;
                    Model.Voltage = Param.Control.ElectricalState.IsNone ? LanguageManager.GetString("Simulator.RealTime.Controller.Voltage.None") : voltage.ToString("F1");

                    List<(double X, double Pwm)> Data = [];

                    for (int i = 0; i < Pwm.Length; i++)
                    {
                        double NewPwmLine = 0;
                        if (Param is VvvfSoundParameter VvvfParam)
                        {
                            NewPwmLine = VvvfParam.OutputMode switch
                            {
                                VvvfSoundParameter.Mode.Line => Pwm[i].U - Pwm[i].V,
                                VvvfSoundParameter.Mode.Phase => (Pwm[i].U - 1) * 2,
                                _ => Pwm[i].U - Pwm[i].V * 0.5 - Pwm[i].W * 0.5
                            };
                        }
                        else if (Param is TrainSoundParameter TrainParam)
                        {
                            NewPwmLine = Pwm[i].U - Pwm[i].V;
                        }

                        if (Data.Count == 0 || Data[^1].Pwm != NewPwmLine)
                            Data.Add(((double)i / Pwm.Length, NewPwmLine));
                    }
                    Data.Add((1.0, Data[^1].Pwm));

                    double GetY(double Pwm)
                    {
                        return -Pwm * WaveformViewer.ActualHeight / 2.0 * (1 / 2.0 - 1 / 10.0) + WaveformViewer.ActualHeight / 2.0;
                    }
                    double GetX(double Ratio)
                    {
                        return Ratio * WaveformViewer.ActualWidth;
                    }

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        PathFigure Figure = new()
                        {
                            StartPoint = new(GetX(Data[0].X), GetY(Data[0].Pwm))
                        };
                        for (int i = 0; i < Data.Count - 1; i++)
                        {
                            Figure.Segments.Add(new LineSegment()
                            {
                                Point = new()
                                {
                                    X = GetX(Data[i].X),
                                    Y = GetY(Data[i].Pwm),
                                },
                                IsStroked = true,
                            });
                            Figure.Segments.Add(new LineSegment()
                            {
                                Point = new()
                                {
                                    X = GetX(Data[i + 1].X),
                                    Y = GetY(Data[i].Pwm),
                                },
                                IsStroked = true,
                            });
                        }
                        Figure.Segments.Add(new LineSegment()
                        {
                            Point = new()
                            {
                                X = GetX(Data[^1].X),
                                Y = GetY(Data[^1].Pwm),
                            },
                            IsStroked = true,
                        });

                        PathGeometry Geometry = new();
                        Geometry.Figures.Add(Figure);

                        System.Windows.Shapes.Path Path = new()
                        {
                            Stroke = ThemeManager.GetBrush("RealtimeControllerWaveformBrush"),
                            StrokeThickness = 3.0,
                            Data = Geometry,
                        };
                        WaveformViewer.Children.Clear();
                        WaveformViewer.Children.Add(Path);
                    });

                }
            });
        }        
        private void SetColor(int c, SolidColorBrush brush)
        {
            if (c == 0) Model.N = brush;

            if (c == -1) Model.B0 = brush;
            if (c == -2) Model.B1 = brush;
            if (c == -3) Model.B2 = brush;
            if (c == -4) Model.B3 = brush;
            if (c == -5) Model.B4 = brush;

            if (c == 1) Model.P0 = brush;
            if (c == 2) Model.P1 = brush;
            if (c == 3) Model.P2 = brush;
            if (c == 4) Model.P3 = brush;
            if (c == 5) Model.P4 = brush;
        }
        private void SetState(int at)
        {
            
            int at_abs = (at < 0) ? -at : at;
            bool nega = at < 0;

            Param.FrequencyChangeRate = (nega ? -1 : 1) * ((at_abs - 1 < 0) ? 0 : at_abs - 1) * Math.PI * 2 * Properties.Settings.Default.RealTimeMasconFrequencyChangeRate;

            bool pre_braking = Param.IsBraking;
            Param.IsFreeRunning = at == 0;
            Param.IsBraking = (at == 0) ? pre_braking : at < 0;

            SolidColorBrush inactive = (SolidColorBrush)ThemeManager.GetBrush("RealtimeControllerDefaultBackgroundBrush");

            if (at == 0)
            {
                for (int i = -5; i < 6; i++)
                {
                    SetColor(i, inactive);
                }
                SetColor(0, (SolidColorBrush)ThemeManager.GetBrush("MasconNeutralBrush"));
                return;
            } else
                SetColor(0, inactive);

            for (int i = 1; i <= 5; i++)
            {
                SolidColorBrush target = (SolidColorBrush)ThemeManager.GetBrush($"Mascon{(nega ? "B" : "P")+(i - 1)}Brush");
                if (at_abs >= i)
                    SetColor(nega ? -i : i, target);
                else
                    SetColor(nega ? -i : i, inactive);

                SetColor(nega ? i : -i, inactive);
            }
        }
        private void SetWindowContentVisibility()
        {
            Visibility VvvfItemVisibility = (Param is VvvfSoundParameter) ? Visibility.Visible : Visibility.Collapsed;
            MenuItem_Vvvf.Visibility = VvvfItemVisibility;
            MenuItem_Vvvf_Separator.Visibility = VvvfItemVisibility;
        }
        //
        // Controller Device Interface
        //
        public int GetPosition()
        {
            return MasconPosition;
        }
        public void PrepareController()
        {
            if(CurrentMode == DeviceMode.PicoMascon)
            {
                if (serialPort.IsOpen) serialPort.Close();
                try
                {
                    serialPort = new SerialPort(MasconComPort)
                    {
                        BaudRate = 9600,
                        Parity = Parity.None,
                        StopBits = StopBits.One,
                        DataBits = 8,
                        Handshake = Handshake.None,
                        DtrEnable = true
                    };
                    serialPort.DataReceived += new SerialDataReceivedEventHandler(OnReceived);
                    serialPort.Open();
                }
                catch (Exception ex)
                {
                    DialogBox.Show(ex.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                }
            }
            else if(CurrentMode == DeviceMode.KeyBoard)
            {
                if (serialPort.IsOpen) serialPort.Close();
            }
        }
        public DeviceMode GetControllerMode()
        {
            return CurrentMode;
        }
        public void SetControllerMode(DeviceMode mode)
        {
            CurrentMode = mode;
        }
        public string GetComPort()
        {
            return MasconComPort;
        }
        public void SetComPort(string port)
        {
            MasconComPort = port;
        }
        public Window GetInstance()
        {
            return this;
        }


        // Controller Input
        private void ControllerHandle_Keyboard(Key key)
        {
            if (key.Equals((Key)Properties.Settings.Default.RealTimeMasconAccelerateKey))
                MasconPosition++;
            else if (key.Equals((Key)Properties.Settings.Default.RealTimeMasconBrakeKey))
                MasconPosition--;
            else if (key.Equals((Key)Properties.Settings.Default.RealTimeMasconNeutralKey))
                MasconPosition = 0;

            if (MasconPosition > 5) MasconPosition = 5;
            if (MasconPosition < -5) MasconPosition = -5;

            SetState(MasconPosition);
        }
        
        private void OnReceived(object sender, SerialDataReceivedEventArgs e)
        {
            if (!serialPort.IsOpen) return;
            String read = serialPort.ReadExisting();
            
            if(CurrentMode == DeviceMode.PicoMascon)
            {
                try
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        int current = Int32.Parse(read);
                        MasconPosition = current - 5;
                        SetState(MasconPosition);
                    });
                }
                catch (Exception) { }
            }

        }

        private void OnMasconMouseEvent(object sender, MouseEventArgs e)
        {
            if (sender is not Border border) return;
            string? tag = border.Tag.ToString();
            if(tag == null) return;
            MasconPosition = int.Parse(tag);
            SetState(MasconPosition);
        }

        //
        // Window Event
        //
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            Key key = e.Key;

            if (CurrentMode == DeviceMode.KeyBoard) ControllerHandle_Keyboard(key);
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem Item) return;
            string[]? Tag = Item.Tag?.ToString()?.Split('_');
            if (Tag == null) return;

            if (Tag[0].Equals("DeviceSetting"))
            {
                MasconDevice rtds = new(this);
                rtds.ShowDialog();
            }
            else if (Tag[0].Equals("CalculateDivision"))
            {
                IntegerNumberInput Input = new(this, LanguageManager.GetString("Simulator.RealTime.Controller.Design2.MenuItem.CalculateDivision"), 0, CalculatePrecision);
                if (!Input.IsEnteredValueValid()) return;
                CalculatePrecision = Input.GetEnteredValue();
                switch (Param is VvvfSoundParameter)
                {
                    case true:
                        Properties.Settings.Default.RealTime_VVVF_Controller_Design2_CalculatePrecision = CalculatePrecision;
                        break;
                    default:
                        Properties.Settings.Default.RealTime_Train_Controller_Design2_CalculatePrecision = CalculatePrecision;
                        break;
                }
                Properties.Settings.Default.Save();
            }
            else if (Tag[0].Equals("Vvvf"))
            {
                if (Param is not VvvfSoundParameter) return;
                if (Tag[1].Equals("WaveForm"))
                {
                    MenuItem[] Candidates = [MenuItem_Vvvf_WaveForm_Line, MenuItem_Vvvf_WaveForm_Phase, MenuItem_Vvvf_WaveForm_PhaseCurrent];
                    VvvfSoundParameter.Mode[] Modes = Enum.GetValues<VvvfSoundParameter.Mode>();

                    for (int i = 0; i < Candidates.Length; i++)
                    {
                        MenuItem Candidate = Candidates[i];
                        bool Selected = Candidate == Item;
                        Candidate.IsChecked = Selected;
                        if (Selected) ((VvvfSoundParameter)Param).OutputMode = Modes[i];
                    }
                }
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (serialPort.IsOpen) serialPort.Close();
            Param.Quit = true;
        }
        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("Close"))
                Close();
            else if (tag.Equals("Maximize"))
            {
                if (WindowState.Equals(WindowState.Maximized))
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else if (tag.Equals("Minimize"))
                WindowState = WindowState.Minimized;
        }
    }
}
