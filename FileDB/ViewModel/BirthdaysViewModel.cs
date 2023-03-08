using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Configuration;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.FilesystemAccess;
using FileDBShared.Model;

namespace FileDB.ViewModel;

public partial class PersonBirthday : ObservableObject
{
    [ObservableProperty]
    private string birthday = string.Empty;

    [ObservableProperty]
    private int daysLeft;

    [ObservableProperty]
    private string daysLeftStr = string.Empty;

    [ObservableProperty]
    private int age;

    [ObservableProperty]
    private string profileFileIdPath;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    private PersonModel person;

    public string Name => $"{Person.Firstname} {Person.Lastname}";

    public PersonBirthday(PersonModel person, string profileFileIdPath)
    {
        this.person = person;
        this.profileFileIdPath = profileFileIdPath;

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
            (!string.IsNullOrEmpty(Person.Description) && Person.Description.Contains(textFilter, StringComparison.OrdinalIgnoreCase));
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

    private Config config;
    private readonly IDbAccess dbAccess;
    private readonly IFilesystemAccess filesystemAccess;

    public BirthdaysViewModel(Config config, IDbAccess dbAccess, IFilesystemAccess filesystemAccess)
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

        this.RegisterForEvent<PersonsUpdated>((x) => UpdatePersons());

        this.RegisterForEvent<DateChanged>((x) => UpdatePersons());
    }

    private void UpdatePersons()
    {
        allPersons.Clear();
        Persons.Clear();

        var configDir = new AppDataConfig<Config>(Utils.ApplicationName).ConfigDirectory;
        var cacheDir = Path.Combine(configDir, DefaultConfigs.CacheSubdir);

        foreach (var person in dbAccess.GetPersons().Where(x => x.DateOfBirth != null && x.Deceased == null))
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
