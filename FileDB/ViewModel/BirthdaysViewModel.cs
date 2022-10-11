using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Configuration;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModel;

public class PersonBirthday
{
    public string Name => $"{person.Firstname} {person.Lastname}";
    public string Birthday { get; }
    public int DaysLeft { get; }
    public string DaysLeftStr { get; }
    public int Age { get; }
    public string ProfileFileIdPath { get; }

    private readonly PersonModel person;

    public PersonBirthday(PersonModel person, string profileFileIdPath)
    {
        this.person = person;
        ProfileFileIdPath = profileFileIdPath;

        var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth!);
        Birthday = dateOfBirth.ToString("d MMMM");
        DaysLeft = DatabaseUtils.GetDaysToNextBirthday(dateOfBirth);
        Age = DatabaseUtils.GetYearsAgo(DateTime.Now, dateOfBirth);

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
        model.PersonsUpdated += Model_PersonsUpdated;
        model.DateChanged += Model_DateChanged;
    }

    private void Model_DateChanged(object? sender, EventArgs e)
    {
        UpdatePersons();
    }

    private void Model_PersonsUpdated(object? sender, EventArgs e)
    {
        UpdatePersons();
    }

    private void UpdatePersons()
    {
        allPersons.Clear();
        var configDir = new AppDataConfig<Config>(Utils.ApplicationName).ConfigDirectory;
        var cacheDir = Path.Combine(configDir, DefaultConfigs.CacheSubdir);

        foreach (var person in model.DbAccess.GetPersons())
        {
            if (person.DateOfBirth != null && person.Deceased == null)
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
                if (!Persons.Contains(person))
                {
                    // TODO: sorting is not correct after adding at end here
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
