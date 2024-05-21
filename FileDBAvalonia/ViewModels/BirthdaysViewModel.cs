using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Extensions;
using FileDBAvalonia.Model;
using FileDBInterface.Model;
using FileDBInterface.Utils;

namespace FileDBAvalonia.ViewModels;

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
    private Bitmap? profilePicture = null;

    [ObservableProperty]
    private int profilePictureRotation = 0;

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
        DaysLeft = TimeUtils.GetDaysToNextBirthday(dateOfBirth);
        Age = TimeUtils.GetAgeInYears(DateTime.Now, dateOfBirth);

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

    [RelayCommand]
    private void AddPersonSearchFilter() => Messenger.Send(new AddPersonSearchFilter(Person));

    [RelayCommand]
    private void SearchForPerson() => Messenger.Send(new SearchForPerson(Person));
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

    private readonly List<PersonBirthday> allPersons = [];

    public ObservableCollection<PersonBirthday> Persons { get; } = [];

    private readonly IPersonsRepository personsRepository;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IImageLoader imageLoader;

    public BirthdaysViewModel(IPersonsRepository personsRepository, IFilesystemAccessProvider filesystemAccessProvider, IDatabaseAccessProvider dbAccessProvider, IImageLoader imageLoader)
    {
        this.personsRepository = personsRepository;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.imageLoader = imageLoader;

        this.RegisterForEvent<ConfigUpdated>((x) => UpdatePersons());

        this.RegisterForEvent<PersonsUpdated>((x) => UpdatePersons());

        this.RegisterForEvent<DateChanged>((x) => UpdatePersons());

        this.RegisterForEvent<ImageLoaded>((x) =>
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                foreach (var personVm in allPersons.Where(p => p.ProfilePictureAbsPath == x.FilePath))
                {
                    var profileFile = dbAccessProvider.DbAccess.GetFileById(personVm.Person.ProfileFileId!.Value);
                    personVm.ProfilePictureRotation = -DatabaseParsing.OrientationToDegrees(profileFile!.Orientation ?? 0);
                    personVm.ProfilePicture = x.Image;
                }
            });
        });

        UpdatePersons();
    }

    private void UpdatePersons()
    {
        allPersons.Clear();

        foreach (var person in personsRepository.Persons.Where(x => x.DateOfBirth is not null && x.Deceased is null))
        {
            string? profileFileIdPath = null;
            if (person.ProfileFileId is not null)
            {
                var profileFile = dbAccessProvider.DbAccess.GetFileById(person.ProfileFileId.Value);
                profileFileIdPath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(profileFile!.Path);
            }

            allPersons.Add(new PersonBirthday(person, profileFileIdPath));
            if (profileFileIdPath is not null)
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
        foreach (var person in allPersons.Where(x => x.Person.MatchesTextFilter(FilterText)))
        {
            Persons.Add(person);
        }
    }

    [RelayCommand]
    private void ClearFilterText() => FilterText = string.Empty;
}
