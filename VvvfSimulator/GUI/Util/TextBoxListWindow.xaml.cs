using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Class;

namespace VvvfSimulator.GUI.Util
{
    /// <summary>
    /// Double_Ask_Form.xaml の相互作用ロジック
    /// </summary>
    public partial class TextBoxListWindow : Window
    {
        public class InputContext(string? title, object value, Type type)
        {
            public Type Type { get; set; } = type;
            public string? Title { get; set; } = title;
            public object Value { get; set; } = value;

        }
        public List<InputContext>? Contexts = null;

        public TextBoxListWindow(Window Owner, string title, List<InputContext> contexts)
        {
            this.Owner = Owner;
            InitializeComponent();
            this.WindowTitle.Content = title;
            Contexts = contexts;
            InitializeView();
            ShowDialog();
            
        }

        private static FrameworkElement GetTitleAndInput(InputContext Context)
        {
            Grid wrapper = new();
            wrapper.RowDefinitions.Add(new RowDefinition());
            wrapper.RowDefinitions.Add(new RowDefinition() { Height=new(50) });

            if(Context.Title != null)
            {
                Label label = new();
                label.Content = Context.Title;
                label.HorizontalAlignment = HorizontalAlignment.Left;
                label.VerticalAlignment = VerticalAlignment.Center;
                label.SetResourceReference(Control.ForegroundProperty, "BackgroundTextBrush");
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, 0);
                wrapper.Children.Add(label);
            }

            TextBox box = new();
            box.Text = Context.Value.ToString();
            box.FontSize = 22;
            box.SetResourceReference(Control.StyleProperty, "SlimTextBox");
            box.TextChanged += (object sender, TextChangedEventArgs e) => {
                if (Context.Type == typeof(int)) Context.Value = ParseTextBox.ParseInt(box);
                else if (Context.Type == typeof(double)) Context.Value = ParseTextBox.ParseDouble(box);
                else if (Context.Type == typeof(string)) Context.Value = box.Text ?? "";
            };
            Grid.SetRow(box, 1);
            Grid.SetColumn(box, 0);
            wrapper.Children.Add(box);

            return wrapper;
        }
        private void InitializeView()
        {
            if (Contexts == null) return;
            for (int i = 0; i < Contexts.Count; i++)
            {
                InputContext context = Contexts[i];
                FrameworkElement InputElement = GetTitleAndInput(context);
                Inputs.Children.Add(InputElement);
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            Button? btn = sender as Button;
            if (btn == null) return;
            string? tag = btn.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("Close"))
            {
                Contexts = null;
                Close();
            }
                
        }
    }
}
