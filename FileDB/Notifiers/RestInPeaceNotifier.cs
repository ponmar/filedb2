﻿using System;
using System.Collections.Generic;
using System.Linq;
using FileDBInterface;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.Notifiers
{
    public class RestInPeaceNotifier : Notifier
    {
        private readonly IEnumerable<PersonModel> persons;

        public RestInPeaceNotifier(IEnumerable<PersonModel> persons)
        {
            this.persons = persons;
        }

        public List<Notification> GetNotifications()
        {
            var today = DateTime.Today;
            List<Notification> notifications = new();

            foreach (var person in persons.Where(x => x.DateOfBirth != null))
            {
                if (person.Deceased != null)
                {
                    var deceased = DatabaseParsing.ParsePersonsDeceased(person.Deceased);
                    if (deceased.Month == today.Month &&
                        deceased.Day == today.Day)
                    {
                        notifications.Add(new Notification(NotificationType.Info, $"Rest in Peace {person.Firstname} {person.Lastname}!"));
                    }
                }
            }

            return notifications;
        }
    }
}
