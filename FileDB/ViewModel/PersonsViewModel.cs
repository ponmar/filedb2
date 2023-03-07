using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Configuration;
using FileDB.Model;
using FileDB.Sorters;
using FileDBInterface.DbAccess;
using FileDBShared.Model;

namespace FileDB.ViewModel;

public record Person(int Id, string Firstname, string Lastname, string? Description, string? DateOfBirth, string? Deceased, int Age, int? ProfileFileId, Sex Sex);

public partial class PersonsViewModel : ObservableObject
{
    [ObservableProperty]
    private bool readWriteMode;

    public ObservableCollection<Person> Persons { get; } = new();

    [ObservableProperty]
    private Person? selectedPerson;

    private readonly IDbAccess dbAccess;

    public PersonsViewModel(Config config, IDbAccess dbAccess)
    {
        readWriteMode = !config.ReadOnly;
        this.dbAccess = dbAccess;

        ReloadPersons();

        WeakReferenceMessenger.Default.Register<ConfigLoaded>(this, (r, m) =>
        {
            ReadWriteMode = !m.Config.ReadOnly;
        });

        WeakReferenceMessenger.Default.Register<PersonsUpdated>(this, (r, m) =>
        {
            ReloadPersons();
        });
    }

    [RelayCommand]
    private void RemovePerson()
    {
        if (Dialogs.Instance.ShowConfirmDialog($"Remove {SelectedPerson!.Firstname} {SelectedPerson.Lastname}?"))
        {
            var filesWithPerson = dbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson.Id }).ToList();
            if (filesWithPerson.Count == 0 || Dialogs.Instance.ShowConfirmDialog($"Person is used in {filesWithPerson.Count} files, remove anyway?"))
            {
                dbAccess.DeletePerson(SelectedPerson.Id);
                WeakReferenceMessenger.Default.Send(new PersonsUpdated());
            }
        }
    }

    [RelayCommand]
    private void EditPerson()
    {
        Dialogs.Instance.ShowAddPersonDialog(SelectedPerson!.Id);
    }

    [RelayCommand]
    private void AddPerson()
    {
        Dialogs.Instance.ShowAddPersonDialog();
    }

    [RelayCommand]
    private void PersonSelection(Person parameter)
    {
        SelectedPerson = parameter;
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        var persons = dbAccess.GetPersons().ToList();
        persons.Sort(new PersonModelByNameSorter());
        var personVms = persons.Select(pm => new Person(pm.Id, pm.Firstname, pm.Lastname, pm.Description, pm.DateOfBirth, pm.Deceased, GetPersonAge(pm), pm.ProfileFileId, pm.Sex));
        foreach (var personVm in personVms)
        {
            Persons.Add(personVm);
        }
    }

    private int GetPersonAge(PersonModel person)
    {
        if (person.DateOfBirth != null)
        {
            var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth);
            if (person.Deceased != null)
            {
                var deceased = DatabaseParsing.ParsePersonDeceasedDate(person.Deceased);
                return DatabaseUtils.GetAgeInYears(deceased, dateOfBirth);
            }
            else
            {
                return DatabaseUtils.GetAgeInYears(DateTime.Now, dateOfBirth);
            }
        }

        return -1;
    }
}
