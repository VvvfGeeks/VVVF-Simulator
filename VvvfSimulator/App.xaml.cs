using System.Windows;
using VvvfSimulator.GUI.Resource.Language;
using VvvfSimulator.GUI.Resource.Theme;
using VvvfSimulator.Yaml.VvvfSound;

namespace VvvfSimulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public void Application_Startup(object sender, StartupEventArgs e)
        {
            ThemeManager.InitializeColorTheme();
            LanguageManager.Initialize();
            if (e.Args.Length > 0)
            {
                YamlVvvfManage.HasArgs = true;
                YamlVvvfManage.StartupArgs = e.Args;
            }
        }
    }
}
