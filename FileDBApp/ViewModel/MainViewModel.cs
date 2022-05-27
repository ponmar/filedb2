using FileDBApp.Comparers;
using FileDBApp.Model;
using Newtonsoft.Json;
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

    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<Person> Persons { get; } = new();

        public MainViewModel()
        {
            _ = LoadPersons();
        }

        private async Task LoadPersons()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("DatabaseExport.json");
            using var reader = new StreamReader(stream);
            var contents = reader.ReadToEnd();

            var data = JsonConvert.DeserializeObject<ExportedDatabaseFileFormat>(contents);

            var persons = data.Persons.Where(x => x.DateOfBirth != null && x.Deceased == null).Select(x => new Person(x)).ToList();
            persons.Sort(new PersonsByDaysLeftUntilBirthdaySorter());
            persons.ForEach(x => Persons.Add(x));
        }
    }
}
