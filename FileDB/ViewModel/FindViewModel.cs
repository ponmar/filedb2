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
using FileDB.Export;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBInterface.Validators;
using FileDBInterface.FilesystemAccess;

namespace FileDB.ViewModel;

public enum RotationDirection { Clockwise, CounterClockwise };

public interface IFolder
{
    string Name { get; }
    List<IFolder> Folders { get; }
    string Path { get; }
}

public class Folder : IFolder
{
    private readonly IFolder? parent;
    public string Name { get; }
    public List<IFolder> Folders { get; } = new();

    public string Path => parent != null ? parent.Path + "/" + Name : Name;

    public Folder(string name, IFolder? parent = null)
    {
        Name = name;
        this.parent = parent;
    }
}

public interface IImagePresenter
{
    void ShowImage(BitmapImage image, double rotateDegrees);
    void CloseImage();
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
    private const string RootFolderName = "root";
    private readonly Random random = new();

    #region Browsing and sorting commands

    [RelayCommand]
    private void ClearSearch()
    {
        SearchResult = null;
    }

    public List<SortMethodDescription> SortMethods { get; } = Utils.GetSortMethods();

    [ObservableProperty]
    private SortMethod selectedSortMethod = SortMethod.Date;

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
        model.RequestTemporaryFullscreen(maximize);
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

    public string SearchBySexSelection
    {
        get => searchBySexSelection;
        set => SetProperty(ref searchBySexSelection, value.Split(" ")[1]);
    }
    private string searchBySexSelection = Sex.NotKnown.ToString();

    [ObservableProperty]
    private DateTime searchStartDate = DateTime.Now;

    [ObservableProperty]
    private DateTime searchEndDate = DateTime.Now;

    [ObservableProperty]
    private string? searchFileGpsPosition;

    [ObservableProperty]
    private string? searchFileGpsPositionUrl;

    [ObservableProperty]
    private string searchFileGpsRadius = "500";

    [ObservableProperty]
    private string? searchPersonAgeFrom;

    [ObservableProperty]
    private string? searchPersonAgeTo;
    
    public List<IFolder> Folders { get; } = new();

    public IFolder? SelectedFolder { get; set; }

    #endregion

    #region Meta-data change commands and properties

    [ObservableProperty]
    private string? fileListSearch;

    [ObservableProperty]
    private string? exportFilesDestinationDirectory;

    [ObservableProperty]
    private string exportFilesHeader = $"{Utils.ApplicationName} Export";

    [ObservableProperty]
    private bool exportIncludesFiles = true;

    partial void OnExportIncludesFilesChanged(bool value)
    {
        if (!value)
        {
            ExportIncludesHtml = false;
            ExportIncludesM3u = false;
        }
    }

    [ObservableProperty]
    private bool exportIncludesHtml = true;

    partial void OnExportIncludesHtmlChanged(bool value)
    {
        if (value)
        {
            ExportIncludesFiles = true;
        }
    }

    [ObservableProperty]
    private bool exportIncludesM3u = true;

    partial void OnExportIncludesM3uChanged(bool value)
    {
        if (value)
        {
            ExportIncludesFiles = true;
        }
    }

    [ObservableProperty]
    private bool exportIncludesFilesWithMetaData = true;

    [ObservableProperty]
    private bool exportIncludesJson = true;

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

