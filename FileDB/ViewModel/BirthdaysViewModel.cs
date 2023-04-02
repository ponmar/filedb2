using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Configuration;
using FileDB.Model;
using FileDBInterface.DbAccess;
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

    public string? ProfilePictureAbsPath { get; }

    [ObservableProperty]
    private BitmapImage? profilePicture = null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    private PersonModel person;

    public string Name => $"{Person.Firstname} {Person.Lastname}";

    public PersonBirthday(PersonModel person, string? profilePictureAbsPath)
    {
        this.person = person;
        ProfilePictureAbsPath = profilePictureAbsPath;
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

    private readonly IPersonsRepository personsRepository;
    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly IImageLoader imageLoader;

    public BirthdaysViewModel(IPersonsRepository personsRepository, IFilesystemAccessRepository filesystemAccessRepository, IDbAccessRepository dbAccessRepository, IImageLoader imageLoader)
    {
        this.personsRepository = personsRepository;
        this.filesystemAccessRepository = filesystemAccessRepository;
        this.dbAccessRepository = dbAccessRepository;
        this.imageLoader = imageLoader;

        this.RegisterForEvent<ConfigUpdated>((x) => UpdatePersons());

        this.RegisterForEvent<PersonsUpdated>((x) => UpdatePersons());

        this.RegisterForEvent<DateChanged>((x) => UpdatePersons());

        this.RegisterForEvent<ImageLoaded>((x) =>
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var personVm in allPersons.Where(p => p.ProfilePictureAbsPath == x.FilePath))
                {
                    personVm.ProfilePicture = x.Image;
                }
            });
        });

        UpdatePersons();
    }

    private void UpdatePersons()
    {
        allPersons.Clear();

        foreach (var person in personsRepository.Persons.Where(x => x.DateOfBirth != null && x.Deceased == null))
        {
            string? profileFileIdPath = null;
            if (person.ProfileFileId != null)
            {
                var profileFile = dbAccessRepository.DbAccess.GetFileById(person.ProfileFileId.Value);
                profileFileIdPath = filesystemAccessRepository.FilesystemAccess.ToAbsolutePath(profileFile!.Path);
            }

            allPersons.Add(new PersonBirthday(person, profileFileIdPath));
            if (profileFileIdPath != null)
            {
                imageLoader.LoadImage(profileFileIdPath);
            }
        }

        allPersons.Sort(new PersonsByDaysLeftUntilBirthdaySorter());

        FilterPersons();
    }

    private void FilterPersons()
    {
        Persons.Clear();
        foreach (var person in allPersons.Where(x => x.MatchesTextFilter(FilterText)))
        {
            Persons.Add(person);
        }
    }
}
