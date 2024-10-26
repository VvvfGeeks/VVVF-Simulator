using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;

namespace VvvfSimulator.GUI.Util
{
    /// <summary>
    /// Double_Ask_Form.xaml の相互作用ロジック
    /// </summary>
    public partial class IntegerNumberInput : Window
    {
        private readonly int LeastValue = 0;
        private readonly int DefaultValue = 0;
        public IntegerNumberInput(Window Owner,string Title, int LeastValue=0, int DefaultValue = 10)
        {
            this.Owner = Owner;
            this.LeastValue = LeastValue;
            this.DefaultValue = DefaultValue;
            InitializeComponent();
            DescriptionBox.Content = Title;
            NumberEnterBox.Text = DefaultValue.ToString();
            ShowDialog();
        }

        private int EnteredValue = 0;
        private bool EnteredValueValid = false;
        public int GetEnteredValue()
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
            int d = ParseTextBox.ParseInt(tb, LeastValue, DefaultValue);
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
