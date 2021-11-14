using System;
using System.Collections.Generic;

namespace FileDB.ViewModel
{
    public class DeceasedPerson
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string DeceasedStr { get; set; }
        public DateTime Deceased { get; set; }
        public int Age { get; set; }
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
            foreach (var person in Utils.FileDBHandle.GetPersons())
            {
                if (person.dateofbirth != null && person.deceased != null)
                {
                    var dateOfBirth = DatabaseUtils.ParseDateOfBirth(person.dateofbirth);
                    var deceased = DatabaseUtils.ParseDeceased(person.deceased);

                    Persons.Add(new DeceasedPerson()
                    {
                        Name = person.firstname + " " + person.lastname,
                        DateOfBirth = person.dateofbirth,
                        DeceasedStr = person.deceased,
                        Deceased = deceased,
                        Age = Utils.GetAgeInYears(dateOfBirth, deceased),
                    });
                }
            }

            Persons.Sort(new PersonsByDeceasedSorter());
            Persons.Reverse();
        }
    }
}
