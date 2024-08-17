using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;

namespace VvvfSimulator.GUI.Simulator.RealTime.Setting
{
    /// <summary>
    /// MasconDevice.xaml の相互作用ロジック
    /// </summary>
    public partial class MasconDevice : Window
    {
        private readonly ViewModel Model = new();
        public class ViewModel : ViewModelBase
        {

            private Visibility _PortVisibility = Visibility.Visible;
            public Visibility PortVisibility { get { return _PortVisibility; } set { _PortVisibility = value; RaisePropertyChanged(nameof(PortVisibility)); } }

            private Visibility _AccelerateKeySettingVisibility = Visibility.Visible;
            public Visibility AccelerateKeySettingVisibility { get { return _AccelerateKeySettingVisibility; } set { _AccelerateKeySettingVisibility = value; RaisePropertyChanged(nameof(AccelerateKeySettingVisibility)); } }

            private Visibility _NeutralKeySettingVisibility = Visibility.Visible;
            public Visibility NeutralKeySettingVisibility { get { return _NeutralKeySettingVisibility; } set { _NeutralKeySettingVisibility = value; RaisePropertyChanged(nameof(NeutralKeySettingVisibility)); } }

            private Visibility _BrakeKeySettingVisibility = Visibility.Visible;
            public Visibility BrakeKeySettingVisibility { get { return _BrakeKeySettingVisibility; } set { _BrakeKeySettingVisibility = value; RaisePropertyChanged(nameof(BrakeKeySettingVisibility)); } }

            private Visibility _FrequencyRateVisibility = Visibility.Visible;
            public Visibility FrequencyRateVisibility { get { return _FrequencyRateVisibility; } set { _FrequencyRateVisibility = value; RaisePropertyChanged(nameof(FrequencyRateVisibility)); } }
        };

        private readonly MasconWindow Main;
        public MasconDevice(MasconWindow Main)
        {
            this.Main = Main;
            Owner = Main;
            DataContext = Model;

            InitializeComponent();
            ModeSelector.ItemsSource = FriendlyNameConverter.GetMasconDeviceModeNames();
            ModeSelector.SelectedValue = Main.CurrentMode;
            SetCOMPorts();
            PortSelector.SelectedItem = Main.MasconComPort;

            SetTextBoxKey();
            SetDoubleInputTextBox();

            SetVisibility(Main.CurrentMode);
        }

        private readonly Dictionary<DeviceMode, string> DevideModeNames = [];
        
        public void SetCOMPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            PortSelector.ItemsSource = ports;
        }

        private static Visibility GetVisibility(bool IsVisible)
        {
            return IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }
        public void SetVisibility(DeviceMode mode)
        {
            Model.PortVisibility = GetVisibility(mode == DeviceMode.PicoMascon);
            Model.AccelerateKeySettingVisibility = GetVisibility(mode == DeviceMode.KeyBoard);
            Model.NeutralKeySettingVisibility = GetVisibility(mode == DeviceMode.KeyBoard);
            Model.BrakeKeySettingVisibility = GetVisibility(mode == DeviceMode.KeyBoard);
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;

            if (tag.Equals("Mode"))
            {
                DeviceMode mode = (DeviceMode)cb.SelectedValue;
                Main.CurrentMode = mode;
                SetVisibility(mode);
            }else if (tag.Equals("Port"))
            {
                string port = (string)cb.SelectedItem;
                Main.MasconComPort = port;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Main.SetConfig();
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

        private void SetTextBoxKey()
        {
            AccelerateKey.Text = ((Key)Properties.Settings.Default.RealTimeMasconAccelerateKey).ToString();
            NeutralKey.Text = ((Key)Properties.Settings.Default.RealTimeMasconNeutralKey).ToString();
            BrakeKey.Text = ((Key)Properties.Settings.Default.RealTimeMasconBrakeKey).ToString();
        }
        private void TextBoxKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (sender is not TextBox box) return;
            string? tag = box.Tag.ToString();
            if (tag == null) return;
            e.Handled = true;
            Key key = e.Key;
            box.Text = key.ToString();
            if (tag.Equals("Accelerate")) Properties.Settings.Default.RealTimeMasconAccelerateKey = (int)key;
            else if (tag.Equals("Neutral")) Properties.Settings.Default.RealTimeMasconNeutralKey = (int)key;
            else if (tag.Equals("Brake")) Properties.Settings.Default.RealTimeMasconBrakeKey = (int)key;
        }
        private void SetDoubleInputTextBox()
        {
            FrequencyRateInput.Text = Properties.Settings.Default.RealTimeMasconFrequencyChangeRate.ToString();
        }
        private void DoubleInputTextBoxChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            string? tag = textBox.Tag.ToString();
            if (tag == null) return;
            double value = ParseTextBox.ParseDouble(textBox);

            if (tag.Equals("FrequencyRate")) Properties.Settings.Default.RealTimeMasconFrequencyChangeRate = value;
        }
    }

    public enum DeviceMode
    {
        KeyBoard, PicoMascon
    }
}
