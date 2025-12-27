using Microsoft.Win32;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Util;
using YamlDotNet.Core;

namespace VvvfSimulator.GUI.BaseFrequency
{
    public class SmallTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Data.BaseFrequency.Struct.Point)
                return DependencyProperty.UnsetValue;

            Data.BaseFrequency.Struct.Point val = (Data.BaseFrequency.Struct.Point)value;

            return "Rate : " + String.Format("{0:F2}", val.Rate) + " , Duration : " + String.Format("{0:F2}", val.Duration) + " , PowerOn : " + val.PowerOn.ToString() + " , Brake : " + val.Brake;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Wow!");
        }
    }
    public class BigTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not Data.BaseFrequency.Struct.Point)
                return DependencyProperty.UnsetValue;

            Data.BaseFrequency.Struct.Point val = (Data.BaseFrequency.Struct.Point)value;

            return val.Order;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("Wow!");
        }
    }

    /// <summary>
    /// ControlEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class SettingWindow
    {
        public SettingWindow()
        {
            InitializeComponent();
            PointList.ItemsSource = Data.BaseFrequency.Manager.Current.Points;
        }
        public void LoadYaml(string path)
        {
            try
            {
                Data.BaseFrequency.Manager.LoadCurrent(path);
                UpdateItemList();
                EditorFrame.Navigate(null);
                DialogBox.Show(this, LanguageManager.GetString("BaseFrequency.SettingWindow.Message.File.Load.Ok.Message"), LanguageManager.GetString("BaseFrequency.SettingWindow.Message.File.Load.Ok.Title"), [DialogBoxButton.Ok], DialogBoxIcon.Ok);
            }
            catch (YamlException ex)
            {
                string error_message = LanguageManager.GetString("BaseFrequency.SettingWindow.Message.File.Load.Error.Message");
                error_message += "\r\n";
                error_message += "\r\n" + ex.End.ToString() + "\r\n";
                DialogBox.Show(this, error_message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string error_message = LanguageManager.GetString("BaseFrequency.SettingWindow.Message.File.Load.Error.Message");
                error_message += "\r\n";
                error_message += "\r\n" + ex.Message + "\r\n";
                DialogBox.Show(this, error_message, LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
            }
        }
        public void SaveYaml(string Path, bool SaveAs)
        {
            string MessagePath = SaveAs ? "SaveAs" : "Save";
            if (Data.BaseFrequency.Manager.SaveCurrent(Path))
                DialogBox.Show(this,
                    LanguageManager.GetString("BaseFrequency.SettingWindow.Message.File." + MessagePath + ".Ok.Message"),
                    LanguageManager.GetString("BaseFrequency.SettingWindow.Message.File." + MessagePath + ".Ok.Title"),
                    [DialogBoxButton.Ok], DialogBoxIcon.Ok);
            else
                DialogBox.Show(this,
                    LanguageManager.GetString("BaseFrequency.SettingWindow.Message.File." + MessagePath + ".Error.Message"),
                    LanguageManager.GetString("Generic.Title.Error"),
                    [DialogBoxButton.Ok], DialogBoxIcon.Error);
        }
        public bool SaveBefore(string MessagePath)
        {
            if (Data.BaseFrequency.Manager.IsCurrentEquivalent(
                Data.BaseFrequency.Manager.LoadPath.Length == 0 ? Data.BaseFrequency.Manager.Template : Data.BaseFrequency.Manager.LoadData))
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
                    FileName = Data.BaseFrequency.Manager.GetLoadedYamlName()
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

        public void UpdateItemList()
        {
            PointList.ItemsSource = Data.BaseFrequency.Manager.Current.Points;
            PointList.Items.Refresh();
        }
        private void FileMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            string? tag = button.Tag?.ToString() ?? "";

            if (tag.Equals("Load"))
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml|All (*.*)|*.*"
                };
                if (SaveBefore("BaseFrequency.SettingWindow.Message.File.SaveBefore.Load") && (dialog.ShowDialog() ?? false))
                    LoadYaml(dialog.FileName);
            }
            else if (tag.Equals("SaveAs"))
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Yaml (*.yaml)|*.yaml",
                    FileName = Data.BaseFrequency.Manager.GetLoadedYamlName()
                };
                if (dialog.ShowDialog() ?? false)
                    SaveYaml(dialog.FileName, true);
            }
            else if (tag.Equals("Save"))
            {
                if (Data.BaseFrequency.Manager.LoadPath.Length == 0)
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
                    SaveYaml(Data.BaseFrequency.Manager.LoadPath, false);
            }
        }
        private void EditMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem button = (MenuItem)sender;
            Object? tag = button.Tag;
            if (tag == null) return;
            if (tag.Equals("Midi"))
            {
                LoadMidi Loader = new(this);
                Loader.ShowDialog();
                var load_data = Loader.LoadConfiguration;
                try
                {
                    Data.BaseFrequency.Struct? data = Data.BaseFrequency.MidiConverter.Convert(load_data);
                    if (data == null) return;
                    Data.BaseFrequency.Manager.Current = data;
                    UpdateItemList();
                    EditorFrame.Navigate(null);
                }
                catch
                {
                    DialogBox.Show(this, LanguageManager.GetString("BaseFrequency.SettingWindow.Message.MidiConvertError"), LanguageManager.GetString("Generic.Title.Error"), [DialogBoxButton.Ok], DialogBoxIcon.Error);
                }
            }
            else if (tag.Equals("Reset"))
            {
                DialogBoxButton? Result = DialogBox.Show(
                    this,
                    LanguageManager.GetString("BaseFrequency.SettingWindow.Message.Edit.Reset.Confirm.Message"),
                    LanguageManager.GetString("BaseFrequency.SettingWindow.Message.Edit.Reset.Confirm.Title"),
                    [DialogBoxButton.Yes, DialogBoxButton.No], DialogBoxIcon.Question);
                if (Result != DialogBoxButton.Yes)
                    return;
                Data.BaseFrequency.Manager.ResetCurrent();
                UpdateItemList();
                EditorFrame.Navigate(null);
            }
        }
        private void ControlItemContextMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object? tag = mi.Tag;

            if (tag.Equals("Delete"))
            {
                int selected = PointList.SelectedIndex;
                if (selected < 0) return;
                Data.BaseFrequency.Manager.Current.Points.RemoveAt(selected);
                UpdateItemList();
            } else if (tag.Equals("Copy"))
            {
                var selected_item = PointList.SelectedItem;
                if (selected_item == null) return;
                Data.BaseFrequency.Struct.Point data = (Data.BaseFrequency.Struct.Point)selected_item;
                Data.BaseFrequency.Manager.Current.Points.Add(data.Clone());
                UpdateItemList();
            }           
        }
        private void ItemListContextMenuClick(object sender, RoutedEventArgs e)
        {
            MenuItem mi = (MenuItem)sender;
            Object? tag = mi.Tag;

            if (tag.Equals("Sort"))
            {
                Data.BaseFrequency.Manager.Current.Points.Sort((a, b) => Math.Sign(a.Order - b.Order));
                UpdateItemList();
            }
        }
        private void ControlItemSelected(object sender, SelectionChangedEventArgs e)
        {
            Data.BaseFrequency.Struct.Point Selected = (Data.BaseFrequency.Struct.Point)PointList.SelectedItem;
            if (Selected == null) return;
            EditorFrame.Navigate(new EditPage(this, Selected));
        }
        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Object tag = btn.Tag;

            if (tag.Equals("Add"))
            {
                Data.BaseFrequency.Manager.Current.Points.Add(new(Data.BaseFrequency.Manager.Current.Points.Count));
                UpdateItemList();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(!SaveBefore("BaseFrequency.SettingWindow.Message.File.SaveBefore.Close"))
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
                if (Path.ToLower().EndsWith(".yaml") && SaveBefore("BaseFrequency.SettingWindow.Message.File.SaveBefore.Load")) LoadYaml(Path);
            }
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.IsRepeat) return;
            if (e.Key.Equals(Key.S) && e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                e.Handled = true;
                if (Data.BaseFrequency.Manager.LoadPath.Length == 0)
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
                    SaveYaml(Data.BaseFrequency.Manager.LoadPath, false);
            }
        }
    }
}
