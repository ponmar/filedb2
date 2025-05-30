﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Comparers;
using FileDB.Extensions;
using FileDB.Model;
using FileDBInterface.Model;
using FileDBInterface.Utils;

namespace FileDB.ViewModels;

public partial class DeceasedPersonViewModel : ObservableObject
{
    public string? ProfilePictureAbsPath { get; }

    [ObservableProperty]
    private Bitmap? profilePicture = null;

    [ObservableProperty]
    private int profilePictureRotation = 0;

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
    public int Age => TimeUtils.GetAgeInYears(Deceased, DatabaseParsing.ParsePersonDateOfBirth(Person.DateOfBirth!));

    private readonly ICriteriaViewModel criteriaViewModel;

    public DeceasedPersonViewModel(ICriteriaViewModel criteriaViewModel, PersonModel person, string? profilePictureAbsPath)
    {
        this.criteriaViewModel = criteriaViewModel;
        this.person = person;
        ProfilePictureAbsPath = profilePictureAbsPath;
    }

    [RelayCommand]
    private void AddPersonSearchFilter() => criteriaViewModel.AddPersonSearchFilter(Person);

    [RelayCommand]
    private void SearchForPerson() => criteriaViewModel.SearchForPersonAsync(Person);
}

public partial class RipViewModel : ObservableObject
{
    [ObservableProperty]
    private string filterText = string.Empty;

    partial void OnFilterTextChanged(string value)
    {
        FilterPersons();
    }

    private readonly List<DeceasedPersonViewModel> allPersons = [];

    public ObservableCollection<DeceasedPersonViewModel> Persons { get; set; } = [];

    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IImageLoader imageLoader;
    private readonly ICriteriaViewModel criteriaViewModel;

    public RipViewModel(IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IImageLoader imageLoader, ICriteriaViewModel criteriaViewModel)
    {
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.imageLoader = imageLoader;
        this.criteriaViewModel = criteriaViewModel;

        this.RegisterForEvent<ConfigUpdated>((x) => UpdatePersons());
        this.RegisterForEvent<PersonsUpdated>((x) => UpdatePersons());

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

        foreach (var person in dbAccessProvider.DbAccess.GetPersons().Where(x => x.DateOfBirth is not null && x.Deceased is not null))
        {
            string? profileFileIdPath = null;
            if (person.ProfileFileId is not null)
            {
                var profileFile = dbAccessProvider.DbAccess.GetFileById(person.ProfileFileId.Value);
                profileFileIdPath = filesystemAccessProvider.FilesystemAccess.ToAbsolutePath(profileFile!.Path);
            }

            allPersons.Add(new DeceasedPersonViewModel(criteriaViewModel, person, profileFileIdPath));
            if (profileFileIdPath is not null)
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
        foreach (var person in allPersons.Where(x => x.Person.MatchesTextFilter(FilterText)))
        {
            Persons.Add(person);
        }
    }

    [RelayCommand]
    private void ClearFilterText() => FilterText = string.Empty;
}
