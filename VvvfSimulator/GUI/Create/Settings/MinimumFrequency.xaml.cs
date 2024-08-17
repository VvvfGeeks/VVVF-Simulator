using System;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.Yaml.VvvfSound;

namespace VvvfSimulator.GUI.Create.Settings
{
    /// <summary>
    /// minimum_freq_setting.xaml の相互作用ロジック
    /// </summary>
    public partial class MinimumFrequency : Page
    {
        public MinimumFrequency()
        {
            InitializeComponent();

            accelerate_min_freq_box.Text = YamlVvvfManage.CurrentData.MinimumFrequency.Accelerating.ToString();
            braking_min_freq_box.Text = YamlVvvfManage.CurrentData.MinimumFrequency.Braking.ToString();
        }
        private void ValueChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = (TextBox) sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("Accelerate"))
                YamlVvvfManage.CurrentData.MinimumFrequency.Accelerating = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("Brake"))
                YamlVvvfManage.CurrentData.MinimumFrequency.Braking = ParseTextBox.ParseDouble(tb);
        }
    }
}
