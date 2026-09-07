using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using FileDB.Configuration;
using FileDB.Dialogs;
using FileDB.Infrastructure;
using FileDB.Lang;
using FileDB.Model;
using FileDB.Notifications;
using FileDB.Services;
using FileDB.ViewModels;
using FileDB.Views;
using System;
using System.Linq;

namespace FileDB;

public partial class App : Application
{
    public const string ConfigFileExtension = ".FileDB";

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        //BindingPlugins.DataValidators.RemoveAt(0);

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

            Bootstrapper.Bootstrap();
            Bootstrapper.StartServices();

            // Needed to fix sorting with Swedish characters right in comboboxes and datagrids
            //Utils.SetCulture(CultureInfo.GetCultureInfo("sv-SE"));

            var dialogs = ServiceLocator.Resolve<IDialogs>();

            if (desktop.Args?.Length != 1)
            {
                await dialogs.ShowErrorDialogAsync(Strings.AppNoConfigSelected);
                desktop.Shutdown(1);
                return;
            }
            
            var startupResult = await ServiceLocator.Resolve<IApplicationStartupService>().StartAsync(desktop.Args.First());
            if (!startupResult.Succeeded)
            {
                if (startupResult.ValidationResult is not null)
                {
                    await dialogs.ShowErrorDialogAsync(startupResult.ValidationResult);
                }
                else if (startupResult.ErrorMessage is not null)
                {
                    await dialogs.ShowErrorDialogAsync(startupResult.ErrorMessage);
                }

                desktop.Shutdown(1);
                return;
            }

            var config = startupResult.Config!;
            var notifications = startupResult.Notifications;
            SetUiCulture(config.Language);

            Messenger.Send(new SetTheme(config.Theme));

            var notificationsHandling = ServiceLocator.Resolve<INotificationManagement>();
            foreach (var notification in notifications)
            {
                notificationsHandling.AddNotification(notification);
            }

            // Only load views and viewmodels when this method did not called shutdown above
            desktop.MainWindow = new MainWindow();

            ServiceLocator.Resolve<SettingsViewModel>().IsDirty = false;

            notificationsHandling.DismissNotifications<SettingsUnsavedNotification>();
            this.RegisterForEvent<ConfigEdited>(x =>
            {
                if (x.HasChanges)
                {
                    notificationsHandling.AddNotification(new SettingsUnsavedNotification());
                }
                else
                {
                    notificationsHandling.DismissNotifications<SettingsUnsavedNotification>();
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
