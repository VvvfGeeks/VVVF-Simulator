using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Create.Waveform.Basic;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.AmplitudeValue;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.AmplitudeValue.Parameter;

namespace VvvfSimulator.GUI.Create.Waveform
{
    /// <summary>
    /// Control_Amplitude.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlAmplitude : UserControl
    {
        private readonly Parameter Context;
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

        public ControlAmplitude(WaveformEditor Editor, Parameter ycd, ControlAmplitudeContent cac)
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

            start_freq_box.Text = Context.StartFrequency.ToString();
            start_amp_box.Text = Context.StartAmplitude.ToString();
            end_freq_box.Text = Context.EndFrequency.ToString();
            end_amp_box.Text = Context.EndAmplitude.ToString();
            cutoff_amp_box.Text = Context.CutOffAmplitude.ToString();
            max_amp_box.Text = Context.MaxAmplitude.ToString();
            polynomial_box.Text = Context.Polynomial.ToString();
            curve_rate_box.Text = Context.CurveChangeRate.ToString();
            DisableRateLimitCheckBox.IsChecked = Context.DisableRangeLimit;
            AmplitudeTableInterpolationCheck.IsChecked = Context.AmplitudeTableInterpolation;

            UpdateVisibility();
        }
        private void UpdateVisibility()
        {
            SetParameterVisibility(Context.Mode);
            SetFunctionParameterGridsVisibility(Context.Mode, ContextType);
        }
        private void SetParameterVisibility(ValueMode Mode)
        {
            TableParameter.Visibility = Mode switch
            {
                ValueMode.Table => Visibility.Visible,
                _ => Visibility.Collapsed,
            };

            FunctionParameter.Visibility = Mode switch
            {
                ValueMode.Table => Visibility.Collapsed,
                _ => Visibility.Visible,
            };
        }
        private void SetFunctionParameterGridsVisibility(ValueMode mode, ControlAmplitudeContent cac)
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

            if (mode == ValueMode.Linear)
                condition_1 = [true, true, true, true, true, true, false, false, true];
            else if (mode == ValueMode.InverseProportional)
                condition_1 = [true, true, true, true, true, true, false, true, true];
            else if (mode == ValueMode.Exponential)
                condition_1 = [false, false, true, true, true, true, false, false, true];
            else if (mode == ValueMode.LinearPolynomial)
                condition_1 = [false, false, true, true, true, true, true, false, true];
            else
                condition_1 = [false, false, true, true, true, true, false, false, true];

            if (cac == ControlAmplitudeContent.Default)
                condition_2 = [true, true, true, true, true, true, true, true, true];
            else if (cac == ControlAmplitudeContent.PowerOn)
                condition_2 = [true, true, true, true, true, true, true, true, true];
            else
                condition_2 = [true, true, true, true, true, true, true, true, true];

            for (int i = 0; i < 9; i++)
            {
                _SetVisiblity(i, (condition_1[i] && condition_2[i]));

            }
        }
        private void TextboxUpdate(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("start_freq"))
                Context.StartFrequency = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("start_amp"))
                Context.StartAmplitude = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end_freq"))
                Context.EndFrequency = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end_amp"))
                Context.EndAmplitude = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("cutoff_amp"))
                Context.CutOffAmplitude = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("max_amp"))
                Context.MaxAmplitude = ParseTextBox.ParseDouble(tb);
            else if(tag.Equals("curve_rate"))
                Context.CurveChangeRate = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("polynomial"))
                Context.Polynomial = ParseTextBox.ParseDouble(tb);
        }
        private void CheckBoxUpdate(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;
            CheckBox Box = (CheckBox)sender;
            string Name = Box.Name;
            bool Checked = Box.IsChecked ?? false;

            if (Name.Equals("DisableRateLimitCheckBox")) Context.DisableRangeLimit = Checked;
            else if (Name.Equals("AmplitudeTableInterpolationCheck")) Context.AmplitudeTableInterpolation = Checked;
        }        
        private void AmplitudeModeSelectorUpdate(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            Context.Mode = (ValueMode)amplitude_mode_selector.SelectedValue;
            UpdateVisibility();
        }
        private void ButtonClicked(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;
            Button Btn = (Button)sender;
            string Name = Btn.Name;

            if(Name.Equals("OpenAmplitudeTableEditorButton"))
            {
                MainWindow.SetInteractive(false);
                new AmplitudeTableEditor(MainWindow.GetInstance(), Context).ShowDialog();
                MainWindow.SetInteractive(true);
            }
        }
    }
    public enum ControlAmplitudeContent
    {
        Default,PowerOn,PowerOff
    }
}
