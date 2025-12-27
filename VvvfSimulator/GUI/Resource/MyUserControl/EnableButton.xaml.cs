using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using VvvfSimulator.GUI.Resource.Theme;

namespace VvvfSimulator.GUI.Resource.MyUserControl
{
    /// <summary>
    /// EnableButton.xaml の相互作用ロジック
    /// </summary>
    public partial class EnableButton : UserControl
    {

        public event EventHandler? OnClicked;

        private bool Enabled = false;
        private bool Activated = false;
        private readonly double ButtonDotMargin = 5.0;

        public EnableButton()
        {
            InitializeComponent();
            ButtonDotMargin = (double)FindResource("ButtonDotMargin");
            SetAppearance();
        }
        private static Color GetForeColor(bool Status) => ThemeManager.GetColor(Status ? "EnableSwitchForeColorEnabled" : "EnableSwitchForeColorDisabled");
        private static Color GetBackColor(bool Status) => ThemeManager.GetColor(Status ? "EnableSwitchBackColorEnabled" : "EnableSwitchBackColorDisabled");
        private static Color GetPressedBackColor(bool Status) => ThemeManager.GetColor(Status ? "EnableSwitchBackColorEnabledPressed" : "EnableSwitchBackColorDisabledPressed");
        private void SetAppearance()
        {
            if (Activated)
                Button.Background = new SolidColorBrush(GetPressedBackColor(Enabled));
            else
            {
                Button.Background = new SolidColorBrush(GetBackColor(Enabled));
                ButtonDot.Fill = new SolidColorBrush(GetForeColor(Enabled));
            }

            ButtonDot.BeginAnimation(MarginProperty, null);
            ButtonDot.HorizontalAlignment = Enabled ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            ButtonDot.Margin = new Thickness(ButtonDotMargin);

        }
        public void SetToggled(bool Enabled, bool Animate = false)
        {
            this.Enabled = Enabled;

            if (!Animate)
            {
                SetAppearance();
                return;
            }

            double AnimateMargin = Button.ActualWidth - ButtonDot.ActualWidth - ButtonDotMargin;
            ButtonDot.HorizontalAlignment = Enabled ? HorizontalAlignment.Left : HorizontalAlignment.Right;
            ThicknessAnimation MoveAnimation = new()
            {
                From = new Thickness(Enabled ? AnimateMargin : ButtonDotMargin, ButtonDotMargin, Enabled ? ButtonDotMargin : AnimateMargin, ButtonDotMargin),
                To = new Thickness(ButtonDotMargin),
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            ColorAnimation ForeColorAnimation = new()
            {
                From = GetForeColor(!Enabled),
                To = GetForeColor(Enabled),
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };
            ColorAnimation BackColorAnimation = new()
            {
                From = GetBackColor(!Enabled),
                To = GetBackColor(Enabled),
                Duration = TimeSpan.FromMilliseconds(200),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            ButtonDot.BeginAnimation(MarginProperty, MoveAnimation);
            ButtonDot.Fill.BeginAnimation(SolidColorBrush.ColorProperty, ForeColorAnimation);
            Button.Background.BeginAnimation(SolidColorBrush.ColorProperty, BackColorAnimation);

            MoveAnimation.Completed += (s, e) => SetAppearance();

        }

        public bool IsToggled()
        {
            return Enabled;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Activated)
            {
                Activated = false;
                SetToggled(!Enabled, true);
                OnClicked?.Invoke(this, EventArgs.Empty);
            }
        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Activated = true;
            SetAppearance();
        }

        private void Grid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Activated) return;
            Activated = false;
            SetAppearance();
        }
    }
}
