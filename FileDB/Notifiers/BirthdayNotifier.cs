using System;
using System.Collections.Generic;
using System.Linq;
using FileDB.Lang;
using FileDB.Notifications;
using FileDBInterface.Model;

namespace FileDB.Notifiers;

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

    public List<INotification> Run()
    {
        var today = DateTime.Today;
        List<INotification> notifications = [];

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
                notifications.Add(isDeceased ?
                    new BirthdayForDeceasedNotification(personName) :
                    new BirthdayNotification(personName));
            }
        }

        return notifications;
    }
}
