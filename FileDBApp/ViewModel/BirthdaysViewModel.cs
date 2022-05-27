using FileDBApp.Comparers;
using FileDBApp.Model;
using FileDBApp.Services;
using System.Collections.ObjectModel;

namespace FileDBApp.ViewModel
{
    public class Person
    {
        public string Name => $"{person.Firstname} {person.Lastname}";
        public string Birthday { get; }
        public int DaysLeft { get; }
        public string DaysLeftStr
        {
            get
            {
                if (DaysLeft == 0)
                {
                    return $"Turned {Age} today!";
                }
                else if (DaysLeft == 1)
                {
                    return $"Turns {Age + 1} tomorrow!";
                }
                else if (DaysLeft <= 14)
                {
                    return $"Turns {Age + 1} in {DaysLeft} days";
                }
                return string.Empty;
            }
        }
        public int Age { get; }

        private readonly PersonModel person;

        public Person(PersonModel person)
        {
            this.person = person;

            var dateOfBirth = Utils.ParsePersonsDateOfBirth(person.DateOfBirth);
            Birthday = dateOfBirth.ToString("d MMMM");
            DaysLeft = Utils.GetDaysToNextBirthday(dateOfBirth);
            Age = Utils.GetYearsAgo(DateTime.Now, dateOfBirth);
        }
    }

    public class BirthdaysViewModel : ViewModelBase
    {
        public ObservableCollection<Person> Persons { get; } = new();

        public Command GetPersonsCommand { get; }

        private readonly PersonService personService;

        public BirthdaysViewModel(PersonService personService)
        {
            this.personService = personService;
            
            GetPersonsCommand = new Command(async () => await GetPersonsAsync());
            GetPersonsCommand.Execute(null);
        }

        private async Task GetPersonsAsync()
        {
            var persons = await personService.GetPersons();

            var personsVms = persons.Where(x => x.DateOfBirth != null && x.Deceased == null).Select(x => new Person(x)).ToList();
            personsVms.Sort(new PersonsByDaysLeftUntilBirthdaySorter());

            Persons.Clear();
            personsVms.ForEach(x => Persons.Add(x));
        }
    }
}
