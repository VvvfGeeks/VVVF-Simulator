using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace VvvfSimulator.GUI.Resource.Theme
{
    public enum ColorTheme
    {
        Light, Dark
    }

    public static class ThemeManager
    {
        private static ResourceDictionary? ColorThemeDictionary = null;

        public static string GetColorThemeFilePath(this ColorTheme theme)
        {
            return theme switch
            {
                ColorTheme.Light => "/GUI/Resource/Theme/WhiteThemeColor.xaml",
                ColorTheme.Dark => "/GUI/Resource/Theme/BlackThemeColor.xaml",
                _ => "/GUI/Resource/Theme/WhiteThemeColor.xaml",
            };
        }

        public static object? GetObject(string name)
        {
            return Application.Current.Resources[name];
        }
        public static Color GetColor(string name)
        {
            return (Color)Application.Current.Resources[name];
        }
        public static Brush GetBrush(string name)
        {
            return (Brush)Application.Current.Resources[name];
        }

        public static void SetColorTheme(this ColorTheme theme)
        {
            if (ColorThemeDictionary == null) return;
            Properties.Settings.Default.ColorTheme = (int)theme;
            Properties.Settings.Default.Save();
            ColorThemeDictionary.Source = new System.Uri(theme.GetColorThemeFilePath(), System.UriKind.Relative);
        }

        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        public static extern bool ShouldSystemUseDarkMode();
        public static ColorTheme GetSystemTheme()
        {
            return ShouldSystemUseDarkMode() switch
            {
                true => ColorTheme.Dark,
                _ => ColorTheme.Light,
            };
        }
        public static ColorTheme GetApplicationTheme()
        {
            int ApplicationTheme = Properties.Settings.Default.ColorTheme;
            if (ApplicationTheme == -1) return GetSystemTheme();
            return (ColorTheme)ApplicationTheme;
        }

        public static void InitializeColorTheme()
        {
            for (int i = 0; i < Application.Current.Resources.MergedDictionaries.Count; i++)
            {
                ResourceDictionary dictionary = Application.Current.Resources.MergedDictionaries[i];
                if (!dictionary.Source.OriginalString.Equals(ColorTheme.Dark.GetColorThemeFilePath())) continue;
                ColorThemeDictionary = dictionary;
                break;
            }
            GetApplicationTheme().SetColorTheme();
        }

    }
}
