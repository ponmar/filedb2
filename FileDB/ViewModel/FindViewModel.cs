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

namespace FileDB.ViewModel
{
    public interface IFolder
    {
        string Name { get; }
        List<IFolder> Folders { get; }
        string Path { get; }
    }

    public class Folder : IFolder
    {
        private readonly IFolder parent;
        public string Name { get; }
        public List<IFolder> Folders { get; } = new();

        public string Path => parent != null ? parent.Path + "/" + Name : Name;

        public Folder(string name, IFolder parent = null)
        {
            Name = name;
            this.parent = parent;
        }
    }

    public interface IImagePresenter
    {
        void ShowImage(BitmapImage image);
    }

    public class PersonToUpdate
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class LocationToUpdate
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class TagToUpdate
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

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
        public string Shortcut { get; set; }
        public string Description => $"Toggle '{ItemName}'";

        public UpdateHistoryItem(UpdateHistoryType type, int itemId, string itemName)
        {
            Type = type;
            ItemId = itemId;
            ItemName = itemName;
        }
    }

    public partial class FindViewModel : ObservableObject
    {
        private const string RootFolderName = "root";
        private readonly IImagePresenter imagePresenter;
        private readonly Random random = new();

        #region Browsing and sorting commands

        [ICommand]
        private void ClearSearch()
        {
            SearchResult = null;
        }

        public List<SortMethodDescription> SortMethods { get; } = Utils.GetSortMethods();

        public SortMethod SelectedSortMethod
        {
            get => selectedSortMethod;
            set
            {
                if (SetProperty(ref selectedSortMethod, value))
                {
                    SortSearchResult(model.Config.KeepSelectionAfterSort);
                }
            }
        }
        private SortMethod selectedSortMethod = SortMethod.Date;

        [ObservableProperty]
        private bool slideshowActive = false;

        [ObservableProperty]
        private bool randomActive = false;

        [ObservableProperty]
        private bool repeatActive = false;

        public bool Maximize
        {
            get => maximize;
            set
            {
                if (SetProperty(ref maximize, value))
                {
                    OnPropertyChanged(nameof(ShowUpdateSection));
                    model.RequestTemporaryFullscreen(maximize);
                }
            }
        }
        private bool maximize = false;

        public bool ShowUpdateSection => !Maximize && ReadWriteMode;

        #endregion

        #region Search commands and properties

        [ObservableProperty]
        private string numRandomFiles = "10";

        [ObservableProperty]
        private string importedFileList;

        [ObservableProperty]
        private string searchPattern;

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
        private string searchFileGpsPosition;

        [ObservableProperty]
        private string searchFileGpsPositionUrl;

        [ObservableProperty]
        private string searchFileGpsRadius = "500";

        [ObservableProperty]
        private string searchPersonAgeFrom;

        [ObservableProperty]
        private string searchPersonAgeTo;
        
        public List<IFolder> Folders { get; } = new();

        public IFolder SelectedFolder { get; set; }

        #endregion

        #region Meta-data change commands and properties

        [ObservableProperty]
        private string fileListSearch;

        [ObservableProperty]
        private string exportFilesDestinationDirectory;

        [ObservableProperty]
        private string exportFilesHeader = $"{Utils.ApplicationName} Export";

        [ObservableProperty]
        private string newFileDescription;

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(ShowUpdateSection))]
        private bool readWriteMode = !Model.Model.Instance.Config.ReadOnly;

        #endregion

        #region Search result

        public ObservableCollection<SearchResult> SearchResultHistory { get; } = new();

        private SearchResult SearchResult
        {
            get => searchResult;
            set
            {
                if (SetProperty(ref searchResult, value))
                {
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
        private SearchResult searchResult = null;

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

            SearchResultHistory.Add(searchResult);

            OnPropertyChanged(nameof(FindFilesFromHistoryEnabled));
        }

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(SearchResultItemNumber))]
        private int searchResultIndex = -1;

        public int SearchResultItemNumber => searchResultIndex + 1;

        public int SearchNumberOfHits => SearchResult != null ? SearchResult.Count : 0;

        public int TotalNumberOfFiles { get; }

        public bool HasSearchResult => SearchResult != null;

        public bool HasNonEmptySearchResult => SearchResult != null && SearchResult.Count > 0;

        #endregion

        #region Current file properties

        [ObservableProperty]
        private string currentFileToolTip;

        [ObservableProperty]
        private string currentFileInternalDirectoryPath;

        [ObservableProperty]
        private string currentFilePath;

        [ObservableProperty]
        private string currentFileDescription;

        [ObservableProperty]
        private string currentFileDateTime;

        [ObservableProperty]
        private string currentFileHeader;

        [ObservableProperty]
        private string currentFilePosition;

        [ObservableProperty]
        private Uri currentFilePositionLink;

        [ObservableProperty]
        private string currentFilePersons;

        [ObservableProperty]
        private string currentFileLocations;

        [ObservableProperty]
        private string currentFileTags;

        [ObservableProperty]
        private string currentFileLoadError;

        #endregion

        public ObservableCollection<PersonToUpdate> Persons { get; } = new();

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(PersonSelected))]
        private PersonToUpdate selectedPersonToUpdate;

        public bool PersonSelected => SelectedPersonToUpdate != null;

        [ObservableProperty]
        private PersonToUpdate selectedPersonSearch;

        [ObservableProperty]
        private PersonToUpdate selectedPerson1Search;

        [ObservableProperty]
        private PersonToUpdate selectedPerson2Search;

        public ObservableCollection<LocationToUpdate> Locations { get; } = new();

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(LocationSelected))]
        public LocationToUpdate selectedLocationToUpdate;

        public bool LocationSelected => SelectedLocationToUpdate != null;

        [ObservableProperty]
        private LocationToUpdate selectedLocationSearch;

        public ObservableCollection<TagToUpdate> Tags { get; } = new();

        [ObservableProperty]
        [AlsoNotifyChangeFor(nameof(TagSelected))]
        private TagToUpdate selectedTagToUpdate;

        public bool TagSelected => SelectedTagToUpdate != null;

        [ObservableProperty]
        private TagToUpdate selectedTagSearch;

        public ObservableCollection<UpdateHistoryItem> UpdateHistoryItems { get; } = new();

        private readonly DispatcherTimer slideshowTimer = new();

        private readonly Model.Model model = Model.Model.Instance;

        public FindViewModel(IImagePresenter imagePresenter)
        {
            this.imagePresenter = imagePresenter;

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

        private void Model_ConfigLoaded(object sender, EventArgs e)
        {
            ReadWriteMode = !model.Config.ReadOnly;
            slideshowTimer.Interval = TimeSpan.FromSeconds(model.Config.SlideshowDelay);
        }

        private void Model_FilesImported(object sender, List<FilesModel> files)
        {
            ImportedFileList = Utils.CreateFileList(files);
        }

        private void Model_PersonsUpdated(object sender, EventArgs e)
        {
            ReloadPersons();
        }

        private void Model_LocationsUpdated(object sender, EventArgs e)
        {
            ReloadLocations();
        }

        private void Model_TagsUpdated(object sender, EventArgs e)
        {
            ReloadTags();
        }

        [ICommand]
        private void PrevFile()
        {
            StopSlideshow();
            SelectPrevFile();
        }

        public bool PrevFileAvailable => SearchResultIndex > 0;
        public bool NextFileAvailable => searchResult != null && SearchResultIndex < searchResult.Count - 1;
        public bool FirstFileAvailable => searchResult != null && SearchResultIndex > 0;
        public bool LastFileAvailable => searchResult != null && SearchResultIndex < SearchResult.Count - 1;
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

        [ICommand]
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
            LoadFile(random.Next(SearchResult.Count));
            FireBrowsingEnabledEvents();
        }

        [ICommand]
        private void PrevDirectory()
        {
            if (!PrevDirectoryAvailable)
            {
                return;
            }

            StopSlideshow();

            if (SearchResultIndex < 1)
                return;

            var currentDirectory = Path.GetDirectoryName(SearchResult.Files[SearchResultIndex].Path);

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

        [ICommand]
        private void NextDirectory()
        {
            if (!NextDirectoryAvailable)
            {
                return;
            }

            StopSlideshow();

            if (SearchResultIndex == -1 || SearchResultIndex == searchResult.Count - 1)
                return;

            var currentDirectory = Path.GetDirectoryName(SearchResult.Files[SearchResultIndex].Path);

            for (int i = SearchResultIndex + 1; i < searchResult.Count; i++)
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

        [ICommand]
        private void FirstFile()
        {
            StopSlideshow();
            LoadFile(0);
            FireBrowsingEnabledEvents();
        }

        [ICommand]
        private void LastFile()
        {
            StopSlideshow();
            if (searchResult != null)
            {
                LoadFile(SearchResult.Count - 1);
            }
            FireBrowsingEnabledEvents();
        }

        public void SortFilesByDate(bool preserveSelection)
        {
            SortFiles(new FilesByDateSorter(), false, preserveSelection);
        }

        public void SortFilesByDateDesc(bool preserveSelection)
        {
            SortFiles(new FilesByDateSorter(), true, preserveSelection);
        }

        public void SortFilesByPath(bool preserveSelection)
        {
            SortFiles(new FilesByPathSorter(), false, preserveSelection);
        }

        public void SortFilesByPathDesc(bool preserveSelection)
        {
            SortFiles(new FilesByPathSorter(), true, preserveSelection);
        }

        private void SortFiles(IComparer<FilesModel> comparer, bool desc, bool preserveSelection)
        {
            StopSlideshow();
            if (HasNonEmptySearchResult)
            {
                var selectedFile = searchResult.Files[searchResultIndex];
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

        [ICommand]
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

        private void SlideshowTimer_Tick(object sender, EventArgs e)
        {
            if (RandomActive)
            {
                SelectNextRandomFile();
            }
            else
            {
                if (RepeatActive)
                {
                    if (SearchResultIndex == SearchResult.Count - 1)
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
                    if (SearchResultIndex == SearchResult.Count - 1)
                    {
                        StopSlideshow();
                    }
                }
            }
        }

        [ICommand]
        private void FindRandomFiles()
        {
            StopSlideshow();

            if (int.TryParse(NumRandomFiles, out var value))
            {
                SearchResult = new SearchResult(model.DbAccess.SearchFilesRandom(value));
            }
        }

        [ICommand]
        private void FindCurrentDirectoryFiles()
        {
            StopSlideshow();
            if (SearchResultIndex == -1)
            {
                Dialogs.ShowErrorDialog("No file opened");
                return;
            }

            var path = SearchResult.Files[SearchResultIndex].Path;
            var dir = Path.GetDirectoryName(path).Replace('\\', '/');
            SearchResult = new SearchResult(model.DbAccess.SearchFilesByPath(dir));
        }

        [ICommand]
        private void FindAllFiles()
        {
            StopSlideshow();
            SearchResult = new SearchResult(model.DbAccess.GetFiles());
        }

        [ICommand]
        private void FindImportedFiles()
        {
            StopSlideshow();
            if (!string.IsNullOrEmpty(ImportedFileList))
            {
                var fileIds = Utils.CreateFileIds(ImportedFileList);
                SearchResult = new SearchResult(model.DbAccess.SearchFilesFromIds(fileIds));
            }
        }

        [ICommand]
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

        [ICommand]
        private void FindFilesBySex()
        {
            StopSlideshow();
            var sex = Enum.Parse<Sex>(searchBySexSelection);
            SearchResult = new SearchResult(model.DbAccess.SearchFilesBySex(sex));
        }

        [ICommand]
        private void FindFilesByDate()
        {
            StopSlideshow();
            SearchResult = new SearchResult(model.DbAccess.SearchFilesByDate(SearchStartDate.Date, SearchEndDate.Date));
        }

        [ICommand]
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

        [ICommand]
        private void FindFilesWithPerson()
        {
            StopSlideshow();
            if (SelectedPersonSearch != null)
            {
                SearchResult = new SearchResult(model.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id }));
            }
        }

        [ICommand]
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

        [ICommand]
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

        [ICommand]
        private void FindFilesWithPersons()
        {
            StopSlideshow();
            if (SelectedPerson1Search != null && SelectedPerson2Search != null)
            {
                SearchResult = new SearchResult(model.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPerson1Search.Id, SelectedPerson2Search.Id }));
            }
        }

        [ICommand]
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

        [ICommand]
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

        [ICommand]
        private void FindFilesWithLocation()
        {
            StopSlideshow();
            if (SelectedLocationSearch != null)
            {
                SearchResult = new SearchResult(model.DbAccess.SearchFilesWithLocations(new List<int>() { SelectedLocationSearch.Id }));
            }
        }

        [ICommand]
        private void FindFilesWithTag()
        {
            StopSlideshow();
            if (SelectedTagSearch != null)
            {
                SearchResult = new SearchResult(model.DbAccess.SearchFilesWithTags(new List<int>() { SelectedTagSearch.Id }));
            }
        }

        [ICommand]
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
                    var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth);
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

        [ICommand]
        private void FindFilesFromUnion()
        {
            if (SearchResultHistory.Count >= 2)
            {
                var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
                var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
                SearchResult = new SearchResult(files1.Union(files2, new FilesModelIdComparer()));
            }
        }

        [ICommand]
        private void FindFilesFromIntersection()
        {
            if (SearchResultHistory.Count >= 2)
            {
                var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
                var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
                SearchResult = new SearchResult(files1.Intersect(files2, new FilesModelIdComparer()));
            }
        }

        [ICommand]
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

        [ICommand]
        private void FindFilesFromMissingCategorization()
        {
            StopSlideshow();
            SearchResult = new SearchResult(model.DbAccess.SearchFilesWithMissingData());
        }

        [ICommand]
        private void FindFilesFromList()
        {
            StopSlideshow();
            if (!string.IsNullOrEmpty(fileListSearch))
            {
                var fileIds = Utils.CreateFileIds(fileListSearch);
                SearchResult = new SearchResult(model.DbAccess.SearchFilesFromIds(fileIds));
            }
        }

        [ICommand]
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

        [ICommand]
        private void OpenFileLocation()
        {
            if (!string.IsNullOrEmpty(CurrentFilePath) &&
                File.Exists(CurrentFilePath))
            {
                Utils.SelectFileInExplorer(CurrentFilePath);
            }
        }

        [ICommand]
        private void ExportFileList()
        {
            ClipboardService.SetText(Utils.CreateFileList(SearchResult.Files));
        }

        [ICommand]
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

            if (Dialogs.ShowConfirmDialog($"Export {SearchResult.Count} files to {ExportFilesDestinationDirectory}?"))
            {
                var exporter = new SearchResultExporter(ExportFilesDestinationDirectory, ExportFilesHeader);
                try
                {
                    exporter.Export(SearchResult.Files);
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

        private Uri CreatePositionLink(string position)
        {
            // TODO: Move this code somewhere else
            var positionParts = position.Split(" ");
            if (positionParts.Length == 2 && !string.IsNullOrEmpty(model.Config.LocationLink))
            {
                var link = model.Config.LocationLink.Replace("LAT", positionParts[0]).Replace("LON", positionParts[1]);
                return new Uri(link);
            }
            return null;
        }

        private void LoadFile(int index)
        {
            if (SearchResult != null &&
                index >= 0 && index < SearchResult.Count)
            {
                SearchResultIndex = index;

                var selection = SearchResult.Files[SearchResultIndex];

                // TODO: set from config option?
                CurrentFileToolTip = selection.Path;

                CurrentFileInternalDirectoryPath = Path.GetDirectoryName(selection.Path).Replace(@"\", "/");
                CurrentFilePath = model.FilesystemAccess.ToAbsolutePath(selection.Path);
                CurrentFileDescription = selection.Description ?? string.Empty;
                CurrentFileDateTime = GetFileDateTimeString(selection.Datetime);
                CurrentFilePosition = selection.Position ?? string.Empty;
                CurrentFilePositionLink = selection.Position != null ? CreatePositionLink(selection.Position) : null;
                CurrentFilePersons = GetFilePersonsString(selection);
                CurrentFileLocations = GetFileLocationsString(selection.Id);
                CurrentFileTags = GetFileTagsString(selection.Id);
                CurrentFileHeader = CurrentFileDateTime != string.Empty ? CurrentFileDateTime : selection.Path;

                NewFileDescription = CurrentFileDescription;

                var uri = new Uri(CurrentFilePath, UriKind.Absolute);
                try
                {
                    CurrentFileLoadError = string.Empty;
                    imagePresenter.ShowImage(new BitmapImage(uri));

                    model.CastFile(CurrentFilePath);
                }
                catch (WebException)
                {
                    CurrentFileLoadError = "Image loading error";
                    imagePresenter.ShowImage(null);
                }
                catch (IOException)
                {
                    CurrentFileLoadError = "Image loading error";
                    imagePresenter.ShowImage(null);
                }
                catch (NotSupportedException)
                {
                    CurrentFileLoadError = "File format not supported";
                    imagePresenter.ShowImage(null);
                }
            }
        }

        private void ResetFile()
        {
            SearchResultIndex = -1;

            CurrentFileToolTip = string.Empty;
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

            NewFileDescription = string.Empty;

            CurrentFileLoadError = "No match";
            imagePresenter.ShowImage(null);
        }

        private string GetFileDateTimeString(string datetimeString)
        {
            var datetime = DatabaseParsing.ParseFilesDatetime(datetimeString);
            if (datetime == null)
            {
                return string.Empty;
            }

            // Note: when no time is available the string is used to avoid including time 00:00
            var resultString = datetimeString.Contains('T') ? datetime.Value.ToString("yyyy-MM-dd HH:mm") : datetimeString;

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

        [ICommand]
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

            var fileId = searchResult.Files[SearchResultIndex].Id;
            if (!model.DbAccess.GetPersonsFromFile(fileId).Any(p => p.Id == SelectedPersonToUpdate.Id))
            {
                model.DbAccess.InsertFilePerson(fileId, SelectedPersonToUpdate.Id);
                LoadFile(SearchResultIndex);
                AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, SelectedPersonToUpdate.Id, SelectedPersonToUpdate.Name);
            }
            else
            {
                Dialogs.ShowErrorDialog("This person has already been added");
            }
        }

        [ICommand]
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

            var fileId = searchResult.Files[SearchResultIndex].Id;
            model.DbAccess.DeleteFilePerson(fileId, SelectedPersonToUpdate.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, SelectedPersonToUpdate.Id, SelectedPersonToUpdate.Name);
        }

        [ICommand]
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

            var fileId = searchResult.Files[SearchResultIndex].Id;
            if (!model.DbAccess.GetLocationsFromFile(fileId).Any(l => l.Id == SelectedLocationToUpdate.Id))
            {
                model.DbAccess.InsertFileLocation(fileId, SelectedLocationToUpdate.Id);
                LoadFile(SearchResultIndex);
                AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, SelectedLocationToUpdate.Id, SelectedLocationToUpdate.Name);
            }
            else
            {
                Dialogs.ShowErrorDialog("This location has already been added");
            }
        }

        [ICommand]
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

            var fileId = searchResult.Files[SearchResultIndex].Id;
            model.DbAccess.DeleteFileLocation(fileId, SelectedLocationToUpdate.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, SelectedLocationToUpdate.Id, SelectedLocationToUpdate.Name);
        }

        [ICommand]
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

            var fileId = searchResult.Files[SearchResultIndex].Id;
            if (!model.DbAccess.GetTagsFromFile(fileId).Any(t => t.Id == SelectedTagToUpdate.Id))
            {
                model.DbAccess.InsertFileTag(fileId, SelectedTagToUpdate.Id);
                LoadFile(SearchResultIndex);
                AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, SelectedTagToUpdate.Id, SelectedTagToUpdate.Name);
            }
            else
            {
                Dialogs.ShowErrorDialog("This tag has already been added");
            }
        }

        [ICommand]
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

            var fileId = searchResult.Files[SearchResultIndex].Id;
            model.DbAccess.DeleteFileTag(fileId, SelectedTagToUpdate.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, SelectedTagToUpdate.Id, SelectedTagToUpdate.Name);
        }

        [ICommand]
        private void SetFileDescription()
        {
            if (SearchResultIndex != -1)
            {
                var selection = SearchResult.Files[SearchResultIndex];
                var fileId = selection.Id;
                NewFileDescription = NewFileDescription?.Trim();
                var description = string.IsNullOrEmpty(NewFileDescription) ? null : NewFileDescription;

                try
                {
                    model.DbAccess.UpdateFileDescription(fileId, description);
                    selection.Description = description;
                    LoadFile(SearchResultIndex);
                }
                catch (DataValidationException e)
                {
                    Dialogs.ShowErrorDialog(e.Message);
                }
            }
        }

        [ICommand]
        private void UpdateFileFromMetaData()
        {
            if (SearchResultIndex != -1)
            {
                if (Dialogs.ShowConfirmDialog("Reload date and GPS position from file meta-data?"))
                {
                    var selection = SearchResult.Files[SearchResultIndex];
                    var fileId = selection.Id;
                    model.DbAccess.UpdateFileFromMetaData(selection.Id, model.FilesystemAccess);

                    var updatedFile = model.DbAccess.GetFileById(fileId);
                    selection.Datetime = updatedFile.Datetime;
                    selection.Position = updatedFile.Position;
                    LoadFile(SearchResultIndex);
                }
            }
        }

        [ICommand]
        private void CreatePerson()
        {
            var window = new AddPersonWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        [ICommand]
        private void CreateLocation()
        {
            var window = new AddLocationWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        [ICommand]
        private void CreateTag()
        {
            var window = new AddTagWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }

        private void ReloadPersons()
        {
            Persons.Clear();
            var persons = model.DbAccess.GetPersons().Select(p => new PersonToUpdate() { Id = p.Id, Name = p.Firstname + " " + p.Lastname }).ToList();
            persons.Sort(new PersonToUpdateSorter());
            foreach (var person in persons)
            {
                Persons.Add(person);
            }
        }

        private void ReloadLocations()
        {
            Locations.Clear();
            var locations = model.DbAccess.GetLocations().Select(l => new LocationToUpdate() { Id = l.Id, Name = l.Name }).ToList();
            locations.Sort(new LocationToUpdateSorter());
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }

        private void ReloadTags()
        {
            Tags.Clear();
            var tags = model.DbAccess.GetTags().Select(t => new TagToUpdate() { Id = t.Id, Name = t.Name }).ToList();
            tags.Sort(new TagToUpdateSorter());
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
        }

        [ICommand]
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
                        var subFolder = (Folder)currentFolder.Folders.FirstOrDefault(x => x.Name == pathPart);
                        if (subFolder == null)
                        {
                            subFolder = new Folder(pathPart, currentFolder);
                            currentFolder.Folders.Add(subFolder);
                        }

                        currentFolder = subFolder;
                    }
                }
            }

            OnPropertyChanged(nameof(Folders));
        }

        private void AddUpdateHistoryItem(UpdateHistoryType type, int itemId, string itemName)
        {
            var duplicatedItem = UpdateHistoryItems.FirstOrDefault(x => x.Type == type && x.ItemName == itemName);
            if (duplicatedItem != null)
            {
                return;
            }

            if (UpdateHistoryItems.Count < 12)
            {
                var shortcut = $"F{UpdateHistoryItems.Count + 1}";
                UpdateHistoryItems.Add(new UpdateHistoryItem(type, itemId, itemName) { Shortcut = shortcut });
            }
        }

        [ICommand]
        private void FunctionKey(string parameter)
        {
            if (!ReadWriteMode || !HasNonEmptySearchResult)
            {
                return;
            }

            var fileId = SearchResult.Files[SearchResultIndex].Id;

            var number = int.Parse(parameter);
            var shortcut = $"F{number}";
            var historyItem = UpdateHistoryItems.FirstOrDefault(x => x.Shortcut == shortcut);
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

        [ICommand]
        private void RemoveHistoryItem(UpdateHistoryItem itemToRemove)
        {
            UpdateHistoryItems.Remove(itemToRemove);
        }
    }
}
