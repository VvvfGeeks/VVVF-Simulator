using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.VvvfStructs;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// DiscreteSetting.xaml の相互作用ロジック
    /// </summary>
    public partial class DiscreteSettingWindow : Window
    {
        private readonly PulseMode pulseMode;
        private readonly bool ignoreUpdate = true;
        public DiscreteSettingWindow(Window? owner, PulseMode pulseMode)
        {
            Owner = owner;
            InitializeComponent();
            this.pulseMode = pulseMode;
            ModeComboBox.ItemsSource = FriendlyNameConverter.GetDiscreteTimeModeNames();
            SetStatus();
            ignoreUpdate = false;
        }
        private void SetStatus()
        {
            EnabledCheckBox.IsChecked = pulseMode.DiscreteTime.Enabled;
            StepsInput.Text = pulseMode.DiscreteTime.Steps.ToString();
            ModeComboBox.SelectedValue = pulseMode.DiscreteTime.Mode;
        }

        private void EnabledCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (ignoreUpdate) return;
            pulseMode.DiscreteTime.Enabled = EnabledCheckBox.IsChecked ?? false;
        }

        private void StepsInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreUpdate) return;
            pulseMode.DiscreteTime.Steps = ParseTextBox.ParseInt(StepsInput);
        }

        private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ignoreUpdate) return;
            pulseMode.DiscreteTime.Mode = (PulseMode.DiscreteTimeConfiguration.DiscreteTimeMode)ModeComboBox.SelectedValue;
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
