using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl.AmplitudeValue;

namespace VvvfSimulator.GUI.Create.Waveform.Basic
{
    /// <summary>
    /// AmplitudeTableEditor.xaml の相互作用ロジック
    /// </summary>
    public partial class AmplitudeTableEditor : Window
    {
        private readonly Parameter Parameter;
        private class DataGridItem
        {
            public double Frequency { get; set; }
            public double Amplitude { get; set; }
            public DataGridItem() { }
            public DataGridItem(double Frequency, double Amplitude)
            {
                this.Frequency = Frequency;
                this.Amplitude = Amplitude;
            }
            
        }
        public AmplitudeTableEditor(Window? Owner,Parameter Parameter)
        {
            this.Owner = Owner;
            this.Parameter = Parameter;

            List<DataGridItem> List = [];
            for(int i =0; i < Parameter.AmplitudeTable.Length; i++)
                List.Add(new(Parameter.AmplitudeTable[i].Frequency, Parameter.AmplitudeTable[i].Amplitude));

            DataContext = List;

            InitializeComponent();
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

        private void DataGridUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {

        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            (double Frequency, double Amplitude)[] Table = new(double,double)[((List<DataGridItem>)DataContext).Count];

            ((List<DataGridItem>)DataContext).Sort((a,b) =>  a.Frequency.CompareTo(b.Frequency));

            for (int i =0; i < Table.Length; i++)
            {
                DataGridItem Item = ((List<DataGridItem>)DataContext)[i];
                Table[i] = (Item.Frequency, Item.Amplitude);
            }
            Parameter.AmplitudeTable = Table;
        }
    }
}
