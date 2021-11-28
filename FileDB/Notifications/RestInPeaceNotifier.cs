using System;
using System.Collections.Generic;
using System.Linq;
using FileDBInterface;
using FileDBInterface.Model;

namespace FileDB.Notifications
{
    public static class RestInPeaceNotifier
    {
        public static List<Notification> GetRestInPeaceNotifications(IEnumerable<PersonModel> persons)
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
