using System;
using System.Linq;
using System.Windows;
using FileDB.Configuration;
using FileDB.Migrators;
using FileDB.Notifiers;
using FileDB.Validators;
using FileDBInterface.DbAccess.SQLite;
using FileDBInterface.FilesystemAccess;
using TextCopy;
using System.Collections.Generic;
using FileDB.Model;
using System.IO.Abstractions;
using FileDB.Resources;
using System.Globalization;
using System.IO;
using FileDB.Extensions;
using FileDBInterface.DbAccess;

namespace FileDB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string ConfigFileExtension = ".FileDB";

        private void Application_Startup(object sender, StartupEventArgs startupEventArgs)
        {
            Bootstrapper.Bootstrap();

            // Needed to fix sorting with Swedish characters right in comboboxes and datagrids
            Utils.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));

            var dialogs = ServiceLocator.Resolve<IDialogs>();

            if (startupEventArgs.Args.Length != 1)
            {
                dialogs.ShowErrorDialog("No files collection selected - use a command line argument");
                Shutdown();
                return;
            }

            var configPath = startupEventArgs.Args.First();
            var filesRootDirectory = Path.GetDirectoryName(configPath)!;
            var databaseFilename = Path.GetFileNameWithoutExtension(configPath) + ".db";
            var databasePath = Path.Combine(filesRootDirectory, databaseFilename);
            var applicationFilePaths = new ApplicationFilePaths(filesRootDirectory, configPath, databasePath);

            if (!configPath.EndsWith(ConfigFileExtension))
            {
                dialogs.ShowErrorDialog($"Command line argument ({configPath}) does not have file extension FileDB");
                Shutdown();
                return;
            }

            if (!File.Exists(configPath))
            {
                dialogs.ShowErrorDialog($"Specified FileDB config file does not exist: {configPath}");
                Shutdown();
                return;
            }

            var fileSystem = ServiceLocator.Resolve<IFileSystem>();
            var config = configPath.FromJson<Config>(fileSystem);

            config = new ConfigMigrator().Migrate(config, DefaultConfigs.Default);
            SetUiCulture(config.Language);

            var validator = new ConfigValidator();
            var result = validator.Validate(config);
            if (!result.IsValid)
            {
                dialogs.ShowErrorDialog(result);
                Shutdown();
                return;
            }

            var notifications = new List<Notification>();
            if (filesRootDirectory.EndsWith("demo"))
            {
                notifications.Add(new(NotificationType.Info, Strings.StartupNotificationDemoConfigurationEnabled, DateTime.Now));
            }

            IDbAccess dbAccess = fileSystem.File.Exists(databasePath) ?
                new SqLiteDbAccess(databasePath) :
                new NoDbAccess();

            var filesystemAccess = new FilesystemAccess(fileSystem, filesRootDirectory);
            var notifierFactory = new NotifierFactory();

            var configUpdater = ServiceLocator.Resolve<IConfigUpdater>();
            configUpdater.InitConfig(applicationFilePaths, config, dbAccess, filesystemAccess, notifierFactory);

            var notificationsHandling = ServiceLocator.Resolve<INotificationHandling>();
            notifications.ForEach(notificationsHandling.AddNotification);
        }

        private static void SetUiCulture(string? culture)
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
