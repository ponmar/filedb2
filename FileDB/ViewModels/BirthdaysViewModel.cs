using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Extensions;
using FileDB.Model;
using FileDBInterface.Model;
using FileDBInterface.Utils;

namespace FileDB.ViewModels;

public partial class PersonBirthdayViewModel : ObservableObject
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

    private readonly ICriteriaViewModel criteriaViewModel;

    public PersonBirthdayViewModel(ICriteriaViewModel criteriaViewModel, PersonModel person, string? profilePictureAbsPath)
    {
        this.criteriaViewModel = criteriaViewModel;
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
    private void AddPersonSearchFilter() => criteriaViewModel.AddPersonSearchFilter(Person);

    [RelayCommand]
    private void SearchForPerson() => criteriaViewModel.SearchForPersonAsync(Person);

    [RelayCommand]
    private void SearchForBirthday()
    {
        var birthday = DatabaseParsing.ParsePersonDateOfBirth(Person.DateOfBirth!);
        criteriaViewModel.SearchForAnnualDateAsync(birthday.Month, birthday.Day);
    }

    [RelayCommand]
    private void AddBirthdayDateSearchFilter()
    {
        var birthday = DatabaseParsing.ParsePersonDateOfBirth(Person.DateOfBirth!);
        criteriaViewModel.AddAnnualDateSearchFilter(birthday.Month, birthday.Day);
    }
}

public class PersonsByDaysLeftUntilBirthdaySorter : IComparer<PersonBirthdayViewModel>
{
    public int Compare(PersonBirthdayViewModel? x, PersonBirthdayViewModel? y)
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

    private readonly List<PersonBirthdayViewModel> allPersons = [];

    public ObservableCollection<PersonBirthdayViewModel> Persons { get; } = [];

    private readonly IPersonsRepository personsRepository;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IImageLoader imageLoader;
    private readonly ICriteriaViewModel criteriaViewModel;

    public BirthdaysViewModel(IPersonsRepository personsRepository, IFilesystemAccessProvider filesystemAccessProvider, IDatabaseAccessProvider dbAccessProvider, IImageLoader imageLoader, ICriteriaViewModel criteriaViewModel)
    {
        this.personsRepository = personsRepository;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.dbAccessProvider = dbAccessProvider;
        this.imageLoader = imageLoader;
        this.criteriaViewModel = criteriaViewModel;

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

            allPersons.Add(new PersonBirthdayViewModel(criteriaViewModel, person, profileFileIdPath));
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
