using System;
using System.Collections.Generic;
using System.Windows.Controls;
using VvvfSimulator.GUI.Create.Waveform.Common;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsyncParameter.YamlAsyncParameterDipolar;

namespace VvvfSimulator.GUI.Create.Waveform.Dipolar
{
    /// <summary>
    /// Control_Dipolar.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlDipolar : UserControl
    {
        private readonly YamlControlData Target;
        private readonly bool IgnoreUpdate = true;

        public ControlDipolar(YamlControlData ycd)
        {
            Target = ycd;
            InitializeComponent();
            InitializeView();
            IgnoreUpdate = false;
        }

        private readonly Dictionary<YamlAsyncParameterDipolarMode, string> DipolarMode = [];
        private void InitializeView()
        {
            dipolar_mode.ItemsSource = FriendlyNameConverter.GetDipolarModeNames();
            dipolar_mode.SelectedValue = Target.AsyncModulationData.DipolarData.Mode;
            SetSelectedMode(Target.AsyncModulationData.DipolarData.Mode);
        }

        private void ValueModeSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;
            YamlAsyncParameterDipolarMode selected = (YamlAsyncParameterDipolarMode)dipolar_mode.SelectedValue;
            Target.AsyncModulationData.DipolarData.Mode = selected;
            SetSelectedMode(selected);
        }

        private void SetSelectedMode(YamlAsyncParameterDipolarMode selected)
        {
            YamlControlData.YamlAsyncParameter.YamlAsyncParameterDipolar value = Target.AsyncModulationData.DipolarData;
            if (selected == YamlAsyncParameterDipolarMode.Const)
                dipolar_param.Navigate(new ControlConstSetting(value.GetType(), value));
            else
                dipolar_param.Navigate(new ControlMovingSetting(Target.AsyncModulationData.DipolarData.MovingValue));
        }
    }
}
