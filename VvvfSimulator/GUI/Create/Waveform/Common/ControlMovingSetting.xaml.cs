using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Util;
using VvvfSimulator.GUI.Resource.Class;
using static OpenCvSharp.Stitcher;
using static VvvfSimulator.Data.Vvvf.Struct.PulseControl;

namespace VvvfSimulator.GUI.Create.Waveform.Common
{
    /// <summary>
    /// Control_Moving_Setting.xaml の相互作用ロジック
    /// </summary>
    public partial class ControlMovingSetting : UserControl
    {
        private readonly bool IgnoreUpdate = true;
        private readonly ViewModel BindingData = new();

        public class ViewModel : ViewModelBase
        {
            private Visibility _Exponential_Visibility = Visibility.Collapsed;
            public Visibility Exponential_Visibility { get { return _Exponential_Visibility; } set { _Exponential_Visibility = value; RaisePropertyChanged(nameof(Exponential_Visibility)); } }

            private Visibility _CurveRate_Visibility = Visibility.Collapsed;
            public Visibility CurveRate_Visibility { get { return _CurveRate_Visibility; } set { _CurveRate_Visibility = value; RaisePropertyChanged(nameof(CurveRate_Visibility)); } }

            public FunctionValue MovingValue { get; set; } = new FunctionValue();

        };
        public ControlMovingSetting(FunctionValue target)
        {

            BindingData.MovingValue = target;
            base.DataContext = BindingData;

            InitializeComponent();

            Move_Mode_Selector.ItemsSource = FriendlyNameConverter.GetMovingValueTypeNames();
            Move_Mode_Selector.SelectedValue = BindingData.MovingValue.Type;
            UpdateView();

            IgnoreUpdate = false;
        }

        private void ValueChanged(object sender, TextChangedEventArgs e)
        {
            if (IgnoreUpdate) return;
            TextBox tb = (TextBox)sender;
            Object? tag = tb.Tag;
            if (tag == null) return;

            if (tag.Equals("start"))
                BindingData.MovingValue.Start = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("start_val"))
                BindingData.MovingValue.StartValue = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end"))
                BindingData.MovingValue.End = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("end_val"))
                BindingData.MovingValue.EndValue = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("degree"))
                BindingData.MovingValue.Degree = ParseTextBox.ParseDouble(tb);
            else if (tag.Equals("curve_rate"))
                BindingData.MovingValue.CurveRate = ParseTextBox.ParseDouble(tb);

        }

        private readonly Dictionary<FunctionValue.FunctionType, string> MovingValueTypeNames = [];

        private void MoveValueTypeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreUpdate) return;

            FunctionValue.FunctionType selected = (FunctionValue.FunctionType)Move_Mode_Selector.SelectedValue;
            BindingData.MovingValue.Type = selected;
            UpdateView();
        }

        private void UpdateView()
        {
            void _Setter(int x, Visibility b)
            {
                if (x == 0) BindingData.Exponential_Visibility = b;
                else if (x == 1) BindingData.CurveRate_Visibility = b;
            }

            FunctionValue.FunctionType selected = BindingData.MovingValue.Type;

            Visibility[] visible_list = [Visibility.Collapsed, Visibility.Collapsed];

            if (selected == FunctionValue.FunctionType.Proportional)
                visible_list = [Visibility.Collapsed, Visibility.Collapsed];
            else if (selected == FunctionValue.FunctionType.Pow2_Exponential)
                visible_list = [Visibility.Visible, Visibility.Collapsed];
            else if (selected == FunctionValue.FunctionType.Inv_Proportional)
                visible_list = [Visibility.Collapsed, Visibility.Visible];
            else if (selected == FunctionValue.FunctionType.Sine)
                visible_list = [Visibility.Collapsed, Visibility.Collapsed];

            for (int i = 0; i < visible_list.Length; i++)
            {
                _Setter(i, visible_list[i]);
            }
        }
    }
}
