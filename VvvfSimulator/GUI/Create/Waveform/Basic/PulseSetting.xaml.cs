using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.Vvvf.Model;
using static VvvfSimulator.Data.Vvvf.Struct;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse.CarrierWaveConfiguration;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// Control_Basic.xaml の相互作用ロジック
    /// </summary>
    public partial class PulseSetting : UserControl
    {
        private readonly PulseControl Data;
        private readonly int Level;
        private bool IgnoreUpdate = true;
        private readonly WaveformEditor Editor;

        public PulseSetting(WaveformEditor Editor, PulseControl Data, int Level)
        {
            InitializeComponent();

            this.Editor = Editor;
            this.Data = Data;
            this.Level = Level;

            InitializeView();
            IgnoreUpdate = false;
        }

        private void InitializeView()
        {
            PulseTypeSelector.ItemsSource = FriendlyNameConverter.GetPulseTypeNames(Config.GetAvailablePulseType(Level));
            PulseTypeSelector.SelectedValue = Data.PulseMode.PulseType;

            BaseWaveSelector.ItemsSource = FriendlyNameConverter.GetBaseWaveTypeNames();
            BaseWaveSelector.SelectedValue = Data.PulseMode.BaseWave;

            UpdateSectionVisibility();
        }

        private static Visibility Bool2Visibility(bool b) => b ? Visibility.Visible : Visibility.Collapsed;
        private void UpdateSectionVisibility()
        {
            IgnoreUpdate = true;

            SetVisibilityOfPulseCountSection();
            SetVisibilityOfAltModeSection();
            SetVisibilityOfBaseWaveSection();
            SetVisibilityOfCarrierWaveSection();

            Border[] Sections = [PulseTypeSection, PulseCountSection, AltModeSection, BaseWaveSection, CarrierWaveSection];
            bool Cornered = false;
            for(int i = 0; i < Sections.Length; i++)
            {
                Border Section = Sections[Sections.Length - 1 - i];
                Section.CornerRadius = Cornered ? new(0) : new(0, 0, 25, 25);
                ((Border)((Grid)Section.Child).Children[0]).CornerRadius = Cornered ? new(0) : new(0, 0, 0, 25);
                if(Section.Visibility == Visibility.Visible) Cornered = true;
            }

            IgnoreUpdate = false;
        }
        private void SetVisibilityOfPulseCountSection()
        {
            int[] AvailablePulses = Config.GetAvailablePulseCount(Data.PulseMode.PulseType, Level);
            
            if(AvailablePulses.Length == 0)
            {
                PulseCountSection.Visibility = Visibility.Collapsed;
                Data.PulseMode.PulseCount = 1;
            }
            else
            {
                PulseCountSection.Visibility = Visibility.Visible;
                if (AvailablePulses.Length >= 1 && AvailablePulses[0] != -1)
                {
                    PulseCountSelector.Visibility = Visibility.Visible;
                    PulseCountBox.Visibility = Visibility.Collapsed;
                    PulseCountSelector.ItemsSource = FriendlyNameConverter.GetPulseCountNames(AvailablePulses);
                    PulseCountSelector.SelectedValue = Data.PulseMode.PulseCount;

                    if (!PulseCountSelector.Items.Contains(PulseCountSelector.SelectedItem))
                    {
                        PulseCountSelector.SelectedIndex = 0;
                        Data.PulseMode.PulseCount = (int)PulseCountSelector.SelectedValue;
                    }
                }
                else if (AvailablePulses.Length == 1)
                {
                    PulseCountSelector.Visibility = Visibility.Collapsed;
                    PulseCountBox.Visibility = Visibility.Visible;
                    PulseCountBox.Text = Data.PulseMode.PulseCount.ToString();
                }
            }
        }
        private void SetVisibilityOfAltModeSection()
        {
            PulseAlternative[] Alts = Config.GetPulseAlternatives(Data.PulseMode, Level);
            AltModeSelector.ItemsSource = FriendlyNameConverter.GetPulseAltModeNames(Alts);
            AltModeSelector.SelectedValue = Data.PulseMode.Alternative;

            if (!AltModeSelector.Items.Contains(AltModeSelector.SelectedItem))
            {
                AltModeSelector.SelectedIndex = 0;
                Data.PulseMode.Alternative = (PulseAlternative)AltModeSelector.SelectedValue;
            }

            AltModeSection.Visibility = Bool2Visibility(Alts.Length > 1 || Alts[0] != PulseAlternative.Default);
        }
        private void SetVisibilityOfBaseWaveSection()
        {
            bool _BaseWaveSelector = Config.IsBaseWaveChangeable(Data.PulseMode, Level);
            bool _HarmonicSettingButton = Config.IsBaseWaveHarmonicAvailable(Data.PulseMode, Level);
            bool _DiscreteSettingButton = Config.IsDiscreteTimeAvailable(Data.PulseMode, Level);

            BaseWaveSelector.Visibility = Bool2Visibility(_BaseWaveSelector);
            HarmonicSettingButton.Visibility = Bool2Visibility(_HarmonicSettingButton);
            DiscreteSettingButton.Visibility = Bool2Visibility(_DiscreteSettingButton);

            BaseWaveSection.Visibility = Bool2Visibility(_BaseWaveSelector || _HarmonicSettingButton || _DiscreteSettingButton);
        }
        private void SetVisibilityOfCarrierWaveSection()
        {
            CarrierWaveTypeSelector.ItemsSource = FriendlyNameConverter.GetCarrierWaveTypeNames();
            CarrierWaveTypeSelector.SelectedValue = Data.PulseMode.CarrierWave.Type;

            CarrierWaveOption[] Options = Config.GetAvailableCarrierWaveOptions(Data.PulseMode, Level);
            CarrierWaveOptionSelector.ItemsSource = FriendlyNameConverter.GetCarrierWaveOptionNames(Options);
            CarrierWaveOptionSelector.SelectedValue = Data.PulseMode.CarrierWave.Option;

            CarrierWaveTypeSelector.Visibility = Bool2Visibility(true);
            CarrierWaveOptionSelector.Visibility = Bool2Visibility(true);

            CarrierWaveSection.Visibility = Bool2Visibility(Config.IsCarrierWaveChangeable(Data.PulseMode, Level));
        }

        private void TextboxUpdate(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("PulseCount"))
            {
                Data.PulseMode.PulseCount = ParseTextBox.ParseInt(tb, Minimum:1, ErrorValue:1);
                MainWindow.GetInstance()?.UpdateControlList();
                UpdateSectionVisibility();
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
                ControlBasicHarmonic cbh = new(MainWindow.GetInstance(), Data.PulseMode);
                cbh.ShowDialog();
                MainWindow.SetInteractive(true);
            }
            else if (tag.Equals("DiscreteSetting"))
            {
                MainWindow.SetInteractive(false);
                DiscreteSettingWindow discreteSetting = new(MainWindow.GetInstance(), Data.PulseMode);
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
                Data.PulseMode.PulseType = selected;
            }
            else if (tag.Equals("PulseCount"))
            {
                int selected = (int)cb.SelectedValue;
                Data.PulseMode.PulseCount = selected;
            }
            else if(tag.Equals("BaseWave"))
            {
                BaseWaveType selected = (BaseWaveType)cb.SelectedValue;
                Data.PulseMode.BaseWave = selected;
            }
            else if (tag.Equals("AltMode"))
            {
                PulseAlternative selected = (PulseAlternative)cb.SelectedValue;
                Data.PulseMode.Alternative = selected;
            }
            else if (tag.Equals("CarrierWaveType"))
            {
                CarrierWaveType Item = (CarrierWaveType)cb.SelectedValue;
                Data.PulseMode.CarrierWave.Type = Item;
            }
            else if (tag.Equals("CarrierWaveOption"))
            {
                CarrierWaveOption Item = (CarrierWaveOption)cb.SelectedValue;
                Data.PulseMode.CarrierWave.Option = Item;
            }

            MainWindow.GetInstance()?.UpdateControlList();
            Editor.SetPulseDataContent();
            Editor.UpdateVisibility();
            UpdateSectionVisibility();
        }
    }
}
