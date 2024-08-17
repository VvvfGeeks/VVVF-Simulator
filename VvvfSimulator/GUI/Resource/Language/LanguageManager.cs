using System;
using System.Globalization;
using System.Windows;

namespace VvvfSimulator.GUI.Resource.Language
{
    public enum Language
    {
        JaJp, EnUs
    }
    public static class LanguageManager
    {
        private static ResourceDictionary? LanguageDictionary = null;

        public static string GetLanguageFilePath(this Language lang)
        {
            return lang switch
            {
                Language.JaJp => "/GUI/Resource/Language/ja-jp.xaml",
                Language.EnUs => "/GUI/Resource/Language/en-us.xaml",
                _ => "/GUI/Resource/Language/en-us.xaml",
            };
        }

        public static object? GetObject(string name)
        {
            return Application.Current.Resources[name];
        }

        public static string GetString(string name)
        {
            try
            {
                return (string)Application.Current.Resources[name];
            }
            catch
            {
                return "_";
            }
        }

        public static string GetStringWithNewLine(string name)
        {
            return GetString(name).Replace("\\n", Environment.NewLine);
        }

        public static void SetLanguage(this Language theme)
        {
            if (LanguageDictionary == null) return;
            LanguageDictionary.Source = new System.Uri(theme.GetLanguageFilePath(), System.UriKind.Relative);
        }

        public static void SetLanguage()
        {
            SetLanguage(GetSystemLanguage());
        }

        public static Language GetSystemLanguage()
        {
            return CultureInfo.CurrentCulture.Name switch
            {
                "ja-JP" => Language.JaJp,
                _ => Language.EnUs,
            };
        }

        public static void Initialize()
        {
            for (int i = 0; i < Application.Current.Resources.MergedDictionaries.Count; i++)
            {
                ResourceDictionary dictionary = Application.Current.Resources.MergedDictionaries[i];
                if (!dictionary.Source.OriginalString.Equals(Language.JaJp.GetLanguageFilePath())) continue;
                LanguageDictionary = dictionary;
                return;
            }
        }
    }
}
