using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FileDB.Configuration;
using FileDB.Dialogs;
using FileDB.Extensions;
using FileDB.Lang;
using FileDB.Migrators;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Validators;
using FileDB.ViewModels;
using FileDB.Views;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.DatabaseAccess.SQLite;
using FileDBInterface.FilesystemAccess;
using Microsoft.Extensions.Logging;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace FileDB;

public partial class App : Application
{
    public const string ConfigFileExtension = ".FileDB";
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
            Bootstrapper.Bootstrap();
            Bootstrapper.StartServices();

            var loggerFactory = ServiceLocator.Resolve<ILoggerFactory>();

            // Needed to fix sorting with Swedish characters right in comboboxes and datagrids
            //Utils.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));

            var dialogs = ServiceLocator.Resolve<IDialogs>();
            var fileSystem = ServiceLocator.Resolve<IFileSystem>();

            if (desktop.Args?.Length != 1)
            {
                await dialogs.ShowErrorDialogAsync(Strings.AppNoConfigSelected);
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
                await dialogs.ShowErrorDialogAsync(string.Format(Strings.AppInvalidCommandLineArgument, configPath));
                desktop.Shutdown(1);
                return;
            }

            Config config;
            if (fileSystem.File.Exists(configPath))
            {
                config = configPath.FromJson<Config>(fileSystem);

                // Config is null for an empty json file
                config = config is null ? DefaultConfigs.Default :
                    new ConfigMigrator().Migrate(config, DefaultConfigs.Default);
            }
            else
            {
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

            IDatabaseAccess dbAccess = fileSystem.File.Exists(databasePath) ?
                new SqLiteDatabaseAccess(databasePath, loggerFactory) :
                new NoDatabaseAccess();

            var notifications = new List<Notification>();

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
                            notifications.Add(new(NotificationSeverity.Info, string.Format(Strings.NotificationDatabaseMigration, dbMigration.FromVersion, dbMigration.ToVersion), DateTime.Now));
                        }
                        else
                        {
                            notifications.Add(new(NotificationSeverity.Error, string.Format(Strings.NotificationDatabaseMigrationError, dbMigration.FromVersion, dbMigration.ToVersion, dbMigration.Exception.Message), DateTime.Now));
                        }
                    }
                }
                catch (Exception e)
                {
                    await dialogs.ShowErrorDialogAsync(Strings.AppUnableToCreateDatabaseBackupBeforeMigration, e);
                }
            }

            var filesystemAccess = new FilesystemAccess(fileSystem, loggerFactory, filesRootDirectory);

            var configUpdater = ServiceLocator.Resolve<IConfigUpdater>();
            configUpdater.InitConfig(applicationFilePaths, config, dbAccess, filesystemAccess);

            if (!config.ReadOnly && !ServiceLocator.Resolve<IFilesWritePermissionChecker>().HasWritePermission)
            {
                config = config with { ReadOnly = true };
                configUpdater.UpdateConfig(config);
                notifications.Add(new(NotificationSeverity.Info, Strings.NotificationNoWritePermission, DateTime.Now));
            }

            Messenger.Send(new SetTheme(config.Theme));

            var notificationsHandling = ServiceLocator.Resolve<INotificationManagement>();
            notifications.ForEach(notificationsHandling.AddNotification);

            // Only load views and viewmodels when this method did not called shutdown above
            desktop.MainWindow = new MainWindow();

            ServiceLocator.Resolve<SettingsViewModel>().IsDirty = false;

            notificationsHandling.DismissNotification(Strings.SettingsUnsavedSettingsNotification);
            this.RegisterForEvent<ConfigEdited>(x =>
            {
                if (x.HasChanges)
                {
                    notificationsHandling.AddNotification(new(NotificationSeverity.Info, Strings.SettingsUnsavedSettingsNotification, DateTime.Now));
                }
                else
                {
                    notificationsHandling.DismissNotification(Strings.SettingsUnsavedSettingsNotification);
                }
            });
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            throw new NotSupportedException();
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
