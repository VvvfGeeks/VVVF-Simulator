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
        public static bool HasArgs = false;
        public static string[] StartupArgs = [];
        public void Application_Startup(object sender, StartupEventArgs e)
        {
            ThemeManager.InitializeColorTheme();
            LanguageManager.Initialize();

            if (e.Args.Length > 0)
            {
                HasArgs = true;
                StartupArgs = e.Args;
            }
        }
    }
}
