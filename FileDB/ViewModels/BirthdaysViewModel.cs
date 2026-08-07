using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Extensions;
using FileDB.Infrastructure;
using FileDB.Lang;
using FileDB.Model;
using FileDB.Services;
using FileDBInterface.Model;
using FileDBInterface.Utils;

namespace FileDB.ViewModels;

public partial class PersonBirthdayViewModel : ObservableObject
{
    [ObservableProperty]
    public partial string Birthday { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int DaysLeft { get; set; }

    [ObservableProperty]
    public partial string DaysLeftStr { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int Age { get; set; }
    public string? ProfilePictureAbsPath { get; }

    [ObservableProperty]
    public partial Bitmap? ProfilePicture { get; set; } = null;

    [ObservableProperty]
    public partial int ProfilePictureRotation { get; set; } = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
    public partial PersonModel Person { get; set; }

    public string Name => Person.FullName;

    private readonly ICriteriaViewModel criteriaViewModel;

    public PersonBirthdayViewModel(ICriteriaViewModel criteriaViewModel, PersonModel person, string? profilePictureAbsPath)
    {
        this.criteriaViewModel = criteriaViewModel;
        Person = person;

        var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth!);
        Birthday = dateOfBirth.ToString("d MMMM");
        DaysLeft = TimeUtils.GetDaysToNextBirthday(dateOfBirth);
        Age = TimeUtils.GetAgeInYears(DateTime.Now, dateOfBirth);

        if (DaysLeft == 0)
        {
            DaysLeftStr = string.Format(Strings.PersonBirthdayViewModelTurnedToday, Age);
        }
        else if (DaysLeft == 1)
        {
            DaysLeftStr = string.Format(Strings.PersonBirthdayViewModelTurnsTomorrow, Age + 1);
        }
        else if (DaysLeft <= 14)
        {
            DaysLeftStr = string.Format(Strings.PersonBirthdayViewModelTurnsInDays, Age + 1, DaysLeft);
        }
        else
        {
            DaysLeftStr = string.Empty;
        }

        ProfilePictureAbsPath = DaysLeft == 0 ? profilePictureAbsPath : null;
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
    public partial string FilterText { get; set; } = string.Empty;

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
            try
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
            }
            catch (Exception)
            {
                // Ignore: application is shutting down
            }
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




