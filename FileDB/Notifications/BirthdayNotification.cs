using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class BirthdayNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public BirthdayNotification(string personName)
    {
        Message = string.Format(Strings.BirthdayNotifierHappyBirthday, personName);
    }
}
