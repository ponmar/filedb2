using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var person in Utils.FileDBHandle.GetPersons())
            {
                if (person.dateofbirth != null && person.deceased == null)
                {
                    var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.dateofbirth);

                    var p = new PersonBirthday()
                    {
                        Name = person.firstname + " " + person.lastname,
                        Birthday = dateOfBirth.ToString("d MMMM"),
                        DaysLeft = DatabaseUtils.GetDaysToNextBirthday(dateOfBirth),
                        Age = DatabaseUtils.GetYearsAgo(DateTime.Now, dateOfBirth),
                        ProfileFileIdPath = person.profilefileid != null ? Utils.FileDBHandle.ToAbsolutePath(Utils.FileDBHandle.GetFileById(person.profilefileid.Value).path) : string.Empty,
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

            if (Utils.Config.BirthdayReminder)
            {
                foreach (var person in Persons.Where(x => x.DaysLeft == 0))
                {
                    Utils.ShowInfoDialog($"Happy Birthday to {person.Name}!");
                }
            }
        }
    }
}
