using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// Control_Basic.xaml の相互作用ロジック
    /// </summary>
    public partial class PulseSetting : UserControl
    {
        private readonly YamlControlData Target;
        private readonly int Level;
        private bool IgnoreUpdate = true;
        private readonly WaveformEditor Editor;

        private readonly ViewModel BindingData = new();
        private class ViewModel : ViewModelBase
        {
            private bool _Harmonic_Visible = true;
            public bool Harmonic_Visible { get { return _Harmonic_Visible; } set { _Harmonic_Visible = value; RaisePropertyChanged(nameof(Harmonic_Visible)); } }

            private bool _BaseWaveSelector_Visible = true;
            public bool BaseWaveSelector_Visible { get { return _BaseWaveSelector_Visible; } set { _BaseWaveSelector_Visible = value; RaisePropertyChanged(nameof(BaseWaveSelector_Visible)); } }

            private bool _AltModeSelector_Visible = true;
            public bool AltModeSelector_Visible { get { return _AltModeSelector_Visible; } set { _AltModeSelector_Visible = value; RaisePropertyChanged(nameof(AltModeSelector_Visible)); } }

            private bool _Discrete_Visible = true;
            public bool Discrete_Visible { get { return _Discrete_Visible; } set { _Discrete_Visible = value; RaisePropertyChanged(nameof(Discrete_Visible)); } }

        }

        public PulseSetting(WaveformEditor Editor, YamlControlData ycd, int Level)
        {
            InitializeComponent();
            this.Editor = Editor;
            Target = ycd;
            this.Level = Level;
            DataContext = BindingData;

            InitializeView();

            IgnoreUpdate = false;
        }

        private void InitializeView()
        {
            PulseTypeSelector.ItemsSource = FriendlyNameConverter.GetPulseTypeNames(PulseModeConfiguration.GetAvailablePulseType(Level));
            PulseTypeSelector.SelectedValue = Target.PulseMode.PulseType;

            BaseWaveSelector.ItemsSource = FriendlyNameConverter.GetBaseWaveTypeNames();
            BaseWaveSelector.SelectedValue = Target.PulseMode.BaseWave;

            SetPulseSettingControls();
        }

        private void SetPulseSettingControls()
        {
            IgnoreUpdate = true;
            SetVisibilityOfControl();
            SetPulseCountControl();
            SetAltSelectorControl();
            IgnoreUpdate = false;
        }

        private void SetVisibilityOfControl()
        {
            YamlPulseMode Mode = Target.PulseMode;
            BindingData.Harmonic_Visible = PulseModeConfiguration.IsCompareWaveEditable(Mode, Level);
            BindingData.BaseWaveSelector_Visible = PulseModeConfiguration.IsCompareWaveEditable(Mode, Level);
            BindingData.Discrete_Visible = true;
        }

        private void SetPulseCountControl()
        {
            int[] AvailablePulses = PulseModeConfiguration.GetAvailablePulseCount(Target.PulseMode.PulseType, Level);
            
            if(AvailablePulses.Length == 0)
            {
                PulseCountSection.Visibility = Visibility.Collapsed;
                Target.PulseMode.PulseCount = 1;
            }
            else
            {
                PulseCountSection.Visibility = Visibility.Visible;
                if (AvailablePulses.Length >= 1 && AvailablePulses[0] != -1)
                {
                    PulseCountSelector.Visibility = Visibility.Visible;
                    PulseCountBox.Visibility = Visibility.Collapsed;
                    PulseCountSelector.ItemsSource = FriendlyNameConverter.GetPulseCountNames(AvailablePulses);
                    PulseCountSelector.SelectedValue = Target.PulseMode.PulseCount;

                    if (!PulseCountSelector.Items.Contains(PulseCountSelector.SelectedItem))
                    {
                        PulseCountSelector.SelectedIndex = 0;
                        Target.PulseMode.PulseCount = (int)PulseCountSelector.SelectedValue;
                    }
                }
                else if (AvailablePulses.Length == 1)
                {
                    PulseCountSelector.Visibility = Visibility.Collapsed;
                    PulseCountBox.Visibility = Visibility.Visible;
                    PulseCountBox.Text = Target.PulseMode.PulseCount.ToString();
                }
            }
        }

        private void SetAltSelectorControl()
        {
            PulseAlternative[] Alts = PulseModeConfiguration.GetPulseAlternatives(Target.PulseMode, Level);
            AltModeSelector.ItemsSource = FriendlyNameConverter.GetPulseAltModeNames(Alts);
            AltModeSelector.SelectedValue = Target.PulseMode.Alternative;

            if (!AltModeSelector.Items.Contains(AltModeSelector.SelectedItem))
            {
                AltModeSelector.SelectedIndex = 0;
                Target.PulseMode.Alternative = (PulseAlternative)AltModeSelector.SelectedValue;
            }

            if (Alts.Length == 1 && Alts[0] == PulseAlternative.Default)
                BindingData.AltModeSelector_Visible = false;
            else
                BindingData.AltModeSelector_Visible = true;
        }
        

        private void TextboxUpdate(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("PulseCount"))
            {
                Target.PulseMode.PulseCount = ParseTextBox.ParseInt(tb, Minimum:1, ErrorValue:1);
                MainWindow.                Instance?.UpdateControlList();
                SetPulseSettingControls();
                Editor.SetPulseDataContent();
                Editor.UpdateVisibility();
            }
        }
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;
            if (sender is not Button button) return;
            Object tag = button.Tag;

            if (tag.Equals("HarmonicSetting"))
            {
                MainWindow.SetInteractive(false);
                ControlBasicHarmonic cbh = new(MainWindow.Instance, Target.PulseMode);
                cbh.ShowDialog();
                MainWindow.SetInteractive(true);
            }
            else if (tag.Equals("DiscreteSetting"))
            {
                MainWindow.SetInteractive(false);
                DiscreteSettingWindow discreteSetting = new(MainWindow.Instance, Target.PulseMode);
                discreteSetting.ShowDialog();
                MainWindow.SetInteractive(true);
            }
        }

        private void SelectorSectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;

            if (tag.Equals("PulseType"))
            {
                PulseTypeName selected = (PulseTypeName)cb.SelectedValue;
                Target.PulseMode.PulseType = selected;
            }
            else if (tag.Equals("PulseCount"))
            {
                int selected = (int)cb.SelectedValue;
                Target.PulseMode.PulseCount = selected;
            }
            else if(tag.Equals("BaseWave"))
            {
                BaseWaveType selected = (BaseWaveType)cb.SelectedValue;
                Target.PulseMode.BaseWave = selected;
            }
            else if (tag.Equals("AltMode"))
            {
                PulseAlternative selected = (PulseAlternative)cb.SelectedValue;
                Target.PulseMode.Alternative = selected;
            }

            MainWindow.
            Instance?.UpdateControlList();
            Editor.SetPulseDataContent();
            Editor.UpdateVisibility();
            SetPulseSettingControls();
        }
    }
}
