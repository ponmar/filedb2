using System.Linq;
using System.Windows;
using FileDB.Configuration;
using FileDB.Migrators;
using FileDB.Notifiers;
using FileDB.Validators;
using TextCopy;

namespace FileDB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Utils.SetInvariantCulture();

            bool demoModeEnabled = e.Args.Any(x => x == "--demo");

            var appDataConfig = new AppDataConfig<Config>(Utils.ApplicationName);

            var model = Model.Model.Instance;

            if (demoModeEnabled)
            {
                model.Config = DefaultConfigs.CreateDemo();
                model.AddNotification(NotificationType.Info, "Demo configuration enabled. Have fun!");
            }
            else if (!appDataConfig.FileExists())
            {
                model.AddNotification(NotificationType.Warning, $"No local {Utils.ApplicationName} configuration file exists. Loading demo configuration.");
                model.Config = DefaultConfigs.CreateDemo();
            }
            else
            {
                var config = appDataConfig.Read() ?? DefaultConfigs.Default;
                model.Config = new ConfigMigrator().Migrate(config, DefaultConfigs.Default);

                var validator = new ConfigValidator();
                var result = validator.Validate(model.Config);
                if (!result.IsValid)
                {
                    model.AddNotification(NotificationType.Error, "Configuration not valid");
                    Dialogs.Default.ShowErrorDialog(result);
                }

                model.StartFileBrowsingPlugins();
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.StackTrace != null)
            {
                ClipboardService.SetText(e.Exception.StackTrace);
            }
            var message = $"Unhandled exception: {e.Exception.GetType().Name} ({e.Exception.Message}). Stacktrace has been copied to clipboard.";
            Dialogs.Default.ShowErrorDialog(message);
        }
    }
}
