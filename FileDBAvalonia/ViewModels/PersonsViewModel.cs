using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Model;
using FileDBShared;
using FileDBShared.Model;

namespace FileDBAvalonia.ViewModels;

public record Person(int Id, string Firstname, string Lastname, string? Description, string? DateOfBirth, string? Deceased, int Age, int? ProfileFileId, Sex Sex);

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

    public ObservableCollection<Person> Persons { get; } = [];

    private readonly List<Person> allPersons = [];

    [ObservableProperty]
    private Person? selectedPerson;

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

        var filesWithPerson = dbAccessProvider.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson.Id }).ToList();
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
    private void PersonSelection(Person parameter)
    {
        SelectedPerson = parameter;
    }

    private void ReloadPersons()
    {
        allPersons.Clear();
        var personVms = personsRepository.Persons.Select(pm => new Person(pm.Id, pm.Firstname, pm.Lastname, pm.Description, pm.DateOfBirth, pm.Deceased, GetPersonAge(pm), pm.ProfileFileId, pm.Sex));
        foreach (var personVm in personVms)
        {
            allPersons.Add(personVm);
        }
        FilterPersons();
    }

    private void FilterPersons()
    {
        Persons.Clear();
        foreach (var tag in allPersons.Where(x =>
            x.Firstname.Contains(FilterText) ||
            x.Lastname.Contains(FilterText) ||
            $"{x.Firstname} {x.Lastname}".Contains(FilterText) ||
            (x.Description is not null && x.Description.Contains(FilterText))))
        {
            Persons.Add(tag);
        }
    }

    private int GetPersonAge(PersonModel person)
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
