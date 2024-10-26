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
    public partial class DialogInputWindow : Window
    {
        public enum InputContextMode
        {
            TextBox, ComboBox, CheckBox
        }
        public class InputContext(string? title, InputContextMode mode, object value, Type type)
        {
            public InputContextMode Mode { get; set; } = mode;
            public Type Type { get; set; } = type;
            public string? Title { get; set; } = title;
            public object Value { get; set; } = value;

        }
        public List<InputContext>? Contexts = null;

        public DialogInputWindow(Window Owner, string title, List<InputContext> contexts)
        {
            this.Owner = Owner;
            InitializeComponent();
            this.WindowTitle.Content = title;
            Contexts = contexts;
            InitializeView();           
        }

        private static Grid GetTextBoxElement(InputContext Context)
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
                label.SetResourceReference(ForegroundProperty, "BackgroundTextBrush");
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, 0);
                wrapper.Children.Add(label);
            }

            TextBox box = new()
            {
                Text = Context.Value.ToString(),
                FontSize = 22
            };
            box.SetResourceReference(StyleProperty, "SlimTextBox");
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
        private static Grid GetComboBoxElement(InputContext Context)
        {
            Grid wrapper = new();
            wrapper.RowDefinitions.Add(new RowDefinition());
            wrapper.RowDefinitions.Add(new RowDefinition() { Height = new(50) });

            if (Context.Title != null)
            {
                Label label = new()
                {
                    Content = Context.Title,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                label.SetResourceReference(ForegroundProperty, "BackgroundTextBrush");
                Grid.SetRow(label, 0);
                Grid.SetColumn(label, 0);
                wrapper.Children.Add(label);
            }

            ComboBox box = new()
            {
                ItemsSource = (System.Collections.IDictionary)Context.Value,
                SelectedValuePath = "Key",
                DisplayMemberPath = "Value",
                SelectedIndex = 0
            };
            box.SelectionChanged += (object sender, SelectionChangedEventArgs e) =>
            {
                Context.Value = box.SelectedValue;
            };
            box.SetResourceReference(StyleProperty, "SlimComboBoxStyle");
            Grid.SetRow(box, 1);
            Grid.SetColumn(box, 0);
            wrapper.Children.Add(box);

            Context.Value = box.SelectedValue;

            return wrapper;
        }
        private static Grid GetCheckBoxElement(InputContext Context)
        {
            Grid wrapper = new();
            wrapper.RowDefinitions.Add(new RowDefinition() { Height = new(50) });
            CheckBox Box = new()
            {
                IsChecked = (bool)Context.Value,
                Content = Context.Title,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Box.SetResourceReference(StyleProperty, "SlimCheckBox");
            Box.Checked += (object sender, RoutedEventArgs e) =>
            {
                Context.Value = true;
            };
            Box.Unchecked += (object sender, RoutedEventArgs e) =>
            {
                Context.Value = false;
            };
            wrapper.Children.Add(Box);
            Context.Value = Box.IsChecked;
            return wrapper;
        }
        private void InitializeView()
        {
            if (Contexts == null) return;
            for (int i = 0; i < Contexts.Count; i++)
            {
                InputContext Context = Contexts[i];
                FrameworkElement InputElement = Context.Mode switch
                {
                    InputContextMode.TextBox => GetTextBoxElement(Context),
                    InputContextMode.CheckBox => GetCheckBoxElement(Context),
                    _ => GetComboBoxElement(Context),
                };
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

        public T? GetValue<T>(int Index)
        {
            if (Contexts == null) return default;
            InputContext Context = Contexts[Index];
            T Value = (T)Context.Value;
            return Value;
        }
    }
}
