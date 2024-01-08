using System;
using System.Linq;
using System.Windows;
using FileDB.Configuration;
using FileDB.Migrators;
using FileDB.Notifiers;
using FileDB.Validators;
using FileDBInterface.DatabaseAccess.SQLite;
using FileDBInterface.FilesystemAccess;
using TextCopy;
using System.Collections.Generic;
using FileDB.Model;
using System.IO.Abstractions;
using FileDB.Resources;
using System.Globalization;
using System.IO;
using FileDB.Extensions;
using FileDBInterface.DatabaseAccess;
using QuestPDF.Infrastructure;

namespace FileDB
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const string ConfigFileExtension = ".FileDB";
        private const string DatabaseFileExtension = ".db";

        private void Application_Startup(object sender, StartupEventArgs startupEventArgs)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            Bootstrapper.Bootstrap();

            // Needed to fix sorting with Swedish characters right in comboboxes and datagrids
            Utils.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));

            var dialogs = ServiceLocator.Resolve<IDialogs>();

            if (startupEventArgs.Args.Length != 1)
            {
                dialogs.ShowErrorDialog("No .FileDB file selected! Double click on your .FileDB file or specify its path via a command line argument.");
                Shutdown(1);
                return;
            }

            var configPath = startupEventArgs.Args.First();
            if (!Path.IsPathFullyQualified(configPath))
            {
                configPath = Path.GetFullPath(configPath);
            }

            var filesRootDirectory = Path.GetDirectoryName(configPath)!;
            var databaseFilename = Path.GetFileNameWithoutExtension(configPath) + DatabaseFileExtension;
            var databasePath = Path.Combine(filesRootDirectory, databaseFilename);
            var applicationFilePaths = new ApplicationFilePaths(filesRootDirectory, configPath, databasePath);

            if (!configPath.EndsWith(ConfigFileExtension))
            {
                dialogs.ShowErrorDialog($"Command line argument ({configPath}) does not have file extension FileDB");
                Shutdown(1);
                return;
            }

            var fileSystem = ServiceLocator.Resolve<IFileSystem>();

            Config config;
            if (fileSystem.File.Exists(configPath))
            {
                config = configPath.FromJson<Config>(fileSystem);
                config = new ConfigMigrator().Migrate(config, DefaultConfigs.Default);
            }
            else
            {
                // Note: new config is stored when manually saved
                config = DefaultConfigs.Default;
            }            

            SetUiCulture(config.Language);

            var validator = new ConfigValidator();
            var result = validator.Validate(config);
            if (!result.IsValid)
            {
                dialogs.ShowErrorDialog(result);
                Shutdown(1);
                return;
            }

            var notifications = new List<Notification>();
            if (filesRootDirectory.EndsWith("demo"))
            {
                notifications.Add(new(NotificationType.Info, Strings.StartupNotificationDemoConfigurationEnabled, DateTime.Now));
            }

            IDatabaseAccess dbAccess = fileSystem.File.Exists(databasePath) ?
                new SqLiteDatabaseAccess(databasePath) :
                new NoDatabaseAccess();

            if (dbAccess.NeedsMigration)
            {
                try
                {
                    new FileBackup(fileSystem, applicationFilePaths.DatabasePath).CreateBackup();

                    var results = dbAccess.Migrate();
                    foreach (var dbMigration in results)
                    {
                        if (dbMigration.Exception is null)
                        {
                            notifications.Add(new(NotificationType.Info, string.Format(Strings.NotificationDatabaseMigration, dbMigration.FromVersion, dbMigration.ToVersion), DateTime.Now));
                        }
                        else
                        {
                            notifications.Add(new(NotificationType.Error, string.Format(Strings.NotificationDatabaseMigrationError, dbMigration.FromVersion, dbMigration.ToVersion, dbMigration.Exception.Message), DateTime.Now));
                        }
                    }
                }
                catch (Exception e)
                {
                    dialogs.ShowErrorDialog("Unable to create database backup before database migration", e);                    
                }
            }

            var filesystemAccess = new FilesystemAccess(fileSystem, filesRootDirectory);

            var configUpdater = ServiceLocator.Resolve<IConfigUpdater>();
            configUpdater.InitConfig(applicationFilePaths, config, dbAccess, filesystemAccess);

            var notificationsHandling = ServiceLocator.Resolve<INotificationHandling>();
            notifications.ForEach(notificationsHandling.AddNotification);

            // Only load views and viewmodels when this method did not called shutdown above
            StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
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
            if (e.Exception.StackTrace is not null)
            {
                ClipboardService.SetText(e.Exception.StackTrace);
            }
            var message = $"Unhandled exception: {e.Exception.GetType().Name} ({e.Exception.Message}). Stacktrace has been copied to clipboard.";
            var dialogs = ServiceLocator.Resolve<IDialogs>();
            dialogs.ShowErrorDialog(message);
        }
    }
}
