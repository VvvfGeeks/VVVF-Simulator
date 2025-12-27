using System;
using System.Windows.Controls;
using VvvfSimulator.GUI.Create.Waveform.Async;
using VvvfSimulator.GUI.Create.Waveform.Async.Vibrato;
using VvvfSimulator.GUI.Create.Waveform.Common;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Data.Vvvf.Struct;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.AsyncControl.CarrierFrequency;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.AsyncControl.RandomModulation;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.AsyncControl.RandomModulation.Parameter;

namespace VvvfSimulator.GUI.Create.Waveform
{
    /// <summary>
    /// Control_Async.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlAsync : UserControl
    {
        private readonly PulseControl Data;
        private readonly bool IgnoreUpdate = true;
        private readonly WaveformEditor Editor;
        public ControlAsync(WaveformEditor Editor, PulseControl ycd)
        {
            this.Editor = Editor;
            Data = ycd;
            InitializeComponent();
            apply_data();
            IgnoreUpdate = false;
        }

        private void apply_data()
        {
            carrier_freq_mode.ItemsSource = FriendlyNameConverter.GetYamlAsyncCarrierModeName();
            carrier_freq_mode.SelectedValue = Data.AsyncModulationData.CarrierWaveData.Mode;

            Random_Range_Type_Selector.ItemsSource = FriendlyNameConverter.GetYamlAsyncParameterRandomValueModeNames();
            Random_Range_Type_Selector.SelectedValue = Data.AsyncModulationData.RandomData.Range.Mode;
            
            Random_Interval_Type_Selector.ItemsSource = FriendlyNameConverter.GetYamlAsyncParameterRandomValueModeNames();
            Random_Interval_Type_Selector.SelectedValue = Data.AsyncModulationData.RandomData.Interval.Mode;

            ShowSelectedCarrierMode(Data.AsyncModulationData.CarrierWaveData.Mode);
            Show_Random_Setting(Random_Range_Setting_Frame, Data.AsyncModulationData.RandomData.Range);
            Show_Random_Setting(Random_Interval_Setting_Frame, Data.AsyncModulationData.RandomData.Interval);
        }

        private void ComboBox_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            ComboBox cb = (ComboBox)sender;
            Object? tag = cb.Tag;
            if (tag == null) return;

            if (tag.Equals("Random_Range"))
            {
                Parameter.ValueMode selected = (Parameter.ValueMode)Random_Range_Type_Selector.SelectedValue;
                Data.AsyncModulationData.RandomData.Range.Mode = selected;
                Show_Random_Setting(Random_Range_Setting_Frame, Data.AsyncModulationData.RandomData.Range);
            }
            else if (tag.Equals("Random_Interval"))
            {
                Parameter.ValueMode selected = (Parameter.ValueMode)Random_Interval_Type_Selector.SelectedValue;
                Data.AsyncModulationData.RandomData.Interval.Mode = selected;
                Show_Random_Setting(Random_Interval_Setting_Frame, Data.AsyncModulationData.RandomData.Interval);
            }
            else if (tag.Equals("Param"))
            {
                PulseControl.AsyncControl.CarrierFrequency.ValueMode selected = (PulseControl.AsyncControl.CarrierFrequency.ValueMode)carrier_freq_mode.SelectedValue;
                Data.AsyncModulationData.CarrierWaveData.Mode = selected;
                ShowSelectedCarrierMode(selected);
            }
        }

        private void ShowSelectedCarrierMode(PulseControl.AsyncControl.CarrierFrequency.ValueMode selected)
        {
            if (selected == PulseControl.AsyncControl.CarrierFrequency.ValueMode.Const)
                carrier_setting.Navigate(new ControlConstSetting(Data.AsyncModulationData.CarrierWaveData.GetType(), Data.AsyncModulationData.CarrierWaveData));
            else if (selected == PulseControl.AsyncControl.CarrierFrequency.ValueMode.Moving)
                carrier_setting.Navigate(new ControlMovingSetting(Data.AsyncModulationData.CarrierWaveData.MovingValue));
            else if (selected == PulseControl.AsyncControl.CarrierFrequency.ValueMode.Vibrato)
                carrier_setting.Navigate(new ControlAsyncVibrato(Data));
            else if(selected == PulseControl.AsyncControl.CarrierFrequency.ValueMode.Table)
                carrier_setting.Navigate(new ControlAsyncCarrierTable(Data));
        }

        private void Show_Random_Setting(Frame ShowFrame, Parameter SettingValue)
        {
            if (SettingValue.Mode == Parameter.ValueMode.Const)
                ShowFrame.Navigate(new ControlConstSetting(SettingValue.GetType(), SettingValue));
            else if (SettingValue.Mode == Parameter.ValueMode.Moving)
                ShowFrame.Navigate(new ControlMovingSetting(SettingValue.MovingValue));
        }
    }
}
