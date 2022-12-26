using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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

    public partial class BirthdaysViewModel : ObservableObject
    {
        public ObservableCollection<Person> Persons { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        bool isBusy;

        public bool IsNotBusy => !isBusy;

        [ObservableProperty]
        bool isRefreshing;

        private readonly PersonService personService;

        public BirthdaysViewModel(PersonService personService)
        {
            this.personService = personService;
            _ = UpdatePersonsAsync();
        }

        [RelayCommand]
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

        [RelayCommand]
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
