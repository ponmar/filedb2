﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
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

    private readonly IConfigProvider configProvider;
    private readonly IDbAccessProvider dbAccessProvider;
    private readonly IDialogs dialogs;
    private readonly IPersonsRepository personsRepository;

    public PersonsViewModel(IConfigProvider configProvider, IDbAccessProvider dbAccessProvider, IDialogs dialogs, IPersonsRepository personsRepository)
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
    private void RemovePerson()
    {
        if (dialogs.ShowConfirmDialog($"Remove {SelectedPerson!.Firstname} {SelectedPerson.Lastname}?"))
        {
            var filesWithPerson = dbAccessProvider.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson.Id }).ToList();
            if (filesWithPerson.Count == 0 || dialogs.ShowConfirmDialog($"Person is used in {filesWithPerson.Count} files, remove anyway?"))
            {
                dbAccessProvider.DbAccess.DeletePerson(SelectedPerson.Id);
                Events.Send<PersonEdited>();
            }
        }
    }

    [RelayCommand]
    private void EditPerson()
    {
        dialogs.ShowAddPersonDialog(SelectedPerson!.Id);
    }

    [RelayCommand]
    private void AddPerson()
    {
        dialogs.ShowAddPersonDialog();
    }

    [RelayCommand]
    private void PersonSelection(Person parameter)
    {
        SelectedPerson = parameter;
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        var personVms = personsRepository.Persons.Select(pm => new Person(pm.Id, pm.Firstname, pm.Lastname, pm.Description, pm.DateOfBirth, pm.Deceased, GetPersonAge(pm), pm.ProfileFileId, pm.Sex));
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
