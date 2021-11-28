﻿using System;
using System.Collections.Generic;
using System.Linq;
using FileDBInterface;
using FileDBInterface.Model;

namespace FileDB.Notifications
{
    public enum BirthdayNotificationFor { Alive, Deceased }

    public static class BirthdayNotifier
    {
        public static List<Notification> GetBirthdayNotifications(IEnumerable<PersonModel> persons, BirthdayNotificationFor birthdayNotificationFor)
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
                        notifications.Add(new Notification(NotificationType.Info, $"Today is the birthday for {person.Firstname} {person.Lastname}!"));
                    }
                    else
                    {
                        notifications.Add(new Notification(NotificationType.Info, $"Happy Birthday {person.Firstname} {person.Lastname}!"));
                    }
                }
            }

            return notifications;
        }
    }
}