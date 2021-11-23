using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using FileDB.Config;

namespace FileDB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            bool demoModeEnabled = e.Args.Any(x => x == "--demo");

            if (demoModeEnabled)
            {
                Utils.Config = DefaultConfigs.CreateDemo();
            }
            else
            {
                Utils.Config = new AppDataConfig<Config.Config>(Utils.ApplicationName).Read() ?? DefaultConfigs.Default;
            }
        }
    }
}
