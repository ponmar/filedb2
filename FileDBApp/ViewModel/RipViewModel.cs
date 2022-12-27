using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBApp.Comparers;
using FileDBApp.Services;
using FileDBApp.View;
using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.ObjectModel;

namespace FileDBApp.ViewModel;

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

        var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(PersonModel.DateOfBirth);
        Deceased = DatabaseParsing.ParsePersonDeceasedDate(PersonModel.Deceased);
        Age = DatabaseUtils.GetYearsAgo(Deceased, dateOfBirth);
    }
}

public partial class RipViewModel : ObservableObject
{
    public ObservableCollection<DeceasedPerson> Persons { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    bool isBusy;

    public bool IsNotBusy => !isBusy;

    private readonly PersonService personService;

    public RipViewModel(PersonService personService)
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

        var personsVms = persons.Where(x => x.DateOfBirth != null && x.Deceased != null).Select(x => new DeceasedPerson(x)).ToList();
        personsVms.Sort(new PersonsByDeceasedSorter());
        personsVms.Reverse();

        Persons.Clear();
        personsVms.ForEach(x => Persons.Add(x));
        
        IsBusy = false;
    }
}
