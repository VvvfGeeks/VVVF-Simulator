using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Simulator.RealTime.Controller;

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
        
        private void InitializeCombobox()
        {
            SelectorControlDesign.ItemsSource = FriendlyNameConverter.GetRealTimeControlStatStyleNames();
            SelectorHexagonDesign.ItemsSource = FriendlyNameConverter.GetRealTimeHexagonStyleNames();
            SelectorControllerStyle.ItemsSource = FriendlyNameConverter.GetRealTimeControllerStyleNames();
        }

        private void SetControl()
        {

            InitializeCombobox();

            var Prop = Properties.Settings.Default;
            if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
            {
                TextBuffSize.Text = Prop.RealTime_VVVF_BuffSize.ToString();

                SelectorControllerStyle.SelectedValue = (ControllerStyle)Prop.RealTime_VVVF_Controller_Style;

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

                SelectorControllerStyle.SelectedValue = (ControllerStyle)Prop.RealTime_Train_Controller_Style;

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

            bool is_checked = cb.IsChecked == true;

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
            if (sender is not TextBox textBox) return;
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

            if (sender is not ComboBox comboBox) return;
            object tag = comboBox.Tag;

            if (tag.Equals("ControlDesign"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Style = (int)comboBox.SelectedValue;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Style = (int)comboBox.SelectedValue;
            }else if (tag.Equals("Language"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Control_Language = (int)comboBox.SelectedValue;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_Control_Language = (int)comboBox.SelectedValue;
            }
            else if (tag.Equals("HexagonDesign"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Hexagon_Style = (int)comboBox.SelectedValue;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_Hexagon_Style = (int)comboBox.SelectedValue;               
            }
            else if (tag.Equals("ControllerStyle"))
            {
                if (_SettingType.Equals(RealTimeBasicSettingMode.VVVF))
                    Properties.Settings.Default.RealTime_VVVF_Controller_Style = (int)comboBox.SelectedValue;
                else if (_SettingType.Equals(RealTimeBasicSettingMode.Train))
                    Properties.Settings.Default.RealTime_Train_Controller_Style = (int)comboBox.SelectedValue;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
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
