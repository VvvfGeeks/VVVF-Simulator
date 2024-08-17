using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;

namespace VvvfSimulator.GUI.TrainAudio.Pages.Mixer
{
    /// <summary>
    /// Volume.xaml の相互作用ロジック
    /// </summary>
    public partial class Volume : Page
    {
        readonly YamlTrainSoundData data;
        public Volume(YamlTrainSoundData data)
        {
            this.data = data;
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            MasterVolume.Value = data.TotalVolumeDb;
            MasterVolumeValue.Text = data.TotalVolumeDb.ToString("F2");
            MotorVolume.Value = data.MotorVolumeDb;
            MotorVolumeValue.Text = data.MotorVolumeDb.ToString("F2");
            EnableFrequencyFilter.SetToggled(data.UseFilteres);
            EnableConvolutionFilter.SetToggled(data.UseConvolutionFilter);
        }

        private bool IgnoreSliderEvent = false;
        private void SliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (IgnoreSliderEvent) return;
            Slider? slider = sender as Slider;
            if (slider == null) return;
            string? tag = slider.Tag.ToString();
            if(tag == null) return;

            double value = (double)e.NewValue;

            switch (tag)
            {
                case "MasterVolume":
                    {
                        data.TotalVolumeDb = value;
                        MasterVolumeValue.Text = value.ToString("F2");
                    }
                    break;
                case "MotorVolume":
                    {
                        data.MotorVolumeDb = value;
                        MotorVolumeValue.Text = value.ToString("F2");
                    }
                    break;
                default:
                    break;

            }            
        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            TextBox? box = sender as TextBox;
            if (box == null) return;
            string? tag = box.Tag.ToString();
            if (tag == null) return;

            switch (tag)
            {
                case "MasterVolume":
                    {
                        double value = ParseTextBox.ParseDouble(box);
                        data.TotalVolumeDb = value;
                        IgnoreSliderEvent = true;
                        MasterVolume.Value = value;
                        IgnoreSliderEvent = false;
                    }
                    break;
                case "MotorVolume":
                    {
                        double value = ParseTextBox.ParseDouble(box);
                        data.MotorVolumeDb = value;
                        IgnoreSliderEvent = true;
                        MotorVolume.Value = value;
                        IgnoreSliderEvent = false;
                    }
                    break;
                default:
                    break;

            }
        }

        private void EnableFrequencyFilter_OnClicked(object sender, EventArgs e)
        {
            data.UseFilteres = EnableFrequencyFilter.IsToggled();
        }

        private void EnableConvolutionFilter_OnClicked(object sender, EventArgs e)
        {
            data.UseConvolutionFilter = EnableConvolutionFilter.IsToggled();
        }

        
    }
}
