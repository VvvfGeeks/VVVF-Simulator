using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Vvvf.Calculate;
using static VvvfSimulator.Yaml.VvvfSound.YamlVvvfSoundData.YamlControlData.YamlAmplitude;

namespace VvvfSimulator.GUI.Create.Waveform
{
    /// <summary>
    /// Control_Amplitude.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlAmplitude : UserControl
    {
        private readonly YamlControlDataAmplitude Context;
        private readonly ControlAmplitudeContent ContextType;
        private readonly WaveformEditor Editor;
        private readonly bool IgnoreUpdate = true;
        private readonly VisibleClass VisibleHandler;

        public class VisibleClass : ViewModelBase
        {
            private bool _start_freq_visible = true;
            public bool start_freq_visible { get { return _start_freq_visible; } set { _start_freq_visible = value; RaisePropertyChanged(nameof(start_freq_visible)); } }
            
            private bool _start_amp_visible = true;
            public bool start_amp_visible { get { return _start_amp_visible; } set { _start_amp_visible = value; RaisePropertyChanged(nameof(start_amp_visible)); } }

            private bool _end_freq_visible = true;
            public bool end_freq_visible { get { return _end_freq_visible; } set { _end_freq_visible = value; RaisePropertyChanged(nameof(end_freq_visible)); } }

            private bool _end_amp_visible = true;
            public bool end_amp_visible { get { return _end_amp_visible; } set { _end_amp_visible = value; RaisePropertyChanged(nameof(end_amp_visible)); } }

            private bool _cut_off_amp_visible = true;
            public bool cut_off_amp_visible { get { return _cut_off_amp_visible; } set { _cut_off_amp_visible = value; RaisePropertyChanged(nameof(cut_off_amp_visible)); } }

            private bool _max_amp_visible = true;
            public bool max_amp_visible { get { return _max_amp_visible; } set { _max_amp_visible = value; RaisePropertyChanged(nameof(max_amp_visible)); } }

            private bool _polynomial_visible = true;
            public bool polynomial_visible { get { return _polynomial_visible; } set { _polynomial_visible = value; RaisePropertyChanged(nameof(polynomial_visible)); } }

            private bool _curve_rate_visible = true;
            public bool curve_rate_visible { get { return _curve_rate_visible; } set { _curve_rate_visible = value; RaisePropertyChanged(nameof(curve_rate_visible)); } }

            private bool _disable_range_visible = true;
            public bool disable_range_visible { get { return _disable_range_visible; } set { _disable_range_visible = value; RaisePropertyChanged(nameof(disable_range_visible)); } }
        };

        public ControlAmplitude(WaveformEditor Editor, YamlControlDataAmplitude ycd, ControlAmplitudeContent cac)
        {
            Context = ycd;
            ContextType = cac;

            InitializeComponent();

            if (cac == ControlAmplitudeContent.Default)
                title.Content = LanguageManager.GetString("Create.Settings.Waveform.Basic.ControlAmplitude.Title.Default");

            else if (cac == ControlAmplitudeContent.PowerOn)
                title.Content = LanguageManager.GetString("Create.Settings.Waveform.Basic.ControlAmplitude.Title.On");

            else
                title.Content = LanguageManager.GetString("Create.Settings.Waveform.Basic.ControlAmplitude.Title.Off");

            VisibleHandler = new VisibleClass();
            DataContext = VisibleHandler;
            this.Editor = Editor;

            ApplySettingToView();

            IgnoreUpdate = false;
        }
        private void ApplySettingToView()
        {
            amplitude_mode_selector.ItemsSource = FriendlyNameConverter.GetAmplitudeModeNames();
            amplitude_mode_selector.SelectedValue = Context.Mode;

            start_freq_box.Text = Context.Parameter.StartFrequency.ToString();
            start_amp_box.Text = Context.Parameter.StartAmplitude.ToString();
            end_freq_box.Text = Context.Parameter.EndFrequency.ToString();
            end_amp_box.Text = Context.Parameter.EndAmplitude.ToString();
            cutoff_amp_box.Text = Context.Parameter.CutOffAmplitude.ToString();
            max_amp_box.Text = Context.Parameter.MaxAmplitude.ToString();
            polynomial_box.Text = Context.Parameter.Polynomial.ToString();
            curve_rate_box.Text = Context.Parameter.CurveChangeRate.ToString();
            disable_range_limit_check.IsChecked = Context.Parameter.DisableRangeLimit;

            SetGridVisibility(Context.Mode, ContextType);
        }
        private void TextboxUpdate(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("start_freq"))
                Context.Parameter.StartFrequency = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("start_amp"))
                Context.Parameter.StartAmplitude = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end_freq"))
                Context.Parameter.EndFrequency = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end_amp"))
                Context.Parameter.EndAmplitude = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("cutoff_amp"))
                Context.Parameter.CutOffAmplitude = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("max_amp"))
                Context.Parameter.MaxAmplitude = ParseTextBox.ParseDouble(tb);
            else if(tag.Equals("curve_rate"))
                Context.Parameter.CurveChangeRate = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("polynomial"))
                Context.Parameter.Polynomial = ParseTextBox.ParseDouble(tb);

            MainWindow.GetInstance()?.UpdateControlList();
        }
        private void CheckBoxUpdate(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            CheckBox cb = (CheckBox)sender;
            Context.Parameter.DisableRangeLimit = cb.IsChecked != false;
            MainWindow.GetInstance()?.UpdateControlList();
        }
        private readonly Dictionary<AmplitudeMode, string> AmplitudeModeSelection = [];
        
        private void AmplitudeModeSelectorUpdate(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            Context.Mode = (AmplitudeMode)amplitude_mode_selector.SelectedValue;
            SetGridVisibility(Context.Mode, ContextType);

            MainWindow.GetInstance()?.UpdateControlList();            
        }
        private void SetGridVisibility(AmplitudeMode mode , ControlAmplitudeContent cac)
        {
            void _SetVisiblity(int i, bool b)
            {
                if (i == 0) VisibleHandler.start_freq_visible = b;
                else if (i == 1) VisibleHandler.start_amp_visible = b;
                else if (i == 2) VisibleHandler.end_freq_visible = b;
                else if (i == 3) VisibleHandler.end_amp_visible = b;
                else if (i == 4) VisibleHandler.cut_off_amp_visible = b;
                else if (i == 5) VisibleHandler.max_amp_visible = b;
                else if (i == 6) VisibleHandler.polynomial_visible = b;
                else if (i == 7) VisibleHandler.curve_rate_visible = b;
                else VisibleHandler.disable_range_visible = b;
            }

            Boolean[] condition_1, condition_2;

            if (mode == AmplitudeMode.Linear)
                condition_1 = [true, true, true, true, true, true, false, false, true];
            else if (mode == AmplitudeMode.Wide_3_Pulse)
                condition_1 = [true, true, true, true, true, true, false, false, true];
            else if (mode == AmplitudeMode.Inv_Proportional)
                condition_1 = [true, true, true, true, true, true, false, true, true];
            else if (mode == AmplitudeMode.Exponential)
                condition_1 = [false, false, true, true, true, true, false, false, true];
            else if (mode == AmplitudeMode.Linear_Polynomial)
                condition_1 = [false, false, true, true, true, true, true, false, true];
            else
                condition_1 = [false, false, true, true, true, true, false, false, true];

            if (cac == ControlAmplitudeContent.Default)
                condition_2 = [true, true, true, true, true, true, true, true, true];
            else if (cac == ControlAmplitudeContent.PowerOn)
                condition_2 = [true, true, true, true, true, true, true, true, true];
            else
                condition_2 = [true, true, true, true, true, true, true, true, true];

            for (int i =0; i < 9; i++)
            {
                _SetVisiblity(i, (condition_1[i] && condition_2[i]));

            }
        }
    }
    public enum ControlAmplitudeContent
    {
        Default,PowerOn,PowerOff
    }
}
