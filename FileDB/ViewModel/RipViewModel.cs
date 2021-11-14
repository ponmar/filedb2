using System;
using System.Collections.ObjectModel;
using FileDBInterface;

namespace FileDB.ViewModel
{
    public class DeceasedPerson
    {
        public string Name { get; set; }
        public string DateOfBirth { get; set; }
        public string Deceased { get; set; }
        public int Age { get; set; }
    }

    public class RipViewModel : ViewModelBase
    {
        public ObservableCollection<DeceasedPerson> Persons { get; set; } = new ObservableCollection<DeceasedPerson>();

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
                        Deceased = person.deceased,
                        Age = Utils.GetAgeInYears(dateOfBirth, deceased),
                    }); ;
                }
            }
        }
    }
}
