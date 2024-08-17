using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.Yaml.VvvfSound;
using VvvfSimulator.GUI.Resource.Class;


namespace VvvfSimulator.GUI.Create.Settings
{
    /// <summary>
    /// level_setting.xaml の相互作用ロジック
    /// </summary>
    public partial class Level : Page
    {

        private readonly ViewData BindingData = new();
        public class ViewData : ViewModelBase
        {
            public int Level
            {
                get
                {
                    return YamlVvvfManage.CurrentData.Level;
                }
                set
                {
                    YamlVvvfManage.CurrentData.Level = value;
                    RaisePropertyChanged(nameof(Level));
                }
            }
        };
        public Level()
        {
            InitializeComponent();
            DataContext = BindingData;
        }
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            String? tag = btn.Tag.ToString();
            if (tag == null) return;
            BindingData.Level = int.Parse(tag);
        }
    }
}
