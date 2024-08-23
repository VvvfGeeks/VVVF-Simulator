using System;
using System.Globalization;
using System.Windows;

namespace VvvfSimulator.GUI.Resource.Language
{
    public enum Language
    {
        JaJp, EnUs, KoKr, ZhCn
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
                Language.KoKr => "/GUI/Resource/Language/ko-kr.xaml",
                Language.ZhCn => "/GUI/Resource/Language/zh-cn.xaml",
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

        public static void SetLanguage(this Language lang)
        {
            if (LanguageDictionary == null) return;
            Properties.Settings.Default.Language = (int)lang;
            Properties.Settings.Default.Save();
            LanguageDictionary.Source = new System.Uri(lang.GetLanguageFilePath(), System.UriKind.Relative);
        }

        public static Language GetSystemLanguage()
        {
            return CultureInfo.CurrentCulture.Name switch
            {
                "ja-JP" => Language.JaJp,
                "ko-KR" => Language.KoKr,
                "zh-CN" => Language.ZhCn,
                _ => Language.EnUs,
            };
        }

        public static Language GetApplicationLanguage()
        {
            int ApplicationLanguage = Properties.Settings.Default.Language;
            if (ApplicationLanguage == -1) return GetSystemLanguage();
            return (Language)ApplicationLanguage;
        }

        public static void Initialize()
        {
            for (int i = 0; i < Application.Current.Resources.MergedDictionaries.Count; i++)
            {
                ResourceDictionary dictionary = Application.Current.Resources.MergedDictionaries[i];
                if (!dictionary.Source.OriginalString.Equals(Language.JaJp.GetLanguageFilePath())) continue;
                LanguageDictionary = dictionary;
                break;
            }
            GetApplicationLanguage().SetLanguage();
        }
    }
}
