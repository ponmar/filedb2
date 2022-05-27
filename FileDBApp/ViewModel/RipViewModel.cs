using FileDBApp.Comparers;
using FileDBApp.Model;
using FileDBApp.Services;
using System.Collections.ObjectModel;

namespace FileDBApp.ViewModel
{
    public class DeceasedPerson
    {
        public string Header => $"{Name} {Age}";

        public int Age { get; }

        public string Name => $"{person.Firstname} {person.Lastname}";

        public string Details => $"{person.DateOfBirth} - {person.Deceased}";

        public DateTime Deceased { get; }

        private readonly PersonModel person;

        public DeceasedPerson(PersonModel person)
        {
            this.person = person;

            var dateOfBirth = Utils.ParsePersonsDateOfBirth(person.DateOfBirth);
            Deceased = Utils.ParsePersonsDeceased(person.Deceased);
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

        public Command UpdatePersonsCommand { get; }

        private readonly PersonService personService;

        public RipViewModel(PersonService personService)
        {
            this.personService = personService;
            
            UpdatePersonsCommand = new Command(async () => await UpdatePersonsAsync());
            UpdatePersonsCommand.Execute(null);
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
