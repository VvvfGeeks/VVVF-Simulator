using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.Data.TrainAudio;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Mixer
{
    /// <summary>
    /// ConvolutionFilter.xaml の相互作用ロジック
    /// </summary>
    public partial class ConvolutionFilter : Page
    {
        readonly Struct data;
        public ConvolutionFilter(Struct data)
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
                Generation.Audio.TrainSound.AudioResourceManager.ReadResourceAudioFileSample(Generation.Audio.TrainSound.AudioResourceManager.SampleIrPath, out float[] Data, out int SampleRate);
                data.ImpulseResponse = Data;
                data.ImpulseResponseSampleRate = SampleRate;
                DialogBox.Show(LanguageManager.GetString("TrainAudio.Pages.Mixer.ConvolutionFilter.Message.Load.Reset"), LanguageManager.GetString("Generic.Title.Info"), [DialogBoxButton.Ok], DialogBoxIcon.Information); return;
            }

            string path = dialog.FileName;
            try
            {
                Generation.Audio.TrainSound.AudioResourceManager.ReadAudioFileSample(path, out float[] Data, out int SampleRate);
                data.ImpulseResponse = Data;
                data.ImpulseResponseSampleRate = SampleRate;
                DialogBox.Show(LanguageManager.GetString("TrainAudio.Pages.Mixer.ConvolutionFilter.Message.Load.Ok"), LanguageManager.GetString("Generic.Title.Info"), [DialogBoxButton.Ok], DialogBoxIcon.Information); return;
            } catch (Exception ex)
            {
                DialogBox.Show(ex.Message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error); return;
            }
        }
    }
}
