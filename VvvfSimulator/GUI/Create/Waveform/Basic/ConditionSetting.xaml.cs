using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// ConditionSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class ConditionSetting : Page
    {
        private readonly YamlControlData Target;
        private readonly bool IgnoreUpdate = true;
        private readonly WaveformEditor Editor;

        public ConditionSetting(WaveformEditor Editor, YamlControlData ControlData)
        {
            InitializeComponent();
            Target = ControlData;
            this.Editor = Editor;
            InitializeView();
            IgnoreUpdate = false;
        }

        private void InitializeView()
        {
            from_text_box.Text = Target.ControlFrequencyFrom.ToString();
            sine_from_text_box.Text = Target.RotateFrequencyFrom.ToString();
            sine_below_text_box.Text = Target.RotateFrequencyBelow.ToString();

            Keep_FreeRun_On_Check.IsChecked = Target.StuckFreeRunOn;
            Keep_FreeRun_Off_Check.IsChecked = Target.StuckFreeRunOff;

            Enable_FreeRun_On_Check.IsChecked = Target.EnableFreeRunOn;
            Enable_FreeRun_Off_Check.IsChecked = Target.EnableFreeRunOff;
            Enable_Normal_Check.IsChecked = Target.EnableNormal;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("From"))
            {
                double parsed = ParseTextBox.ParseDouble(tb);
                Target.ControlFrequencyFrom = parsed;
                MainWindow.GetInstance()?.UpdateControlList();
            }
            else if (tag.Equals("SineFrom"))
            {
                double parsed = ParseTextBox.ParseDouble(tb);
                Target.RotateFrequencyFrom = parsed;
                MainWindow.GetInstance()?.UpdateControlList();
            }
            else if (tag.Equals("SineBelow"))
            {
                double parsed = ParseTextBox.ParseDouble(tb);
                Target.RotateFrequencyBelow = parsed;
                MainWindow.GetInstance()?.UpdateControlList();
            }
        }

        private void CheckedChanged(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            CheckBox tb = (CheckBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;
            string[] Tags = tag_str.Split('.');

            bool check = tb.IsChecked != false;

            if (Tags[0].Equals("Enable"))
            {
                if (Tags[1].Equals("Normal"))
                    Target.EnableNormal = check;
                else if (Tags[1].Equals("JerkOn"))
                    Target.EnableFreeRunOn = check;
                else if (Tags[1].Equals("JerkOff"))
                    Target.EnableFreeRunOff = check;
            }else if (Tags[0].Equals("Keep"))
            {
                if (Tags[1].Equals("JerkOn"))
                    Target.StuckFreeRunOn = check;
                else if (Tags[1].Equals("JerkOff"))
                    Target.StuckFreeRunOff = check;
            }

            

            MainWindow.GetInstance()?.UpdateControlList();
            MainWindow.GetInstance()?.UpdateContentSelected();
            return;
        }

        private void Open_Harmonic_Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetInteractive(false);
            ControlBasicHarmonic cbh = new(MainWindow.GetInstance(), Target.PulseMode);
            cbh.ShowDialog();
            MainWindow.SetInteractive(true);
        }

        private void Open_Discrete_Setting_Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.SetInteractive(false);
            DiscreteSettingWindow discreteSetting = new(MainWindow.GetInstance(), Target.PulseMode);
            discreteSetting.ShowDialog();
            MainWindow.SetInteractive(true);
        }
    }
}
