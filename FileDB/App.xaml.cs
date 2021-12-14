using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using FileDB.Config;
using FileDB.Notifiers;
using FileDB.Validators;

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

            var appDataConfig = new AppDataConfig<Config.Config>(Utils.ApplicationName);

            if (demoModeEnabled)
            {
                Utils.Config = DefaultConfigs.CreateDemo();
                Model.Model.Instance.AddNotification(new Notification(NotificationType.Info, "Demo configuration enabled, have fun!"));
            }
            else if (!appDataConfig.FileExists())
            {
                Utils.ShowInfoDialog($"No local {Utils.ApplicationName} configuration file exists. Loading demo configuration.");
                Utils.Config = DefaultConfigs.CreateDemo();
            }
            else
            {
                Utils.Config = appDataConfig.Read() ?? DefaultConfigs.Default;

                var validator = new ConfigValidator();
                var result = validator.Validate(Utils.Config);
                if (!result.IsValid)
                {
                    Utils.ShowErrorDialog(result);
                }
            }
        }
    }
}
