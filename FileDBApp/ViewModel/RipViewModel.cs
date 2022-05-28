using FileDBApp.Comparers;
using FileDBApp.Model;
using FileDBApp.Services;
using FileDBApp.View;
using System.Collections.ObjectModel;

namespace FileDBApp.ViewModel
{
    public class DeceasedPerson
    {
        public string Header => $"{Name} {Age}";

        public int Age { get; }

        public string Name => $"{PersonModel.Firstname} {PersonModel.Lastname}";

        public string Details => $"{PersonModel.DateOfBirth} - {PersonModel.Deceased}";

        public DateTime Deceased { get; }

        public PersonModel PersonModel { get; }

        public DeceasedPerson(PersonModel personModel)
        {
            PersonModel = personModel;

            var dateOfBirth = Utils.ParsePersonsDateOfBirth(PersonModel.DateOfBirth);
            Deceased = Utils.ParsePersonsDeceased(PersonModel.Deceased);
            Age = Utils.GetYearsAgo(Deceased, dateOfBirth);
        }
    }

    public class RipViewModel : ViewModelBase
    {
        public ObservableCollection<DeceasedPerson> Persons { get; } = new();

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

        public Command GoToPersonDetailsCommand { get; }

        public Command UpdatePersonsCommand { get; }

        private readonly PersonService personService;

        public RipViewModel(PersonService personService)
        {
            this.personService = personService;
            
            UpdatePersonsCommand = new Command(async () => await UpdatePersonsAsync());
            UpdatePersonsCommand.Execute(null);

            GoToPersonDetailsCommand = new Command(async (x) => await GoToPersonDetailsAsync((x as DeceasedPerson).PersonModel));
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

            var personsVms = persons.Where(x => x.DateOfBirth != null && x.Deceased != null).Select(x => new DeceasedPerson(x)).ToList();
            personsVms.Sort(new PersonsByDeceasedSorter());
            personsVms.Reverse();

            Persons.Clear();
            personsVms.ForEach(x => Persons.Add(x));
            
            IsBusy = false;
        }
    }
}
