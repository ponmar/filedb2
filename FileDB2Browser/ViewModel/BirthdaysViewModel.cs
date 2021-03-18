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
    }

    public class BirthdaysViewModel
    {
        private readonly FileDB2Handle fileDB2Handle;

        public ObservableCollection<PersonBirthday> Birthdays { get; set; } = new ObservableCollection<PersonBirthday>();

        public BirthdaysViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;

            foreach (var person in fileDB2Handle.GetPersons())
            {
                if (person.dateofbirth != null && DateTime.TryParse(person.dateofbirth, out var dateOfBirth))
                {
                    Birthdays.Add(new PersonBirthday()
                    {
                        Name = person.firstname + " " + person.lastname,
                        Birthday = dateOfBirth.ToString("d"),
                        DaysLeft = GetDaysLeft(dateOfBirth),
                    });
                }
            }
        }

        private int GetDaysLeft(DateTime birthday)
        {
            var today = DateTime.Today;
            var next = birthday.AddYears(today.Year - birthday.Year);

            if (next < today)
                next = next.AddYears(1);

            return (next - today).Days;
        }
    }
}