    private SearchResult? SearchResult
    {
        get => searchResult;
        set
        {
            if (!EqualityComparer<SearchResult>.Default.Equals(searchResult, value))
            {
                searchResult = value;
                if (searchResult != null)
                {
                    if (searchResult.Count > 0)
                    {
                        LoadFile(0);
                        SortSearchResult(false);
                        AddSearchResultToHistory();
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
    private string currentFileHeader = string.Empty;

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

    [ObservableProperty]
    private int currentFileRotation = 0;

    #endregion

    public ObservableCollection<PersonToUpdate> Persons { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PersonSelected))]
    private PersonToUpdate? selectedPersonToUpdate;

    public bool PersonSelected => SelectedPersonToUpdate != null;

    [ObservableProperty]
    private PersonToUpdate? selectedPersonSearch;

    [ObservableProperty]
    private PersonToUpdate? selectedPerson1Search;

    [ObservableProperty]
    private PersonToUpdate? selectedPerson2Search;

    public ObservableCollection<LocationToUpdate> Locations { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LocationSelected))]
    public LocationToUpdate? selectedLocationToUpdate;

    public bool LocationSelected => SelectedLocationToUpdate != null;

    [ObservableProperty]
    private LocationToUpdate? selectedLocationSearch;

    public ObservableCollection<TagToUpdate> Tags { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TagSelected))]
    private TagToUpdate? selectedTagToUpdate;

    public bool TagSelected => SelectedTagToUpdate != null;

    [ObservableProperty]
    private TagToUpdate? selectedTagSearch;

    public ObservableCollection<UpdateHistoryItem> UpdateHistoryItems { get; } = new();

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
        ReloadFolders();

        model.PersonsUpdated += Model_PersonsUpdated;
        model.LocationsUpdated += Model_LocationsUpdated;
        model.TagsUpdated += Model_TagsUpdated;
        model.FilesImported += Model_FilesImported;
        model.ConfigLoaded += Model_ConfigLoaded;

        slideshowTimer.Tick += SlideshowTimer_Tick;
        slideshowTimer.Interval = TimeSpan.FromSeconds(model.Config.SlideshowDelay);
    }

    private void Model_ConfigLoaded(object? sender, EventArgs e)
    {
        ReadWriteMode = !model.Config.ReadOnly;
        slideshowTimer.Interval = TimeSpan.FromSeconds(model.Config.SlideshowDelay);
    }

    private void Model_FilesImported(object? sender, List<FilesModel> files)
    {
        ImportedFileList = Utils.CreateFileList(files);
    }

    private void Model_PersonsUpdated(object? sender, EventArgs e)
    {
        ReloadPersons();
    }

    private void Model_LocationsUpdated(object? sender, EventArgs e)
    {
        ReloadLocations();
    }

    private void Model_TagsUpdated(object? sender, EventArgs e)
    {
        ReloadTags();
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
        var sex = Enum.Parse<Sex>(searchBySexSelection);
        SearchResult = new SearchResult(model.DbAccess.SearchFilesBySex(sex));
    }

    [RelayCommand]
    private void FindFilesByDate()
    {
        StopSlideshow();
        SearchResult = new SearchResult(model.DbAccess.SearchFilesByDate(SearchStartDate.Date, SearchEndDate.Date));
    }

    [RelayCommand]
    private void FindFilesByGpsPosition()
    {
        StopSlideshow();

        if (string.IsNullOrEmpty(SearchFileGpsPosition) &&
            string.IsNullOrEmpty(SearchFileGpsPositionUrl))
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

        double latitude;
        double longitude;

        if (!string.IsNullOrEmpty(SearchFileGpsPosition))
        {
            var gpsPos = DatabaseParsing.ParseFilesPosition(SearchFileGpsPosition);
            if (gpsPos == null)
            {
                Dialogs.ShowErrorDialog("Invalid GPS position");
                return;
            }
            latitude = gpsPos.Value.lat;
            longitude = gpsPos.Value.lon;
        }
        else
        {
            var gpsPos = DatabaseParsing.ParseFilesPositionFromUrl(SearchFileGpsPositionUrl);
            if (gpsPos == null)
            {
                Dialogs.ShowErrorDialog("Invalid Google Maps URL");
                return;
            }
            latitude = gpsPos.Value.lat;
            longitude = gpsPos.Value.lon;
        }

        var nearFiles = model.DbAccess.SearchFilesNearGpsPosition(latitude, longitude, radius).ToList();

        // TODO: checkbox for selecting if this should be included?
        var nearLocations = model.DbAccess.SearchLocationsNearGpsPosition(latitude, longitude, radius);
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
    private void FindFilesSelectedFolder()
    {
        StopSlideshow();
        if (SelectedFolder != null)
        {
            var folderPath = SelectedFolder.Path;
            if (folderPath.StartsWith(RootFolderName))
            {
                folderPath = folderPath.Substring(RootFolderName.Length);
            }
            if (folderPath.StartsWith("/"))
            {
                folderPath = folderPath.Substring("/".Length);
            }
            SearchResult = new SearchResult(model.DbAccess.SearchFilesByPath(folderPath));
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
    private void ExportFileList()
    {
        ClipboardService.SetText(Utils.CreateFileList(SearchResult!.Files));
    }

    [RelayCommand]
    private void ExportFiles()
    {
        if (string.IsNullOrEmpty(ExportFilesHeader))
        {
            Dialogs.ShowErrorDialog("No header specified");
            return;
        }

        if (string.IsNullOrEmpty(ExportFilesDestinationDirectory))
        {
            Dialogs.ShowErrorDialog("No destination directory specified");
            return;
        }

        if (!Directory.Exists(ExportFilesDestinationDirectory))
        {
            Dialogs.ShowErrorDialog("Destination directory does not exist");
            return;
        }

        if (!IsDirectoryEmpty(ExportFilesDestinationDirectory))
        {
            Dialogs.ShowErrorDialog("Destination directory is not empty");
            return;
        }

        var selection = new List<bool>() { ExportIncludesFiles, ExportIncludesHtml, ExportIncludesM3u, ExportIncludesFilesWithMetaData, ExportIncludesJson };
        if (!selection.Any(x => x))
        {
            Dialogs.ShowErrorDialog("Nothing to export");
            return;
        }

        if (Dialogs.ShowConfirmDialog($"Export selected data for {SearchResult!.Count} files to {ExportFilesDestinationDirectory}?"))
        {
            try
            {
                new SearchResultExporter().Export(ExportFilesDestinationDirectory, ExportFilesHeader, SearchResult.Files,
                    ExportIncludesFiles, ExportIncludesHtml, ExportIncludesM3u, ExportIncludesFilesWithMetaData, ExportIncludesJson);
            }
            catch (IOException e)
            {
                Dialogs.ShowErrorDialog("Export error: " + e.Message);
            }
        }
    }

    private bool IsDirectoryEmpty(string path)
    {
        return !Directory.EnumerateFileSystemEntries(path).Any();
    }

    private void LoadFile(int index)
    {
        if (SearchResult != null &&
            index >= 0 && index < SearchResult.Count)
        {
            SearchResultIndex = index;

            var selection = SearchResult.Files[SearchResultIndex];

            CurrentFileInternalDirectoryPath = Path.GetDirectoryName(selection.Path)!.Replace(@"\", "/");
            CurrentFileInternalPath = selection.Path;
            CurrentFilePath = model.FilesystemAccess.ToAbsolutePath(selection.Path);
            CurrentFileDescription = selection.Description ?? string.Empty;
            CurrentFileDateTime = GetFileDateTimeString(selection.Datetime);
            CurrentFilePosition = selection.Position ?? string.Empty;
            CurrentFilePositionLink = selection.Position != null ? Utils.CreatePositionUri(selection.Position, model.Config.LocationLink) : null;
            CurrentFilePersons = GetFilePersonsString(selection);
            CurrentFileLocations = GetFileLocationsString(selection.Id);
            CurrentFileTags = GetFileTagsString(selection.Id);
            CurrentFileHeader = CurrentFileDateTime != string.Empty ? CurrentFileDateTime : selection.Path;

            // Note: reading of orientation is done here to get correct visualization for files added to database before orientation was parsed
            CurrentFileRotation = DatabaseParsing.OrientationToDegrees(selection.Orientation ?? model.FilesystemAccess.ParseFileMetadata(CurrentFilePath).Orientation);

            NewFileDescription = CurrentFileDescription;
            NewFileDateTime = selection.Datetime;

            var uri = new Uri(CurrentFilePath, UriKind.Absolute);
            try
            {
                CurrentFileLoadError = string.Empty;
                model.ImagePresenter!.ShowImage(new BitmapImage(uri), -currentFileRotation);

                model.FileLoaded(selection);
            }
            catch (WebException e)
            {
                CurrentFileLoadError = $"Image loading error: {e.Message}";
                model.ImagePresenter!.CloseImage();
            }
            catch (IOException e)
            {
                CurrentFileLoadError = $"Image loading error: {e.Message}";
                model.ImagePresenter!.CloseImage();
            }
            catch (NotSupportedException e)
            {
                CurrentFileLoadError = $"File format not supported: {e.Message}";
                model.ImagePresenter!.CloseImage();
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
        CurrentFileHeader = string.Empty;
        CurrentFilePosition = string.Empty;
        CurrentFilePositionLink = null;
        CurrentFilePersons = string.Empty;
        CurrentFileLocations = string.Empty;
        CurrentFileTags = string.Empty;
        CurrentFileRotation = 0;

        NewFileDescription = string.Empty;
        NewFileDateTime = string.Empty;

        CurrentFileLoadError = "No match";
        model.ImagePresenter!.CloseImage();
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
        var persons = model.DbAccess.GetPersonsFromFile(selection.Id);
        var personStrings = persons.Select(p => $"{p.Firstname} {p.Lastname}{Utils.GetPersonAgeInFileString(selection.Datetime, p.DateOfBirth)}");
        return string.Join("\n", personStrings);
    }

    private string GetFileLocationsString(int fileId)
    {
        var locations = model.DbAccess.GetLocationsFromFile(fileId);
        var locationStrings = locations.Select(l => l.Name);
        return string.Join("\n", locationStrings);
    }

    private string GetFileTagsString(int fileId)
    {
        var tags = model.DbAccess.GetTagsFromFile(fileId);
        var tagStrings = tags.Select(t => t.Name);
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
        else
        {
            Dialogs.ShowErrorDialog("This person has already been added");
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
        else
        {
            Dialogs.ShowErrorDialog("This location has already been added");
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
        else
        {
            Dialogs.ShowErrorDialog("This tag has already been added");
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
            int cameraNewDegrees = CurrentFileRotation;
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
        var locations = model.DbAccess.GetLocations().ToList();
        locations.Sort(new LocationModelByNameSorter());
        foreach (var location in locations.Select(l => new LocationToUpdate(l.Id, l.Name)))
        {
            Locations.Add(location);
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

    [RelayCommand]
    private void ReloadFolders()
    {
        var root = new Folder(RootFolderName);

        Folders.Clear();
        Folders.Add(root);

        foreach (var file in model.DbAccess.GetFiles())
        {
            var directoryEndIndex = file.Path.LastIndexOf("/");
            if (directoryEndIndex == -1)
            {
                // This fils is in the root directory
                continue;
            }

            var directoryPath = file.Path.Substring(0, directoryEndIndex);
            var directories = directoryPath.Split("/");

            if (directories.Length > 0)
            {
                var currentFolder = root;

                foreach (var pathPart in directories)
                {
                    var subFolder = currentFolder.Folders.FirstOrDefault(x => x.Name == pathPart);
                    if (subFolder == null)
                    {
                        subFolder = new Folder(pathPart, currentFolder);
                        currentFolder.Folders.Add(subFolder);
                    }

                    currentFolder = (Folder)subFolder;
                }
            }
        }

        OnPropertyChanged(nameof(Folders));
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
                UpdateHistoryItems.Add(new UpdateHistoryItem(type, itemId, itemName, i));
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
        UpdateHistoryItems.Remove(itemToRemove);
    }
}
