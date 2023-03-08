﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Comparers;
using FileDB.Configuration;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.FilesystemAccess;
using FileDBShared.Model;

namespace FileDB.ViewModel;

public partial class DeceasedPerson : ObservableObject
{
    [ObservableProperty]
    private string profileFileIdPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    [NotifyPropertyChangedFor(nameof(DateOfBirth))]
    [NotifyPropertyChangedFor(nameof(DeceasedStr))]
    [NotifyPropertyChangedFor(nameof(Deceased))]
    [NotifyPropertyChangedFor(nameof(Age))]
    private PersonModel person;

    public string Name => $"{Person.Firstname} {Person.Lastname}";
    public string DateOfBirth => Person.DateOfBirth!;
    public string DeceasedStr => Person.Deceased!;
    public DateTime Deceased => DatabaseParsing.ParsePersonDeceasedDate(Person.Deceased!);
    public int Age => DatabaseUtils.GetAgeInYears(Deceased, DatabaseParsing.ParsePersonDateOfBirth(Person.DateOfBirth!));

    public DeceasedPerson(PersonModel person, string profileFileIdPath)
    {
        this.profileFileIdPath = profileFileIdPath;
        this.person = person;
    }

    public bool MatchesTextFilter(string textFilter)
    {
        return string.IsNullOrEmpty(textFilter) || 
            Name.Contains(textFilter, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrEmpty(Person.Description) && Person.Description.Contains(textFilter, StringComparison.OrdinalIgnoreCase));
    }
}

public partial class RipViewModel : ObservableObject
{
    [ObservableProperty]
    private string filterText = string.Empty;

    partial void OnFilterTextChanged(string value)
    {
        FilterPersons();
    }

    private readonly List<DeceasedPerson> allPersons = new();

    public ObservableCollection<DeceasedPerson> Persons { get; set; } = new();

    private Config config;
    private readonly IDbAccess dbAccess;
    private readonly IFilesystemAccess filesystemAccess;

    public RipViewModel(Config config, IDbAccess dbAccess, IFilesystemAccess filesystemAccess)
    {
        this.config = config;
        this.dbAccess = dbAccess;
        this.filesystemAccess = filesystemAccess;

        UpdatePersons();

        this.RegisterForEvent<ConfigLoaded>((x) =>
        {
            this.config = x.Config;
            UpdatePersons();
        });

        this.RegisterForEvent<PersonsUpdated>((x) =>
        {
            UpdatePersons();
        });
    }

    private void UpdatePersons()
    {
        allPersons.Clear();
        Persons.Clear();

        var configDir = new AppDataConfig<Config>(Utils.ApplicationName).ConfigDirectory;
        var cacheDir = Path.Combine(configDir, DefaultConfigs.CacheSubdir);

        foreach (var person in dbAccess.GetPersons().Where(x => x.DateOfBirth != null && x.Deceased != null))
        {
            string profileFileIdPath;
            if (person.ProfileFileId != null)
            {
                if (config.CacheFiles)
                {
                    profileFileIdPath = Path.Combine(cacheDir, $"{person.ProfileFileId.Value}");
                }
                else
                {
                    var profileFile = dbAccess.GetFileById(person.ProfileFileId.Value);
                    profileFileIdPath = filesystemAccess.ToAbsolutePath(profileFile!.Path);
                }
            }
            else
            {
                profileFileIdPath = string.Empty;
            }

            allPersons.Add(new DeceasedPerson(person, profileFileIdPath));
        }

        foreach (var person in allPersons)
        {
            var observablePerson = Persons.FirstOrDefault(x => x.Person.Id == person.Person.Id);
            if (observablePerson != null)
            {
                observablePerson.Person = person.Person;
            }
        }

        allPersons.Sort(new PersonsByDeceasedSorter());
        allPersons.Reverse();

        FilterPersons();
    }

    private void FilterPersons()
    {
        foreach (var person in allPersons)
        {
            if (person.MatchesTextFilter(FilterText))
            {
                if (!Persons.Any(x => x.Person.Id == person.Person.Id))
                {
                    // TODO: sorting is not correct after adding at end here. Insert at correct index?
                    Persons.Add(person);
                }
            }
            else
            {
                Persons.Remove(person);
            }
        }
    }
}
