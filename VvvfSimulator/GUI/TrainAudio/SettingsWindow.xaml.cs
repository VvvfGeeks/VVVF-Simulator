using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.TrainAudio.Pages.Gear;
using VvvfSimulator.GUI.TrainAudio.Pages.Mixer;
using VvvfSimulator.GUI.TrainAudio.Pages.Motor;
using VvvfSimulator.GUI.Util;
using YamlDotNet.Core;

namespace VvvfSimulator.GUI.TrainAudio
{
    /// <summary>
    /// TrainAudio_Harmonic_Window.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            PageFrame.Navigate(new MotorSetting(Data.TrainAudio.Manager.Current));
        }
        public void LoadYaml(string Path)
        {
            try
            {
                Data.TrainAudio.Manager.LoadCurrent(Path);
                PageFrame.Navigate(new MotorSetting(Data.TrainAudio.Manager.Current));
                DialogBox.Show(this, LanguageManager.GetString("TrainAudio.SettingWindow.Message.File.Load.Ok.Message"), LanguageManager.GetString("Generic.Title.Info"), [DialogBoxButton.Ok], DialogBoxIcon.Ok);
            }
            catch (YamlException ex)
            {
                string error_message = LanguageManager.GetString("TrainAudio.SettingWindow.Message.File.Load.Error.Message");
                error_message += "\r\n";
                error_message += "\r\n" + ex.End.ToString() + "\r\n";
                DialogBox.Show(this, error_message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string error_message = LanguageManager.GetString("TrainAudio.SettingWindow.Message.File.Load.Error.Message");
                error_message += "\r\n";
                error_message += "\r\n" + ex.Message + "\r\n";
                DialogBox.Show(this, error_message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
            }
        }
        public void SaveYaml(string Path, bool SaveAs)
        {
            string MessagePath = SaveAs ? "SaveAs" : "Save";
            if (Data.TrainAudio.Manager.SaveCurrent(Path))
                DialogBox.Show(this,
                    LanguageManager.GetString("TrainAudio.SettingWindow.Message.File." + MessagePath + ".Ok.Message"),
                    LanguageManager.GetString("Generic.Title.Info"),
                    [DialogBoxButton.Ok], DialogBoxIcon.Ok);
            else
                DialogBox.Show(this,
                    LanguageManager.GetString("TrainAudio.SettingWindow.Message.File." + MessagePath + ".Error.Message"),
                    LanguageManager.GetString("Generic.Title.Error"),
                    [DialogBoxButton.Ok], DialogBoxIcon.Error);
        }
        public bool SaveBefore(string MessagePath)
        {
            if (Data.TrainAudio.Manager.IsCurrentEquivalent(
                Data.TrainAudio.Manager.LoadPath.Length == 0 ? Data.TrainAudio.Manager.Template : Data.TrainAudio.Manager.LoadData))
                return true;

            DialogBoxButton? Result = DialogBox.Show(
                    this,
                    LanguageManager.GetString(MessagePath + ".Message"),
                    LanguageManager.GetString(MessagePath + ".Title"),
                    [DialogBoxButton.Yes, DialogBoxButton.No, DialogBoxButton.Cancel], DialogBoxIcon.Question);

            if (Result == DialogBoxButton.Yes)
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Data.TrainAudio.Manager.GetLoadedYamlName()
                };
                if (dialog.ShowDialog() ?? false)
                {
                    SaveYaml(dialog.FileName, true);
                    return true;
                }
                else
                    return false;
            }
            else if (Result == DialogBoxButton.No)
                return true;
            else
                return false;
        }
        private void File_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            object? tag = button.Tag;
            if (tag == null) return;
            if (tag.Equals("Load"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (SaveBefore("TrainAudio.SettingWindow.Message.File.SaveBefore.Load") && (dialog.ShowDialog() ?? false))
                    LoadYaml(dialog.FileName);
            }
            else if (tag.Equals("SaveAs"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Data.TrainAudio.Manager.GetLoadedYamlName()
                };
                if (dialog.ShowDialog() ?? false)
                    SaveYaml(dialog.FileName, true);
            }
            else if (tag.Equals("Save"))
            {
                if (Data.TrainAudio.Manager.LoadPath.Length == 0)
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "Yaml (*.yaml)|*.yaml",
                        FileName = "VVVF"
                    };
                    if (dialog.ShowDialog() ?? false)
                        SaveYaml(dialog.FileName, true);
                }
                else
                    SaveYaml(Data.TrainAudio.Manager.LoadPath, false);
            }
        }
        private void Mixer_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem btn = (MenuItem)sender;
            object tag = btn.Tag;
            if (tag.Equals("Frequency"))
                PageFrame.Navigate(new FrequencyFilter(Data.TrainAudio.Manager.Current));
            else if (tag.Equals("Convolution"))
                PageFrame.Navigate(new ConvolutionFilter(Data.TrainAudio.Manager.Current));
            else if (tag.Equals("Volume"))
                PageFrame.Navigate(new Volume(Data.TrainAudio.Manager.Current));
        }
        private void Parameter_Menu_Click(object sender, RoutedEventArgs e)
        {
            MenuItem btn = (MenuItem)sender;
            object tag = btn.Tag;

            if (tag.Equals("Gear"))
                PageFrame.Navigate(new GearSetting(this, Data.TrainAudio.Manager.Current));
            else if (tag.Equals("Motor"))
                PageFrame.Navigate(new MotorSetting(Data.TrainAudio.Manager.Current));
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!SaveBefore("TrainAudio.SettingWindow.Message.File.SaveBefore.Close"))
                e.Cancel = true;
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
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string Path = (((Array)e.Data.GetData(DataFormats.FileDrop)).GetValue(0) ?? "").ToString() ?? "";
                if (Path.ToLower().EndsWith(".yaml") && SaveBefore("TrainAudio.SettingWindow.Message.File.SaveBefore.Load")) LoadYaml(Path);
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;
            if (e.Key.Equals(Key.S) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (Data.TrainAudio.Manager.LoadPath.Length == 0)
                {
                    var dialog = new SaveFileDialog
                    {
                        Filter = "Yaml (*.yaml)|*.yaml",
                        FileName = "VVVF"
                    };
                    if (dialog.ShowDialog() ?? false)
                        SaveYaml(dialog.FileName, true);
                }
                else
                    SaveYaml(Data.TrainAudio.Manager.LoadPath, false);
            }
        }
    }
}
