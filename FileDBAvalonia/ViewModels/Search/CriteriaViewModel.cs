﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System;
using FileDBShared.Model;
using System.Linq;
using TextCopy;
using System.IO;
using System.Collections.ObjectModel;
using FileDBInterface.Extensions;
using FileDBAvalonia.Model;
using FileDBAvalonia.Dialogs;
using System.Threading.Tasks;
using FileDBAvalonia.ViewModels.Search;
using FileDBAvalonia.Comparers;

namespace FileDBAvalonia.ViewModels;

public interface ISearchResultRepository
{
    IEnumerable<FileModel> Files { get; }

    FileModel? SelectedFile { get; }
}

public record PersonForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public record LocationForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public record TagForSearch(int Id, string Name)
{
    public override string ToString() => Name;
}

public partial class CriteriaViewModel : ObservableObject, ISearchResultRepository
{
    public IEnumerable<FileModel> Files { get; private set; } = [];

    [ObservableProperty]
    private string numRandomFiles = "10";

    [ObservableProperty]
    private string importedFileList = string.Empty;

    [ObservableProperty]
    private LocationForSearch? selectedLocationForPositionSearch;

    partial void OnSelectedLocationForPositionSearchChanged(LocationForSearch? value)
    {
        if (value is not null)
        {
            var location = dbAccessProvider.DbAccess.GetLocationById(value.Id);
            if (location.Position is not null)
            {
                SearchFileGpsPosition = location.Position;
            }
            else
            {
                SearchFileGpsPosition = string.Empty;
                dialogs.ShowInfoDialogAsync("This location has no GPS position set.");
            }
        }
    }

    [ObservableProperty]
    private string? searchFileGpsPosition;

    [ObservableProperty]
    private string? searchFileGpsPositionUrl;

    partial void OnSearchFileGpsPositionUrlChanged(string? value)
    {
        if (value.HasContent())
        {
            var gpsPos = DatabaseParsing.ParseFilesPositionFromUrl(SearchFileGpsPositionUrl);
            if (gpsPos is not null)
            {
                SearchFileGpsPosition = $"{gpsPos.Value.lat} {gpsPos.Value.lon}";
                return;
            }
        }
        SearchFileGpsPosition = string.Empty;
    }

