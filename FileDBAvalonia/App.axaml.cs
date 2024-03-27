using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FileDBAvalonia.Configuration;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Extensions;
using FileDBAvalonia.Lang;
using FileDBAvalonia.Migrators;
using FileDBAvalonia.Model;
using FileDBAvalonia.Notifiers;
using FileDBAvalonia.Validators;
using FileDBAvalonia.ViewModels;
using FileDBAvalonia.Views;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.DatabaseAccess.SQLite;
using FileDBInterface.FilesystemAccess;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace FileDBAvalonia;

public partial class App : Application
{
    private const string ConfigFileExtension = ".FileDB";
    private const string DatabaseFileExtension = ".db";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        this.RegisterForEvent<Quit>((x) =>
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopApp)
            {
                desktopApp.Shutdown();
            }
        });

        this.RegisterForEvent<SetTheme>((x) =>
        {
            RequestedThemeVariant = x.Theme switch
            {
                Theme.Dark => ThemeVariant.Dark,
                Theme.Light => ThemeVariant.Light,
                Theme.Default or _ => ThemeVariant.Default,
            };
        });

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            Bootstrapper.Reset();
            Bootstrapper.StartServices();

            var args = desktop.Args;

            // Needed to fix sorting with Swedish characters right in comboboxes and datagrids
            //Utils.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));

            var dialogs = ServiceLocator.Resolve<IDialogs>();

            if (desktop.Args?.Length != 1)
            {
                await dialogs.ShowErrorDialogAsync("No .FileDB file selected! Double click on your .FileDB file or specify its path via a command line argument.");
                desktop.Shutdown(1);
                return;
            }

            var configPath = desktop.Args.First();
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
                await dialogs.ShowErrorDialogAsync($"Command line argument ({configPath}) does not have file extension FileDB");
                desktop.Shutdown(1);
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
                await dialogs.ShowErrorDialogAsync(result);
                desktop.Shutdown(1);
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
                    await dialogs.ShowErrorDialogAsync("Unable to create database backup before database migration", e);
                }
            }

            var filesystemAccess = new FilesystemAccess(fileSystem, filesRootDirectory);

            var configUpdater = ServiceLocator.Resolve<IConfigUpdater>();
            configUpdater.InitConfig(applicationFilePaths, config, dbAccess, filesystemAccess);

            var notificationsHandling = ServiceLocator.Resolve<INotificationHandling>();
            notifications.ForEach(notificationsHandling.AddNotification);

            Messenger.Send(new SetTheme(config.Theme));

            // Only load views and viewmodels when this method did not called shutdown above
            desktop.MainWindow = new MainWindow();
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel(ServiceLocator.Resolve<IConfigProvider>(), ServiceLocator.Resolve<INotificationsRepository>())
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void SetUiCulture(string? culture)
    {
        // Note: system UI culture will be used as default when no culture specified
        if (culture is not null)
        {
            Utils.SetUICulture(culture);
        }
    }
}
