using System;
using System.ComponentModel;
using System.IO.Ports;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VvvfSimulator.GUI.Resource.Theme;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Simulator.RealTime.Setting;
using static VvvfSimulator.Generation.Audio.GenerateRealTimeCommon;
using static VvvfSimulator.VvvfCalculate;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;

namespace VvvfSimulator.GUI.Simulator.RealTime
{
    /// <summary>
    /// Mascon.xaml の相互作用ロジック
    /// </summary>
    public partial class MasconWindow : Window
    {
        private readonly RealTimeParameter Param;
        public MasconWindow(RealTimeParameter parameter)
        {
            Param = parameter;

            InitializeComponent();
            SetState(0);
            DataContext = Model;
        }

        public void StartTask()
        {

            Task.Run(() => {
                while (!Param.Quit)
                {
                    System.Threading.Thread.Sleep(20);
                    Model.sine_freq = Param.Control.GetVideoSineFrequency();
                    Model.pulse_state = GetPulseName();
                }
            });
            Task.Run(() => {
                while (!Param.Quit)
                {
                    VvvfValues solve_control = Param.Control.Clone();
                    solve_control.SetRandomFrequencyMoveAllowed(false);
                    double voltage = Generation.Video.ControlInfo.GenerateControlCommon.GetVoltageRate(solve_control, Param.VvvfSoundData, false) * 100;
                    Model.voltage = voltage;
                }
            });
        }

        private readonly ViewModel Model = new ();
        public class ViewModel : ViewModelBase
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


            private double _sine_freq = 0;
            public double sine_freq { get { return _sine_freq; } set { _sine_freq = value; RaisePropertyChanged(nameof(sine_freq)); } }

            private double _voltage = 0;
            public double voltage { get { return _voltage; } set { _voltage = value; RaisePropertyChanged(nameof(voltage)); } }

            private String _pulse_state = PulseModeName.Async.ToString();
            public String pulse_state { get { return _pulse_state; } set { _pulse_state = value; RaisePropertyChanged(nameof(pulse_state)); } }
        };
        private String GetPulseName()
        {
            // Recalculate
            VvvfValues solve_control = Param.Control.Clone();
            Task re_calculate = Task.Run(() =>
            {
                solve_control.SetRandomFrequencyMoveAllowed(false);
                ControlStatus cv = new ControlStatus
                {
                    brake = solve_control.IsBraking(),
                    mascon_on = !solve_control.IsMasconOff(),
                    free_run = solve_control.IsFreeRun(),
                    wave_stat = solve_control.GetControlFrequency()
                };
                PwmCalculateValues calculated_Values = Yaml.VvvfSound.YamlVvvfWave.CalculateYaml(solve_control, cv, Param.VvvfSoundData);
                CalculatePhases(solve_control, calculated_Values, 0);
            });
            re_calculate.Wait();

            PulseMode mode_p = solve_control.GetVideoPulseMode();
            PulseModeName mode = mode_p.PulseName;
            //Not in sync
            if (mode == PulseModeName.Async)
            {
                CarrierFreq carrier_freq_data = solve_control.GetVideoCarrierFrequency();
                String default_s = String.Format(carrier_freq_data.base_freq.ToString("F2"));
                return default_s;
            }

            //Abs
            if (mode == PulseModeName.P_Wide_3)
                return "W 3";

            if (mode.ToString().StartsWith("CHM"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = ((contain_wide) ? "W " : "") + mode_name_type[1];

                return "CHM " + final_mode_name;
            }
            else if (mode.ToString().StartsWith("SHE"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = (contain_wide) ? "W " : "" + mode_name_type[1];

                return "SHE " + final_mode_name;
            }
            else if (mode.ToString().StartsWith("HOP"))
            {
                String mode_name = mode.ToString();
                bool contain_wide = mode_name.Contains("Wide");
                mode_name = mode_name.Replace("_Wide", "");

                String[] mode_name_type = mode_name.Split("_");

                String final_mode_name = (contain_wide) ? "W " : "" + mode_name_type[1];

                return "HOP " + final_mode_name;
            }
            else
            {
                String[] mode_name_type = mode.ToString().Split("_");
                return mode_name_type[1];
            }
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

            SolidColorBrush inactive = (SolidColorBrush)ThemeManager.GetBrush("MasconDefaultBackgroundBrush");

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

        public int MasconPosition = 0;
        public DeviceMode CurrentMode = DeviceMode.KeyBoard;

        // Serial Port
        public string MasconComPort = "COM3";
        public SerialPort serialPort = new();
        public void SetConfig()
        {
            if(CurrentMode == DeviceMode.PicoMascon)
            {
                if (serialPort.IsOpen) serialPort.Close();
                try
                {
                    serialPort = new SerialPort(MasconComPort);

                    serialPort.BaudRate = 9600;
                    serialPort.Parity = Parity.None;
                    serialPort.StopBits = StopBits.One;
                    serialPort.DataBits = 8;
                    serialPort.Handshake = Handshake.None;
                    serialPort.DtrEnable = true;

                    serialPort.DataReceived += new SerialDataReceivedEventHandler(OnReceived);

                    serialPort.Open();



                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error" , MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else if(CurrentMode == DeviceMode.KeyBoard)
            {
                if (serialPort.IsOpen) serialPort.Close();
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (CurrentMode != DeviceMode.KeyBoard) return;
            Key key = e.Key;

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
                catch (Exception)
                {

                }
            }

        }


        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            MenuItem menuItem = (MenuItem)sender;
            Object tag = menuItem.Tag;

            if (tag.Equals("DeviceSetting"))
            {
                MasconDevice rtds = new(this);
                rtds.ShowDialog();
            }
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {

        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (serialPort.IsOpen) serialPort.Close();
            Param.Quit = true;
        }

        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
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
