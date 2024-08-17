using Microsoft.Win32;
using System;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;

namespace VvvfSimulator.GUI.Mascon
{
    /// <summary>
    /// LoadMidi.xaml の相互作用ロジック
    /// </summary>
    public partial class LoadMidi : Window
    {

        readonly bool IgnoreUpdate = true;
        public LoadMidi(Window Owner)
        {
            this.Owner = Owner;
            InitializeComponent();
            IgnoreUpdate = false;
        }
        public class MidiLoadData
        {
            public int track = 1;
            public int priority = 1;
            public double division = 1;
            public string path = "";
        }
        public MidiLoadData LoadConfiguration { get; set; } = new();
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;
            TextBox tb = (TextBox)sender;
            Object tag = tb.Tag;

            if (tag.Equals("Track"))
            {
                int track = ParseTextBox.ParseInt(tb, 1);
                LoadConfiguration.track = track;
            }
            else if (tag.Equals("Priority"))
            {
                int priority = ParseTextBox.ParseInt(tb, 1);
                LoadConfiguration.priority = priority;
            }else if (tag.Equals("Division"))
            {
                double d = ParseTextBox.ParseDouble(tb, 1);
                LoadConfiguration.division = d;
            }
        }

        private void Select_File_Button_Click(object sender, RoutedEventArgs e)
        {
            if (IgnoreUpdate) return;

            var dialog = new OpenFileDialog
            {
                Filter = "Midi (*.mid)|*.mid|All (*.*)|*.*"
            };
            if (dialog.ShowDialog() == false) return;

            String path = dialog.FileName;
            LoadConfiguration.path = path;
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
