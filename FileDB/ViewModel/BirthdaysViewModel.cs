﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Configuration;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModel;

[ObservableObject]
public partial class PersonBirthday
{
    [ObservableProperty]
    private string birthday;

    [ObservableProperty]
    private int daysLeft;

    [ObservableProperty]
    private string daysLeftStr;

    [ObservableProperty]
    private int age;

    [ObservableProperty]
    private string profileFileIdPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    private PersonModel person;

    public string Name => $"{person.Firstname} {person.Lastname}";

    public PersonBirthday(PersonModel person, string profileFileIdPath)
    {
        this.person = person;
        ProfileFileIdPath = profileFileIdPath;

        Update(person);
    }

    public void Update(PersonModel person)
    {
        Person = person;

        var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth!);
        Birthday = dateOfBirth.ToString("d MMMM");
        DaysLeft = DatabaseUtils.GetDaysToNextBirthday(dateOfBirth);
        Age = DatabaseUtils.GetAgeInYears(DateTime.Now, dateOfBirth);

        if (DaysLeft == 0)
        {
            DaysLeftStr = $"Turned {Age} today!";
        }
        else if (DaysLeft == 1)
        {
            DaysLeftStr = $"Turns {Age + 1} tomorrow!";
        }
        else if (DaysLeft <= 14)
        {
            DaysLeftStr = $"Turns {Age + 1} in {DaysLeft} days";
        }
        else
        {
            DaysLeftStr = string.Empty;
        }
    }

    public bool MatchesTextFilter(string textFilter)
    {
        return string.IsNullOrEmpty(textFilter) ||
            Name.Contains(textFilter, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrEmpty(person.Description) && person.Description.Contains(textFilter, StringComparison.OrdinalIgnoreCase));
    }
}

public class PersonsByDaysLeftUntilBirthdaySorter : IComparer<PersonBirthday>
{
    public int Compare(PersonBirthday? x, PersonBirthday? y)
    {
        if (x!.DaysLeft == y!.DaysLeft)
        {
            return x.Name.CompareTo(y.Name);
        }

        return x.DaysLeft.CompareTo(y.DaysLeft);
    }
}

public partial class BirthdaysViewModel : ObservableObject
{
    [ObservableProperty]
    private string filterText = string.Empty;

    partial void OnFilterTextChanged(string value)
    {
        FilterPersons();
    }

    private readonly List<PersonBirthday> allPersons = new();

    public ObservableCollection<PersonBirthday> Persons { get; } = new();

    private readonly Model.Model model = Model.Model.Instance;

    public BirthdaysViewModel()
    {
        UpdatePersons();
        WeakReferenceMessenger.Default.Register<PersonsUpdated>(this, (r, m) =>
        {
            UpdatePersons();
        });

        WeakReferenceMessenger.Default.Register<DateChanged>(this, (r, m) =>
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

        foreach (var person in model.DbAccess.GetPersons().Where(x => x.DateOfBirth != null && x.Deceased == null))
        {
            string profileFileIdPath;
            if (person.ProfileFileId != null)
            {
                if (model.Config.CacheFiles)
                {
                    profileFileIdPath = Path.Combine(cacheDir, $"{person.ProfileFileId.Value}");
                }
                else
                {
                    var profileFile = model.DbAccess.GetFileById(person.ProfileFileId.Value);
                    profileFileIdPath = model.FilesystemAccess.ToAbsolutePath(profileFile!.Path);
                }
            }
            else
            {
                profileFileIdPath = string.Empty;
            } 

            allPersons.Add(new PersonBirthday(person, profileFileIdPath));
        }

        foreach (var person in allPersons)
        {
            var observablePerson = Persons.FirstOrDefault(x => x.Person.Id == person.Person.Id);
            observablePerson?.Update(person.Person);
        }

        allPersons.Sort(new PersonsByDaysLeftUntilBirthdaySorter());

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
