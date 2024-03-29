using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Extensions;
using FileDBAvalonia.Model;
using FileDBShared;
using FileDBShared.Model;

namespace FileDBAvalonia.ViewModels;

public class PersonWithAge : PersonModel
{
    public int Age { get; set; }
}

public partial class PersonsViewModel : ObservableObject
{
    [ObservableProperty]
    private string filterText = string.Empty;

    partial void OnFilterTextChanged(string value)
    {
        FilterPersons();
    }

    [ObservableProperty]
    private bool readWriteMode;

    public ObservableCollection<PersonWithAge> Persons { get; } = [];

    private readonly List<PersonWithAge> allPersons = [];

    [ObservableProperty]
    private PersonWithAge? selectedPerson;

    private readonly IConfigProvider configProvider;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly IPersonsRepository personsRepository;

    public PersonsViewModel(IConfigProvider configProvider, IDatabaseAccessProvider dbAccessProvider, IDialogs dialogs, IPersonsRepository personsRepository)
    {
        this.configProvider = configProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.dialogs = dialogs;
        this.personsRepository = personsRepository;

        readWriteMode = !configProvider.Config.ReadOnly;

        ReloadPersons();

        this.RegisterForEvent<ConfigUpdated>((x) =>
        {
            ReadWriteMode = !this.configProvider.Config.ReadOnly;
        });

        this.RegisterForEvent<PersonsUpdated>((x) =>
        {
            ReloadPersons();
        });
    }

    [RelayCommand]
    private async Task RemovePersonAsync()
    {
        if (!await dialogs.ShowConfirmDialogAsync($"Remove {SelectedPerson!.Firstname} {SelectedPerson.Lastname}?"))
        {
            return;
        }

        var filesWithPerson = dbAccessProvider.DbAccess.SearchFilesWithPersons([SelectedPerson.Id]).ToList();
        if (filesWithPerson.Count == 0 || await dialogs.ShowConfirmDialogAsync($"Person is used in {filesWithPerson.Count} files, remove anyway?"))
        {
            dbAccessProvider.DbAccess.DeletePerson(SelectedPerson.Id);
            Messenger.Send<PersonEdited>();
        }
    }

    [RelayCommand]
    private void EditPerson()
    {
        dialogs.ShowAddPersonDialogAsync(SelectedPerson!.Id);
    }

    [RelayCommand]
    private void AddPerson()
    {
        dialogs.ShowAddPersonDialogAsync();
    }

    [RelayCommand]
    private void PersonSelection(PersonWithAge parameter)
    {
        SelectedPerson = parameter;
    }

    private void ReloadPersons()
    {
        allPersons.Clear();
        var persons = personsRepository.Persons.Select(pm => new PersonWithAge
        {
            Id = pm.Id,
            Firstname = pm.Firstname,
            Lastname = pm.Lastname,
            Description = pm.Description,
            Deceased = pm.Deceased,
            ProfileFileId = pm.ProfileFileId,
            Sex = pm.Sex,
            Age = GetPersonAge(pm),
        });
        foreach (var person in persons)
        {
            allPersons.Add(person);
        }
        FilterPersons();
    }

    private void FilterPersons()
    {
        Persons.Clear();
        foreach (var tag in allPersons.Where(x => x.MatchesTextFilter(FilterText)))
        {
            Persons.Add(tag);
        }
    }

    private static int GetPersonAge(PersonModel person)
    {
        if (person.DateOfBirth is not null)
        {
            var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth);
            if (person.Deceased is not null)
            {
                var deceased = DatabaseParsing.ParsePersonDeceasedDate(person.Deceased);
                return TimeUtils.GetAgeInYears(deceased, dateOfBirth);
            }
            else
            {
                return TimeUtils.GetAgeInYears(DateTime.Now, dateOfBirth);
            }
        }

        return -1;
    }
}
