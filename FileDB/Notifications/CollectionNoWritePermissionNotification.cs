using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class CollectionNoWritePermissionNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message => Strings.NotificationNoWritePermission;
    public DateTime DateTime { get; } = DateTime.Now;
}