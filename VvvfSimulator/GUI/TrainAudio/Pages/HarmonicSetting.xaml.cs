using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;

namespace VvvfSimulator.GUI.TrainAudio.Pages
{
    /// <summary>
    /// TrainAudio_Harmonic_Setting_Page.xaml の相互作用ロジック
    /// </summary>
    public partial class HarmonicSetting : Page
    {
        YamlTrainSoundData.HarmonicData Harmonic_Data;
        ListView ListView;
        bool no_update = true;
        public HarmonicSetting(YamlTrainSoundData.HarmonicData harmonic_Data, ListView listView)
        {
            Harmonic_Data = harmonic_Data;
            ListView = listView;
            InitializeComponent();

            Harmonic.Text = Harmonic_Data.Harmonic.ToString();
            Disappear_Frequency.Text = Harmonic_Data.Disappear.ToString();
            Range_Start_Frequency.Text = Harmonic_Data.Range.Start.ToString();
            Range_End_Frequency.Text = Harmonic_Data.Range.End.ToString();
            Start_Frequency.Text = Harmonic_Data.Amplitude.Start.ToString();
            End_Frequency.Text = Harmonic_Data.Amplitude.End.ToString();
            Start_Amplitude.Text = Harmonic_Data.Amplitude.StartValue.ToString();
            End_Amplitude.Text = Harmonic_Data.Amplitude.EndValue.ToString();
            Max_Amplitude.Text = Harmonic_Data.Amplitude.MaximumValue.ToString();
            Min_Amplitude.Text = Harmonic_Data.Amplitude.MinimumValue.ToString();

            no_update = false;
        }

        public void TextBox_TextChanged(object sender,RoutedEventArgs e)
        {
            if (no_update) return;

            TextBox tb = (TextBox)sender;
            String name = tb.Name;
            double d = ParseTextBox.ParseDouble(tb);

            if (ListView != null)
                ListView.Items.Refresh();

            if (name.Equals("Harmonic")) Harmonic_Data.Harmonic = d;
            else if (name.Equals("Disappear_Frequency")) Harmonic_Data.Disappear = d;
            else if (name.Equals("Range_Start_Frequency")) Harmonic_Data.Range.Start = d;
            else if (name.Equals("Range_End_Frequency")) Harmonic_Data.Range.End = d;
            else if (name.Equals("Start_Frequency")) Harmonic_Data.Amplitude.Start = d;
            else if (name.Equals("End_Frequency")) Harmonic_Data.Amplitude.End = d;
            else if (name.Equals("Start_Amplitude")) Harmonic_Data.Amplitude.StartValue = d;
            else if (name.Equals("End_Amplitude")) Harmonic_Data.Amplitude.EndValue = d;
            else if (name.Equals("Max_Amplitude")) Harmonic_Data.Amplitude.MaximumValue = d;
            else if (name.Equals("Min_Amplitude")) Harmonic_Data.Amplitude.MinimumValue = d;
        }

    }
}
