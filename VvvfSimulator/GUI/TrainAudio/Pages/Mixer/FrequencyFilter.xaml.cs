using System;
using System.Collections.Generic;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze.YamlTrainSoundData.SoundFilter;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Mixer
{
    /// <summary>
    /// TrainAudio_Filter_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class FrequencyFilter : Page
    {
        readonly YamlTrainSoundData data;
        public FrequencyFilter(YamlTrainSoundData train_Harmonic_Data)
        {
            data = train_Harmonic_Data;

            InitializeComponent();
            filterType_Selector.ItemsSource = FriendlyNameConverter.GetFrequencyFilterTypeNames();

            try
            {
                Filter_DataGrid.ItemsSource = data.Filteres;
            }
            catch
            {

            }
        }
    }
}
