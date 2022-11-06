using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FileDB.Comparers;
using FileDB.View;
using FileDB.Sorters;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;
using TextCopy;
using FileDBInterface.DbAccess;
using FileDB.Configuration;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBInterface.Validators;
using FileDB.Extensions;
using CommunityToolkit.Mvvm.Messaging;
using FileDB.Model;

namespace FileDB.ViewModel;

public enum RotationDirection { Clockwise, CounterClockwise }

[AttributeUsage(AttributeTargets.Field)]
public class FileExtensionsAttribute : Attribute
{
    public string[] FileExtensions { get; }

    public FileExtensionsAttribute(string[] fileExtensions)
    {
        FileExtensions = fileExtensions;
    }
}

public enum FileType
{
    [FileExtensions(new string[] { ".jpg", ".png", ".bmp", ".gif" })]
    Picture,

    [FileExtensions(new string[] { ".mkv", ".avi", ".mpg", ".mov", ".mp4" })]
    Movie,

    [FileExtensions(new string[] { ".doc", ".pdf", ".txt", ".md" })]
    Document,

    [FileExtensions(new string[] { ".mp3", ".wav" })]
    Audio,
}

public record PersonToUpdate(int Id, string Name);
public record LocationToUpdate(int Id, string Name);
public record TagToUpdate(int Id, string Name);

public class SearchResult
{
    public string Name => $"{Count} files";

    public string From => DateTime.ToString("HH:mm:ss");

    public DateTime DateTime { get; } = DateTime.Now;

    public int Count => Files.Count;

    public List<FilesModel> Files { get; }

    public SearchResult(IEnumerable<FilesModel> files) : this(files.ToList())
    {
    }

    public SearchResult(List<FilesModel> files)
    {
        Files = files;
    }
}

public enum UpdateHistoryType
{
    TogglePerson,
    ToggleLocation,
    ToggleTag,
}

public class UpdateHistoryItem
{
    public UpdateHistoryType Type { get; }
    public int ItemId { get; }
    public string ItemName { get; }
    public int FunctionKey { get; }
    public string FunctionKeyText => $"F{FunctionKey}";
    public string Description => $"Toggle '{ItemName}'";

    public UpdateHistoryItem(UpdateHistoryType type, int itemId, string itemName, int functionKey)
    {
        Type = type;
        ItemId = itemId;
        ItemName = itemName;
        FunctionKey = functionKey;
    }
}

public partial class FindViewModel : ObservableObject
{
    private readonly Random random = new();

    #region Browsing and sorting commands

    [RelayCommand]
    private void ClearSearch()
    {
        SearchResult = null;
    }

    public List<SortMethodDescription> SortMethods { get; } = Utils.GetSortMethods();

    [ObservableProperty]
    private SortMethod selectedSortMethod = Model.Model.Instance.Config.DefaultSortMethod;

    partial void OnSelectedSortMethodChanged(SortMethod value)
    {
        SortSearchResult(model.Config.KeepSelectionAfterSort);
    }

    [ObservableProperty]
    private bool slideshowActive = false;

    [ObservableProperty]
    private bool randomActive = false;

