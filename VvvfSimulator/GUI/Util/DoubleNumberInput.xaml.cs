using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;

namespace VvvfSimulator.GUI.Util
{
    /// <summary>
    /// Double_Ask_Form.xaml の相互作用ロジック
    /// </summary>
    public partial class DoubleNumberInput : Window
    {
        private readonly double LeastValue = 0.0;
        private readonly double DefaultValue = 0.0;
        public DoubleNumberInput(Window Owner,string Title, double LeastValue = 0.0,double DefaultValue=10.0)
        {
            this.Owner = Owner;
            this.LeastValue = LeastValue;
            this.DefaultValue = DefaultValue;
            InitializeComponent();
            DescriptionBox.Content = Title;
            NumberEnterBox.Text = DefaultValue.ToString();
            ShowDialog();
        }

        private double EnteredValue = 0.0;
        private bool EnteredValueValid = false;
        public double GetEnteredValue()
        {
            return EnteredValue;
        }
        public bool IsEnteredValueValid()
        {
            return EnteredValueValid;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextBox tb = NumberEnterBox;
            double d = ParseTextBox.ParseDouble(tb, LeastValue, DefaultValue);
            EnteredValue = d;
            EnteredValueValid = true;
            Close();
        }

        private void NumberEnterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox tb = NumberEnterBox;
            ParseTextBox.ParseDouble(tb);
        }

        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
            string? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("Close"))
            {
                Close();
            }
                
        }
    }
}