    [ObservableProperty]
    private string searchFileGpsRadius = "500";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CombineSearchResultPossible))]
    private string combineSearch1 = string.Empty;

    partial void OnCombineSearch1Changed(string value)
    {
        CombineSearchResult = string.Empty;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CombineSearchResultPossible))]
    private string combineSearch2 = string.Empty;

    partial void OnCombineSearch2Changed(string value)
    {
        CombineSearchResult = string.Empty;
    }

    [ObservableProperty]
    private string combineSearchResult = string.Empty;

    public bool CombineSearchResultPossible => CombineSearch1.HasContent() && CombineSearch2.HasContent();

    [ObservableProperty]
    private PersonForSearch? selectedPersonSearch;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Person1And2Selected))]
    private PersonForSearch? selectedPerson1Search;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Person1And2Selected))]
    private PersonForSearch? selectedPerson2Search;

    public bool Person1And2Selected => SelectedPerson1Search is not null && SelectedPerson2Search is not null;

    [ObservableProperty]
    private TagForSearch? selectedTagSearch;

    [ObservableProperty]
    private LocationForSearch? selectedLocationSearch;

    [ObservableProperty]
    private ObservableCollection<FilterSettingsViewModel> filterSettings = [];

    public bool FilterCanBeRemoved => FilterSettings.Count > 1;

    public bool FileSelected => SelectedFile is not null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSelected))]
    [NotifyPropertyChangedFor(nameof(CurrentFileHasPosition))]
    private FileModel? selectedFile;

    public bool CurrentFileHasPosition => SelectedFile is not null && SelectedFile.Position.HasContent();

    public ObservableCollection<PersonForSearch> Persons { get; } = [];
    public ObservableCollection<LocationForSearch> Locations { get; } = [];
    public ObservableCollection<LocationForSearch> LocationsWithPosition { get; } = [];
    public ObservableCollection<TagForSearch> Tags { get; } = [];

    private readonly IConfigProvider configProvider;
    private readonly IDialogs dialogs;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IPersonsRepository personsRepository;
    private readonly ILocationsRepository locationsRepository;
    private readonly ITagsRepository tagsRepository;

    public CriteriaViewModel(IConfigProvider configProvider, IDialogs dialogs, IDatabaseAccessProvider dbAccessProvider, IPersonsRepository personsRepository, ILocationsRepository locationsRepository, ITagsRepository tagsRepository)
    {
        this.configProvider = configProvider;
        this.dialogs = dialogs;
        this.dbAccessProvider = dbAccessProvider;
        this.personsRepository = personsRepository;
        this.locationsRepository = locationsRepository;
        this.tagsRepository = tagsRepository;

        filterSettings.Add(new(personsRepository, locationsRepository, tagsRepository));

        ReloadPersons();
        ReloadLocations();
        ReloadTags();

        this.RegisterForEvent<PersonsUpdated>((x) => ReloadPersons());
        this.RegisterForEvent<LocationsUpdated>((x) => ReloadLocations());
        this.RegisterForEvent<TagsUpdated>((x) => ReloadTags());

        this.RegisterForEvent<FilesImported>((x) =>
        {
            ImportedFileList = Utils.CreateFileList(x.Files);
        });

        this.RegisterForEvent<SelectSearchResultFile>((x) =>
        {
            SelectedFile = x.File;
        });

        this.RegisterForEvent<CloseSearchResultFile>((x) =>
        {
            SelectedFile = null;
        });
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        foreach (var person in personsRepository.Persons.Select(p => new PersonForSearch(p.Id, $"{p.Firstname} {p.Lastname}")))
        {
            Persons.Add(person);
        }
    }

    private void ReloadLocations()
    {
        Locations.Clear();
        LocationsWithPosition.Clear();

        foreach (var location in locationsRepository.Locations)
        {
            var locationToUpdate = new LocationForSearch(location.Id, location.Name);
            Locations.Add(locationToUpdate);
            if (location.Position is not null)
            {
                LocationsWithPosition.Add(locationToUpdate);
            }
        }
    }

    private void ReloadTags()
    {
        Tags.Clear();
        foreach (var tag in tagsRepository.Tags.Select(t => new TagForSearch(t.Id, t.Name)))
        {
            Tags.Add(tag);
        }
    }

    [RelayCommand]
    private void FindRandomFiles()
    {
        if (int.TryParse(NumRandomFiles, out var value))
        {
            Files = dbAccessProvider.DbAccess.SearchFilesRandom(value);
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private async Task FindCurrentDirectoryFilesAsync()
    {
        if (SelectedFile is null)
        {
            await dialogs.ShowErrorDialogAsync("No file opened");
            return;
        }

        var path = SelectedFile.Path;
        var dir = Path.GetDirectoryName(path)!.Replace('\\', '/');
        Files = dbAccessProvider.DbAccess.SearchFilesByPath(dir);
        Messenger.Send<NewSearchResult>();
    }

    [RelayCommand]
    private void FindAllFiles()
    {
        Files = dbAccessProvider.DbAccess.GetFiles();
        Messenger.Send<NewSearchResult>();
    }

    [RelayCommand]
    private void FindImportedFiles()
    {
        if (ImportedFileList.HasContent())
        {
            var fileIds = Utils.CreateFileIds(ImportedFileList);
            Files = dbAccessProvider.DbAccess.SearchFilesFromIds(fileIds);
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private void FindBrowsedFiles()
    {
        // TODO
        /*
        var selectedDir = dialogs.ShowBrowseDirectoriesDialog();
        if (selectedDir is not null)
        {
            Files = dbAccessProvider.DbAccess.SearchFilesByPath(selectedDir);
            Messenger.Send<NewSearchResult>();
        }
        */
    }

    [RelayCommand]
    private void SearchFilePositionFromCurrentFile()
    {
        SearchFileGpsPosition = SelectedFile is not null ? SelectedFile.Position : string.Empty;
    }

    [RelayCommand]
    private async Task FindFilesByGpsPositionAsync()
    {
        if (!SearchFileGpsPosition.HasContent())
        {
            await dialogs.ShowErrorDialogAsync("No position specified");
            return;
        }

        if (!SearchFileGpsRadius.HasContent())
        {
            await dialogs.ShowErrorDialogAsync("No radius specified");
            return;
        }
        if (!double.TryParse(SearchFileGpsRadius, out var radius) || radius < 1)
        {
            await dialogs.ShowErrorDialogAsync("Invalid radius");
            return;
        }

        var gpsPos = DatabaseParsing.ParseFilesPosition(SearchFileGpsPosition);
        if (gpsPos is null)
        {
            await dialogs.ShowErrorDialogAsync("Invalid GPS position");
            return;
        }

        var nearFiles = dbAccessProvider.DbAccess.SearchFilesNearGpsPosition(gpsPos.Value.lat, gpsPos.Value.lon, radius).ToList();

        // TODO: checkbox for selecting if this should be included?
        var nearLocations = dbAccessProvider.DbAccess.SearchLocationsNearGpsPosition(gpsPos.Value.lat, gpsPos.Value.lon, radius);
        nearFiles.AddRange(dbAccessProvider.DbAccess.SearchFilesWithLocations(nearLocations.Select(x => x.Id)));

        Files = nearFiles;
        Messenger.Send<NewSearchResult>();
    }

    [RelayCommand]
    private void FindFilesWithPerson()
    {
        if (SelectedPersonSearch is not null)
        {
            Files = dbAccessProvider.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id });
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonUnique()
    {
        if (SelectedPersonSearch is not null)
        {
            var files = dbAccessProvider.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id });
            Files = files.Where(x => dbAccessProvider.DbAccess.GetPersonsFromFile(x.Id).Count() == 1);
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonGroup()
    {
        if (SelectedPersonSearch is not null)
        {
            var files = dbAccessProvider.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id });
            Files = files.Where(x => dbAccessProvider.DbAccess.GetPersonsFromFile(x.Id).Count() > 1);
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private void FindFilesWithPersons()
    {
        if (SelectedPerson1Search is not null && SelectedPerson2Search is not null)
        {
            Files = dbAccessProvider.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id, SelectedPerson2Search.Id });
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonsUnique()
    {
        if (SelectedPerson1Search is not null && SelectedPerson2Search is not null)
        {
            var files = dbAccessProvider.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id });
            Files = files.Where(x =>
            {
                var filePersons = dbAccessProvider.DbAccess.GetPersonsFromFile(x.Id).ToList();
                return filePersons.Count == 2 && filePersons.Any(y => y.Id == SelectedPerson2Search.Id);
            });
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonsGroup()
    {
        if (SelectedPerson1Search is not null && SelectedPerson2Search is not null)
        {
            var files = dbAccessProvider.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id });
            Files = files.Where(x =>
            {
                var filePersons = dbAccessProvider.DbAccess.GetPersonsFromFile(x.Id).ToList();
                return filePersons.Count > 2 && filePersons.Any(y => y.Id == SelectedPerson2Search.Id);
            });
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private void FindFilesWithLocation()
    {
        if (SelectedLocationSearch is not null)
        {
            Files = dbAccessProvider.DbAccess.SearchFilesWithLocations(new List<int>() { SelectedLocationSearch.Id });
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private void FindFilesWithTag()
    {
        if (SelectedTagSearch is not null)
        {
            Files = dbAccessProvider.DbAccess.SearchFilesWithTags(new List<int>() { SelectedTagSearch.Id });
            Messenger.Send<NewSearchResult>();
        }
    }

    [RelayCommand]
    private void CombineSearchIntersection()
    {
        var files1 = Utils.CreateFileIds(CombineSearch1);
        var files2 = Utils.CreateFileIds(CombineSearch2);
        var result = files1.Intersect(files2);
        CombineSearchResult = Utils.CreateFileList(result);
    }

    [RelayCommand]
    private void CombineSearchUnion()
    {
        var files1 = Utils.CreateFileIds(CombineSearch1);
        var files2 = Utils.CreateFileIds(CombineSearch2);
        var result = files1.Union(files2);
        CombineSearchResult = Utils.CreateFileList(result);
    }

    [RelayCommand]
    private void CombineSearchDifference()
    {
        var files1 = Utils.CreateFileIds(CombineSearch1);
        var files2 = Utils.CreateFileIds(CombineSearch2);
        var uniqueFiles1 = files1.Except(files2);
        var uniqueFiles2 = files2.Except(files1);
        var result = uniqueFiles1.Union(uniqueFiles2);
        CombineSearchResult = Utils.CreateFileList(result);
    }

    [RelayCommand]
    private void CombineSearchResultCopy()
    {
        ClipboardService.SetText(CombineSearchResult);
    }

    [RelayCommand]
    private void CombineSearchResultShow()
    {
        var fileIds = Utils.CreateFileIds(CombineSearchResult);
        Files = dbAccessProvider.DbAccess.SearchFilesFromIds(fileIds);
        Messenger.Send<NewSearchResult>();
    }

    [RelayCommand]
    private void AddFilter()
    {
        FilterSettings.Add(new FilterSettingsViewModel(personsRepository, locationsRepository, tagsRepository));
        OnPropertyChanged(nameof(FilterCanBeRemoved));
    }

    [RelayCommand]
    private void RemoveFilter(FilterSettingsViewModel vm)
    {
        FilterSettings.Remove(vm);
        OnPropertyChanged(nameof(FilterCanBeRemoved));
    }

    [RelayCommand]
    private async Task FindFilesFromFiltersAsync()
    {
        var fileModelComparer = new FileModelByIdComparer();
        var filters = FilterSettings.Select(x => (viewModel: x, filter: x.Create())).ToList();
        var filtersWithInvalidSettings = filters.Where(x => !x.filter.CanRun());
        if (filtersWithInvalidSettings.Any())
        {
            await dialogs.ShowErrorDialogAsync($"Invalid settings for filters: {string.Join(", ", filtersWithInvalidSettings.Select(x => x.viewModel.SelectedFilterType.ToFriendlyString()))}");
            return;
        }

        var result = Enumerable.Empty<FileModel>();
        foreach (var filter in filters.Select(x => x.filter))
        {
            var files = filter.Run(dbAccessProvider.DbAccess);
            result = filter == filters.First().filter ? files : result.Intersect(files, fileModelComparer);

            if (!result.Any())
            {
                // No need to continue with db queries because the next intersection will throw them away
                break;
            }
        }

        Files = result;
        Messenger.Send<NewSearchResult>();
    }
}
