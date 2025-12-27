using System;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace VvvfSimulator.GUI.Util
{
    public enum DialogBoxButton
    {
        Ok, Cancel, Abort, Retry, Ignore, Try, Continue, Yes, No
    }
    public enum DialogBoxIcon
    {
        None, Error, Question, Warning, Information, Ok
    }

    /// <summary>
    /// DialogBox.xaml の相互作用ロジック
    /// </summary>
    public partial class DialogBox : Window
    {
        private readonly DialogBoxButton[] BoxButtons;
        private readonly DialogBoxIcon BoxIcon;
        private DialogBoxButton? PressedButton = null;
        private bool ShowAnimationRan = false;
        private bool CloseAnimationRan = false;
        private Grid GetIcon(DialogBoxIcon Icon)
        {
            return Icon switch
            {
                DialogBoxIcon.None => IconNone,
                DialogBoxIcon.Error => IconError,
                DialogBoxIcon.Question => IconQuestion,
                DialogBoxIcon.Warning => IconWarning,
                DialogBoxIcon.Information => IconInformation,
                DialogBoxIcon.Ok => IconOk,
                _ => IconNone,
            };
        }
        private Button GetButton(DialogBoxButton Button)
        {
            return Button switch
            {
                DialogBoxButton.Ok => ButtonOk,
                DialogBoxButton.Cancel => ButtonCancel,
                DialogBoxButton.Abort => ButtonAbort,
                DialogBoxButton.Retry => ButtonRetry,
                DialogBoxButton.Ignore => ButtonIgnore,
                DialogBoxButton.Try => ButtonTry,
                DialogBoxButton.Continue => ButtonContinue,
                DialogBoxButton.Yes => ButtonYes,
                DialogBoxButton.No => ButtonNo,
                _ => ButtonOk,
            };
        }
        private static SystemSound? GetSystemSound(DialogBoxIcon Icon)
        {
            return Icon switch
            {
                DialogBoxIcon.None => null,
                DialogBoxIcon.Error => SystemSounds.Hand,
                DialogBoxIcon.Question => SystemSounds.Beep,
                DialogBoxIcon.Warning => SystemSounds.Beep,
                DialogBoxIcon.Information => SystemSounds.Beep,
                DialogBoxIcon.Ok => SystemSounds.Beep,
                _ => SystemSounds.Beep
            };
        }
        private void SetDialog()
        {
            GetIcon(BoxIcon).Visibility = Visibility.Visible;

            ButtonGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < BoxButtons.Length; i++)
            {
                ButtonGrid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 100 });
                Button BoxButton = GetButton(BoxButtons[i]);
                BoxButton.Visibility = Visibility.Visible;
                Grid.SetColumn(BoxButton, i);
            }
        }
        private void SetDialogResult(Button EventButton)
        {
            PressedButton = (DialogBoxButton)EventButton.Tag;
            Close();
        }
        public DialogBox(string Message, string Title, DialogBoxButton[] Button, DialogBoxIcon Icon, Window? Owner = null, bool IsDialog = false)
        {
            if (Owner == null) WindowStartupLocation = WindowStartupLocation.CenterScreen;
            else { this.Owner = Owner; WindowStartupLocation = WindowStartupLocation.CenterOwner; }
            InitializeComponent();
            WindowTitle.Content = Title;
            MessageLabel.Content = Message;
            BoxButtons = Button;
            BoxIcon = Icon;
            SetDialog();
            GetSystemSound(BoxIcon)?.Play();
            if (IsDialog) ShowDialog();
            else Show();
        }
        public static DialogBoxButton? Show(Window? Owner, string Message, string Title, DialogBoxButton[] Buttons, DialogBoxIcon Icon)
        {
            return Application.Current.Dispatcher.Invoke(() => new DialogBox(Message, Title, Buttons, Icon, Owner, true).PressedButton);
        }
        public static DialogBoxButton? Show(string Message, string Title, DialogBoxButton[] Buttons, DialogBoxIcon Icon)
        {
            return Application.Current.Dispatcher.Invoke(() => new DialogBox(Message, Title, Buttons, Icon, IsDialog: true).PressedButton);
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Opacity = 0.0;
        }
        private static readonly TimeSpan AnimationDuarion = TimeSpan.FromMilliseconds(100);
        private void OnContentRendered(object sender, EventArgs e)
        {
            if (ShowAnimationRan) return;
            ShowAnimationRan = true;

            DoubleAnimation OpacityAnimation = new()
            {
                From = 0.0,
                To = 1.0,
                Duration = AnimationDuarion,
            };
            DoubleAnimation LocationAnimation = new()
            {
                From = Top * 1.1,
                To = Top,
                Duration = AnimationDuarion,
            };
            DoubleAnimation ScaleAnimation = new()
            {
                From = 0.8,
                To = 1.0,
                Duration = AnimationDuarion,
            };
            BeginAnimation(OpacityProperty, OpacityAnimation);
            WindowScaleTransform.CenterX = ActualWidth / 2;
            WindowScaleTransform.CenterY = ActualHeight / 2;
            WindowScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, ScaleAnimation);
            WindowScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, ScaleAnimation);
            BeginAnimation(TopProperty, LocationAnimation);
        }
        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CloseAnimationRan) return;
            CloseAnimationRan = true;

            DoubleAnimation OpacityAnimation = new()
            {
                From = 1.0,
                To = 0.0,
                Duration = AnimationDuarion,
            };
            OpacityAnimation.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, OpacityAnimation);
            e.Cancel = true;
        }
        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button Button) return;
            SetDialogResult(Button);
        }
        private void OnKeydown(object sender, KeyEventArgs e)
        {
            if (BoxButtons.Length == 0) return;
            if (e.Key.Equals(Key.Enter)) SetDialogResult(GetButton(BoxButtons[0]));
            else if (e.Key.Equals(Key.Escape)) SetDialogResult(GetButton(BoxButtons[^1]));
        }
        private void OnWindowControlButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button btn) return;
            string? tag = btn.Tag.ToString();
            if (tag?.Equals("Close") ?? false)
                Close();
        }
    }
}
