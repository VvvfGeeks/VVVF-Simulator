using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Create.Waveform.Basic;
using VvvfSimulator.GUI.Create.Waveform.Dipolar;
using VvvfSimulator.GUI.Resource.Class;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData;

namespace VvvfSimulator.GUI.Create.Waveform
{
    /// <summary>
    /// Level_3_Page_Control_Common_Async.xaml の相互作用ロジック
    /// </summary>
    public partial class Top : Page
    {
        private ViewModel viewModel = new();
        private class ViewModel : ViewModelBase
        {
            private Visibility _basic;
            public Visibility basic { get { return _basic; } set { _basic = value; RaisePropertyChanged(nameof(basic)); } }

            private Visibility _freerun;
            public Visibility freerun { get { return _freerun; } set { _freerun = value; RaisePropertyChanged(nameof(freerun)); } }

            private Visibility _amp_def;
            public Visibility amp_def { get { return _amp_def; } set { _amp_def = value; RaisePropertyChanged(nameof(amp_def)); } }

            private Visibility _amp_free_on;
            public Visibility amp_free_on { get { return _amp_free_on; } set { _amp_free_on = value; RaisePropertyChanged(nameof(amp_free_on)); } }

            private Visibility _amp_free_off;
            public Visibility amp_free_off { get { return _amp_free_off; } set { _amp_free_off = value; RaisePropertyChanged(nameof(amp_free_off)); } }

            private Visibility _dipolar;
            public Visibility dipolar { get { return _dipolar; } set { _dipolar = value; RaisePropertyChanged(nameof(dipolar)); } }

            private Visibility _async;
            public Visibility async { get { return _async; } set { _async = value; RaisePropertyChanged(nameof(async)); } }
        }

        private void Set_Visibility(YamlControlData ycd , int level)
        {
            PulseModeName mode = ycd.PulseMode.PulseName;

            viewModel.basic = Visibility.Visible;
            viewModel.freerun = Visibility.Visible;
            viewModel.amp_def = Visibility.Visible;
            viewModel.amp_free_on = ycd.EnableFreeRunOn ? Visibility.Visible : Visibility.Collapsed;
            viewModel.amp_free_off = ycd.EnableFreeRunOff ? Visibility.Visible : Visibility.Collapsed;

            if (level == 3 && mode == PulseModeName.Async)
                viewModel.dipolar = Visibility.Visible;
            else
                viewModel.dipolar = Visibility.Collapsed;

            if (mode == PulseModeName.Async)
                viewModel.async = Visibility.Visible;
            else
                viewModel.async = Visibility.Collapsed;
        }

        public Top(YamlControlData ycd, int level)
        {
            InitializeComponent();
            DataContext = viewModel;
            Set_Visibility(ycd, level);

            Control_Basic.Navigate(new ControlBasic(ycd, level));
            Control_When_FreeRun.Navigate(new ControlFreeRun(ycd));

            Control_Amplitude_Default.Navigate(new ControlAmplitude(ycd.Amplitude.DefaultAmplitude, ControlAmplitudeContent.Default));

            Control_Amplitude_FreeRun_On.Navigate(new ControlAmplitude(ycd.Amplitude.FreeRunAmplitude.On, ControlAmplitudeContent.JerkOn));
            Control_Amplitude_FreeRun_Off.Navigate(new ControlAmplitude(ycd.Amplitude.FreeRunAmplitude.Off, ControlAmplitudeContent.JerkOff));

            Control_Dipolar.Navigate(new ControlDipolar(ycd));

            Control_Async.Navigate(new ControlAsync(ycd));            
        }
    }
}
