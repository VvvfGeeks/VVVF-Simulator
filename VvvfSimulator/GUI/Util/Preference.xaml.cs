using System.Windows;
using System.Windows.Controls;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Resource.Theme;

namespace VvvfSimulator.GUI.Util
{
    /// <summary>
    /// Preference.xaml の相互作用ロジック
    /// </summary>
    public partial class Preference : Window
    {
        public Preference(Window Owner)
        {
            this.Owner = Owner;

            InitializeComponent();
            SetSelectorView();
        }

        private bool IgnoreSelectorUpdateEvent = false;
        private void SetSelectorView()
        {
            IgnoreSelectorUpdateEvent = true;
            ColorThemeSelector.ItemsSource = FriendlyNameConverter.GetColorThemeNames();
            ColorThemeSelector.SelectedValue = ThemeManager.GetApplicationTheme();

            LanguageSelector.ItemsSource = FriendlyNameConverter.GetLanguageNames();
            LanguageSelector.SelectedValue = LanguageManager.GetApplicationLanguage();
            IgnoreSelectorUpdateEvent = false;
        }

        private void SelectorChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IgnoreSelectorUpdateEvent) return;

            if (sender is not ComboBox box) return;
            string? tag = box.Tag.ToString();
            if (tag == null) return;

            if (tag.Equals("ColorTheme"))
            {
                ColorTheme Selected = (ColorTheme)box.SelectedValue;
                Selected.SetColorTheme();
            }
            else if (tag.Equals("Language"))
            {
                Language language = (Language)box.SelectedValue;
                language.SetLanguage();
            }

            SetSelectorView();
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
