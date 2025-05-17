using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class PersonBirthdayForDeceasedNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public PersonBirthdayForDeceasedNotification(string personName)
    {
        Message = string.Format(Strings.BirthdayNotifierTodayIsTheBirthdayFor, personName);
    }
}