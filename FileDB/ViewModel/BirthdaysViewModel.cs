using System;
using System.Collections.Generic;
using FileDBInterface;

namespace FileDB.ViewModel
{
    public class PersonBirthday
    {
        public string Name { get; set; }
        public string Birthday { get; set; }
        public int DaysLeft { get; set; }
        public int BornYearsAgo { get; set; }
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
                    var dateOfBirth = DatabaseUtils.ParseDateOfBirth(person.dateofbirth);

                    Persons.Add(new PersonBirthday()
                    {
                        Name = person.firstname + " " + person.lastname,
                        Birthday = dateOfBirth.ToString("d MMMM"),
                        DaysLeft = Utils.GetDaysToNextBirthday(dateOfBirth),
                        BornYearsAgo = Utils.GetYearsAgo(DateTime.Now, dateOfBirth),
                    });
                }
            }

            Persons.Sort(new PersonsByDaysLeftUntilBirthdaySorter());
        }
    }
}
