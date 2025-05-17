using System;
using FileDB.Lang;

namespace FileDB.Notifications;

public class PersonBirthdayNotification : INotification
{
    public NotificationSeverity Severity => NotificationSeverity.Info;
    public string Message { get; }
    public DateTime DateTime { get; } = DateTime.Now;

    public PersonBirthdayNotification(string personName)
    {
        Message = string.Format(Strings.BirthdayNotifierHappyBirthday, personName);
    }
}
