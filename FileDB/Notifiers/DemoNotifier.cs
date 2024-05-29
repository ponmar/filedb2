using System;
using System.Collections.Generic;
using System.IO;
using FileDB.Lang;
using FileDB.Model;

namespace FileDB.Notifiers;

public class DemoNotifier : INotifier
{
    private readonly bool isDemo;

    public DemoNotifier(IConfigProvider configProvider)
    {
        isDemo = Path.GetFileName(configProvider.FilePaths.ConfigPath) == App.DemoFilename;
    }

    public List<Notification> Run()
    {
        if (isDemo)
        {
            return [new(NotificationType.Info, Strings.StartupNotificationDemoConfigurationEnabled, DateTime.Now)];
        }
        return [];
    }
}
