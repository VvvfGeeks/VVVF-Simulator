using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace VvvfSimulator.GUI.Simulator.RealTime.Setting
{
    /// <summary>
    /// ComPortSelector.xaml の相互作用ロジック
    /// </summary>
    public partial class ComPortSelector : Window
    {
        public ComPortSelector(Window Owner)
        {
            this.Owner = Owner;
            InitializeComponent();
            SetComPorts();
        }

        public void SetComPorts()
        {
            string[] ports = SerialPort.GetPortNames();
            PortSelector.ItemsSource = ports;
            if (ports.Length > 0) PortSelector.SelectedIndex = 0;
        }

        public string GetComPortName()
        {
            return (string)PortSelector.SelectedValue;
        }

        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
            string? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("Close"))
                Close();
        }
    }
}
