using System;
using System.Collections.Generic;
using System.Linq;
using FileDB.Notifications;
using FileDBInterface;

namespace FileDB.ViewModel
{
    public class DeceasedPerson
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string DeceasedStr { get; set; }
        public DateTime Deceased { get; set; }
        public int Age { get; set; }
        public string ProfileFileIdPath { get; set; }
    }

    public class PersonsByDeceasedSorter : IComparer<DeceasedPerson>
    {
        public int Compare(DeceasedPerson x, DeceasedPerson y)
        {
            return x.Deceased.CompareTo(y.Deceased);
        }
    }

    public class RipViewModel : ViewModelBase
    {
        public List<DeceasedPerson> Persons { get; set; } = new();

        public RipViewModel()
        {
            var persons = Utils.DatabaseWrapper.GetPersons();
            foreach (var person in persons)
            {
                if (person.DateOfBirth != null && person.Deceased != null)
                {
                    var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth);
                    var deceased = DatabaseParsing.ParsePersonsDeceased(person.Deceased);

                    Persons.Add(new DeceasedPerson()
                    {
                        Name = person.Firstname + " " + person.Lastname,
                        DateOfBirth = person.DateOfBirth,
                        DeceasedStr = person.Deceased,
                        Deceased = deceased,
                        Age = DatabaseUtils.GetYearsAgo(deceased, dateOfBirth),
                        ProfileFileIdPath = person.ProfileFileId != null ? Utils.DatabaseWrapper.ToAbsolutePath(Utils.DatabaseWrapper.GetFileById(person.ProfileFileId.Value).Path) : string.Empty,
                    }) ;
                }
            }

            Persons.Sort(new PersonsByDeceasedSorter());
            Persons.Reverse();

            if (Utils.Config.RipReminder)
            {
                var notifications = RestInPeaceNotifier.GetRestInPeaceNotifications(persons);
                foreach (var notification in notifications)
                {
                    Utils.ShowNotification(notification);
                }
            }
        }
    }
}
