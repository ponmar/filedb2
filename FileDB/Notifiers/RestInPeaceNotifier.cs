﻿using System;
using System.Collections.Generic;
using System.Linq;
using FileDB.Notifications;
using FileDBInterface.Model;

namespace FileDB.Notifiers;

public class RestInPeaceNotifier : INotifier
{
    private readonly IEnumerable<PersonModel> persons;

    public RestInPeaceNotifier(IEnumerable<PersonModel> persons)
    {
        this.persons = persons;
    }

    public IEnumerable<INotification> Run()
    {
        var today = DateTime.Today;
        List<INotification> notifications = [];

        foreach (var person in persons.Where(x => x.DateOfBirth is not null))
        {
            if (person.Deceased is not null)
            {
                var deceased = DatabaseParsing.ParsePersonDeceasedDate(person.Deceased);
                if (deceased.Month == today.Month &&
                    deceased.Day == today.Day)
                {
                    var personName = $"{person.Firstname} {person.Lastname}";
                    notifications.Add(new PersonRestInPeaceNotification(personName));
                }
            }
        }

        return notifications;
    }
}
