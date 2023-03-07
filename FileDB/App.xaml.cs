using System;
using System.Linq;
using System.Windows;
using FileDB.Configuration;
using FileDB.Migrators;
using FileDB.Notifiers;
using FileDB.Validators;
using FileDBInterface.DbAccess.SQLite;
using FileDBInterface.DbAccess;
using FileDBInterface.FilesystemAccess;
using TextCopy;
using System.Collections.Generic;
using System.Globalization;

namespace FileDB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs startupEventArgs)
        {
            Utils.SetInvariantCulture();

            bool demoModeEnabled = startupEventArgs.Args.Any(x => x == "--demo");

            var appDataConfig = new AppDataConfig<Config>(Utils.ApplicationName);

            var model = Model.Model.Instance;
            Config config;

            var notifications = new List<Notification>();

            if (demoModeEnabled)
            {
                config = DefaultConfigs.CreateDemo();
                notifications.Add(new(NotificationType.Info, "Demo configuration enabled. Have fun!", DateTime.Now));
            }
            else if (!appDataConfig.FileExists())
            {
                notifications.Add(new(NotificationType.Warning, $"No local {Utils.ApplicationName} configuration file exists. Loading demo configuration.", DateTime.Now));
                config = DefaultConfigs.CreateDemo();
            }
            else
            {
                config = appDataConfig.Read() ?? DefaultConfigs.Default;
                config = new ConfigMigrator().Migrate(config, DefaultConfigs.Default);

                var validator = new ConfigValidator();
                var result = validator.Validate(config);
                if (!result.IsValid)
                {
                    notifications.Add(new(NotificationType.Error, "Configuration not valid", DateTime.Now));
                    model.Dialogs.ShowErrorDialog(result);
                }
            }

            if (config.CultureOverride != null)
            {
                CultureInfo.CurrentCulture = new CultureInfo(config.CultureOverride, false);
            }

            IDbAccess dbAccess;
            try
            {
                dbAccess = new SqLiteDbAccess(config.Database);
            }
            catch (Exception e)
            {
                notifications.Add(new Notification(NotificationType.Error, e.Message, DateTime.Now));
                dbAccess = new NoDbAccess();
            }

            var filesystemAccess = new FilesystemAccess() { FilesRootDirectory = config.FilesRootDirectory };
            var notifierFactory = new NotifierFactory();

            model.InitConfig(config, dbAccess, filesystemAccess, notifierFactory);

            notifications.ForEach(model.AddNotification);
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.StackTrace != null)
            {
                ClipboardService.SetText(e.Exception.StackTrace);
            }
            var message = $"Unhandled exception: {e.Exception.GetType().Name} ({e.Exception.Message}). Stacktrace has been copied to clipboard.";
            Model.Model.Instance.Dialogs.ShowErrorDialog(message);
        }
    }
}
