using System;
using System.Collections.Generic;
using System.Linq;
using FileDBAvalonia.Lang;
using FileDBShared.Model;

namespace FileDBAvalonia.Notifiers;

public enum BirthdayNotificationFor { Alive, Deceased }

public class BirthdayNotifier : INotifier
{
    private readonly IEnumerable<PersonModel> persons;
    private readonly BirthdayNotificationFor birthdayNotificationFor;

    public BirthdayNotifier(IEnumerable<PersonModel> persons, BirthdayNotificationFor birthdayNotificationFor)
    {
        this.persons = persons;
        this.birthdayNotificationFor = birthdayNotificationFor;
    }

    public List<Notification> Run()
    {
        var today = DateTime.Today;
        List<Notification> notifications = [];

        foreach (var person in persons.Where(x => x.DateOfBirth is not null))
        {
            var isDeceased = person.Deceased is not null;
            var checkPerson =
                (!isDeceased && birthdayNotificationFor == BirthdayNotificationFor.Alive) ||
                (isDeceased && (birthdayNotificationFor == BirthdayNotificationFor.Deceased));

            if (!checkPerson)
            {
                continue;
            }

            var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth!);
            if (dateOfBirth.Month == today.Month &&
                dateOfBirth.Day == today.Day)
            {
                var personName = $"{person.Firstname} {person.Lastname}";
                if (isDeceased)
                {
                    notifications.Add(new Notification(NotificationType.Info, string.Format(Strings.BirthdayNotifierTodayIsTheBirthdayFor, personName), DateTime.Now));
                }
                else
                {
                    notifications.Add(new Notification(NotificationType.Info, string.Format(Strings.BirthdayNotifierHappyBirthday, personName), DateTime.Now));
                }
            }
        }

        return notifications;
    }
}
