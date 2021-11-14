using System;
using System.Collections.ObjectModel;
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

    public class BirthdaysViewModel : ViewModelBase
    {
        public ObservableCollection<PersonBirthday> Persons { get; set; } = new ObservableCollection<PersonBirthday>();

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
        }
    }
}
