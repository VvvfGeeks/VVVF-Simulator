using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.TrainAudio.Pages.Mixer;
using VvvfSimulator.GUI.TrainAudio.Pages.Gear;
using VvvfSimulator.GUI.TrainAudio.Pages.Motor;
using YamlDotNet.Core;
using static VvvfSimulator.Yaml.TrainAudioSetting.YamlTrainSoundAnalyze;
using System.Diagnostics;
using VvvfSimulator.GUI.Resource.Language;

namespace VvvfSimulator.GUI.TrainAudio
{
    /// <summary>
    /// TrainAudio_Harmonic_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private YamlTrainSoundData soundData;
        public SettingsWindow(YamlTrainSoundData thd)
        {
            soundData = thd;
            InitializeComponent();
            PageFrame.Navigate(new MotorSetting(soundData));
        }
        private void MenuParameterClick(object sender, RoutedEventArgs e)
        {
            MenuItem btn = (MenuItem)sender;
            object tag = btn.Tag;

            if (tag.Equals("Gear"))
                PageFrame.Navigate(new GearSetting(this, soundData));
            else if (tag.Equals("Motor"))
                PageFrame.Navigate(new MotorSetting(soundData));

        }
        private string load_path = "acoustic.yaml";
        private void MenuFileClick(object sender, RoutedEventArgs e)
        {
            MenuItem btn = (MenuItem)sender;
            object tag = btn.Tag;

            if (tag.Equals("Open"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (dialog.ShowDialog() == false) return;

                try
                {
                    
                    YamlTrainSoundDataManage.CurrentData = YamlTrainSoundDataManage.LoadYaml(dialog.FileName);
                    this.soundData = YamlTrainSoundDataManage.CurrentData;
                    PageFrame.Navigate(new MotorSetting(soundData));
                    MessageBox.Show(LanguageManager.GetString("TrainAudio.SettingWindow.Message.File.Open.Ok.Message"), LanguageManager.GetString("Generic.Title.Info"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (YamlException ex)
                {
                    String error_message = LanguageManager.GetString("TrainAudio.SettingWindow.Message.File.Open.Error.Message");
                    error_message += "\r\n";
                    error_message += "\r\n" + ex.End.ToString() + "\r\n";
                    MessageBox.Show(error_message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }


                load_path = dialog.FileName;
                return;
            }

            if (tag.Equals("Save"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Path.GetFileName(load_path)
                };
                if (dialog.ShowDialog() == false) return;

                if (YamlTrainSoundDataManage.SaveYaml(dialog.FileName))
                    MessageBox.Show(LanguageManager.GetString("TrainAudio.SettingWindow.Message.File.Save.Ok.Message"), LanguageManager.GetString("Generic.Title.Info"), MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show(LanguageManager.GetString("TrainAudio.SettingWindow.Message.File.Save.Error.Message"), LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private void MenuMixerClick(object sender, RoutedEventArgs e)
        {
            MenuItem btn = (MenuItem)sender;
            object tag = btn.Tag;

            if (tag.Equals("Frequency"))
                PageFrame.Navigate(new FrequencyFilter(soundData));
            else if (tag.Equals("Convolution"))
                PageFrame.Navigate(new ConvolutionFilter(soundData));
            else if (tag.Equals("Volume"))
                PageFrame.Navigate(new Volume(soundData));
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

        private void Window_Drop(object sender, DragEventArgs e)
        {
            string path = (((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0) ?? "").ToString() ?? "";
            if (path.ToLower().EndsWith(".yaml"))
            {
                try
                {
                    YamlTrainSoundDataManage.CurrentData = YamlTrainSoundDataManage.LoadYaml(path);
                    this.soundData = YamlTrainSoundDataManage.CurrentData;
                    PageFrame.Navigate(new MotorSetting(soundData));
                    MessageBox.Show(LanguageManager.GetString("TrainAudio.SettingWindow.Message.File.Open.Ok.Message"), LanguageManager.GetString("Generic.Title.Info"), MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (YamlException ex)
                {
                    String error_message = LanguageManager.GetString("TrainAudio.SettingWindow.Message.File.Open.Error.Message");
                    error_message += "\r\n";
                    error_message += "\r\n" + ex.End.ToString() + "\r\n";
                    MessageBox.Show(error_message, LanguageManager.GetString("Generic.Title.Error"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                load_path = path;
            }
        }
    }
}
