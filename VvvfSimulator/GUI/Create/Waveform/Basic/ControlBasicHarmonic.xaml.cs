using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.Pulse;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// Control_Basic_Harmonic.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlBasicHarmonic : Window
    {
        readonly ViewData vd = new();
        public class ViewData : ViewModelBase
        {
            public List<PulseHarmonic> _HarmonicList = [];
            public List<PulseHarmonic> HarmonicList { get { return _HarmonicList; } set { _HarmonicList = value; RaisePropertyChanged(nameof(HarmonicList)); } }
        }
        //
        //Harmonic Presets
        public enum PresetHarmonics
        {
            THI, HFI, SquareFourier
        }

        public static List<PulseHarmonic> GetPresetHarmonics(PresetHarmonics harmonic)
        {
            switch (harmonic)
            {
                case PresetHarmonics.THI:
                    return [ 
                        new() { Amplitude = 0.2, Harmonic = 3 } 
                    ];
                case PresetHarmonics.HFI:
                    return [
                        new () { Amplitude = 0.5, Harmonic = 250, IsAmplitudeProportional=false, IsHarmonicProportional = false}
                    ];
                default:
                    List<PulseHarmonic> harmonics = [];
                    for (int i = 0; i < 10; i++)
                    {
                        harmonics.Add(new PulseHarmonic() { Amplitude = 1.0 / (2 * i + 3), Harmonic = 2 * i + 3 });
                    }
                    return harmonics;


            }
        }

        private readonly Pulse Target;
        private readonly bool IgnoreUpdate = true;
        public ControlBasicHarmonic(Window? owner, Pulse data)
        {
            Owner = owner;
            vd.HarmonicList = data.PulseHarmonics;
            DataContext = vd;
            Target = data;

            InitializeComponent();

            WaveformTypeSelector.ItemsSource = FriendlyNameConverter.GetPulseHarmonicTypeNames();
            PresetSelector.ItemsSource = (PresetHarmonics[])Enum.GetValues(typeof(PresetHarmonics));
            PresetSelector.SelectedIndex = 0;

            IgnoreUpdate = false;
        }

        private void DataGrid_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            if (IgnoreUpdate) return;
            Target.PulseHarmonics = vd.HarmonicList;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Harmonic_Editor.CommitEdit();
        }

        private void Harmonic_Editor_Unloaded(object sender, RoutedEventArgs e)
        {
            Harmonic_Editor.CommitEdit();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            Object tag = btn.Tag;

            PresetHarmonics selected = (PresetHarmonics)PresetSelector.SelectedItem;
            List<PulseHarmonic> harmonics = GetPresetHarmonics(selected);

            if (tag.Equals("Add"))
                vd.HarmonicList.AddRange(harmonics);
            else if (tag.Equals("Set"))
                vd.HarmonicList = harmonics;

            Harmonic_Editor.Items.Refresh();
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
