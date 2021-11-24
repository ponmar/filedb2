using System;
using System.Collections.Generic;
using System.Linq;
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
            foreach (var person in Utils.DatabaseWrapper.GetPersons())
            {
                if (person.dateofbirth != null && person.deceased != null)
                {
                    var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.dateofbirth);
                    var deceased = DatabaseParsing.ParsePersonsDeceased(person.deceased);

                    Persons.Add(new DeceasedPerson()
                    {
                        Name = person.firstname + " " + person.lastname,
                        DateOfBirth = person.dateofbirth,
                        DeceasedStr = person.deceased,
                        Deceased = deceased,
                        Age = DatabaseUtils.GetYearsAgo(deceased, dateOfBirth),
                        ProfileFileIdPath = person.profilefileid != null ? Utils.DatabaseWrapper.ToAbsolutePath(Utils.DatabaseWrapper.GetFileById(person.profilefileid.Value).path) : string.Empty,
                    }) ;
                }
            }

            Persons.Sort(new PersonsByDeceasedSorter());
            Persons.Reverse();

            if (Utils.Config.RipReminder)
            {
                var today = DateTime.Today;
                foreach (var person in Persons.Where(x => x.Deceased == today))
                {
                    Utils.ShowInfoDialog($"Rest in Peace {person.Name}!");
                }
            }
        }
    }
}
