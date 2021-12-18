using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FileDBInterface.DbAccess;

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
        public ObservableCollection<PersonBirthday> Persons { get; } = new();

        private readonly Model.Model model = Model.Model.Instance;

        public BirthdaysViewModel()
        {
            UpdatePersons();
            model.PersonsUpdated += Model_PersonsUpdated;
            model.DateChanged += Model_DateChanged;
        }

        private void Model_DateChanged(object sender, EventArgs e)
        {
            UpdatePersons();
        }

        private void Model_PersonsUpdated(object sender, EventArgs e)
        {
            UpdatePersons();
        }

        private void UpdatePersons()
        {
            var persons = new List<PersonBirthday>();

            foreach (var person in model.DbAccess.GetPersons())
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
                        ProfileFileIdPath = person.ProfileFileId != null ? model.FilesystemAccess.ToAbsolutePath(model.DbAccess.GetFileById(person.ProfileFileId.Value).Path) : string.Empty,
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

                    persons.Add(p);
                }
            }

            persons.Sort(new PersonsByDaysLeftUntilBirthdaySorter());

            Persons.Clear();
            persons.ForEach(x => Persons.Add(x));
        }
    }
}
