using System.Collections.Generic;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.Vvvf;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode.PulseDataValue;

namespace VvvfSimulator.GUI.Create.Waveform.Common
{
    /// <summary>
    /// Control_Dipolar.xaml の相互作用ロジック
    /// </summary>
    public partial class PulseDataSetting : UserControl
    {
        private readonly Dictionary<PulseDataKey, PulseDataValue> Data;
        private readonly bool IgnoreUpdate = true;
        private readonly WaveformEditor Editor;
        private readonly PulseDataKey DataKey;

        public PulseDataSetting(WaveformEditor Editor, Dictionary<PulseDataKey, PulseDataValue> Data, PulseDataKey DataKey)
        {
            this.Data = Data;
            this.Editor = Editor;
            this.DataKey = DataKey;
            InitializeComponent();
            InitializeView();
            IgnoreUpdate = false;
            SettingTitle.Content = FriendlyNameConverter.GetPulseDataKeyName(DataKey);
        }

        private void InitializeView()
        {
            ValueMode.ItemsSource = FriendlyNameConverter.GetPulseDataValueModeNames();
            if(Data.GetValueOrDefault(DataKey) == null)
            {
                PulseDataValue Value = new()
                {
                    Constant = Struct.PulseModeConfiguration.GetPulseDataKeyDefaultConstant(DataKey)
                };
                Data.Add(DataKey, Value);
            }
                
            ValueMode.SelectedValue = Data.GetValueOrDefault(DataKey, new()).Mode;
            SetSelectedMode((PulseDataValueMode)ValueMode.SelectedValue);
        }

        private void ValueModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;
            PulseDataValueMode selected = (PulseDataValueMode)ValueMode.SelectedValue;
            Data.GetValueOrDefault(DataKey, new()).Mode = selected;
            SetSelectedMode(selected);
        }

        private void SetSelectedMode(PulseDataValueMode selected)
        {
            PulseDataValue Value = Data.GetValueOrDefault(DataKey, new());
            if (selected == PulseDataValueMode.Const)
                ParameterFrame.Navigate(new ControlConstSetting(typeof(PulseDataValue), Value));
            else
                ParameterFrame.Navigate(new ControlMovingSetting(Value.MovingValue));
        }
    }
}
