using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using FontFamily = System.Drawing.FontFamily;

namespace VvvfSimulator.GUI.Simulator.RealTime.Setting
{
    /// <summary>
    /// RealTime_Settings.xaml の相互作用ロジック
    /// </summary>
    public partial class Basic : Window
    {
        private readonly bool IgnoreUpdate = true;
        private readonly RealTimeBasicSettingMode _SettingType;

        public enum RealTimeBasicSettingMode
        {
            VVVF, Train
        }

        public Basic(Window owner,RealTimeBasicSettingMode SettingType)
        {
            _SettingType = SettingType;
            Owner = owner;
            InitializeComponent();
            SetControl();
            IgnoreUpdate = false;
        }

        private readonly Dictionary<RealtimeDisplay.ControlStatus.RealTimeControlStatStyle, string> ControlDesignList = new();
        
        private readonly Dictionary<RealtimeDisplay.Hexagon.RealTimeHexagonStyle, string> HexagonStyleList = new();
        
        private void InitializeCombobox()
        {
            SelectorControlDesign.ItemsSource = FriendlyNameConverter.GetRealTimeControlStatStyleNames();
            SelectorHexagonDesign.ItemsSource = FriendlyNameConverter.GetRealTimeHexagonStyleNames();
        }

        private void SetControl()
        {

            InitializeCombobox();

            var Prop = Properties.Settings.Default;
            if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
            {
                TextBuffSize.Text = Prop.RealTime_VVVF_BuffSize.ToString();
                BoxWaveFormLine.IsChecked = Prop.RealTime_VVVF_WaveForm_Line_Show;
                BoxWaveFormPhase.IsChecked = Prop.RealTime_VVVF_WaveForm_Phase_Show;
                BoxFFT.IsChecked = Prop.RealTime_VVVF_FFT_Show;
                BoxFS.IsChecked = Prop.RealTime_VVVF_FS_Show;
                BoxRealTimeEdit.IsChecked = Prop.RealTime_VVVF_EditAllow;

                BoxShowControl.IsChecked = Prop.RealTime_VVVF_Control_Show;
                BoxControlPrecise.IsChecked = Prop.RealTime_VVVF_Control_Precise;
                SelectorControlDesign.SelectedValue = (RealtimeDisplay.ControlStatus.RealTimeControlStatStyle)Prop.RealTime_VVVF_Control_Style;

                BoxShowHexagon.IsChecked = Prop.RealTime_VVVF_Hexagon_Show;
                BoxShowZeroVectorCicle.IsChecked = Prop.RealTime_VVVF_Hexagon_ZeroVector;
                SelectorHexagonDesign.SelectedValue = (RealtimeDisplay.Hexagon.RealTimeHexagonStyle)Prop.RealTime_VVVF_Hexagon_Style;

                SamplingFrequencyInput.Text = Prop.RealtimeVvvfSamplingFrequency.ToString();
                CalculateDivisionInput.Text = Prop.RealtimeVvvfCalculateDivision.ToString();

                WindowTitle.Content = LanguageManager.GetString("Simulator.RealTime.Setting.Basic.Title.Vvvf");
            }
            else
            {
                TextBuffSize.Text = Prop.RealTime_Train_BuffSize.ToString();
                BoxWaveFormLine.IsChecked = Prop.RealTime_Train_WaveForm_Line_Show;
                BoxWaveFormPhase.IsChecked = Prop.RealTime_Train_WaveForm_Phase_Show;
                BoxFFT.IsChecked = Prop.RealTime_Train_FFT_Show;
                BoxFS.IsChecked = Prop.RealTime_Train_FS_Show;
                BoxRealTimeEdit.IsChecked = Prop.RealTime_Train_EditAllow;

                BoxShowControl.IsChecked = Prop.RealTime_Train_Control_Show;
                BoxControlPrecise.IsChecked = Prop.RealTime_Train_Control_Precise;
                SelectorControlDesign.SelectedValue = (RealtimeDisplay.ControlStatus.RealTimeControlStatStyle)Prop.RealTime_Train_Control_Style;

                BoxShowHexagon.IsChecked = Prop.RealTime_Train_Hexagon_Show;
                SelectorHexagonDesign.SelectedValue = (RealtimeDisplay.Hexagon.RealTimeHexagonStyle)Prop.RealTime_Train_Hexagon_Style;
                BoxShowZeroVectorCicle.IsChecked = Prop.RealTime_Train_Hexagon_ZeroVector;

                SamplingFrequencyInput.Text = Prop.RealtimeTrainSamplingFrequency.ToString();
                CalculateDivisionInput.Text = Prop.RealtimeTrainCalculateDivision.ToString();

                WindowTitle.Content = LanguageManager.GetString("Simulator.RealTime.Setting.Basic.Title.Train");
            }

        }
        private void BoxChecked(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;
            CheckBox cb = (CheckBox)sender;
            string[] tags = (cb.Tag.ToString() ?? "").Split(".");

            Boolean is_checked = cb.IsChecked == true;

            if (tags[0].Equals("WaveForm"))
            {
                if (tags[1].Equals("Line"))
                {
                    if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                        Properties.Settings.Default.RealTime_VVVF_WaveForm_Line_Show = is_checked;
                    else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                        Properties.Settings.Default.RealTime_Train_WaveForm_Line_Show = is_checked;
                    return;
                }

                if (tags[1].Equals("Phase"))
                {
                    if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                        Properties.Settings.Default.RealTime_VVVF_WaveForm_Phase_Show = is_checked;
                    else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                        Properties.Settings.Default.RealTime_Train_WaveForm_Phase_Show = is_checked;
                }

                return;
            }
                
            if (tags[0].Equals("Edit"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_EditAllow = is_checked;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_EditAllow = is_checked;

                return;
            }

            if (tags[0].Equals("Control"))
            {
                if(tags.Length == 1)
                {
                    if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                        Properties.Settings.Default.RealTime_VVVF_Control_Show = is_checked;
                    else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                        Properties.Settings.Default.RealTime_Train_Control_Show = is_checked;
                    return;
                }

                if (tags[1].Equals("Precise"))
                {
                    if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                        Properties.Settings.Default.RealTime_VVVF_Control_Precise = is_checked;
                    else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                        Properties.Settings.Default.RealTime_Train_Control_Precise = is_checked;
                    return;
                }

                return;
            }

            if (tags[0].Equals("Hexagon"))
            {
                if(tags.Length == 1)
                {
                    if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                        Properties.Settings.Default.RealTime_VVVF_Hexagon_Show = is_checked;
                    else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                        Properties.Settings.Default.RealTime_Train_Hexagon_Show = is_checked;
                    return;
                }

                if (tags[1].Equals("Zero"))
                {
                    if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                        Properties.Settings.Default.RealTime_VVVF_Hexagon_ZeroVector = is_checked;
                    else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                        Properties.Settings.Default.RealTime_Train_Hexagon_ZeroVector = is_checked;
                    return;
                }
            }

            if (tags[0].Equals("FFT"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_FFT_Show = is_checked;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_FFT_Show = is_checked;
                return;
            }
            if (tags[0].Equals("FS"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_FS_Show = is_checked;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_FS_Show = is_checked;
                return;
            }

        }

        private void TextBoxChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;
            TextBox? textBox = sender as TextBox;
            if (textBox == null) return;
            string? tag = textBox.Tag.ToString();
            if (tag == null) return;

            switch (tag)
            {
                case "AudioBuffSize":
                    {
                        int i = ParseTextBox.ParseInt(TextBuffSize);
                        if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                            Properties.Settings.Default.RealTime_VVVF_BuffSize = i;
                        else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                            Properties.Settings.Default.RealTime_Train_BuffSize = i;
                        break;
                    }
                case "SamplingFrequency":
                    {
                        int i = ParseTextBox.ParseInt(SamplingFrequencyInput);
                        if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                            Properties.Settings.Default.RealtimeVvvfSamplingFrequency = i;
                        else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                            Properties.Settings.Default.RealtimeTrainSamplingFrequency = i;
                        break;
                    }
                case "CalculateDivision":
                    {
                        int i = ParseTextBox.ParseInt(CalculateDivisionInput);
                        if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                            Properties.Settings.Default.RealtimeVvvfCalculateDivision = i;
                        else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                            Properties.Settings.Default.RealtimeTrainCalculateDivision = i;
                        break;
                    }
            }            
        }

        private void SelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            ComboBox cb = (ComboBox)sender;
            Object tag = cb.Tag;
            if (tag.Equals("ControlDesign"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Style = (int)cb.SelectedValue;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Style = (int)cb.SelectedValue;
            }else if (tag.Equals("Language"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Language = (int)cb.SelectedValue;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Language = (int)cb.SelectedValue;
            }

            else if (tag.Equals("HexagonDesign"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Hexagon_Style = (int)cb.SelectedValue;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_Hexagon_Style = (int)cb.SelectedValue;               
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {

            bool font_check = false;
            if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF) && Properties.Settings.Default.RealTime_VVVF_Control_Show) font_check = true;
            if (_SettingType.Equals(RealTimeBasicSettingMode.Train) && Properties.Settings.Default.RealTime_Train_Control_Show) font_check = true;
            Properties.Settings.Default.Save();

            if (!font_check) return;
            var selected_style = Properties.Settings.Default.RealTime_VVVF_Control_Style;
            if(selected_style == (int)RealtimeDisplay.ControlStatus.RealTimeControlStatStyle.Original1)
            {
                try
                {
                    Font[] fonts = new Font[]{
                        new(new FontFamily("Fugaz One"), 75, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel), //topic
                    };
                }
                catch
                {
                    MessageBox.Show(
                        LanguageManager.GetString("Simulator.RealTime.Setting.Basic.Message.NoFont") + "\r\n\r\n" +
                        "Fugaz One\r\n",
                        LanguageManager.GetString("Generic.Title.Error"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );

                }
            }
            else if(selected_style == (int)RealtimeDisplay.ControlStatus.RealTimeControlStatStyle.Original2)
            {
                try
                {
                    Font value_Font = new(new FontFamily("DSEG14 Modern"), 40, System.Drawing.FontStyle.Italic, GraphicsUnit.Pixel);
                    Font unit_font = new(new FontFamily("Fugaz One"), 25, System.Drawing.FontStyle.Regular, GraphicsUnit.Pixel);
                }
                catch
                {
                    MessageBox.Show(
                        LanguageManager.GetString("Simulator.RealTime.Setting.Basic.Message.NoFont") + "\r\n\r\n" +
                        "Fugaz One\r\n" +
                        "DSEG14 Modern Italic\r\n",
                        LanguageManager.GetString("Generic.Title.Error"),
                        MessageBoxButton.OK,
                        MessageBoxImage.Error
                        );
                }
            }
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
