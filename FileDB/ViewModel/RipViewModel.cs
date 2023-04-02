using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Comparers;
using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBShared.Model;

namespace FileDB.ViewModel;

public partial class DeceasedPerson : ObservableObject
{
    public string? ProfilePictureAbsPath { get; }

    [ObservableProperty]
    private BitmapImage? profilePicture = null;

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

    public DeceasedPerson(PersonModel person, string? profilePictureAbsPath)
    {
        this.person = person;
        ProfilePictureAbsPath = profilePictureAbsPath;
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

    private readonly IDbAccessRepository dbAccessRepository;
    private readonly IFilesystemAccessRepository filesystemAccessRepository;
    private readonly IImageLoader imageLoader;

    public RipViewModel(IDbAccessRepository dbAccessRepository, IFilesystemAccessRepository filesystemAccessRepository, IImageLoader imageLoader)
    {
        this.dbAccessRepository = dbAccessRepository;
        this.filesystemAccessRepository = filesystemAccessRepository;
        this.imageLoader = imageLoader;

        this.RegisterForEvent<ConfigUpdated>((x) => UpdatePersons());
        this.RegisterForEvent<PersonsUpdated>((x) => UpdatePersons());

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

        foreach (var person in dbAccessRepository.DbAccess.GetPersons().Where(x => x.DateOfBirth != null && x.Deceased != null))
        {
            string? profileFileIdPath = null;
            if (person.ProfileFileId != null)
            {
                var profileFile = dbAccessRepository.DbAccess.GetFileById(person.ProfileFileId.Value);
                profileFileIdPath = filesystemAccessRepository.FilesystemAccess.ToAbsolutePath(profileFile!.Path);
            }

            allPersons.Add(new DeceasedPerson(person, profileFileIdPath));
            if (profileFileIdPath != null)
            {
                imageLoader.LoadImage(profileFileIdPath);
            }
        }

        allPersons.Sort(new PersonsByDeceasedSorter());
        allPersons.Reverse();

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
