using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Comparers;
using FileDB.Configuration;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModel;

[ObservableObject]
public partial class DeceasedPerson
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

    public string Name => $"{person.Firstname} {person.Lastname}";
    public string DateOfBirth => person.DateOfBirth!;
    public string DeceasedStr => person.Deceased!;
    public DateTime Deceased => DatabaseParsing.ParsePersonDeceasedDate(person.Deceased!);
    public int Age => DatabaseUtils.GetAgeInYears(Deceased, DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth!));

    public DeceasedPerson(PersonModel person, string profileFileIdPath)
    {
        ProfileFileIdPath = profileFileIdPath;
        Person = person;
    }

    public bool MatchesTextFilter(string textFilter)
    {
        return string.IsNullOrEmpty(textFilter) || 
            Name.Contains(textFilter, StringComparison.OrdinalIgnoreCase) ||
            (!string.IsNullOrEmpty(person.Description) && person.Description.Contains(textFilter, StringComparison.OrdinalIgnoreCase));
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

    private readonly Model.Model model = Model.Model.Instance;

    public RipViewModel()
    {
        UpdatePersons();

        WeakReferenceMessenger.Default.Register<PersonsUpdated>(this, (r, m) =>
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

        foreach (var person in model.DbAccess.GetPersons().Where(x => x.DateOfBirth != null && x.Deceased != null))
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
