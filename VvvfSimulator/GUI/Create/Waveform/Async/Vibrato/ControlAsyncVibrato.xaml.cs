﻿using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Create.Waveform.Common;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsync.CarrierFrequency.YamlAsyncParameterCarrierFreqVibrato;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAsync.CarrierFrequency.YamlAsyncParameterCarrierFreqVibrato.YamlAsyncParameterVibratoValue;

namespace VvvfSimulator.GUI.Create.Waveform.Async.Vibrato
{
    /// <summary>
    /// Control_Async_Vibrato.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlAsyncVibrato : UserControl
    {
        private readonly YamlControlData Target;
        private readonly bool IgnoreUpdate = true;
        public ControlAsyncVibrato(YamlControlData data)
        {
            Target = data;
            InitializeComponent();
            apply_data();
            IgnoreUpdate = false;
        }

        private void apply_data()
        {
            base_wave.ItemsSource = FriendlyNameConverter.GetYamlAsyncParameterVibratoBaseWaveTypeNames();
            highest_mode.ItemsSource = FriendlyNameConverter.GetYamlAsyncParameterVibratoModeNames();
            lowest_mode.ItemsSource = FriendlyNameConverter.GetYamlAsyncParameterVibratoModeNames();
            interval_mode.ItemsSource = FriendlyNameConverter.GetYamlAsyncParameterVibratoModeNames();

            var vibrato_data = Target.AsyncModulationData.CarrierWaveData.VibratoData;

            highest_mode.SelectedValue = vibrato_data.Highest.Mode;
            SetSelected(0, vibrato_data.Highest.Mode);

            lowest_mode.SelectedValue = vibrato_data.Lowest.Mode;
            SetSelected(1, vibrato_data.Lowest.Mode);

            interval_mode.SelectedValue = vibrato_data.Interval.Mode;
            SetSelected(2, vibrato_data.Interval.Mode);

            base_wave.SelectedValue = vibrato_data.BaseWave;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            ComboBox cb = (ComboBox)sender;
            Object? tag = cb.Tag;

            YamlAsyncParameterVibratoMode mode = (YamlAsyncParameterVibratoMode)cb.SelectedValue;
            if (tag.Equals("Highest"))
            {
                Target.AsyncModulationData.CarrierWaveData.VibratoData.Highest.Mode = mode;
                SetSelected(0, mode);
            }
            else if (tag.Equals("Lowest"))
            {
                Target.AsyncModulationData.CarrierWaveData.VibratoData.Lowest.Mode = mode;
                SetSelected(1, mode);
            }
            else if (tag.Equals("Interval"))
            {
                Target.AsyncModulationData.CarrierWaveData.VibratoData.Interval.Mode = mode;
                SetSelected(2, mode);
            }

        }
        private void BaseWave_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            ComboBox cb = (ComboBox)sender;

            Target.AsyncModulationData.CarrierWaveData.VibratoData.BaseWave = (YamlAsyncParameterVibratoBaseWaveType)cb.SelectedValue;
        }

        /// <summary>
        /// 0 => highest
        /// 1 => lowest
        /// 2 => interval
        /// </summary>
        /// <param name="cate"></param>
        /// <param name="mode"></param>
        private void SetSelected(int cate, YamlAsyncParameterVibratoMode mode)
        {
            if (cate == 0)
            {
                if(mode == YamlAsyncParameterVibratoMode.Const)
                    highest_param_frame.Navigate(new ControlConstSetting(Target.AsyncModulationData.CarrierWaveData.VibratoData.Highest.GetType(), Target.AsyncModulationData.CarrierWaveData.VibratoData.Highest));
                else
                    highest_param_frame.Navigate(new ControlMovingSetting(Target.AsyncModulationData.CarrierWaveData.VibratoData.Highest.MovingValue));
            }
            else if(cate == 1)
            {
                if (mode == YamlAsyncParameterVibratoMode.Const)
                    lowest_param_frame.Navigate(new ControlConstSetting(Target.AsyncModulationData.CarrierWaveData.VibratoData.Lowest.GetType(), Target.AsyncModulationData.CarrierWaveData.VibratoData.Lowest));
                else
                    lowest_param_frame.Navigate(new ControlMovingSetting(Target.AsyncModulationData.CarrierWaveData.VibratoData.Lowest.MovingValue));
            }
            else if (cate == 2)
            {
                if (mode == YamlAsyncParameterVibratoMode.Const)
                    interval_mode_frame.Navigate(new ControlConstSetting(Target.AsyncModulationData.CarrierWaveData.VibratoData.Interval.GetType(), Target.AsyncModulationData.CarrierWaveData.VibratoData.Interval));
                else
                    interval_mode_frame.Navigate(new ControlMovingSetting(Target.AsyncModulationData.CarrierWaveData.VibratoData.Interval.MovingValue));
            }
        }
    }
}
