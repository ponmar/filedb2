using System;
using System.Collections.Generic;
using System.Linq;
using FileDB.Resources;
using FileDBShared.Model;

namespace FileDB.Notifiers;

public class RestInPeaceNotifier : INotifier
{
    private readonly IEnumerable<PersonModel> persons;

    public RestInPeaceNotifier(IEnumerable<PersonModel> persons)
    {
        this.persons = persons;
    }

    public List<Notification> Run()
    {
        var today = DateTime.Today;
        List<Notification> notifications = [];

        foreach (var person in persons.Where(x => x.DateOfBirth != null))
        {
            if (person.Deceased != null)
            {
                var deceased = DatabaseParsing.ParsePersonDeceasedDate(person.Deceased);
                if (deceased.Month == today.Month &&
                    deceased.Day == today.Day)
                {
                    var personName = $"{person.Firstname} {person.Lastname}";
                    notifications.Add(new Notification(NotificationType.Info, string.Format(Strings.RestInPeaceNotifierRestInPeace, personName), DateTime.Now));
                }
            }
        }

        return notifications;
    }
}
