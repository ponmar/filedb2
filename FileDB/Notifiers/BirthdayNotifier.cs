using System;
using System.Collections.Generic;
using System.Linq;
using FileDBInterface;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.Notifiers
{
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
            List<Notification> notifications = new();

            foreach (var person in persons.Where(x => x.DateOfBirth != null))
            {
                var isDeceased = person.Deceased != null;
                var checkPerson =
                    (!isDeceased && birthdayNotificationFor == BirthdayNotificationFor.Alive) ||
                    (isDeceased && (birthdayNotificationFor == BirthdayNotificationFor.Deceased));

                if (!checkPerson)
                {
                    continue;
                }

                var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth);
                if (dateOfBirth.Month == today.Month &&
                    dateOfBirth.Day == today.Day)
                {
                    if (isDeceased)
                    {
                        notifications.Add(new Notification(NotificationType.Info, $"Today is the birthday for {person.Firstname} {person.Lastname}!", DateTime.Now));
                    }
                    else
                    {
                        notifications.Add(new Notification(NotificationType.Info, $"Happy Birthday {person.Firstname} {person.Lastname}!", DateTime.Now));
                    }
                }
            }

            return notifications;
        }
    }
}
