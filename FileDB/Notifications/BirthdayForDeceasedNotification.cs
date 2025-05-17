using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class BirthdayForDeceasedNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public BirthdayForDeceasedNotification(string personName)
    {
        Message = string.Format(Strings.BirthdayNotifierTodayIsTheBirthdayFor, personName);
    }
}