using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDB2Interface;

namespace FileDB2Browser.ViewModel
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
        public ObservableCollection<PersonBirthday> Birthdays { get; set; } = new ObservableCollection<PersonBirthday>();

        public BirthdaysViewModel(FileDB2Handle fileDB2Handle)
        {
            foreach (var person in fileDB2Handle.GetPersons())
            {
                if (person.dateofbirth != null && DateTime.TryParse(person.dateofbirth, out var dateOfBirth))
                {
                    Birthdays.Add(new PersonBirthday()
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
