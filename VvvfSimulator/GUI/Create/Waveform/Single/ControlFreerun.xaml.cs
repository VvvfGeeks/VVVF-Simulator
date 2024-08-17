using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VvvfSimulator;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlFreeRunCondition;

namespace VvvfSimulator.GUI.Create.Waveform
{
    /// <summary>
    /// Control_When_FreeRun.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlFreeRun : UserControl
    {
        YamlControlData target;
        private bool no_update = true;

        public ControlFreeRun(YamlControlData ycd)
        {
            InitializeComponent();
            target = ycd;
            apply_view();

            no_update = false;
        }

        private void apply_view()
        {
            on_skip.IsChecked = target.FreeRunCondition.On.Skip;
            off_skip.IsChecked = target.FreeRunCondition.Off.Skip;

            on_stuck.IsChecked = target.FreeRunCondition.On.StuckAtHere;
            off_stuck.IsChecked = target.FreeRunCondition.Off.StuckAtHere;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (no_update) return;

            CheckBox cb = (CheckBox)sender;
            Object? tag = cb.Tag;
            if (tag == null) return;
            String? tag_str = tag.ToString();
            if (tag_str == null) return;

            String[] mode = tag_str.Split("_");

            YamlFreeRunConditionSingle condition;
            if (mode[0].Equals("ON")) condition = target.FreeRunCondition.On;
            else condition = target.FreeRunCondition.Off;

            bool is_cheked = cb.IsChecked != false;

            if (mode[1].Equals("Stuck"))
            {
                condition.StuckAtHere = is_cheked;
            }
            else
            {
                condition.Skip = is_cheked;
            }

            MainWindow.GetInstance()?.UpdateControlList();

        }
    }
}