    [ObservableProperty]
    private bool repeatActive = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowUpdateSection))]
    private bool maximize = false;

    partial void OnMaximizeChanged(bool value)
    {
        WeakReferenceMessenger.Default.Send(new FullscreenBrowsingRequested(value));
    }

    public bool ShowUpdateSection => !Maximize && ReadWriteMode;

    #endregion

    #region Search commands and properties

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
            var location = model.DbAccess.GetLocationById(value.Id);
            if (location.Position != null)
            {
                SearchFileGpsPosition = location.Position;
            }
            else
            {
                SearchFileGpsPosition = string.Empty;
                Dialogs.ShowInfoDialog("This location has no GPS position set.");
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
        int.TryParse(searchPersonAgeFrom, out var from) &&
        int.TryParse(searchPersonAgeTo, out var to) &&
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

    public bool CombineSearchResultPossible => !string.IsNullOrEmpty(combineSearch1) && !string.IsNullOrEmpty(combineSearch2);

    #endregion

    #region Meta-data change commands and properties

    [ObservableProperty]
    private string? fileListSearch;

    [ObservableProperty]
    private string? newFileDescription;

    [ObservableProperty]
    private string? newFileDateTime;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowUpdateSection))]
    private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

    #endregion

    #region Search result

    public ObservableCollection<SearchResult> SearchResultHistory { get; } = new();

    [ObservableProperty]
    private SearchResult? searchResultHistorySelection;

    partial void OnSearchResultHistorySelectionChanged(SearchResult? value)
    {
        if (value != null)
        {
            StopSlideshow();
            SearchResult = value;
        }
    }

    private SearchResult? SearchResult
    {
        get => searchResult;
        set
        {
            if (!EqualityComparer<SearchResult>.Default.Equals(searchResult, value))
            {
                searchResult = value;

                var updateViaHistorySelection = searchResult == searchResultHistorySelection;
                SearchResultHistorySelection = null;

                if (searchResult != null)
                {
                    if (searchResult.Count > 0)
                    {
                        LoadFile(0);
                        SortSearchResult(false);
                        if (!updateViaHistorySelection)
                        {
                            // Searching via history should not add more items to history
                            AddSearchResultToHistory();
                        }
                    }
                    else
                    {
                        ResetFile();
                    }
                }
                else
                {
                    ResetFile();
                }

                FireSearchResultUpdatedEvents();
                FireBrowsingEnabledEvents();
            }
        }
    }
    private SearchResult? searchResult = null;

    private void SortSearchResult(bool preserveSelection)
    {
        switch (SelectedSortMethod)
        {
            case SortMethod.Date:
                SortFilesByDate(preserveSelection);
                break;

            case SortMethod.DateDesc:
                SortFilesByDateDesc(preserveSelection);
                break;

            case SortMethod.Path:
                SortFilesByPath(preserveSelection);
                break;

            case SortMethod.PathDesc:
                SortFilesByPathDesc(preserveSelection);
                break;
        }
    }

    private void AddSearchResultToHistory()
    {
        if (SearchResultHistory.Count == model.Config.SearchHistorySize)
        {
            SearchResultHistory.RemoveAt(0);
        }

        SearchResultHistory.Add(searchResult!);

        OnPropertyChanged(nameof(FindFilesFromHistoryEnabled));
        OnPropertyChanged(nameof(SearchResultHistory));
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SearchResultItemNumber))]
    private int searchResultIndex = -1;

    public int SearchResultItemNumber => searchResultIndex + 1;

    public int SearchNumberOfHits => SearchResult != null ? SearchResult.Count : 0;

    public int TotalNumberOfFiles { get; }

    public bool HasSearchResult => SearchResult != null;

    public bool HasNonEmptySearchResult => SearchResult != null && SearchResult.Count > 0;

    #endregion

    #region Current file properties

    [ObservableProperty]
    private string currentFileInternalPath = string.Empty;

    [ObservableProperty]
    private string currentFileInternalDirectoryPath = string.Empty;

    [ObservableProperty]
    private string currentFilePath = string.Empty;

    [ObservableProperty]
    private string currentFileDescription = string.Empty;

    [ObservableProperty]
    private string currentFileDateTime = string.Empty;

    [ObservableProperty]
    private string currentFilePosition = string.Empty;

    [ObservableProperty]
    private Uri? currentFilePositionLink;

    [ObservableProperty]
    private string currentFilePersons = string.Empty;

    [ObservableProperty]
    private string currentFileLocations = string.Empty;

    [ObservableProperty]
    private string currentFileTags = string.Empty;

    [ObservableProperty]
    private string currentFileLoadError = string.Empty;

    private int currentFileRotation = 0;

    private IEnumerable<PersonModel> currentFilePersonList = new List<PersonModel>();
    private IEnumerable<LocationModel> currentFileLocationList = new List<LocationModel>();
    private IEnumerable<TagModel> currentFileTagList = new List<TagModel>();

    #endregion

    public ObservableCollection<PersonToUpdate> Persons { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedPersonCanBeAdded))]
    [NotifyPropertyChangedFor(nameof(SelectedPersonCanBeRemoved))]
    private PersonToUpdate? selectedPersonToUpdate;
    
    public bool SelectedPersonCanBeAdded =>
        SearchResultIndex != -1 &&
        SelectedPersonToUpdate != null &&
        !currentFilePersonList.Any(x => x.Id == SelectedPersonToUpdate.Id);

    public bool SelectedPersonCanBeRemoved =>
        SearchResultIndex != -1 &&
        SelectedPersonToUpdate != null &&
        currentFilePersonList.Any(x => x.Id == SelectedPersonToUpdate.Id);

    [ObservableProperty]
    private PersonToUpdate? selectedPersonSearch;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Person1And2Selected))]
    private PersonToUpdate? selectedPerson1Search;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Person1And2Selected))]
    private PersonToUpdate? selectedPerson2Search;

    public bool Person1And2Selected => selectedPerson1Search != null && selectedPerson2Search != null;

    public ObservableCollection<LocationToUpdate> Locations { get; } = new();

    public ObservableCollection<LocationToUpdate> LocationsWithPosition { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedLocationCanBeAdded))]
    [NotifyPropertyChangedFor(nameof(SelectedLocationCanBeRemoved))]
    public LocationToUpdate? selectedLocationToUpdate;

    public bool SelectedLocationCanBeAdded =>
        SearchResultIndex != -1 &&
        SelectedLocationToUpdate != null &&
        !currentFileLocationList.Any(x => x.Id == SelectedLocationToUpdate.Id);

    public bool SelectedLocationCanBeRemoved =>
        SearchResultIndex != -1 &&
        SelectedLocationToUpdate != null &&
        currentFileLocationList.Any(x => x.Id == SelectedLocationToUpdate.Id);

    [ObservableProperty]
    private LocationToUpdate? selectedLocationSearch;

    public ObservableCollection<TagToUpdate> Tags { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedTagCanBeAdded))]
    [NotifyPropertyChangedFor(nameof(SelectedTagCanBeRemoved))]
    private TagToUpdate? selectedTagToUpdate;

    public bool SelectedTagCanBeAdded =>
        SearchResultIndex != -1 &&
        SelectedTagToUpdate != null &&
        !currentFileTagList.Any(x => x.Id == SelectedTagToUpdate.Id);

    public bool SelectedTagCanBeRemoved =>
        SearchResultIndex != -1 &&
        SelectedTagToUpdate != null &&
        currentFileTagList.Any(x => x.Id == SelectedTagToUpdate.Id);

    [ObservableProperty]
    private TagToUpdate? selectedTagSearch;

    public ObservableCollection<UpdateHistoryItem> UpdateHistoryItems { get; } = new();

    public bool HasUpdateHistory => UpdateHistoryItems.Count > 0;

    private readonly DispatcherTimer slideshowTimer = new();

    private readonly Model.Model model = Model.Model.Instance;

    public static FindViewModel Instance => instance ??= new();
    private static FindViewModel? instance;

    private int? prevEditedFileId = null;

    private FindViewModel()
    {
        TotalNumberOfFiles = model.DbAccess.GetFileCount();

        ReloadPersons();
        ReloadLocations();
        ReloadTags();

        WeakReferenceMessenger.Default.Register<FilesImported>(this, (r, m) =>
        {
            ImportedFileList = Utils.CreateFileList(m.Files);
        });

        slideshowTimer.Tick += SlideshowTimer_Tick;
        slideshowTimer.Interval = TimeSpan.FromSeconds(model.Config.SlideshowDelay);

        WeakReferenceMessenger.Default.Register<PersonsUpdated>(this, (r, m) =>
        {
            ReloadPersons();
        });

        WeakReferenceMessenger.Default.Register<LocationsUpdated>(this, (r, m) =>
        {
            ReloadLocations();
        });

        WeakReferenceMessenger.Default.Register<TagsUpdated>(this, (r, m) =>
        {
            ReloadTags();
        });

        WeakReferenceMessenger.Default.Register<ConfigLoaded>(this, (r, m) =>
        {
            ReadWriteMode = !model.Config.ReadOnly;
            slideshowTimer.Interval = TimeSpan.FromSeconds(model.Config.SlideshowDelay);
        });

    }

    [RelayCommand]
    private void PrevFile()
    {
        StopSlideshow();
        SelectPrevFile();
    }

    public bool PrevFileAvailable => SearchResultIndex > 0;
    public bool NextFileAvailable => searchResult != null && SearchResultIndex < searchResult.Count - 1;
    public bool FirstFileAvailable => searchResult != null && SearchResultIndex > 0;
    public bool LastFileAvailable => searchResult != null && SearchResultIndex < searchResult.Count - 1;
    public bool PrevDirectoryAvailable => HasNonEmptySearchResult;
    public bool NextDirectoryAvailable => HasNonEmptySearchResult;

    private void FireSearchResultUpdatedEvents()
    {
        OnPropertyChanged(nameof(HasSearchResult));
        OnPropertyChanged(nameof(HasNonEmptySearchResult));
        OnPropertyChanged(nameof(SearchNumberOfHits));
    }

    private void FireBrowsingEnabledEvents()
    {
        OnPropertyChanged(nameof(PrevFileAvailable));
        OnPropertyChanged(nameof(NextFileAvailable));
        OnPropertyChanged(nameof(FirstFileAvailable));
        OnPropertyChanged(nameof(LastFileAvailable));
        OnPropertyChanged(nameof(PrevDirectoryAvailable));
        OnPropertyChanged(nameof(NextDirectoryAvailable));
    }

    [RelayCommand]
    public void NextFile()
    {
        StopSlideshow();
        SelectNextFile();
    }

    private void SelectPrevFile()
    {
        LoadFile(SearchResultIndex - 1);
        FireBrowsingEnabledEvents();
    }

    private void SelectNextFile()
    {
        LoadFile(SearchResultIndex + 1);
        FireBrowsingEnabledEvents();
    }

    private void SelectNextRandomFile()
    {
        LoadFile(random.Next(SearchResult!.Count));
        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void PrevDirectory()
    {
        if (!PrevDirectoryAvailable)
        {
            return;
        }

        StopSlideshow();

        if (SearchResultIndex < 1)
            return;

        var currentDirectory = Path.GetDirectoryName(SearchResult!.Files[SearchResultIndex].Path);

        for (int i = SearchResultIndex - 1; i >= 0; i--)
        {
            var directory = Path.GetDirectoryName(SearchResult.Files[i].Path);
            if (directory != currentDirectory)
            {
                LoadFile(i);
                return;
            }
        }

        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void NextDirectory()
    {
        if (!NextDirectoryAvailable)
        {
            return;
        }

        StopSlideshow();

        if (SearchResultIndex == -1 || SearchResultIndex == SearchResult!.Count - 1)
            return;

        var currentDirectory = Path.GetDirectoryName(SearchResult.Files[SearchResultIndex].Path);

        for (int i = SearchResultIndex + 1; i < SearchResult.Count; i++)
        {
            var directory = Path.GetDirectoryName(SearchResult.Files[i].Path);
            if (directory != currentDirectory)
            {
                LoadFile(i);
                return;
            }
        }

        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void FirstFile()
    {
        StopSlideshow();
        LoadFile(0);
        FireBrowsingEnabledEvents();
    }

    [RelayCommand]
    private void LastFile()
    {
        StopSlideshow();
        if (searchResult != null)
        {
            LoadFile(SearchResult!.Count - 1);
        }
        FireBrowsingEnabledEvents();
    }

    public void SortFilesByDate(bool preserveSelection)
    {
        SortFiles(new FilesModelByDateSorter(), false, preserveSelection);
    }

    public void SortFilesByDateDesc(bool preserveSelection)
    {
        SortFiles(new FilesModelByDateSorter(), true, preserveSelection);
    }

    public void SortFilesByPath(bool preserveSelection)
    {
        SortFiles(new FilesModelByPathSorter(), false, preserveSelection);
    }

    public void SortFilesByPathDesc(bool preserveSelection)
    {
        SortFiles(new FilesModelByPathSorter(), true, preserveSelection);
    }

    private void SortFiles(IComparer<FilesModel> comparer, bool desc, bool preserveSelection)
    {
        StopSlideshow();
        if (HasNonEmptySearchResult)
        {
            var selectedFile = SearchResult!.Files[searchResultIndex];
            if (desc)
            {
                SearchResult.Files.Sort((x, y) => comparer.Compare(y, x));
            }
            else
            {
                SearchResult.Files.Sort(comparer);
            }
            LoadFile(preserveSelection ? SearchResult.Files.IndexOf(selectedFile) : 0);
        }
    }

    [RelayCommand]
    private void ToggleSlideshow()
    {
        if (SlideshowActive)
        {
            StartSlideshow();
        }
        else
        {
            StopSlideshow();
        }
    }

    private void StartSlideshow()
    {
        if (SearchResult != null && SearchResult.Count > 1)
        {
            slideshowTimer.Start();
        }
    }

    private void StopSlideshow()
    {
        slideshowTimer.Stop();
        SlideshowActive = false;
    }

    private void SlideshowTimer_Tick(object? sender, EventArgs e)
    {
        if (RandomActive)
        {
            SelectNextRandomFile();
        }
        else
        {
            if (RepeatActive)
            {
                if (SearchResultIndex == SearchResult!.Count - 1)
                {
                    LoadFile(0);
                }
                else
                {
                    SelectNextFile();
                }
            }
            else
            {
                SelectNextFile();
                if (SearchResultIndex == SearchResult!.Count - 1)
                {
                    StopSlideshow();
                }
            }
        }
    }

    [RelayCommand]
    private void FindRandomFiles()
    {
        StopSlideshow();

        if (int.TryParse(NumRandomFiles, out var value))
        {
            SearchResult = new SearchResult(model.DbAccess.SearchFilesRandom(value));
        }
    }

    [RelayCommand]
    private void FindCurrentDirectoryFiles()
    {
        StopSlideshow();
        if (SearchResultIndex == -1)
        {
            Dialogs.ShowErrorDialog("No file opened");
            return;
        }

        var path = SearchResult!.Files[SearchResultIndex].Path;
        var dir = Path.GetDirectoryName(path)!.Replace('\\', '/');
        SearchResult = new SearchResult(model.DbAccess.SearchFilesByPath(dir));
    }

    [RelayCommand]
    private void FindAllFiles()
    {
        StopSlideshow();
        SearchResult = new SearchResult(model.DbAccess.GetFiles());
    }

    [RelayCommand]
    private void FindImportedFiles()
    {
        StopSlideshow();
        if (!string.IsNullOrEmpty(ImportedFileList))
        {
            var fileIds = Utils.CreateFileIds(ImportedFileList);
            SearchResult = new SearchResult(model.DbAccess.SearchFilesFromIds(fileIds));
        }
    }

    [RelayCommand]
    private void FindBrowsedFiles()
    {
        StopSlideshow();

        var window = new BrowseDirectoriesWindow
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        var windowVm = (BrowseDirectoriesViewModel)window.DataContext;
        var selectedDir = windowVm.SelectedDirectoryPath;
        if (selectedDir != null)
        {
            SearchResult = new SearchResult(model.DbAccess.SearchFilesByPath(selectedDir));
        }
    }

    [RelayCommand]
    private void FindFilesByText()
    {
        StopSlideshow();
        if (!string.IsNullOrEmpty(SearchPattern))
        {
            SearchResult = new SearchResult(model.DbAccess.SearchFiles(SearchPattern));
        }
        else
        {
            SearchResult = null;
        }
    }

    [RelayCommand]
    private void FindFilesBySex()
    {
        StopSlideshow();
        if (SearchBySexSelection != null)
        {
            SearchResult = new SearchResult(model.DbAccess.SearchFilesBySex(SearchBySexSelection.Value));
        }
    }

    [RelayCommand]
    private void FindFilesByDate()
    {
        StopSlideshow();
        SearchResult = new SearchResult(model.DbAccess.SearchFilesByDate(SearchStartDate.Date, SearchEndDate.Date));
    }

    [RelayCommand]
    private void FindFilesByType()
    {
        StopSlideshow();
        if (SelectedFileType == null)
        {
            return;
        }

        var fileExtensions = SelectedFileType.GetAttribute<FileExtensionsAttribute>().FileExtensions;
        
        var result = new List<FilesModel>();
        foreach (var extension in fileExtensions)
        {
            result.AddRange(model.DbAccess.SearchFilesByExtension(extension));
        }

        SearchResult = new SearchResult(result);
    }

    [RelayCommand]
    private void SearchFilePositionFromCurrentFile()
    {
        SearchFileGpsPosition = currentFilePosition;
    }

    [RelayCommand]
    private void FindFilesByGpsPosition()
    {
        StopSlideshow();

        if (string.IsNullOrEmpty(SearchFileGpsPosition))
        {
            Dialogs.ShowErrorDialog("No position specified");
            return;
        }

        if (string.IsNullOrEmpty(SearchFileGpsRadius))
        {
            Dialogs.ShowErrorDialog("No radius specified");
            return;
        }
        if (!double.TryParse(SearchFileGpsRadius, out var radius) || radius < 1)
        {
            Dialogs.ShowErrorDialog("Invalid radius");
            return;
        }

        var gpsPos = DatabaseParsing.ParseFilesPosition(SearchFileGpsPosition);
        if (gpsPos == null)
        {
            Dialogs.ShowErrorDialog("Invalid GPS position");
            return;
        }

        var nearFiles = model.DbAccess.SearchFilesNearGpsPosition(gpsPos.Value.lat, gpsPos.Value.lon, radius).ToList();

        // TODO: checkbox for selecting if this should be included?
        var nearLocations = model.DbAccess.SearchLocationsNearGpsPosition(gpsPos.Value.lat, gpsPos.Value.lon, radius);
        nearFiles.AddRange(model.DbAccess.SearchFilesWithLocations(nearLocations.Select(x => x.Id)));

        SearchResult = new SearchResult(nearFiles);
    }

    [RelayCommand]
    private void FindFilesWithPerson()
    {
        StopSlideshow();
        if (SelectedPersonSearch != null)
        {
            SearchResult = new SearchResult(model.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id }));
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonUnique()
    {
        StopSlideshow();
        if (SelectedPersonSearch != null)
        {
            var files = model.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id });
            var result = files.Where(x => model.DbAccess.GetPersonsFromFile(x.Id).Count() == 1);
            SearchResult = new SearchResult(result);
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonGroup()
    {
        StopSlideshow();
        if (SelectedPersonSearch != null)
        {
            var files = model.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id });
            var result = files.Where(x => model.DbAccess.GetPersonsFromFile(x.Id).Count() > 1);
            SearchResult = new SearchResult(result);
        }
    }

    [RelayCommand]
    private void FindFilesWithPersons()
    {
        StopSlideshow();
        if (SelectedPerson1Search != null && SelectedPerson2Search != null)
        {
            SearchResult = new SearchResult(model.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id, SelectedPerson2Search.Id }));
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonsUnique()
    {
        StopSlideshow();
        if (SelectedPerson1Search != null && SelectedPerson2Search != null)
        {
            var files = model.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id });
            var result = files.Where(x =>
            {
                var filePersons = model.DbAccess.GetPersonsFromFile(x.Id).ToList();
                return filePersons.Count() == 2 && filePersons.Any(y => y.Id == SelectedPerson2Search.Id);
            });
            SearchResult = new SearchResult(result);
        }
    }

    [RelayCommand]
    private void FindFilesWithPersonsGroup()
    {
        StopSlideshow();
        if (SelectedPerson1Search != null && SelectedPerson2Search != null)
        {
            var files = model.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id });
            var result = files.Where(x =>
            {
                var filePersons = model.DbAccess.GetPersonsFromFile(x.Id).ToList();
                return filePersons.Count() > 2 && filePersons.Any(y => y.Id == SelectedPerson2Search.Id);
            });
            SearchResult = new SearchResult(result);
        }
    }

    [RelayCommand]
    private void FindFilesWithLocation()
    {
        StopSlideshow();
        if (SelectedLocationSearch != null)
        {
            SearchResult = new SearchResult(model.DbAccess.SearchFilesWithLocations(new List<int>() { SelectedLocationSearch.Id }));
        }
    }

    [RelayCommand]
    private void FindFilesWithTag()
    {
        StopSlideshow();
        if (SelectedTagSearch != null)
        {
            SearchResult = new SearchResult(model.DbAccess.SearchFilesWithTags(new List<int>() { SelectedTagSearch.Id }));
        }
    }

    [RelayCommand]
    private void FindFilesByPersonAge()
    {
        StopSlideshow();
        if (!string.IsNullOrEmpty(SearchPersonAgeFrom))
        {
            if (!int.TryParse(SearchPersonAgeFrom, out var ageFrom))
            {
                Dialogs.ShowErrorDialog("Invalid age format");
                return;
            }

            int ageTo;
            if (string.IsNullOrEmpty(SearchPersonAgeTo))
            {
                ageTo = ageFrom;
            }
            else if (!int.TryParse(SearchPersonAgeTo, out ageTo))
            {
                Dialogs.ShowErrorDialog("Invalid age format");
                return;
            }

            var result = new List<FilesModel>();
            var personsWithAge = model.DbAccess.GetPersons().Where(p => p.DateOfBirth != null);

            foreach (var person in personsWithAge)
            {
                var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth!);
                foreach (var file in model.DbAccess.SearchFilesWithPersons(new List<int>() { person.Id }))
                {
                    var fileDatetime = DatabaseParsing.ParseFilesDatetime(file.Datetime);
                    if (fileDatetime != null)
                    {
                        int personAgeInFile = DatabaseUtils.GetYearsAgo(fileDatetime.Value, dateOfBirth);
                        if (personAgeInFile >= ageFrom && personAgeInFile <= ageTo)
                        {
                            result.Add(file);
                        }
                    }
                }
            }

            SearchResult = new SearchResult(result);
        }
    }

    public bool FindFilesFromHistoryEnabled => SearchResultHistory.Count >= 2;

    [RelayCommand]
    private void FindFilesFromUnion()
    {
        if (SearchResultHistory.Count >= 2)
        {
            var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
            var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
            SearchResult = new SearchResult(files1.Union(files2, new FilesModelIdComparer()));
        }
    }

    [RelayCommand]
    private void FindFilesFromIntersection()
    {
        if (SearchResultHistory.Count >= 2)
        {
            var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
            var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
            SearchResult = new SearchResult(files1.Intersect(files2, new FilesModelIdComparer()));
        }
    }

    [RelayCommand]
    private void FindFilesFromDifference()
    {
        if (SearchResultHistory.Count >= 2)
        {
            var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
            var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
            var uniqueFiles1 = files1.Except(files2, new FilesModelIdComparer());
            var uniqueFiles2 = files2.Except(files1, new FilesModelIdComparer());
            SearchResult = new SearchResult(uniqueFiles1.Union(uniqueFiles2, new FilesModelIdComparer()));
        }
    }

    [RelayCommand]
    private void FindFilesFromMissingCategorization()
    {
        StopSlideshow();
        SearchResult = new SearchResult(model.DbAccess.SearchFilesWithMissingData());
    }

    [RelayCommand]
    private void FindFilesFromList()
    {
        StopSlideshow();
        if (!string.IsNullOrEmpty(fileListSearch))
        {
            var fileIds = Utils.CreateFileIds(fileListSearch);
            SearchResult = new SearchResult(model.DbAccess.SearchFilesFromIds(fileIds));
        }
    }

    [RelayCommand]
    private void FindFilesFromListComplement()
    {
        StopSlideshow();
        if (!string.IsNullOrEmpty(fileListSearch))
        {
            var fileIds = Utils.CreateFileIds(fileListSearch);
            var allFiles = model.DbAccess.GetFiles();
            var allFilesComplement = allFiles.Where(x => !fileIds.Contains(x.Id));
            SearchResult = new SearchResult(allFilesComplement);
        }
    }

    [RelayCommand]
    private void OpenFileLocation()
    {
        if (!string.IsNullOrEmpty(CurrentFilePath) && File.Exists(CurrentFilePath))
        {
            Utils.SelectFileInExplorer(CurrentFilePath);
        }
    }

    [RelayCommand]
    private void OpenFileWithDefaultApp()
    {
        if (!string.IsNullOrEmpty(CurrentFilePath) && File.Exists(CurrentFilePath))
        {
            Utils.OpenFileWithDefaultApp(CurrentFilePath);
        }
    }

    [RelayCommand]
    private void CopyFileId()
    {
        if (SearchResultIndex != -1)
        {
            var selection = SearchResult!.Files[SearchResultIndex];
            ClipboardService.SetText(Utils.CreateFileList(new List<FilesModel>() { selection }));
        }
    }

    [RelayCommand]
    private void ExportFileList()
    {
        var window = new ExportWindow
        {
            Owner = Application.Current.MainWindow
        };
        var viewModel = (ExportViewModel)window.DataContext;
        viewModel.SearchResult = SearchResult;
        window.ShowDialog();
    }

    [RelayCommand]
    private void CopyFileList()
    {
        ClipboardService.SetText(Utils.CreateFileList(SearchResult!.Files));
    }

    private void LoadFile(int index)
    {
        if (SearchResult != null &&
            index >= 0 && index < SearchResult.Count)
        {
            SearchResultIndex = index;

            var selection = SearchResult.Files[SearchResultIndex];

            currentFilePersonList = model.DbAccess.GetPersonsFromFile(selection.Id);
            OnPropertyChanged(nameof(SelectedPersonCanBeAdded));
            OnPropertyChanged(nameof(SelectedPersonCanBeRemoved));

            currentFileLocationList = model.DbAccess.GetLocationsFromFile(selection.Id);
            OnPropertyChanged(nameof(SelectedLocationCanBeAdded));
            OnPropertyChanged(nameof(SelectedLocationCanBeRemoved));

            currentFileTagList = model.DbAccess.GetTagsFromFile(selection.Id);
            OnPropertyChanged(nameof(SelectedTagCanBeAdded));
            OnPropertyChanged(nameof(SelectedTagCanBeRemoved));

            CurrentFileInternalDirectoryPath = Path.GetDirectoryName(selection.Path)!.Replace(@"\", "/");
            CurrentFileInternalPath = selection.Path;
            CurrentFilePath = model.FilesystemAccess.ToAbsolutePath(selection.Path);
            CurrentFileDescription = selection.Description ?? string.Empty;
            CurrentFileDateTime = GetFileDateTimeString(selection.Datetime);
            CurrentFilePosition = selection.Position != null ? Utils.CreateShortFilePositionString(selection.Position) : string.Empty;
            CurrentFilePositionLink = selection.Position != null ? Utils.CreatePositionUri(selection.Position, model.Config.LocationLink) : null;
            CurrentFilePersons = GetFilePersonsString(selection);
            CurrentFileLocations = GetFileLocationsString(selection.Id);
            CurrentFileTags = GetFileTagsString(selection.Id);

            // Note: reading of orientation from Exif is done here to get correct visualization for files added to database before orientation was parsed
            currentFileRotation = DatabaseParsing.OrientationToDegrees(selection.Orientation ?? model.FilesystemAccess.ParseFileMetadata(CurrentFilePath).Orientation);

            NewFileDescription = CurrentFileDescription;
            NewFileDateTime = selection.Datetime;

            var uri = new Uri(CurrentFilePath, UriKind.Absolute);
            try
            {
                CurrentFileLoadError = string.Empty;
                WeakReferenceMessenger.Default.Send(new ShowImage(new BitmapImage(uri), -currentFileRotation));

                model.FileLoaded(selection);
            }
            catch (WebException e)
            {
                CurrentFileLoadError = $"Image loading error:\n{e.Message}";
                WeakReferenceMessenger.Default.Send(new CloseImage());
            }
            catch (IOException e)
            {
                CurrentFileLoadError = $"Image loading error:\n{e.Message}";
                WeakReferenceMessenger.Default.Send(new CloseImage());
            }
            catch (NotSupportedException e)
            {
                CurrentFileLoadError = $"File format not supported (use the Open button to open file with the default application):\n{e.Message}";
                WeakReferenceMessenger.Default.Send(new CloseImage());
            }
        }
    }

    private void ResetFile()
    {
        SearchResultIndex = -1;

        CurrentFileInternalPath = string.Empty;
        CurrentFileInternalDirectoryPath = string.Empty;
        CurrentFilePath = string.Empty;
        CurrentFileDescription = string.Empty;
        CurrentFileDateTime = string.Empty;
        CurrentFilePosition = string.Empty;
        CurrentFilePositionLink = null;
        CurrentFilePersons = string.Empty;
        CurrentFileLocations = string.Empty;
        CurrentFileTags = string.Empty;
        currentFileRotation = 0;

        NewFileDescription = string.Empty;
        NewFileDateTime = string.Empty;

        CurrentFileLoadError = "No match";
        WeakReferenceMessenger.Default.Send(new CloseImage());
    }

    private string GetFileDateTimeString(string? datetimeString)
    {
        var datetime = DatabaseParsing.ParseFilesDatetime(datetimeString);
        if (datetime == null)
        {
            return string.Empty;
        }

        // Note: when no time is available the string is used to avoid including time 00:00
        var resultString = datetimeString!.Contains('T') ? datetime.Value.ToString("yyyy-MM-dd HH:mm") : datetimeString;

        var now = DateTime.Now;
        int yearsAgo = DatabaseUtils.GetYearsAgo(now, datetime.Value);
        if (yearsAgo == 0 && now.Year == datetime.Value.Year)
        {
            resultString = $"{resultString} (this year)";
        }
        else if (yearsAgo <= 1)
        {
            resultString = $"{resultString} ({yearsAgo} year ago)";
        }
        else if (yearsAgo > 1)
        {
            resultString = $"{resultString} ({yearsAgo} years ago)";
        }
        return resultString;
    }

    private string GetFilePersonsString(FilesModel selection)
    {
        var personStrings = currentFilePersonList.Select(p => $"{p.Firstname} {p.Lastname}{Utils.GetPersonAgeInFileString(selection.Datetime, p.DateOfBirth)}");
        return string.Join("\n", personStrings);
    }

    private string GetFileLocationsString(int fileId)
    {
        var locationStrings = currentFileLocationList.Select(l => l.Name);
        return string.Join("\n", locationStrings);
    }

    private string GetFileTagsString(int fileId)
    {
        var tagStrings = currentFileTagList.Select(t => t.Name);
        return string.Join("\n", tagStrings);
    }

    [RelayCommand]
    private void AddFilePerson()
    {
        if (SearchResultIndex == -1)
        {
            Dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedPersonToUpdate == null)
        {
            Dialogs.ShowErrorDialog("No person selected");
            return;
        }

        AddFilePersonToCurrentFile(SelectedPersonToUpdate);
    }

    private void AddFilePersonToCurrentFile(PersonToUpdate person)
    {
        var fileId = SearchResult!.Files[SearchResultIndex].Id;
        if (!model.DbAccess.GetPersonsFromFile(fileId).Any(p => p.Id == person.Id))
        {
            model.DbAccess.InsertFilePerson(fileId, person.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, person.Id, person.Name);
            prevEditedFileId = fileId;
        }
    }

    [RelayCommand]
    private void RemoveFilePerson()
    {
        if (SearchResultIndex == -1)
        {
            Dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedPersonToUpdate == null)
        {
            Dialogs.ShowErrorDialog("No person selected");
            return;
        }

        var fileId = SearchResult!.Files[SearchResultIndex].Id;
        model.DbAccess.DeleteFilePerson(fileId, SelectedPersonToUpdate.Id);
        LoadFile(SearchResultIndex);
        AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, SelectedPersonToUpdate.Id, SelectedPersonToUpdate.Name);
        prevEditedFileId = fileId;
    }

    [RelayCommand]
    private void AddFileLocation()
    {
        if (SearchResultIndex == -1)
        {
            Dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedLocationToUpdate == null)
        {
            Dialogs.ShowErrorDialog("No location selected");
            return;
        }

        AddFileLocationToCurrentFile(SelectedLocationToUpdate);
    }

    private void AddFileLocationToCurrentFile(LocationToUpdate location)
    {
        var fileId = SearchResult!.Files[SearchResultIndex].Id;
        if (!model.DbAccess.GetLocationsFromFile(fileId).Any(l => l.Id == location.Id))
        {
            model.DbAccess.InsertFileLocation(fileId, location.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, location.Id, location.Name);
            prevEditedFileId = fileId;
        }
    }

    [RelayCommand]
    private void RemoveFileLocation()
    {
        if (SearchResultIndex == -1)
        {
            Dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedLocationToUpdate == null)
        {
            Dialogs.ShowErrorDialog("No location selected");
            return;
        }

        var fileId = SearchResult!.Files[SearchResultIndex].Id;
        model.DbAccess.DeleteFileLocation(fileId, SelectedLocationToUpdate.Id);
        LoadFile(SearchResultIndex);
        AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, SelectedLocationToUpdate.Id, SelectedLocationToUpdate.Name);
        prevEditedFileId = fileId;
    }

    [RelayCommand]
    private void AddFileTag()
    {
        if (SearchResultIndex == -1)
        {
            Dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedTagToUpdate == null)
        {
            Dialogs.ShowErrorDialog("No tag selected");
            return;
        }

        AddFileTagToCurrentFile(SelectedTagToUpdate);
    }

    private void AddFileTagToCurrentFile(TagToUpdate tag)
    {
        var fileId = SearchResult!.Files[SearchResultIndex].Id;
        if (!model.DbAccess.GetTagsFromFile(fileId).Any(t => t.Id == tag.Id))
        {
            model.DbAccess.InsertFileTag(fileId, tag.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, tag.Id, tag.Name);
            prevEditedFileId = fileId;
        }
    }

    [RelayCommand]
    private void RemoveFileTag()
    {
        if (SearchResultIndex == -1)
        {
            Dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (SelectedTagToUpdate == null)
        {
            Dialogs.ShowErrorDialog("No tag selected");
            return;
        }

        var fileId = SearchResult!.Files[SearchResultIndex].Id;
        model.DbAccess.DeleteFileTag(fileId, SelectedTagToUpdate.Id);
        LoadFile(SearchResultIndex);
        AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, SelectedTagToUpdate.Id, SelectedTagToUpdate.Name);
        prevEditedFileId = fileId;
    }

    [RelayCommand]
    private void SetFileDescription()
    {
        if (SearchResultIndex != -1)
        {
            var selection = SearchResult!.Files[SearchResultIndex];
            var fileId = selection.Id;
            NewFileDescription = NewFileDescription?.Trim().ReplaceLineEndings(FilesModelValidator.DescriptionLineEnding);
            var description = string.IsNullOrEmpty(NewFileDescription) ? null : NewFileDescription;

            try
            {
                model.DbAccess.UpdateFileDescription(fileId, description);
                selection.Description = description;
                LoadFile(SearchResultIndex);
                prevEditedFileId = fileId;
            }
            catch (DataValidationException e)
            {
                Dialogs.ShowErrorDialog(e.Message);
            }
        }
    }

    [RelayCommand]
    private void SetFileDateTime()
    {
        if (SearchResultIndex != -1)
        {
            var selection = SearchResult!.Files[SearchResultIndex];
            var fileId = selection.Id;
            NewFileDateTime = NewFileDateTime?.Trim();

            var dateTime = string.IsNullOrEmpty(NewFileDateTime) ? null : NewFileDateTime;

            try
            {
                model.DbAccess.UpdateFileDatetime(fileId, dateTime);
                selection.Datetime = dateTime;
                LoadFile(SearchResultIndex);
                prevEditedFileId = fileId;
            }
            catch (DataValidationException e)
            {
                Dialogs.ShowErrorDialog(e.Message);
            }
        }
    }

    [RelayCommand]
    private void ReApplyFileMetaData()
    {
        if (SearchResultIndex == -1)
        {
            Dialogs.ShowErrorDialog("No file selected");
            return;
        }

        if (prevEditedFileId == null)
        {
            Dialogs.ShowErrorDialog("Meta-data not edited previously");
            return;
        }

        var selection = SearchResult!.Files[SearchResultIndex];
        var fileId = selection.Id;

        try
        {
            var prevEditedFile = model.DbAccess.GetFileById(prevEditedFileId.Value)!;
            
            var prevPersons = model.DbAccess.GetPersonsFromFile(prevEditedFileId.Value);
            var prevLocations = model.DbAccess.GetLocationsFromFile(prevEditedFileId.Value);
            var prevTags = model.DbAccess.GetTagsFromFile(prevEditedFileId.Value);

            var persons = model.DbAccess.GetPersonsFromFile(fileId);
            var locations = model.DbAccess.GetLocationsFromFile(fileId);
            var tags = model.DbAccess.GetTagsFromFile(fileId);

            foreach (var person in prevPersons.Where(x => !persons.Select(x => x.Id).Contains(x.Id)))
            {
                model.DbAccess.InsertFilePerson(fileId, person.Id);
            }
            foreach (var location in prevLocations.Where(x => !locations.Select(x => x.Id).Contains(x.Id)))
            {
                model.DbAccess.InsertFileLocation(fileId, location.Id);
            }
            foreach (var tag in prevTags.Where(x => !tags.Select(x => x.Id).Contains(x.Id)))
            {
                model.DbAccess.InsertFileTag(fileId, tag.Id);
            }
            
            model.DbAccess.UpdateFileDescription(fileId, prevEditedFile.Description);
            selection.Description = prevEditedFile.Description;

            LoadFile(SearchResultIndex);
        }
        catch (DataValidationException e)
        {
            Dialogs.ShowErrorDialog(e.Message);
        }
    }

    [RelayCommand]
    private void RotateFileClockwise()
    {
        RotateFile(RotationDirection.Clockwise);
    }

    [RelayCommand]
    private void RotateFileCounterClockwise()
    {
        RotateFile(RotationDirection.CounterClockwise);
    }

    private void RotateFile(RotationDirection imageRotationDirection)
    {
        if (SearchResultIndex != -1)
        {
            int cameraNewDegrees = currentFileRotation;
            if (imageRotationDirection == RotationDirection.CounterClockwise)
            {
                cameraNewDegrees += 90;
                if (cameraNewDegrees > 270)
                {
                    cameraNewDegrees = 0;
                }
            }
            else if (imageRotationDirection == RotationDirection.Clockwise)
            {
                cameraNewDegrees -= 90;
                if (cameraNewDegrees < 0)
                {
                    cameraNewDegrees = 270;
                }
            }

            var selection = SearchResult!.Files[SearchResultIndex];
            var newOrientation = DatabaseParsing.DegreesToOrientation(cameraNewDegrees);
            model.DbAccess.UpdateFileOrientation(selection.Id, newOrientation);
            selection.Orientation = newOrientation;

            LoadFile(SearchResultIndex);
        }
    }

    [RelayCommand]
    private void UpdateFileOrientationFromMetaData()
    {
        if (SearchResultIndex != -1)
        {
            if (Dialogs.ShowConfirmDialog("Reload orientation from file meta-data?"))
            {
                var selection = SearchResult!.Files[SearchResultIndex];
                var fileMetadata = model.FilesystemAccess.ParseFileMetadata(model.FilesystemAccess.ToAbsolutePath(selection.Path));
                model.DbAccess.UpdateFileOrientation(selection.Id, fileMetadata.Orientation);
                selection.Orientation = fileMetadata.Orientation;
                LoadFile(SearchResultIndex);
            }
        }
    }

    [RelayCommand]
    private void UpdateFileFromMetaData()
    {
        if (SearchResultIndex != -1)
        {
            if (Dialogs.ShowConfirmDialog("Reload date and GPS position from file meta-data?"))
            {
                var selection = SearchResult!.Files[SearchResultIndex];
                model.DbAccess.UpdateFileFromMetaData(selection.Id, model.FilesystemAccess);

                var updatedFile = model.DbAccess.GetFileById(selection.Id)!;
                selection.Datetime = updatedFile.Datetime;
                selection.Position = updatedFile.Position;
                selection.Orientation = updatedFile.Orientation;
                LoadFile(SearchResultIndex);
            }
        }
    }

    [RelayCommand]
    private void CreatePerson()
    {
        var window = new AddPersonWindow
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        if (window.DataContext is AddPersonViewModel viewModel && viewModel.AffectedPerson != null)
        {
            AddFilePersonToCurrentFile(new PersonToUpdate(viewModel.AffectedPerson.Id, $"{viewModel.AffectedPerson.Firstname} {viewModel.AffectedPerson.Lastname}"));
        }
    }

    [RelayCommand]
    private void CreateLocation()
    {
        var window = new AddLocationWindow
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();

        if (window.DataContext is AddLocationViewModel viewModel && viewModel.AffectedLocation != null)
        {
            AddFileLocationToCurrentFile(new LocationToUpdate(viewModel.AffectedLocation.Id, viewModel.AffectedLocation.Name));
        }
    }

    [RelayCommand]
    private void CreateTag()
    {
        var window = new AddTagWindow
        {
            Owner = Application.Current.MainWindow
        };
        window.ShowDialog();
        
        if (window.DataContext is AddTagViewModel viewModel && viewModel.AffectedTag != null)
        {
            AddFileTagToCurrentFile(new TagToUpdate(viewModel.AffectedTag.Id, viewModel.AffectedTag.Name));
        }
    }

    private void ReloadPersons()
    {
        Persons.Clear();
        var persons = model.DbAccess.GetPersons().ToList();
        persons.Sort(new PersonModelByNameSorter());
        foreach (var person in persons.Select(p => new PersonToUpdate(p.Id, $"{p.Firstname} {p.Lastname}")))
        {
            Persons.Add(person);
        }
    }

    private void ReloadLocations()
    {
        Locations.Clear();
        LocationsWithPosition.Clear();
        
        var locations = model.DbAccess.GetLocations().ToList();
        locations.Sort(new LocationModelByNameSorter());
        
        foreach (var location in locations)
        {
            var locationToUpdate = new LocationToUpdate(location.Id, location.Name);
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
        var tags = model.DbAccess.GetTags().ToList();
        tags.Sort(new TagModelByNameSorter());
        foreach (var tag in tags.Select(t => new TagToUpdate(t.Id, t.Name)))
        {
            Tags.Add(tag);
        }
    }

    private void AddUpdateHistoryItem(UpdateHistoryType type, int itemId, string itemName)
    {
        if (UpdateHistoryItems.Count >= 12)
        {
            return;
        }

        var duplicatedItem = UpdateHistoryItems.FirstOrDefault(x => x.Type == type && x.ItemName == itemName);
        if (duplicatedItem != null)
        {
            return;
        }

        for (int i=1; i<=12; i++)
        {
            var item = UpdateHistoryItems.FirstOrDefault(x => x.FunctionKey == i);
            if (item == null)
            {
                UpdateHistoryItems.Insert(i - 1, new UpdateHistoryItem(type, itemId, itemName, i));
                OnPropertyChanged(nameof(HasUpdateHistory));
                return;
            }
        }
    }

    [RelayCommand]
    private void FunctionKey(string parameter)
    {
        if (!ReadWriteMode || !HasNonEmptySearchResult)
        {
            return;
        }

        var fileId = SearchResult!.Files[SearchResultIndex].Id;

        var functionKey = int.Parse(parameter);
        var historyItem = UpdateHistoryItems.FirstOrDefault(x => x.FunctionKey == functionKey);
        if (historyItem == null)
        {
            return;
        }

        switch (historyItem.Type)
        {
            case UpdateHistoryType.TogglePerson:
                var personId = historyItem.ItemId;
                if (model.DbAccess.GetPersonsFromFile(fileId).Any(x => x.Id == personId))
                {
                    model.DbAccess.DeleteFilePerson(fileId, personId);
                }
                else
                {
                    model.DbAccess.InsertFilePerson(fileId, personId);
                }
                break;

            case UpdateHistoryType.ToggleLocation:
                var locationId = historyItem.ItemId;
                if (model.DbAccess.GetLocationsFromFile(fileId).Any(x => x.Id == locationId))
                {
                    model.DbAccess.DeleteFileLocation(fileId, locationId);
                }
                else
                {
                    model.DbAccess.InsertFileLocation(fileId, locationId);
                }
                break;

            case UpdateHistoryType.ToggleTag:
                var tagId = historyItem.ItemId;
                if (model.DbAccess.GetTagsFromFile(fileId).Any(x => x.Id == tagId))
                {
                    model.DbAccess.DeleteFileTag(fileId, tagId);
                }
                else
                {
                    model.DbAccess.InsertFileTag(fileId, tagId);
                }
                break;
        }

        LoadFile(SearchResultIndex);
    }

    [RelayCommand]
    private void RemoveHistoryItem(UpdateHistoryItem itemToRemove)
    {
        if (UpdateHistoryItems.Remove(itemToRemove))
        {
            OnPropertyChanged(nameof(HasUpdateHistory));
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
        StopSlideshow();
        var fileIds = Utils.CreateFileIds(CombineSearchResult);
        var files = model.DbAccess.SearchFilesFromIds(fileIds);
        SearchResult = new SearchResult(files);
    }

    [RelayCommand]
    private void OpenPresentationWindow()
    {
        var window = new PresentationWindow()
        {
            Owner = Application.Current.MainWindow,
            Title = $"{Utils.ApplicationName} {Utils.GetVersionString()} - Presentation"
        };

        if (CurrentFilePath != string.Empty)
        {
            var uri = new Uri(CurrentFilePath, UriKind.Absolute);
            window.ShowImage(new BitmapImage(uri), -currentFileRotation);
        }

        window.Show();
    }
}
