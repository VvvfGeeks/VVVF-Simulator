using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// Control_Basic.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlBasic : UserControl
    {
        private readonly YamlControlData Target;
        private readonly int Level;
        private readonly bool IgnoreUpdate = true;

        private readonly ViewModel BindingData = new();
        private class ViewModel : ViewModelBase
        {
            private bool _Harmonic_Visible = true;
            public bool Harmonic_Visible { get { return _Harmonic_Visible; } set { _Harmonic_Visible = value; RaisePropertyChanged(nameof(Harmonic_Visible)); } }

            private bool _BaseWaveSelector_Visible = true;
            public bool BaseWaveSelector_Visible { get { return _BaseWaveSelector_Visible; } set { _BaseWaveSelector_Visible = value; RaisePropertyChanged(nameof(BaseWaveSelector_Visible)); } }

            private bool _AltModeSelector_Visible = true;
            public bool AltModeSelector_Visible { get { return _AltModeSelector_Visible; } set { _AltModeSelector_Visible = value; RaisePropertyChanged(nameof(AltModeSelector_Visible)); } }

            private bool _Shifted_Visible = true;
            public bool Shifted_Visible { get { return _Shifted_Visible; } set { _Shifted_Visible = value; RaisePropertyChanged(nameof(Shifted_Visible)); } }

            private bool _Square_Visible = true;
            public bool Square_Visible { get { return _Square_Visible; } set { _Square_Visible = value; RaisePropertyChanged(nameof(Square_Visible)); } }

            private bool _Discrete_Visible = true;
            public bool Discrete_Visible { get { return _Discrete_Visible; } set { _Discrete_Visible = value; RaisePropertyChanged(nameof(Discrete_Visible)); } }

        }

        public ControlBasic(YamlControlData ycd, int level)
        {
            InitializeComponent();

            Target = ycd;
            this.Level = level;
            DataContext = BindingData;

            InitializeView();

            IgnoreUpdate = false;
        }

        private void InitializeView()
        {
            from_text_box.Text = Target.ControlFrequencyFrom.ToString();
            sine_from_text_box.Text = Target.RotateFrequencyFrom.ToString();
            sine_below_text_box.Text = Target.RotateFrequencyBelow.ToString();

            PulseNameSelector.ItemsSource = FriendlyNameConverter.GetPulseModeNames(PulseModeConfiguration.ValidPulseModeNames(Level));
            PulseNameSelector.SelectedValue = Target.PulseMode.PulseName;

            Shifted_Box.IsChecked = Target.PulseMode.Shift;
            Square_Box.IsChecked = Target.PulseMode.Square;

            BaseWaveSelector.ItemsSource = FriendlyNameConverter.GetBaseWaveTypeNames();
            BaseWaveSelector.SelectedValue = Target.PulseMode.BaseWave;

            AltModeSelector.ItemsSource = FriendlyNameConverter.GetPulseAltModeNames(PulseModeConfiguration.GetPulseAltModes(Target.PulseMode, Level));
            AltModeSelector.SelectedValue = Target.PulseMode.AltMode;

            Enable_FreeRun_On_Check.IsChecked = Target.EnableFreeRunOn;
            Enable_FreeRun_Off_Check.IsChecked = Target.EnableFreeRunOff;
            Enable_Normal_Check.IsChecked = Target.EnableNormal;

            SetControl();
        }

        private void SetControl()
        {
            PulseMode mode = Target.PulseMode;

            BindingData.Harmonic_Visible = PulseModeConfiguration.IsPulseHarmonicBaseWaveChangeAvailable(mode, Level);
            BindingData.Shifted_Visible = PulseModeConfiguration.IsPulseShiftedAvailable(mode, Level);
            BindingData.BaseWaveSelector_Visible = PulseModeConfiguration.IsPulseHarmonicBaseWaveChangeAvailable(mode, Level);
            BindingData.Square_Visible = PulseModeConfiguration.IsPulseSquareAvail(mode, Level);
            BindingData.Discrete_Visible = PulseModeConfiguration.IsDiscreteTimeValid(mode, Level);

            PulseAlternativeMode[] AltModes = PulseModeConfiguration.GetPulseAltModes(Target.PulseMode, Level);
            AltModeSelector.ItemsSource = FriendlyNameConverter.GetPulseAltModeNames(AltModes);
            AltModeSelector.SelectedValue = Target.PulseMode.AltMode;

            if (!AltModeSelector.Items.Contains(AltModeSelector.SelectedItem))
            {
                AltModeSelector.SelectedIndex = 0;
                PulseAlternativeMode selected = (PulseAlternativeMode)AltModeSelector.SelectedValue;
                Target.PulseMode.AltMode = selected;
            }

            if (AltModes.Length == 1 && AltModes[0] == PulseAlternativeMode.Default)
                BindingData.AltModeSelector_Visible = false;
            else
                BindingData.AltModeSelector_Visible = true;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("From"))
            {
                double parsed = ParseTextBox.ParseDouble(tb);
                Target.ControlFrequencyFrom = parsed;
                MainWindow.GetInstance()?.UpdateControlList();
            }
            else if (tag.Equals("SineFrom"))
            {
                double parsed = ParseTextBox.ParseDouble(tb);
                Target.RotateFrequencyFrom = parsed;
                MainWindow.GetInstance()?.UpdateControlList();
            }
            else if (tag.Equals("SineBelow"))
            {
                double parsed = ParseTextBox.ParseDouble(tb);
                Target.RotateFrequencyBelow = parsed;
                MainWindow.GetInstance()?.UpdateControlList();
            }
        }

        private void CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            CheckBox tb = (CheckBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            bool check = tb.IsChecked != false;

            if (tag_str.Equals("Normal"))
                Target.EnableNormal = check;
            else if (tag_str.Equals("FreeRunOn"))
                Target.EnableFreeRunOn = check;
            else if (tag_str.Equals("FreeRunOff"))
                Target.EnableFreeRunOff = check;
            else if (tag_str.Equals("Shifted"))
                Target.PulseMode.Shift = check;
            else if (tag_str.Equals("Square"))
                Target.PulseMode.Square = check;

            SetControl();
            MainWindow.GetInstance()?.UpdateControlList();
            MainWindow.GetInstance()?.UpdateContentSelected();
            return;
        }

        private void Open_Harmonic_Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetInteractive(false);
            ControlBasicHarmonic cbh = new(MainWindow.GetInstance(), Target.PulseMode);
            cbh.ShowDialog();
            MainWindow.SetInteractive(true);
        }

        private void Selector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;

            if (tag.Equals("PulseName"))
            {
                PulseModeName selected = (PulseModeName)cb.SelectedValue;
                Target.PulseMode.PulseName = selected;
                MainWindow.GetInstance()?.UpdateControlList();
                MainWindow.GetInstance()?.UpdateContentSelected();
                return;
            }
            else if(tag.Equals("BaseWave"))
            {
                BaseWaveType selected = (BaseWaveType)cb.SelectedValue;
                Target.PulseMode.BaseWave = selected;
            }
            else if (tag.Equals("AltMode"))
            {
                PulseAlternativeMode selected = (PulseAlternativeMode)cb.SelectedValue;
                Target.PulseMode.AltMode = selected;
            }

            MainWindow.GetInstance()?.UpdateControlList();
            SetControl();
        }

        private void Open_Discrete_Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetInteractive(false);
            DiscreteSettingWindow discreteSetting = new(MainWindow.GetInstance(), Target.PulseMode);
            discreteSetting.ShowDialog();
            MainWindow.SetInteractive(true);
        }
    }
}
