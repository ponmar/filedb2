using System;
using System.Collections.Generic;
using FileDB.Notifiers;
using FileDBInterface;

namespace FileDB.ViewModel
{
    public class PersonBirthday
    {
        public string Name { get; set; }
        public string Birthday { get; set; }
        public int DaysLeft { get; set; }
        public string DaysLeftStr { get; set; }
        public int Age { get; set; }
        public string ProfileFileIdPath { get; set; }
    }

    public class PersonsByDaysLeftUntilBirthdaySorter : IComparer<PersonBirthday>
    {
        public int Compare(PersonBirthday x, PersonBirthday y)
        {
            if (x.DaysLeft == y.DaysLeft)
            {
                return x.Name.CompareTo(y.Name);
            }

            return x.DaysLeft.CompareTo(y.DaysLeft);
        }
    }

    public class BirthdaysViewModel : ViewModelBase
    {
        public List<PersonBirthday> Persons { get; } = new();

        public BirthdaysViewModel()
        {
            var persons = Utils.DbAccess.GetPersons();

            foreach (var person in persons)
            {
                if (person.DateOfBirth != null && person.Deceased == null)
                {
                    var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth);

                    var p = new PersonBirthday()
                    {
                        Name = person.Firstname + " " + person.Lastname,
                        Birthday = dateOfBirth.ToString("d MMMM"),
                        DaysLeft = DatabaseUtils.GetDaysToNextBirthday(dateOfBirth),
                        Age = DatabaseUtils.GetYearsAgo(DateTime.Now, dateOfBirth),
                        ProfileFileIdPath = person.ProfileFileId != null ? Utils.FilesystemAccess.ToAbsolutePath(Utils.DbAccess.GetFileById(person.ProfileFileId.Value).Path) : string.Empty,
                    };

                    if (p.DaysLeft == 0)
                    {
                        p.DaysLeftStr = $"Turned {p.Age} today!";
                    }
                    else if (p.DaysLeft == 1)
                    {
                        p.DaysLeftStr = $"Turns {p.Age + 1} tomorrow!";
                    }
                    else if (p.DaysLeft <= 14)
                    {
                        p.DaysLeftStr = p.DaysLeft <= 14 ? $"Turns {p.Age + 1} in {p.DaysLeft} days" : string.Empty;
                    }
                    else
                    {
                        p.DaysLeftStr = string.Empty;
                    }

                    Persons.Add(p);
                }
            }

            Persons.Sort(new PersonsByDaysLeftUntilBirthdaySorter());

            List<Notification> notifications = new();

            if (Utils.Config.MissingFilesRootDirNotification)
            {
                var notifier = new MissingFilesRootDirNotifier(Utils.Config.FilesRootDirectory);
                notifications.AddRange(notifier.GetNotifications());
            }

            if (Utils.Config.BirthdayReminder)
            {
                var notifier = new BirthdayNotifier(persons, BirthdayNotificationFor.Alive);
                notifications.AddRange(notifier.GetNotifications());
            }

            if (Utils.Config.BirthdayReminderForDeceased)
            {
                var notifier = new BirthdayNotifier(persons, BirthdayNotificationFor.Deceased);
                notifications.AddRange(notifier.GetNotifications());
            }

            foreach (var notification in notifications)
            {
                Utils.ShowNotification(notification);
            }
        }
    }
}
