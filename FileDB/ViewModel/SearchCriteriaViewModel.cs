using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using System.Collections.Generic;
using System;
using FileDBShared.Model;
using System.Linq;
using TextCopy;
using System.IO;
using FileDBInterface.DbAccess;
using FileDB.Extensions;
using System.Collections.ObjectModel;
using FileDB.Sorters;

namespace FileDB.ViewModel;

public partial class SearchCriteriaViewModel : ObservableObject
{
    [ObservableProperty]
    private string numRandomFiles = "10";

    [ObservableProperty]
    private string importedFileList = string.Empty;

    [ObservableProperty]
    private string? searchPattern;

    [ObservableProperty]
    private DateTime searchStartDate = DateTime.Now;

    [ObservableProperty]
    private DateTime searchEndDate = DateTime.Now;

    public static IEnumerable<Sex> PersonSexValues => Enum.GetValues<Sex>().OrderBy(x => x.ToString());

    [ObservableProperty]
    private Sex? searchBySexSelection;

    public static IEnumerable<FileType> FileTypes => Enum.GetValues<FileType>().OrderBy(x => x.ToString());

    [ObservableProperty]
    private FileType? selectedFileType = null;

    [ObservableProperty]
    private LocationToUpdate? selectedLocationForPositionSearch;

    partial void OnSelectedLocationForPositionSearchChanged(LocationToUpdate? value)
    {
        if (value != null)
        {
            var location = dbAccessRepository.DbAccess.GetLocationById(value.Id);
            if (location.Position != null)
            {
                SearchFileGpsPosition = location.Position;
            }
            else
            {
                SearchFileGpsPosition = string.Empty;
                dialogs.ShowInfoDialog("This location has no GPS position set.");
            }
        }
    }

    [ObservableProperty]
    private string? searchFileGpsPosition;

    [ObservableProperty]
    private string? searchFileGpsPositionUrl;

    partial void OnSearchFileGpsPositionUrlChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var gpsPos = DatabaseParsing.ParseFilesPositionFromUrl(SearchFileGpsPositionUrl);
            if (gpsPos != null)
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
    [NotifyPropertyChangedFor(nameof(PersonAgeRangeValid))]
    private string? searchPersonAgeFrom;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PersonAgeRangeValid))]
    private string? searchPersonAgeTo;

    public bool PersonAgeRangeValid =>
        int.TryParse(SearchPersonAgeFrom, out var from) &&
        int.TryParse(SearchPersonAgeTo, out var to) &&
        from >= 0 && from <= to;

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

    public bool CombineSearchResultPossible => !string.IsNullOrEmpty(CombineSearch1) && !string.IsNullOrEmpty(CombineSearch2);

    [ObservableProperty]
    private PersonToUpdate? selectedPersonSearch;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Person1And2Selected))]
    private PersonToUpdate? selectedPerson1Search;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Person1And2Selected))]
    private PersonToUpdate? selectedPerson2Search;

    public bool Person1And2Selected => SelectedPerson1Search != null && SelectedPerson2Search != null;

    [ObservableProperty]
    private TagToUpdate? selectedTagSearch;

    [ObservableProperty]
    private LocationToUpdate? selectedLocationSearch;

    [ObservableProperty]
    private string? fileListSearch;

    public bool FileSelected => SelectedFile != null;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FileSelected))]
    [NotifyPropertyChangedFor(nameof(CurrentFileHasPosition))]
    private FilesModel? selectedFile;

    public bool CurrentFileHasPosition => SelectedFile != null && !string.IsNullOrEmpty(SelectedFile.Position);

    public ObservableCollection<PersonToUpdate> Persons { get; } = new();
    public ObservableCollection<LocationToUpdate> Locations { get; } = new();
    public ObservableCollection<LocationToUpdate> LocationsWithPosition { get; } = new();
    public ObservableCollection<TagToUpdate> Tags { get; } = new();

    private readonly IConfigRepository configRepository;
    private readonly IDialogs dialogs;
    private readonly IDbAccessRepository dbAccessRepository;

    public SearchCriteriaViewModel(IConfigRepository configRepository, IDialogs dialogs, IDbAccessRepository dbAccessRepository)
    {
        this.configRepository = configRepository;
        this.dialogs = dialogs;
        this.dbAccessRepository = dbAccessRepository;

        ReloadPersons();
        ReloadLocations();
        ReloadTags();

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
        var persons = dbAccessRepository.DbAccess.GetPersons().ToList();
        persons.Sort(new PersonModelByNameSorter());
        foreach (var person in persons.Select(p => new PersonToUpdate(p.Id, $"{p.Firstname} {p.Lastname}", Utils.CreateShortText($"{p.Firstname} {p.Lastname}", configRepository.Config.ShortItemNameMaxLength))))
        {
            Persons.Add(person);
        }
    }

    private void ReloadLocations()
    {
        Locations.Clear();
        LocationsWithPosition.Clear();

        var locations = dbAccessRepository.DbAccess.GetLocations().ToList();
        locations.Sort(new LocationModelByNameSorter());

        foreach (var location in locations)
        {
            var locationToUpdate = new LocationToUpdate(location.Id, location.Name, Utils.CreateShortText(location.Name, configRepository.Config.ShortItemNameMaxLength));
            Locations.Add(locationToUpdate);
            if (location.Position != null)
            {
                LocationsWithPosition.Add(locationToUpdate);
            }
        }
    }

    private void ReloadTags()
    {
        Tags.Clear();
        var tags = dbAccessRepository.DbAccess.GetTags().ToList();
        tags.Sort(new TagModelByNameSorter());
        foreach (var tag in tags.Select(t => new TagToUpdate(t.Id, t.Name, Utils.CreateShortText(t.Name, configRepository.Config.ShortItemNameMaxLength))))
        {
            Tags.Add(tag);
        }
    }

    [RelayCommand]
    private void FindRandomFiles()
    {
        if (int.TryParse(NumRandomFiles, out var value))
        {
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesRandom(value));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindCurrentDirectoryFiles()
    {
        if (SelectedFile == null)
        {
            dialogs.ShowErrorDialog("No file opened");
            return;
        }

        var path = SelectedFile.Path;
        var dir = Path.GetDirectoryName(path)!.Replace('\\', '/');
        var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesByPath(dir));
        Events.Send(new NewSearchResult(searchResult));
    }

    [RelayCommand]
    private void FindAllFiles()
    {
        var searchResult = new SearchResult(dbAccessRepository.DbAccess.GetFiles());
        Events.Send(new NewSearchResult(searchResult));
    }

    [RelayCommand]
    private void FindImportedFiles()
    {
        if (!string.IsNullOrEmpty(ImportedFileList))
        {
            var fileIds = Utils.CreateFileIds(ImportedFileList);
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesFromIds(fileIds));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindBrowsedFiles()
    {
        var selectedDir = dialogs.ShowBrowseDirectoriesDialog();
        if (selectedDir != null)
        {
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesByPath(selectedDir));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesByText()
    {
        if (!string.IsNullOrEmpty(SearchPattern))
        {
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFiles(SearchPattern));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesBySex()
    {
        if (SearchBySexSelection != null)
        {
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesBySex(SearchBySexSelection.Value));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesByDate()
    {
        var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesByDate(SearchStartDate.Date, SearchEndDate.Date));
        Events.Send(new NewSearchResult(searchResult));
    }

    [RelayCommand]
    private void FindFilesByType()
    {
        if (SelectedFileType == null)
        {
            return;
        }

        var fileExtensions = SelectedFileType.GetAttribute<FileExtensionsAttribute>().FileExtensions;

        var result = new List<FilesModel>();
        foreach (var extension in fileExtensions)
        {
            result.AddRange(dbAccessRepository.DbAccess.SearchFilesByExtension(extension));
        }

        var searchResult = new SearchResult(result);
        Events.Send(new NewSearchResult(searchResult));
    }

    [RelayCommand]
    private void SearchFilePositionFromCurrentFile()
    {
        SearchFileGpsPosition = SelectedFile != null ? SelectedFile.Position : string.Empty;
    }

    [RelayCommand]
    private void FindFilesByGpsPosition()
    {
        if (string.IsNullOrEmpty(SearchFileGpsPosition))
        {
            dialogs.ShowErrorDialog("No position specified");
            return;
        }

        if (string.IsNullOrEmpty(SearchFileGpsRadius))
        {
            dialogs.ShowErrorDialog("No radius specified");
            return;
        }
        if (!double.TryParse(SearchFileGpsRadius, out var radius) || radius < 1)
        {
            dialogs.ShowErrorDialog("Invalid radius");
            return;
        }

        var gpsPos = DatabaseParsing.ParseFilesPosition(SearchFileGpsPosition);
        if (gpsPos == null)
        {
            dialogs.ShowErrorDialog("Invalid GPS position");
            return;
        }

        var nearFiles = dbAccessRepository.DbAccess.SearchFilesNearGpsPosition(gpsPos.Value.lat, gpsPos.Value.lon, radius).ToList();

        // TODO: checkbox for selecting if this should be included?
        var nearLocations = dbAccessRepository.DbAccess.SearchLocationsNearGpsPosition(gpsPos.Value.lat, gpsPos.Value.lon, radius);
        nearFiles.AddRange(dbAccessRepository.DbAccess.SearchFilesWithLocations(nearLocations.Select(x => x.Id)));

        var searchResult = new SearchResult(nearFiles);
        Events.Send(new NewSearchResult(searchResult));
    }

    [RelayCommand]
    private void FindFilesWithPerson()
    {
        if (SelectedPersonSearch != null)
        {
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id }));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonUnique()
    {
        if (SelectedPersonSearch != null)
        {
            var files = dbAccessRepository.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id });
            var result = files.Where(x => dbAccessRepository.DbAccess.GetPersonsFromFile(x.Id).Count() == 1);
            var searchResult = new SearchResult(result);
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonGroup()
    {
        if (SelectedPersonSearch != null)
        {
            var files = dbAccessRepository.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id });
            var result = files.Where(x => dbAccessRepository.DbAccess.GetPersonsFromFile(x.Id).Count() > 1);
            var searchResult = new SearchResult(result);
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesWithPersons()
    {
        if (SelectedPerson1Search != null && SelectedPerson2Search != null)
        {
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id, SelectedPerson2Search.Id }));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonsUnique()
    {
        if (SelectedPerson1Search != null && SelectedPerson2Search != null)
        {
            var files = dbAccessRepository.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id });
            var result = files.Where(x =>
            {
                var filePersons = dbAccessRepository.DbAccess.GetPersonsFromFile(x.Id).ToList();
                return filePersons.Count() == 2 && filePersons.Any(y => y.Id == SelectedPerson2Search.Id);
            });
            var searchResult = new SearchResult(result);
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonsGroup()
    {
        if (SelectedPerson1Search != null && SelectedPerson2Search != null)
        {
            var files = dbAccessRepository.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id });
            var result = files.Where(x =>
            {
                var filePersons = dbAccessRepository.DbAccess.GetPersonsFromFile(x.Id).ToList();
                return filePersons.Count() > 2 && filePersons.Any(y => y.Id == SelectedPerson2Search.Id);
            });
            var searchResult = new SearchResult(result);
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesWithLocation()
    {
        if (SelectedLocationSearch != null)
        {
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesWithLocations(new List<int>() { SelectedLocationSearch.Id }));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesWithTag()
    {
        if (SelectedTagSearch != null)
        {
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesWithTags(new List<int>() { SelectedTagSearch.Id }));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesByPersonAge()
    {
        if (!string.IsNullOrEmpty(SearchPersonAgeFrom))
        {
            if (!int.TryParse(SearchPersonAgeFrom, out var ageFrom))
            {
                dialogs.ShowErrorDialog("Invalid age format");
                return;
            }

            int ageTo;
            if (string.IsNullOrEmpty(SearchPersonAgeTo))
            {
                ageTo = ageFrom;
            }
            else if (!int.TryParse(SearchPersonAgeTo, out ageTo))
            {
                dialogs.ShowErrorDialog("Invalid age format");
                return;
            }

            var result = new List<FilesModel>();
            var personsWithAge = dbAccessRepository.DbAccess.GetPersons().Where(p => p.DateOfBirth != null);

            foreach (var person in personsWithAge)
            {
                var dateOfBirth = DatabaseParsing.ParsePersonDateOfBirth(person.DateOfBirth!);
                foreach (var file in dbAccessRepository.DbAccess.SearchFilesWithPersons(new List<int>() { person.Id }))
                {
                    var fileDatetime = DatabaseParsing.ParseFilesDatetime(file.Datetime);
                    if (fileDatetime != null)
                    {
                        int personAgeInFile = DatabaseUtils.GetAgeInYears(fileDatetime.Value, dateOfBirth);
                        if (personAgeInFile >= ageFrom && personAgeInFile <= ageTo)
                        {
                            result.Add(file);
                        }
                    }
                }
            }

            var searchResult = new SearchResult(result);
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesFromUnion()
    {
        /*
        if (SearchResultHistory.Count >= 2)
        {
            var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
            var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
            SearchResult = new SearchResult(files1.Union(files2, new FilesModelIdComparer()));
        }
        */
    }

    [RelayCommand]
    private void FindFilesFromIntersection()
    {
        /*
        if (SearchResultHistory.Count >= 2)
        {
            var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
            var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
            SearchResult = new SearchResult(files1.Intersect(files2, new FilesModelIdComparer()));
        }
        */
    }

    [RelayCommand]
    private void FindFilesFromDifference()
    {
        /*
        if (SearchResultHistory.Count >= 2)
        {
            var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
            var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
            var uniqueFiles1 = files1.Except(files2, new FilesModelIdComparer());
            var uniqueFiles2 = files2.Except(files1, new FilesModelIdComparer());
            SearchResult = new SearchResult(uniqueFiles1.Union(uniqueFiles2, new FilesModelIdComparer()));
        }
        */
    }

    [RelayCommand]
    private void FindFilesFromMissingCategorization()
    {
        var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesWithMissingData());
        Events.Send(new NewSearchResult(searchResult));
    }

    [RelayCommand]
    private void FindFilesFromList()
    {
        if (!string.IsNullOrEmpty(FileListSearch))
        {
            var fileIds = Utils.CreateFileIds(FileListSearch);
            var searchResult = new SearchResult(dbAccessRepository.DbAccess.SearchFilesFromIds(fileIds));
            Events.Send(new NewSearchResult(searchResult));
        }
    }

    [RelayCommand]
    private void FindFilesFromListComplement()
    {
        if (!string.IsNullOrEmpty(FileListSearch))
        {
            var fileIds = Utils.CreateFileIds(FileListSearch);
            var allFiles = dbAccessRepository.DbAccess.GetFiles();
            var allFilesComplement = allFiles.Where(x => !fileIds.Contains(x.Id));
            var searchResult = new SearchResult(allFilesComplement);
            Events.Send(new NewSearchResult(searchResult));
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
        var files = dbAccessRepository.DbAccess.SearchFilesFromIds(fileIds);
        var searchResult = new SearchResult(files);
        Events.Send(new NewSearchResult(searchResult));
    }
}
