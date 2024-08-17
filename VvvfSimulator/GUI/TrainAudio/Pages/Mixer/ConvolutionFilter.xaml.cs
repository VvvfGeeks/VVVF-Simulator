using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Mixer
{
    /// <summary>
    /// ConvolutionFilter.xaml の相互作用ロジック
    /// </summary>
    public partial class ConvolutionFilter : Page
    {
        readonly YamlTrainSoundData data;
        public ConvolutionFilter(YamlTrainSoundData data)
        {
            this.data = data;
            InitializeComponent();
        }

        private void OnLoadButtonClick(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Wav (*.wav)|*.wav|All (*.*)|*.*",
            };
            if (dialog.ShowDialog() == false)
            {
                data.ImpulseResponse = Generation.Audio.TrainSound.AudioResourceManager.ReadResourceAudioFileSample(Generation.Audio.TrainSound.AudioResourceManager.SampleIrPath);
                MessageBox.Show(LanguageManager.GetString("TrainAudio.Pages.Mixer.ConvolutionFilter.Message.Load.Reset"), LanguageManager.GetString("Generic.Title.Info"), MessageBoxButton.OK, MessageBoxImage.Information); return;
            }

            string path = dialog.FileName;
            try
            {
                data.ImpulseResponse = Generation.Audio.TrainSound.AudioResourceManager.ReadAudioFileSample(path);
                MessageBox.Show(LanguageManager.GetString("TrainAudio.Pages.Mixer.ConvolutionFilter.Message.Load.Ok"), LanguageManager.GetString("Generic.Title.Info"), MessageBoxButton.OK, MessageBoxImage.Information); return;
            } catch (Exception ex)
            {
                MessageBox.Show(ex.Message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error); return;
            }
        }
    }
}
