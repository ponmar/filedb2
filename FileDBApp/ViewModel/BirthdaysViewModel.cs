using FileDBApp.Comparers;
using FileDBApp.Model;
using FileDBApp.Services;
using FileDBApp.View;
using System.Collections.ObjectModel;

namespace FileDBApp.ViewModel
{
    public class Person
    {
        public string Header => $"{Name} {Age}";
        public string Details => $"{Birthday} | {DaysLeftStr}";
        public string Name => $"{PersonModel.Firstname} {PersonModel.Lastname}";
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
                else
                {
                    return $"Turns {Age + 1} in {DaysLeft} days";
                }
            }
        }
        public int Age { get; }

        public PersonModel PersonModel { get; }

        public Person(PersonModel personModel)
        {
            PersonModel = personModel;

            var dateOfBirth = Utils.ParsePersonsDateOfBirth(personModel.DateOfBirth);
            Birthday = dateOfBirth.ToString("d MMMM");
            DaysLeft = Utils.GetDaysToNextBirthday(dateOfBirth);
            Age = Utils.GetYearsAgo(DateTime.Now, dateOfBirth);
        }
    }

    public class BirthdaysViewModel : ViewModelBase
    {
        public ObservableCollection<Person> Persons { get; } = new();

        public bool IsBusy
        {
            get => isBusy;
            set
            {
                if (SetProperty(ref isBusy, value))
                {
                    OnPropertyChanged(nameof(IsNotBusy));
                }
            }
        }
        private bool isBusy;

        public bool IsNotBusy => !IsBusy;

        public bool IsRefreshing
        {
            get => isRefreshing;
            set => SetProperty(ref isRefreshing, value);
        }
        private bool isRefreshing;

        public Command UpdatePersonsCommand { get; }

        public Command GoToPersonDetailsCommand { get; }

        private readonly PersonService personService;

        public BirthdaysViewModel(PersonService personService)
        {
            this.personService = personService;
            
            UpdatePersonsCommand = new Command(async () => await UpdatePersonsAsync());
            UpdatePersonsCommand.Execute(null);

            GoToPersonDetailsCommand = new Command(async (x) => await GoToPersonDetailsAsync((x as Person).PersonModel));
        }

        private async Task GoToPersonDetailsAsync(PersonModel personModel)
        {
            if (personModel is null)
            {
                return;
            }

            await Shell.Current.GoToAsync(nameof(PersonDetailsPage), true,
                new Dictionary<string, object>
                {
                    { "PersonModel", personModel },
                });
        }

        private async Task UpdatePersonsAsync()
        {
            IsBusy = true;
            var persons = await personService.GetPersons();

            var personsVms = persons.Where(x => x.DateOfBirth != null && x.Deceased == null).Select(x => new Person(x)).ToList();
            personsVms.Sort(new PersonsByDaysLeftUntilBirthdaySorter());

            Persons.Clear();
            personsVms.ForEach(x => Persons.Add(x));
            
            IsBusy = false;
            IsRefreshing = false;
        }
    }
}
