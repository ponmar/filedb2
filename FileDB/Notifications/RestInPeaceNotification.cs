using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class RestInPeaceNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public RestInPeaceNotification(string personName)
    {
        Message = string.Format(Strings.RestInPeaceNotifierRestInPeace, personName);
    }
}