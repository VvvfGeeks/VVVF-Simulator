using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using static VvvfSimulator.Yaml.MasconControl.YamlMasconAnalyze.YamlMasconData;

namespace VvvfSimulator.GUI.Mascon
{
    /// <summary>
    /// EditPage.xaml の相互作用ロジック
    /// </summary>
    public partial class EditPage : Page
    {
        private readonly YamlMasconDataPoint data;
        private readonly bool no_update = true;
        private readonly ControlEditor main_viewer;
        public EditPage(ControlEditor main,YamlMasconDataPoint ympd)
        {
            InitializeComponent();

            data = ympd;
            main_viewer = main;

            UpdateView();

            no_update = false;
        }

        private void UpdateView()
        {
            order_box.Text = data.order.ToString();
            duration_box.Text = data.duration.ToString();
            rate_box.Text = data.rate.ToString();

            is_brake.IsChecked = data.brake;
            is_mascon_on.IsChecked = data.mascon_on;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            TextBox tb = (TextBox)sender;
            Object tag = tb.Tag;

            if (tag.Equals("Duration"))
            {
                double d = ParseTextBox.ParseDouble(tb);
                data.duration = d;
                main_viewer.UpdateItemList();
            }
            else if (tag.Equals("Rate"))
            {
                double d = ParseTextBox.ParseDouble(tb);
                data.rate = d;
                main_viewer.UpdateItemList();
            }
            else if (tag.Equals("Order"))
            {
                int d = ParseTextBox.ParseInt(tb,0);
                data.order = d;
                main_viewer.UpdateItemList();
            }
        }

        private void Check_Changed(object sender, RoutedEventArgs e)
        {
            if (no_update) return;
            CheckBox cb = (CheckBox)sender;
            Object tag = cb.Tag;

            bool is_checked = cb.IsChecked == true;

            if (tag.Equals("Mascon"))
                data.mascon_on = is_checked;
            else if (tag.Equals("Brake"))
                data.brake = is_checked;
            main_viewer.UpdateItemList();
        }
    }
}
