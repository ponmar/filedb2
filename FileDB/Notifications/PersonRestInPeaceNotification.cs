using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class PersonRestInPeaceNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public PersonRestInPeaceNotification(string personName)
    {
        Message = string.Format(Strings.RestInPeaceNotifierRestInPeace, personName);
    }
}