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

        public static Brush GetBrush(string name)
        {
            return (Brush)Application.Current.Resources[name];
        }

        public static void SetColorTheme(this ColorTheme theme)
        {
            if (ColorThemeDictionary == null) return;
            ColorThemeDictionary.Source = new System.Uri(theme.GetColorThemeFilePath(), System.UriKind.Relative);
        }

        public static void InitializeColorTheme()
        {
            for (int i = 0; i < Application.Current.Resources.MergedDictionaries.Count; i++)
            {
                ResourceDictionary dictionary = Application.Current.Resources.MergedDictionaries[i];
                if (!dictionary.Source.OriginalString.Equals(ColorTheme.Dark.GetColorThemeFilePath())) continue;
                ColorThemeDictionary = dictionary;
                return;
            }
        }

    }
}
