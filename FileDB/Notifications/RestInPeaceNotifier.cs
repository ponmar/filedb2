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

            foreach (var person in persons.Where(x => x.dateofbirth != null))
            {
                if (person.deceased != null)
                {
                    var deceased = DatabaseParsing.ParsePersonsDeceased(person.deceased);
                    if (deceased.Month == today.Month &&
                        deceased.Day == today.Day)
                    {
                        notifications.Add(new Notification(NotificationType.Info, $"Rest in Peace {person.firstname} {person.lastname}!"));
                    }
                }
            }

            return notifications;
        }
    }
}
