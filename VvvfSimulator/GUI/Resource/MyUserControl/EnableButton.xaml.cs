using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Resource.Theme;

namespace VvvfSimulator.GUI.Resource.MyUserControl
{
    /// <summary>
    /// EnableButton.xaml の相互作用ロジック
    /// </summary>
    public partial class EnableButton : UserControl
    {

        public event EventHandler? OnClicked;

        public EnableButton()
        {
            InitializeComponent();
            UpdateState();
        }

      
        private bool Enabled = false;
        private bool Activated = false;

        private void SetDisplayStatus()
        {
            Status.Content = Enabled ? LanguageManager.GetString("Generic.Status.Enabled") : LanguageManager.GetString("Generic.Status.Disabled");
        }

        private void SetBorderStatus()
        {
            Brush EnabledBrush = ThemeManager.GetBrush("EnableSwitchBackgroundEnabledBrush");
            Brush EnabledPressedBrush = ThemeManager.GetBrush("EnableSwitchBackgroundEnabledPressedBrush");
            Brush DisabledBrush = ThemeManager.GetBrush("EnableSwitchBackgroundDisabledBrush");
            Brush DisabledPressedBrush = ThemeManager.GetBrush("EnableSwitchBackgroundDisabledPressedBrush");
            Brush EnabledTextBrush = ThemeManager.GetBrush("EnableSwitchTextColorEnabledBrush");
            Brush DisabledTextBrush = ThemeManager.GetBrush("EnableSwitchTextColorDisabledBrush");

            if (Activated)
                Button.Background = Enabled ? EnabledPressedBrush : DisabledPressedBrush;
            else
            {
                Button.Background = Enabled ? EnabledBrush : DisabledBrush;
                Status.Foreground = Enabled ? EnabledTextBrush : DisabledTextBrush;
            }
        }

        public void SetToggled(bool enabled)
        {
            Enabled = enabled;
            UpdateState();
        }

        public bool IsToggled()
        {
            return Enabled;
        }

        public void UpdateState()
        {
            SetDisplayStatus();
            SetBorderStatus();
        }        

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Activated)
            {
                Activated = false;
                Enabled = !Enabled;
                OnClicked?.Invoke(this, EventArgs.Empty);
                UpdateState();
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Activated = true;
            UpdateState();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            Activated = false;
            UpdateState();
        }
    }

    public class FontSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double d = ((double)(value) / 2.0);
            if (d == 0) d = 1;
            return d;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
