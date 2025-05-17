using System;
using System.Collections.Generic;
using System.IO;
using FileDB.Lang;
using FileDB.Model;
using FileDB.Notifications;

namespace FileDB.Notifiers;

public class DemoNotifier : INotifier
{
    private readonly bool isDemo;
    private const string DemoFilename = $"Demo{App.ConfigFileExtension}";

    public DemoNotifier(IConfigProvider configProvider)
    {
        isDemo = Path.GetFileName(configProvider.FilePaths.ConfigPath) == DemoFilename;
    }

    public List<Notification> Run()
    {
        if (isDemo)
        {
            return [new(NotificationSeverity.Info, Strings.StartupNotificationDemoConfigurationEnabled, DateTime.Now)];
        }
        return [];
    }
}
