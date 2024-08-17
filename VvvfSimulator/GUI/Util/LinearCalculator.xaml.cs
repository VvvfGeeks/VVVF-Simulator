using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;

namespace VvvfSimulator.GUI.Util
{
    /// <summary>
    /// Linear_Calculator.xaml の相互作用ロジック
    /// </summary>
    public partial class LinearCalculator : Window
    {
        public LinearCalculator()
        {
            InitializeComponent();

            SetResult(a_textbox);
            SetResult(x_textbox);
            SetResult(b_textbox);

            IgnoreUpdate = false;
        }

        private double A = 0, X = 0, B = 0;
        private bool IgnoreUpdate = true;
        private void CopyButtonClicked(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetData(DataFormats.Text, ans_textbox.Text);
            }
            catch { }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;
            TextBox tb = (TextBox)sender;
            SetResult(tb);
        }

        private void SetResult(TextBox tb)
        {
            Object tag = tb.Tag;
            double d = ParseTextBox.ParseDouble(tb);
            if (tag.Equals("A")) A = d;
            else if (tag.Equals("X")) X = d;
            else B = d;

            ans_textbox.Text = (A * X + B).ToString();
        }

        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
            string? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("Close"))
                Close();
            else if (tag.Equals("Maximize"))
            {
                if (WindowState.Equals(WindowState.Maximized))
                    WindowState = WindowState.Normal;
                else
                    WindowState = WindowState.Maximized;
            }
            else if (tag.Equals("Minimize"))
                WindowState = WindowState.Minimized;
        }
    }
}
