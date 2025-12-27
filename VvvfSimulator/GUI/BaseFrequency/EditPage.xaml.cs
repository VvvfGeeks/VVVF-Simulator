using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;

namespace VvvfSimulator.GUI.BaseFrequency
{
    /// <summary>
    /// EditPage.xaml の相互作用ロジック
    /// </summary>
    public partial class EditPage : Page
    {
        private readonly Data.BaseFrequency.Struct.Point data;
        private readonly bool no_update = true;
        private readonly SettingWindow main_viewer;
        public EditPage(SettingWindow main, Data.BaseFrequency.Struct.Point ympd)
        {
            InitializeComponent();

            data = ympd;
            main_viewer = main;

            UpdateView();

            no_update = false;
        }

        private void UpdateView()
        {
            order_box.Text = data.Order.ToString();
            duration_box.Text = data.Duration.ToString();
            rate_box.Text = data.Rate.ToString();

            is_brake.IsChecked = data.Brake;
            is_power_on.IsChecked = data.PowerOn;
        }

        private void TextChanged(object sender, TextChangedEventArgs e)
        {
            if (no_update) return;
            TextBox tb = (TextBox)sender;
            Object tag = tb.Tag;

            if (tag.Equals("Duration"))
            {
                double d = ParseTextBox.ParseDouble(tb);
                data.Duration = d;
                main_viewer.UpdateItemList();
            }
            else if (tag.Equals("Rate"))
            {
                double d = ParseTextBox.ParseDouble(tb);
                data.Rate = d;
                main_viewer.UpdateItemList();
            }
            else if (tag.Equals("Order"))
            {
                int d = ParseTextBox.ParseInt(tb,0);
                data.Order = d;
                main_viewer.UpdateItemList();
            }
        }

        private void Check_Changed(object sender, RoutedEventArgs e)
        {
            if (no_update) return;
            CheckBox cb = (CheckBox)sender;
            Object tag = cb.Tag;

            bool is_checked = cb.IsChecked == true;

            if (tag.Equals("PowerOn"))
                data.PowerOn = is_checked;
            else if (tag.Equals("Brake"))
                data.Brake = is_checked;
            main_viewer.UpdateItemList();
        }
    }
}
