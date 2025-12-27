using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.Data.Vvvf;
using static VvvfSimulator.Data.Vvvf.Struct.JerkSettings.Jerk;

namespace VvvfSimulator.GUI.Create.Settings
{
    /// <summary>
    /// Jerk.xaml の相互作用ロジック
    /// </summary>
    public partial class Jerk : Page
    {
        private class Controller : INotifyPropertyChanged
        {
            private bool _IsAccelerateActive = true;
            public bool IsAccelerateActive { get { return _IsAccelerateActive; } set { _IsAccelerateActive = value; RaisePropertyChanged(nameof(IsAccelerateActive)); } }

            private bool _IsTurnOnActive = true;
            public bool IsTurnOnActive
            {
                get { return _IsTurnOnActive; }
                set { _IsTurnOnActive = value; RaisePropertyChanged(nameof(IsTurnOnActive)); }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected virtual void RaisePropertyChanged(string propertyName)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        public Jerk()
        {
            IgnoreUpdateValue = true;
            InitializeComponent();
            DataContext = new Controller();
            SetView();
            IgnoreUpdateValue = false;
        }

        public void SetView()
        {
            Controller dc = (Controller)this.DataContext;
            Struct.JerkSettings Setting = Manager.Current.JerkSetting;
            Struct.JerkSettings.Jerk pattern = dc.IsAccelerateActive ? Setting.Accelerating : Setting.Braking;
            JerkInfo mode = dc.IsTurnOnActive ? pattern.On : pattern.Off;
            MaxVoltageFreqInput.Text = mode.MaxControlFrequency.ToString();
            FreqChangeRateInput.Text = mode.FrequencyChangeRate.ToString();
        }

        bool IgnoreUpdateValue = false;
        private void ValueUpdated(object sender, TextChangedEventArgs e)
        {
            Controller dc = (Controller)this.DataContext;
            if (dc == null) return;
            if (IgnoreUpdateValue) return;

            Struct.JerkSettings Setting = Manager.Current.JerkSetting;
            Struct.JerkSettings.Jerk pattern = dc.IsAccelerateActive ? Setting.Accelerating : Setting.Braking;
            JerkInfo mode = dc.IsTurnOnActive ? pattern.On : pattern.Off;
            mode.MaxControlFrequency = ParseTextBox.ParseDouble(MaxVoltageFreqInput);
            mode.FrequencyChangeRate = ParseTextBox.ParseDouble(FreqChangeRateInput);
        }

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            
            Button button = (Button)sender;
            Controller dc = (Controller)this.DataContext;

            IgnoreUpdateValue = true;

            string name = button.Name;
            if (name.Equals("ButtonModeAccelerate")) dc.IsAccelerateActive = true;
            else if (name.Equals("ButtonModeBrake")) dc.IsAccelerateActive = false;
            if (name.Equals("ButtonTurnOn")) dc.IsTurnOnActive = true;
            else if (name.Equals("ButtonTurnOff")) dc.IsTurnOnActive = false;
            SetView();

            IgnoreUpdateValue = false;

        }
    }
}
