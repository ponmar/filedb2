using FileDBApp.Model;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Globalization;

namespace FileDBApp.ViewModel
{
    public class FileFormat
    {
        public List<PersonModel> Persons { get; set; }
    }

    public class Person
    {
        public string Name => $"{person.Firstname} {person.Lastname}";
        public string Birthday { get; }
        public int DaysLeft { get; }
        public string DaysLeftStr { get; }
        public int Age { get; }

        private readonly PersonModel person;

        public Person(PersonModel person)
        {
            this.person = person;

            var dateOfBirth = ParsePersonsDateOfBirth(person.DateOfBirth);
            Birthday = dateOfBirth.ToString("d MMMM");
            DaysLeft = GetDaysToNextBirthday(dateOfBirth);
            Age = GetYearsAgo(DateTime.Now, dateOfBirth);

            if (DaysLeft == 0)
            {
                DaysLeftStr = $"Turned {Age} today!";
            }
            else if (DaysLeft == 1)
            {
                DaysLeftStr = $"Turns {Age + 1} tomorrow!";
            }
            else if (DaysLeft <= 14)
            {
                DaysLeftStr = $"Turns {Age + 1} in {DaysLeft} days";
            }
            else
            {
                DaysLeftStr = string.Empty;
            }
        }

        public static DateTime ParsePersonsDateOfBirth(string dateOfBirthStr)
        {
            return DateTime.ParseExact(dateOfBirthStr, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None);
        }

        private static int GetDaysToNextBirthday(DateTime birthday)
        {
            var today = DateTime.Today;
            var next = birthday.AddYears(today.Year - birthday.Year);

            if (next < today)
            {
                next = next.AddYears(1);
            }

            return (next - today).Days;
        }

        public static int GetYearsAgo(DateTime now, DateTime dateTime)
        {
            int yearsAgo = now.Year - dateTime.Year;

            try
            {
                if (new DateTime(dateTime.Year, now.Month, now.Day) < dateTime)
                {
                    yearsAgo--;
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Current date did not exist the year that person was born
            }

            return yearsAgo;
        }
    }

    public class MainViewModel : ViewModelBase
    {
        public ObservableCollection<Person> Persons { get; } = new();

        public MainViewModel()
        {
            _ = LoadMauiAsset();
        }

        private async Task LoadMauiAsset()
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync("DatabaseExport.json");
            using var reader = new StreamReader(stream);
            var contents = reader.ReadToEnd();

            var data = JsonConvert.DeserializeObject<FileFormat>(contents);

            var persons = data.Persons.Where(x => x.DateOfBirth != null && x.Deceased == null).Select(x => new Person(x)).ToList();
            persons.Sort(new PersonsByDaysLeftUntilBirthdaySorter());
            persons.ForEach(x => Persons.Add(x));
        }
    }

    public class PersonsByDaysLeftUntilBirthdaySorter : IComparer<Person>
    {
        public int Compare(Person x, Person y)
        {
            if (x.DaysLeft == y.DaysLeft)
            {
                return x.Name.CompareTo(y.Name);
            }

            return x.DaysLeft.CompareTo(y.DaysLeft);
        }
    }
}
