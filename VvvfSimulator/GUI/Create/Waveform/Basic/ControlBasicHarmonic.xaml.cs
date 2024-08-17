using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VvvfSimulator.GUI.Resource.Class;
using VvvfSimulator.GUI.Resource.Language;
using static VvvfSimulator.VvvfStructs;
using static VvvfSimulator.VvvfStructs.PulseMode;
using static VvvfSimulator.VvvfStructs.PulseMode.PulseHarmonic;

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
            public List<PulseHarmonic> _harmonic_data = new List<PulseHarmonic>();
            public List<PulseHarmonic> harmonic_data { get { return _harmonic_data; } set { _harmonic_data = value; RaisePropertyChanged(nameof(harmonic_data)); } }
        }
        //
        //Harmonic Presets
        public enum PresetHarmonics
        {
            THI, SVM, HFI, DPM1, DPM2, DPM3, DPM4, SquareFourier
        }

        public List<PulseHarmonic> Get_Preset_Harmonics(PresetHarmonics harmonic)
        {
            switch (harmonic)
            {
                case PresetHarmonics.THI:
                    return new List<PulseHarmonic>() { 
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3 } 
                    };
                case PresetHarmonics.SVM:
                    return new List<PulseHarmonic>() { 
                        new PulseHarmonic() { Amplitude = 0.25, Harmonic = 3 , Type = PulseHarmonic.PulseHarmonicType.Saw} 
                    };
                case PresetHarmonics.HFI:
                    return new List<PulseHarmonic>() {
                        new PulseHarmonic() { Amplitude = 0.5, Harmonic = 250 , Type = PulseHarmonic.PulseHarmonicType.Sine, InitialPhase=0, IsAmplitudeProportional=false, IsHarmonicProportional = false}
                    };
                case PresetHarmonics.DPM1:
                    return new List<PulseHarmonic>() {
                        new PulseHarmonic() { Amplitude = -0.05, Harmonic = 3 },
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Square }
                    };
                case PresetHarmonics.DPM2:
                    return new List<PulseHarmonic>() {
                        new PulseHarmonic() { Amplitude = -0.05, Harmonic = 3, InitialPhase = 1.57079633, Type = PulseHarmonic.PulseHarmonicType.Saw},
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Square }
                    };
                case PresetHarmonics.DPM3:
                    return new List<PulseHarmonic>() {
                        new PulseHarmonic() { Amplitude = -0.05, Harmonic = 3, InitialPhase = -1.57079633, Type = PulseHarmonic.PulseHarmonicType.Saw},
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Square }
                    };
                case PresetHarmonics.DPM4: //case Preset_Harmonics.DPM4:
                    return new List<PulseHarmonic>() {
                        new PulseHarmonic() { Amplitude = 0.05, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Saw},
                        new PulseHarmonic() { Amplitude = 0.2, Harmonic = 3, Type = PulseHarmonic.PulseHarmonicType.Square }
                    };
                default:
                    List<PulseHarmonic> harmonics = new();
                    for (int i = 0; i < 10; i++)
                    {
                        harmonics.Add(new PulseHarmonic() { Amplitude = 1.0 / (2 * i + 3), Harmonic = 2 * i + 3 });
                    }
                    return harmonics;


            }
        }

        private readonly PulseMode Target;
        private readonly bool IgnoreUpdate = true;
        public ControlBasicHarmonic(Window? owner, PulseMode data)
        {
            Owner = owner;
            vd.harmonic_data = data.PulseHarmonics;
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
            Target.PulseHarmonics = vd.harmonic_data;
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
            List<PulseHarmonic> harmonics = Get_Preset_Harmonics(selected);

            if (tag.Equals("Add"))
                vd.harmonic_data.AddRange(harmonics);
            else if (tag.Equals("Set"))
                vd.harmonic_data = harmonics;

            Harmonic_Editor.Items.Refresh();
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
