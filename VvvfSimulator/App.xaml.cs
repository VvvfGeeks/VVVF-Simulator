using System.Runtime.InteropServices;
using System.Windows;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Resource.Theme;

namespace VvvfSimulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        [DllImport("UXTheme.dll", SetLastError = true, EntryPoint = "#138")]
        public static extern bool ShouldSystemUseDarkMode();

        public void Application_Startup(object sender, StartupEventArgs e)
        {
            ThemeManager.InitializeColorTheme();
            if (ShouldSystemUseDarkMode()) ColorTheme.Dark.SetColorTheme();
            else ColorTheme.Light.SetColorTheme();

            LanguageManager.Initialize();
            LanguageManager.SetLanguage();
        }

    }
}
