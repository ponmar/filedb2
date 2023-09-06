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
using FileDB.Model;
using System.IO.Abstractions;
using FileDB.Resources;

namespace FileDB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string DemoCommandLineArgument = "--demo";

        private void Application_Startup(object sender, StartupEventArgs startupEventArgs)
        {
            Bootstrapper.Bootstrap();
            Utils.SetInvariantCulture();

            var appDataConfig = new AppDataConfig<Config>(Utils.ApplicationName, ServiceLocator.Resolve<IFileSystem>());

            var configUpdater = ServiceLocator.Resolve<IConfigUpdater>();
            Config config;

            var notifications = new List<Notification>();

            var demoModeEnabled = startupEventArgs.Args.Contains(DemoCommandLineArgument);
            if (demoModeEnabled)
            {
                config = DefaultConfigs.CreateDemo();
                SetUiCulture(config.CultureOverride);
                notifications.Add(new(NotificationType.Info, Strings.StartupNotificationDemoConfigurationEnabled, DateTime.Now));
            }
            else if (!appDataConfig.FileExists())
            {
                config = DefaultConfigs.CreateDemo();
                SetUiCulture(config.CultureOverride);
                notifications.Add(new(NotificationType.Warning, string.Format(Strings.StartupNotificationNoConfigurationFile, Utils.ApplicationName), DateTime.Now));
            }
            else
            {
                config = appDataConfig.Read() ?? DefaultConfigs.Default;
                config = new ConfigMigrator().Migrate(config, DefaultConfigs.Default);
                SetUiCulture(config.CultureOverride);

                var validator = new ConfigValidator();
                var result = validator.Validate(config);
                if (!result.IsValid)
                {
                    notifications.Add(new(NotificationType.Error, Strings.StartupNotificationConfigurationNotValid, DateTime.Now));
                    var dialogs = ServiceLocator.Resolve<IDialogs>();
                    dialogs.ShowErrorDialog(result);
                }
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

            var fileSystem = ServiceLocator.Resolve<IFileSystem>();
            var filesystemAccess = new FilesystemAccess(fileSystem) { FilesRootDirectory = config.FilesRootDirectory };
            var notifierFactory = new NotifierFactory();

            configUpdater.InitConfig(config, dbAccess, filesystemAccess, notifierFactory);

            var notificationsHandling = ServiceLocator.Resolve<INotificationHandling>();
            notifications.ForEach(notificationsHandling.AddNotification);
        }

        private void SetUiCulture(string? culture)
        {
            // Note: system UI culture will be used as default when no culture specified
            if (culture is not null)
            {
                Utils.SetUICulture(culture);
            }
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception.StackTrace != null)
            {
                ClipboardService.SetText(e.Exception.StackTrace);
            }
            var message = $"Unhandled exception: {e.Exception.GetType().Name} ({e.Exception.Message}). Stacktrace has been copied to clipboard.";
            var dialogs = ServiceLocator.Resolve<IDialogs>();
            dialogs.ShowErrorDialog(message);
        }
    }
}
