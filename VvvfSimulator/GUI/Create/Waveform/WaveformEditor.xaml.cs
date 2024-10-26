using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using VvvfSimulator.GUI.Create.Waveform.Basic;
using VvvfSimulator.GUI.Create.Waveform.Common;
using VvvfSimulator.GUI.Resource.Class;
using static VvvfSimulator.Vvvf.Struct;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlPulseMode;
using UserControl = System.Windows.Controls.UserControl;

namespace VvvfSimulator.GUI.Create.Waveform
{
    /// <summary>
    /// Level_3_Page_Control_Common_Async.xaml の相互作用ロジック
    /// </summary>
    public partial class WaveformEditor : Page
    {
        private readonly YamlControlData Control;
        private readonly int Level;
        private readonly ViewModel viewModel = new();
        private partial class ViewModel : ViewModelBase
        {
            private Visibility _PulseSettingVisible;
            public Visibility PulseSettingVisible { get { return _PulseSettingVisible; } set { _PulseSettingVisible = value; RaisePropertyChanged(nameof(PulseSettingVisible)); } }

            private Visibility _ConditionSettingVisible;
            public Visibility ConditionSettingVisible { get { return _ConditionSettingVisible; } set { _ConditionSettingVisible = value; RaisePropertyChanged(nameof(ConditionSettingVisible)); } }

            private Visibility _AmplitudeDefaultVisible;
            public Visibility AmplitudeDefaultVisible { get { return _AmplitudeDefaultVisible; } set { _AmplitudeDefaultVisible = value; RaisePropertyChanged(nameof(AmplitudeDefaultVisible)); } }

            private Visibility _AmplitudePowerOnVisible;
            public Visibility AmplitudePowerOnVisible { get { return _AmplitudePowerOnVisible; } set { _AmplitudePowerOnVisible = value; RaisePropertyChanged(nameof(AmplitudePowerOnVisible)); } }

            private Visibility _AmplitudePowerOffVisible;
            public Visibility AmplitudePowerOffVisible { get { return _AmplitudePowerOffVisible; } set { _AmplitudePowerOffVisible = value; RaisePropertyChanged(nameof(AmplitudePowerOffVisible)); } }

            private Visibility _PulseDataVisible;
            public Visibility PulseDataVisible { get { return _PulseDataVisible; } set { _PulseDataVisible = value; RaisePropertyChanged(nameof(PulseDataVisible)); } }

            private Visibility _AsyncVisible;
            public Visibility AsyncVisible { get { return _AsyncVisible; } set { _AsyncVisible = value; RaisePropertyChanged(nameof(AsyncVisible)); } }
        }

        public void UpdateVisibility()
        {
            PulseTypeName mode = Control.PulseMode.PulseType;

            viewModel.ConditionSettingVisible = Visibility.Visible;
            viewModel.PulseSettingVisible = Visibility.Visible;
            viewModel.PulseDataVisible = PulseDataSettings.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
            viewModel.AsyncVisible = mode == PulseTypeName.ASYNC ? Visibility.Visible : Visibility.Collapsed;
            viewModel.AmplitudeDefaultVisible = Visibility.Visible;
            viewModel.AmplitudePowerOnVisible = Control.EnableFreeRunOn ? Visibility.Visible : Visibility.Collapsed;
            viewModel.AmplitudePowerOffVisible = Control.EnableFreeRunOff ? Visibility.Visible : Visibility.Collapsed;
        }

        public void SetContent()
        {
            ConditionSetting.Navigate(new ConditionSetting(this, Control));
            PulseSetting.Navigate(new PulseSetting(this, Control, Level));
            AsyncSetting.Navigate(new ControlAsync(this, Control));
            Control_Amplitude_Default.Navigate(new ControlAmplitude(this, Control.Amplitude.Default, ControlAmplitudeContent.Default));
            Control_Amplitude_FreeRun_On.Navigate(new ControlAmplitude(this, Control.Amplitude.PowerOn, ControlAmplitudeContent.PowerOn));
            Control_Amplitude_FreeRun_Off.Navigate(new ControlAmplitude(this, Control.Amplitude.PowerOff, ControlAmplitudeContent.PowerOff));
        }

        public void SetPulseDataContent()
        {
            PulseDataKey[] PulseDataKeys = PulseModeConfiguration.GetAvailablePulseDataKey(Control.PulseMode, Level);
            PulseDataSettings.Children.Clear();
            foreach (PulseDataKey Key in Control.PulseMode.PulseData.Keys)
            {
                if (!PulseDataKeys.Contains(Key)) Control.PulseMode.PulseData.Remove(Key);
            }
            for (int i = 0; i < PulseDataKeys.Length; i++)
            {
                UserControl PulseDataEditor = new PulseDataSetting(this, Control.PulseMode.PulseData, PulseDataKeys[i])
                {
                    Margin = new Thickness(0, 2, 0, 0)
                };
                PulseDataSettings.Children.Add(PulseDataEditor);
            }
        }

        public WaveformEditor(YamlControlData Control, int Level)
        {
            this.Control = Control;
            this.Level = Level;
            InitializeComponent();
            DataContext = viewModel;

            SetContent();
            SetPulseDataContent();
            UpdateVisibility();
        }
    }
}
