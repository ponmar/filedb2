using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FileDB.Comparers;
using FileDB.View;
using FileDB.Sorters;
using FileDBInterface;
using FileDBInterface.Exceptions;
using FileDBInterface.Model;
using TextCopy;

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

    public class FindViewModel : ViewModelBase
    {
        private const string RootFolderName = "root";
        private readonly IImagePresenter imagePresenter;
        private readonly Random random = new();

        #region Browsing and sorting commands

        public ICommand PrevFileCommand => prevFileCommand ??= new CommandHandler(StopSlideshowAndSelectPrevFile, PrevFileAvailable);
        private ICommand prevFileCommand;

        public ICommand NextFileCommand => nextFileCommand ??= new CommandHandler(StopSlideshowAndSelectNextFile, NextFileAvailable);
        private ICommand nextFileCommand;

        public ICommand PrevDirectoryCommand => prevDirectoryCommand ??= new CommandHandler(PrevDirectory, () => HasNonEmptySearchResult);
        private ICommand prevDirectoryCommand;

        public ICommand NextDirectoryCommand => nextDirectoryCommand ??= new CommandHandler(NextDirectory, () => HasNonEmptySearchResult);
        private ICommand nextDirectoryCommand;

        public ICommand FirstFileCommand => firstFileCommand ??= new CommandHandler(FirstFile, FirstFileAvailable);
        private ICommand firstFileCommand;

        public ICommand LastFileCommand => lastFileCommand ??= new CommandHandler(LastFile, LastFileAvailable);
        private ICommand lastFileCommand;

        public ICommand ToggleSlideshowCommand => toggleSlideshowCommand ??= new CommandHandler(ToggleSlideshow, () => HasNonEmptySearchResult);
        private ICommand toggleSlideshowCommand;

        public ICommand ToggleRandomCommand => toggleRandomCommand ??= new CommandHandler(ToggleRandom, () => HasNonEmptySearchResult);
        private ICommand toggleRandomCommand;

        public ICommand ToggleRepeatCommand => toggleRepeatCommand ??= new CommandHandler(ToggleRepeat, () => HasNonEmptySearchResult);
        private ICommand toggleRepeatCommand;

        public ICommand SortFilesByDateCommand => sortFilesByDateCommand ??= new CommandHandler(SortFilesByDate, () => HasNonEmptySearchResult);
        private ICommand sortFilesByDateCommand;

        public ICommand SortFilesByDateDescCommand => sortFilesByDateDescCommand ??= new CommandHandler(SortFilesByDateDesc, () => HasNonEmptySearchResult);
        private ICommand sortFilesByDateDescCommand;

        public ICommand SortFilesByPathCommand => sortFilesByPathCommand ??= new CommandHandler(SortFilesByPath, () => HasNonEmptySearchResult);
        private ICommand sortFilesByPathCommand;

        public ICommand SortFilesByPathDescCommand => sortFilesByPathDescCommand ??= new CommandHandler(SortFilesByPathDesc, () => HasNonEmptySearchResult);
        private ICommand sortFilesByPathDescCommand;

        public bool SlideshowActive
        {
            get => slideshowActive;
            set { SetProperty(ref slideshowActive, value); }
        }
        private bool slideshowActive = false;

        public bool RandomActive
        {
            get => randomActive;
            set { SetProperty(ref randomActive, value); }
        }
        private bool randomActive = false;

        public bool RepeatActive
        {
            get => repeatActive;
            set { SetProperty(ref repeatActive, value); }
        }
        private bool repeatActive = false;

        #endregion

        #region Search commands and properties

        public ICommand FindRandomFilesCommand => findRandomFilesCommand ??= new CommandHandler(FindRandomFiles);
        private ICommand findRandomFilesCommand;

        public string NumRandomFiles
        {
            get => numRandomFiles;
            set => SetProperty(ref numRandomFiles, value);
        }
        private string numRandomFiles = "10";

        public ICommand FindCurrentDirectoryFilesCommand => findCurrentDirectoryFilesCommand ??= new CommandHandler(FindCurrentDirectoryFiles, () => HasNonEmptySearchResult);
        private ICommand findCurrentDirectoryFilesCommand;

        public ICommand FindAllFilesCommand => findAllFilesCommand ??= new CommandHandler(FindAllFiles);
        private ICommand findAllFilesCommand;

        public ICommand FindFilesByTextCommand => findFilesByTextCommand ??= new CommandHandler(FindFilesByText);
        private ICommand findFilesByTextCommand;

        public string SearchPattern
        {
            get => searchPattern;
            set => SetProperty(ref searchPattern, value);
        }
        private string searchPattern;

        public ICommand FindFilesBySexCommand => findFilesBySexCommand ??= new CommandHandler(FindFilesBySex);
        private ICommand findFilesBySexCommand;

        public string SearchBySexSelection
        {
            get => searchBySexSelection;
            set => SetProperty(ref searchBySexSelection, value.Split(" ")[1]);
        }
        private string searchBySexSelection = Sex.NotKnown.ToString();

        public ICommand FindFilesByDateCommand => findFilesByDateCommand ??= new CommandHandler(FindFilesByDate);
        private ICommand findFilesByDateCommand;

        public ICommand FindFilesByGpsPositionCommand => findFilesByGpsPositionCommand ??= new CommandHandler(FindFilesByGpsPosition);
        private ICommand findFilesByGpsPositionCommand;

        public DateTime SearchStartDate
        {
            get => searchStartDate;
            set => SetProperty(ref searchStartDate, value);
        }
        private DateTime searchStartDate = DateTime.Now;

        public DateTime SearchEndDate
        {
            get => searchEndDate;
            set => SetProperty(ref searchEndDate, value);
        }
        private DateTime searchEndDate = DateTime.Now;

        public string SearchFileGpsPosition
        {
            get => searchFileGpsPosition;
            set => SetProperty(ref searchFileGpsPosition, value);
        }
        private string searchFileGpsPosition;

        public string SearchFileGpsRadius
        {
            get => searchFileGpsRadius;
            set => SetProperty(ref searchFileGpsRadius, value);
        }
        private string searchFileGpsRadius = "500";

        public ICommand FindFilesWithPersonCommand => findFilesWithPersonCommand ??= new CommandHandler(FindFilesWithPerson);
        private ICommand findFilesWithPersonCommand;

        public ICommand FindFilesWithLocationCommand => findFilesWithLocationCommand ??= new CommandHandler(FindFilesWithLocation);
        private ICommand findFilesWithLocationCommand;

        public ICommand FindFilesWithTagCommand => findFilesWithTagCommand ??= new CommandHandler(FindFilesWithTag);
        private ICommand findFilesWithTagCommand;

        public ICommand FindFilesByPersonAgeCommand => findFilesByPersonAgeCommand ??= new CommandHandler(FindFilesByPersonAge);
        private ICommand findFilesByPersonAgeCommand;

        public string SearchPersonAgeFrom
        {
            get => searchPersonAgeFrom;
            set => SetProperty(ref searchPersonAgeFrom, value);
        }
        private string searchPersonAgeFrom;

        public string SearchPersonAgeTo
        {
            get => searchPersonAgeTo;
            set => SetProperty(ref searchPersonAgeTo, value);
        }
        private string searchPersonAgeTo;

        public ICommand FindFilesFromUnionCommand => findFilesFromUnionCommand ??= new CommandHandler(FindFilesFromUnion, FindFilesFromHistoryEnabled);
        private ICommand findFilesFromUnionCommand;

        public ICommand FindFilesFromIntersectionCommand => findFilesFromIntersectionCommand ??= new CommandHandler(FindFilesFromIntersection, FindFilesFromHistoryEnabled);
        private ICommand findFilesFromIntersectionCommand;

        public ICommand FindFilesFromDifferenceCommand => findFilesFromDifferenceCommand ??= new CommandHandler(FindFilesFromDifference, FindFilesFromHistoryEnabled);
        private ICommand findFilesFromDifferenceCommand;

        public List<IFolder> Folders { get; } = new();

        public IFolder SelectedFolder { get; set; }

        #endregion

        #region Meta-data change commands and properties

        public ICommand OpenFileLocationCommand => openFileLocationCommand ??= new CommandHandler(OpenFileLocation, () => HasNonEmptySearchResult);
        private ICommand openFileLocationCommand;

        public ICommand FindFilesFromMissingCategorizationCommand => findFilesFromMissingCategorizationCommand ??= new CommandHandler(FindFilesFromMissingCategorization);
        private ICommand findFilesFromMissingCategorizationCommand;

        public ICommand FindFilesFromListCommand => findFilesFromListCommand ??= new CommandHandler(FindFilesFromList);
        private ICommand findFilesFromListCommand;

        public ICommand FindFilesSelectedFolderCommand => findFilesSelectedFolderCommand ??= new CommandHandler(FindFilesSelectedFolder);
        private ICommand findFilesSelectedFolderCommand;

        public ICommand ReloadFoldersCommand => reloadFoldersCommand ??= new CommandHandler(ReloadFolders);
        private ICommand reloadFoldersCommand;

        public string FileListSearch
        {
            get => fileListSearch;
            set { SetProperty(ref fileListSearch, value); }
        }
        private string fileListSearch;

        public ICommand ExportFileListCommand => exportFileListCommand ??= new CommandHandler(ExportFileList, () => HasNonEmptySearchResult);
        private ICommand exportFileListCommand;

        public ICommand AddFilePersonCommand => addFilePersonCommand ??= new CommandHandler(AddFilePerson, PersonSelected);
        private ICommand addFilePersonCommand;

        public ICommand RemoveFilePersonCommand => removeFilePersonCommand ??= new CommandHandler(RemoveFilePerson, PersonSelected);
        private ICommand removeFilePersonCommand;

        public ICommand AddFileLocationCommand => addFileLocationCommand ??= new CommandHandler(AddFileLocation, LocationSelected);
        private ICommand addFileLocationCommand;

        public ICommand RemoveFileLocationCommand => removeFileLocationCommand ??= new CommandHandler(RemoveFileLocation, LocationSelected);
        private ICommand removeFileLocationCommand;

        public ICommand AddFileTagCommand => addFileTagCommand ??= new CommandHandler(AddFileTag, TagSelected);
        private ICommand addFileTagCommand;

        public ICommand RemoveFileTagCommand => removeFileTagCommand ??= new CommandHandler(RemoveFileTag, TagSelected);
        private ICommand removeFileTagCommand;

        public ICommand SetFileDescriptionCommand => setFileDescriptionCommand ??= new CommandHandler(SetFileDescription, () => HasNonEmptySearchResult);
        private ICommand setFileDescriptionCommand;

        public string NewFileDescription
        {
            get => newFileDescription;
            set { SetProperty(ref newFileDescription, value); }
        }
        private string newFileDescription;

        public ICommand UpdateFileFromMetaDataCommand => updateFileFromMetaDataCommand ??= new CommandHandler(UpdateFileFromMetaData);
        private ICommand updateFileFromMetaDataCommand;

        public ICommand CreatePersonCommand => createPersonCommand ??= new CommandHandler(CreatePerson);
        private ICommand createPersonCommand;

        public ICommand CreateLocationCommand => createLocationCommand ??= new CommandHandler(CreateLocation);
        private ICommand createLocationCommand;

        public ICommand CreateTagCommand => createTagCommand ??= new CommandHandler(CreateTag);
        private ICommand createTagCommand;

        public bool ReadWriteMode
        {
            get => readWriteMode;
            set => SetProperty(ref readWriteMode, value);
        }
        private bool readWriteMode = !Utils.Config.ReadOnly;

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
                        SearchNumberOfHits = searchResult.Count;
                        if (searchResult.Count > 0)
                        {
                            LoadFile(0);
                            AddSearchResultToHistory();
                        }
                        else
                        {
                            ResetFile();
                        }
                    }
                    else
                    {
                        SearchNumberOfHits = 0;
                        ResetFile();
                    }

                    OnPropertyChanged(nameof(HasSearchResult));
                    OnPropertyChanged(nameof(HasNonEmptySearchResult));
                }
            }
        }

        private void AddSearchResultToHistory()
        {
            if (SearchResultHistory.Count == Utils.Config.SearchHistorySize)
            {
                SearchResultHistory.RemoveAt(0);
            }
            SearchResultHistory.Add(searchResult);
        }

        private SearchResult searchResult = null;

        public int SearchResultIndex
        {
            get => searchResultIndex;
            private set
            {
                SetProperty(ref searchResultIndex, value);
                SearchResultItemNumber = searchResultIndex + 1;
            }
        }
        private int searchResultIndex = -1;

        public int SearchResultItemNumber
        {
            get => searchResultItemNumber;
            private set { SetProperty(ref searchResultItemNumber, value); }
        }
        private int searchResultItemNumber = 0;

        public int SearchNumberOfHits
        {
            get => searchNumberOfHits;
            private set { SetProperty(ref searchNumberOfHits, value); }
        }
        private int searchNumberOfHits = 0;

        public int TotalNumberOfFiles { get; }

        public bool HasSearchResult => SearchResult != null;

        public bool HasNonEmptySearchResult => SearchResult != null && SearchResult.Count > 0;

        #endregion

        #region Current file properties

        public string CurrentFileToolTip
        {
            get => currentFileToolTip;
            private set => SetProperty(ref currentFileToolTip, value);
        }
        private string currentFileToolTip;

        public string CurrentFileInternalPath
        {
            get => currentFileInternalPath;
            private set => SetProperty(ref currentFileInternalPath, value);
        }
        private string currentFileInternalPath;

        public string CurrentFileInternalDirectoryPath
        {
            get => currentFileInternalDirectoryPath;
            private set => SetProperty(ref currentFileInternalDirectoryPath, value);
        }
        private string currentFileInternalDirectoryPath;

        public string CurrentFilePath
        {
            get => currentFilePath;
            private set => SetProperty(ref currentFilePath, value);
        }
        private string currentFilePath;

        public string CurrentFileDescription
        {
            get => currentFileDescription;
            private set => SetProperty(ref currentFileDescription, value);
        }
        private string currentFileDescription;

        public string CurrentFileDateTime
        {
            get => currentFiledateTime;
            private set => SetProperty(ref currentFiledateTime, value);
        }
        private string currentFiledateTime;

        public string CurrentFilePosition
        {
            get => currentFilePosition;
            private set => SetProperty(ref currentFilePosition, value);
        }
        private string currentFilePosition;

        public string CurrentFilePositionLink
        {
            get => currentFilePositionLink;
            private set => SetProperty(ref currentFilePositionLink, value);
        }
        private string currentFilePositionLink;

        public string CurrentFilePersons
        {
            get => currentFilePersons;
            private set => SetProperty(ref currentFilePersons, value);
        }
        private string currentFilePersons;

        public string CurrentFileLocations
        {
            get => currentFileLocations;
            private set => SetProperty(ref currentFileLocations, value);
        }
        private string currentFileLocations;

        public string CurrentFileTags
        {
            get => currentFileTags;
            private set => SetProperty(ref currentFileTags, value);
        }
        private string currentFileTags;

        public string CurrentFileLoadError
        {
            get => currentFileLoadError;
            private set => SetProperty(ref currentFileLoadError, value);
        }
        private string currentFileLoadError;

        #endregion

        public ObservableCollection<PersonToUpdate> Persons { get; } = new();

        public PersonToUpdate SelectedPersonToUpdate { get; set; }

        public PersonToUpdate SelectedPersonSearch { get; set; }

        public ObservableCollection<LocationToUpdate> Locations { get; } = new();

        public LocationToUpdate SelectedLocationToUpdate { get; set; }

        public LocationToUpdate SelectedLocationSearch { get; set; }

        public ObservableCollection<TagToUpdate> Tags { get; } = new();

        public TagToUpdate SelectedTagToUpdate { get; set; }

        public TagToUpdate SelectedTagSearch { get; set; }

        public ObservableCollection<UpdateHistoryItem> UpdateHistoryItems { get; } = new();

        private readonly DispatcherTimer slideshowTimer = new();

        public FindViewModel(IImagePresenter imagePresenter)
        {
            this.imagePresenter = imagePresenter;

            TotalNumberOfFiles = Utils.DbAccess.GetFileCount();

            ReloadPersons();
            ReloadLocations();
            ReloadTags();
            ReloadFolders();

            slideshowTimer.Tick += SlideshowTimer_Tick;
            slideshowTimer.Interval = TimeSpan.FromSeconds(Utils.Config.SlideshowDelay);
        }

        public void StopSlideshowAndSelectPrevFile()
        {
            StopSlideshow();
            LoadFile(SearchResultIndex - 1);
        }

        public bool PrevFileAvailable()
        {
            return SearchResultIndex > 0;
        }

        public void StopSlideshowAndSelectNextFile()
        {
            StopSlideshow();
            NextFile();
        }

        private void NextFile()
        {
            LoadFile(SearchResultIndex + 1);
        }

        public bool NextFileAvailable()
        {
            return searchResult != null && SearchResultIndex < searchResult.Count - 1;
        }

        private void NextRandomFile()
        {
            LoadFile(random.Next(SearchResult.Count));
        }

        public void PrevDirectory()
        {
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
        }

        public void NextDirectory()
        {
            StopSlideshow();

            if (SearchResultIndex == -1 || SearchResultIndex == searchResult.Count - 1)
                return;

            var currentDirectory = Path.GetDirectoryName(SearchResult.Files[SearchResultIndex].Path);

            for (int i=SearchResultIndex + 1; i<searchResult.Count; i++)
            {
                var directory = Path.GetDirectoryName(SearchResult.Files[i].Path);
                if (directory != currentDirectory)
                {
                    LoadFile(i);
                    return;
                }
            }
        }

        public void FirstFile()
        {
            StopSlideshow();
            LoadFile(0);
        }

        public bool FirstFileAvailable()
        {
            return searchResult != null && SearchResultIndex > 0;
        }

        public void LastFile()
        {
            StopSlideshow();
            if (searchResult != null)
            {
                LoadFile(SearchResult.Count - 1);
            }
        }

        public bool LastFileAvailable()
        {
            return searchResult != null && SearchResultIndex < SearchResult.Count - 1;
        }

        public void SortFilesByDate()
        {
            SortFiles(new FilesByDateSorter());
        }

        public void SortFilesByDateDesc()
        {
            SortFiles(new FilesByDateSorter(), true);
        }

        public void SortFilesByPath()
        {
            SortFiles(new FilesByPathSorter());
        }

        public void SortFilesByPathDesc()
        {
            SortFiles(new FilesByPathSorter(), true);
        }

        private void SortFiles(IComparer<FilesModel> comparer, bool desc = false)
        {
            StopSlideshow();
            if (searchResult != null)
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
                LoadFile(SearchResult.Files.IndexOf(selectedFile));
            }
        }

        public void ToggleSlideshow()
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

        public void ToggleRandom()
        {
        }

        public void ToggleRepeat()
        {
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
                NextRandomFile();
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
                        NextFile();
                    }
                }
                else
                {
                    NextFile();
                    if (SearchResultIndex == SearchResult.Count - 1)
                    {
                        StopSlideshow();
                    }
                }
            }
        }

        public void FindRandomFiles()
        {
            StopSlideshow();

            if (int.TryParse(NumRandomFiles, out var value))
            {
                SearchResult = new SearchResult(Utils.DbAccess.SearchFilesRandom(value));
            }
        }

        public void FindCurrentDirectoryFiles()
        {
            StopSlideshow();
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file opened");
                return;
            }

            var path = SearchResult.Files[SearchResultIndex].Path;
            var dir = Path.GetDirectoryName(path).Replace('\\', '/');
            SearchResult = new SearchResult(Utils.DbAccess.SearchFilesByPath(dir));
        }

        public void FindAllFiles()
        {
            StopSlideshow();
            SearchResult = new SearchResult(Utils.DbAccess.GetFiles());
        }

        public void FindFilesByText()
        {
            StopSlideshow();
            if (!string.IsNullOrEmpty(SearchPattern))
            {
                SearchResult = new SearchResult(Utils.DbAccess.SearchFiles(SearchPattern));
            }
            else
            {
                SearchResult = null;
            }
        }

        public void FindFilesBySex()
        {
            StopSlideshow();
            var sex = Enum.Parse<Sex>(searchBySexSelection);
            SearchResult = new SearchResult(Utils.DbAccess.SearchFilesBySex(sex));
        }

        public void FindFilesByDate()
        {
            StopSlideshow();
            SearchResult = new SearchResult(Utils.DbAccess.SearchFilesByDate(SearchStartDate.Date, SearchEndDate.Date));
        }

        public void FindFilesByGpsPosition()
        {
            StopSlideshow();

            if (!double.TryParse(SearchFileGpsRadius, out var radius) || radius < 1)
            {
                Utils.ShowErrorDialog("Invalid radius");
                return;
            }

            var gpsPos = DatabaseParsing.ParseFilesPosition(SearchFileGpsPosition);
            if (gpsPos == null)
            {
                Utils.ShowErrorDialog("Invalid GPS position");
                return;
            }

            SearchResult = new SearchResult(Utils.DbAccess.SearchFilesNearGpsPosition(gpsPos.Value.lat, gpsPos.Value.lon, radius));
        }

        public void FindFilesWithPerson()
        {
            StopSlideshow();
            if (SelectedPersonSearch != null)
            {
                SearchResult = new SearchResult(Utils.DbAccess.SearchFilesWithPersons(new List<int>() { SelectedPersonSearch.Id }));
            }
        }

        public void FindFilesWithLocation()
        {
            StopSlideshow();
            if (SelectedLocationSearch != null)
            {
                SearchResult = new SearchResult(Utils.DbAccess.SearchFilesWithLocations(new List<int>() { SelectedLocationSearch.Id }));
            }
        }

        public void FindFilesWithTag()
        {
            StopSlideshow();
            if (SelectedTagSearch != null)
            {
                SearchResult = new SearchResult(Utils.DbAccess.SearchFilesWithTags(new List<int>() { SelectedTagSearch.Id }));
            }
        }

        public void FindFilesByPersonAge()
        {
            StopSlideshow();
            if (!string.IsNullOrEmpty(SearchPersonAgeFrom))
            {
                if (!int.TryParse(SearchPersonAgeFrom, out var ageFrom))
                {
                    Utils.ShowErrorDialog("Invalid age format");
                    return;
                }

                int ageTo;
                if (string.IsNullOrEmpty(SearchPersonAgeTo))
                {
                    ageTo = ageFrom;
                }
                else if (!int.TryParse(SearchPersonAgeTo, out ageTo))
                {
                    Utils.ShowErrorDialog("Invalid age format");
                    return;
                }

                var result = new List<FilesModel>();
                var personsWithAge = Utils.DbAccess.GetPersons().Where(p => p.DateOfBirth != null);

                foreach (var person in personsWithAge)
                {
                    var dateOfBirth = DatabaseParsing.ParsePersonsDateOfBirth(person.DateOfBirth);
                    foreach (var file in Utils.DbAccess.SearchFilesWithPersons(new List<int>() { person.Id }))
                    {
                        if (DatabaseParsing.ParseFilesDatetime(file.Datetime, out var fileDatetime))
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

        public bool FindFilesFromHistoryEnabled()
        {
            return SearchResultHistory.Count >= 2;
        }

        public void FindFilesFromUnion()
        {
            if (SearchResultHistory.Count >= 2)
            {
                var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
                var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
                SearchResult = new SearchResult(files1.Union(files2, new FilesModelIdComparer()).ToList());
            }
        }

        public void FindFilesFromIntersection()
        {
            if (SearchResultHistory.Count >= 2)
            {
                var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
                var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
                SearchResult = new SearchResult(files1.Intersect(files2, new FilesModelIdComparer()).ToList());
            }
        }

        public void FindFilesFromDifference()
        {
            if (SearchResultHistory.Count >= 2)
            {
                var files1 = SearchResultHistory[SearchResultHistory.Count - 1].Files;
                var files2 = SearchResultHistory[SearchResultHistory.Count - 2].Files;
                var uniqueFiles1 = files1.Except(files2, new FilesModelIdComparer());
                var uniqueFiles2 = files2.Except(files1, new FilesModelIdComparer());
                SearchResult = new SearchResult(uniqueFiles1.Union(uniqueFiles2, new FilesModelIdComparer()).ToList());
            }
        }

        public void FindFilesFromMissingCategorization()
        {
            StopSlideshow();
            SearchResult = new SearchResult(Utils.DbAccess.SearchFilesWithMissingData());
        }

        public void FindFilesFromList()
        {
            StopSlideshow();
            if (!string.IsNullOrEmpty(fileListSearch))
            {
                var items = fileListSearch.Split(';');
                var fileIds = new List<int>();
                foreach (var item in items)
                {
                    if (int.TryParse(item, out var fileId))
                    {
                        fileIds.Add(fileId);
                    }
                }

                // TODO: add query with many file ids
                SearchResult = new SearchResult(fileIds.Select(f => Utils.DbAccess.GetFileById(f)).ToList());
            }
        }

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
                SearchResult = new SearchResult(Utils.DbAccess.SearchFilesByPath(folderPath));
            }
        }

        public void OpenFileLocation()
        {
            if (!string.IsNullOrEmpty(CurrentFilePath) &&
                File.Exists(CurrentFilePath))
            {
                var explorerPath = CurrentFilePath.Replace("/", @"\");
                Process.Start("explorer.exe", "/select, " + explorerPath);
            }
        }

        public void ExportFileList()
        {
            if (HasNonEmptySearchResult)
            {
                var fileIdList = string.Join(';', SearchResult.Files.Select(f => f.Id));
                ClipboardService.SetText(fileIdList);
            }
            else
            {
                ClipboardService.SetText("");
            }
        }

        private string CreatePositionLink(string position, string defaultValue)
        {
            var positionParts = position.Split(" ");
            if (positionParts.Length == 2 && !string.IsNullOrEmpty(Utils.Config.LocationLink))
            {
                return Utils.Config.LocationLink.Replace("LAT", positionParts[0]).Replace("LON", positionParts[1]);
            }
            return defaultValue;
        }

        private void LoadFile(int index)
        {
            if (SearchResult != null &&
                index >= 0 && index < SearchResult.Count)
            {
                SearchResultIndex = index;

                var selection = SearchResult.Files[SearchResultIndex];

                CurrentFileToolTip = $"{selection.Path} (Id: {selection.Id})";
                CurrentFileInternalPath = selection.Path;
                CurrentFileInternalDirectoryPath = Path.GetDirectoryName(selection.Path).Replace(@"\", "/");
                CurrentFilePath = Utils.FilesystemAccess.ToAbsolutePath(selection.Path);
                CurrentFileDescription = selection.Description ?? string.Empty;
                CurrentFileDateTime = GetFileDateTimeString(selection.Datetime);
                CurrentFilePosition = selection.Position ?? string.Empty;
                CurrentFilePositionLink = selection.Position != null ? CreatePositionLink(selection.Position, string.Empty) : string.Empty;
                CurrentFilePersons = GetFilePersonsString(selection);
                CurrentFileLocations = GetFileLocationsString(selection.Id);
                CurrentFileTags = GetFileTagsString(selection.Id);

                NewFileDescription = CurrentFileDescription;

                var uri = new Uri(CurrentFilePath, UriKind.Absolute);
                try
                {
                    CurrentFileLoadError = string.Empty;
                    imagePresenter.ShowImage(new BitmapImage(uri));
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
            CurrentFileInternalPath = string.Empty;
            CurrentFileInternalDirectoryPath = string.Empty;
            CurrentFilePath = string.Empty;
            CurrentFileDescription = string.Empty;
            CurrentFileDateTime = string.Empty;
            CurrentFilePosition = string.Empty;
            CurrentFilePositionLink = string.Empty;
            CurrentFilePersons = string.Empty;
            CurrentFileLocations = string.Empty;
            CurrentFileTags = string.Empty;

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
            var resultString = datetimeString.Contains("T") ? datetime.Value.ToString("yyyy-MM-dd HH:mm") : datetimeString;

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
            var persons = Utils.DbAccess.GetPersonsFromFile(selection.Id);
            var personStrings = persons.Select(p => $"{p.Firstname} {p.Lastname}{GetPersonAgeInFileString(selection.Datetime, p.DateOfBirth)}");
            return string.Join("\n", personStrings);
        }

        private string GetPersonAgeInFileString(string fileDatetimeStr, string personDateOfBirthStr)
        {
            DateTime? fileDatetime = fileDatetimeStr == null ? null : DatabaseParsing.ParseFilesDatetime(fileDatetimeStr);
            if (fileDatetime == null)
                return string.Empty;

            DateTime? personDateOfBirth = personDateOfBirthStr == null ? null : DatabaseParsing.ParsePersonsDateOfBirth(personDateOfBirthStr);
            if (personDateOfBirth == null)
                return string.Empty;

            var age = DatabaseUtils.GetYearsAgo(fileDatetime.Value, personDateOfBirth.Value);
            return $" ({age})";
        }

        private string GetFileLocationsString(int fileId)
        {
            var locations = Utils.DbAccess.GetLocationsFromFile(fileId);
            var locationStrings = locations.Select(l => l.Name);
            return string.Join("\n", locationStrings);
        }

        private string GetFileTagsString(int fileId)
        {
            var tags = Utils.DbAccess.GetTagsFromFile(fileId);
            var tagStrings = tags.Select(t => t.Name);
            return string.Join("\n", tagStrings);
        }

        public void AddFilePerson()
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedPersonToUpdate == null)
            {
                Utils.ShowErrorDialog("No person selected");
                return;
            }

            var fileId = searchResult.Files[SearchResultIndex].Id;
            if (!Utils.DbAccess.GetPersonsFromFile(fileId).Any(p => p.Id == SelectedPersonToUpdate.Id))
            {
                Utils.DbAccess.InsertFilePerson(fileId, SelectedPersonToUpdate.Id);
                LoadFile(SearchResultIndex);
                AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, SelectedPersonToUpdate.Id, SelectedPersonToUpdate.Name);
            }
            else
            {
                Utils.ShowErrorDialog("This person has already been added");
            }
        }

        public void RemoveFilePerson()
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedPersonToUpdate == null)
            {
                Utils.ShowErrorDialog("No person selected");
                return;
            }

            var fileId = searchResult.Files[SearchResultIndex].Id;
            Utils.DbAccess.DeleteFilePerson(fileId, SelectedPersonToUpdate.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.TogglePerson, SelectedPersonToUpdate.Id, SelectedPersonToUpdate.Name);
        }

        public bool PersonSelected()
        {
            return SelectedPersonToUpdate != null;
        }

        public void AddFileLocation()
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedLocationToUpdate == null)
            {
                Utils.ShowErrorDialog("No location selected");
                return;
            }

            var fileId = searchResult.Files[SearchResultIndex].Id;
            if (!Utils.DbAccess.GetLocationsFromFile(fileId).Any(l => l.Id == SelectedLocationToUpdate.Id))
            {
                Utils.DbAccess.InsertFileLocation(fileId, SelectedLocationToUpdate.Id);
                LoadFile(SearchResultIndex);
                AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, SelectedLocationToUpdate.Id, SelectedLocationToUpdate.Name);
            }
            else
            {
                Utils.ShowErrorDialog("This location has already been added");
            }
        }

        public void RemoveFileLocation()
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedLocationToUpdate == null)
            {
                Utils.ShowErrorDialog("No location selected");
                return;
            }

            var fileId = searchResult.Files[SearchResultIndex].Id;
            Utils.DbAccess.DeleteFileLocation(fileId, SelectedLocationToUpdate.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, SelectedLocationToUpdate.Id, SelectedLocationToUpdate.Name);
        }

        public bool LocationSelected()
        {
            return SelectedLocationToUpdate != null;
        }

        public void AddFileTag()
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedTagToUpdate == null)
            {
                Utils.ShowErrorDialog("No tag selected");
                return;
            }

            var fileId = searchResult.Files[SearchResultIndex].Id;
            if (!Utils.DbAccess.GetTagsFromFile(fileId).Any(t => t.Id == SelectedTagToUpdate.Id))
            {
                Utils.DbAccess.InsertFileTag(fileId, SelectedTagToUpdate.Id);
                LoadFile(SearchResultIndex);
                AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, SelectedTagToUpdate.Id, SelectedTagToUpdate.Name);
            }
            else
            {
                Utils.ShowErrorDialog("This tag has already been added");
            }
        }

        public void RemoveFileTag()
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedTagToUpdate == null)
            {
                Utils.ShowErrorDialog("No tag selected");
                return;
            }

            var fileId = searchResult.Files[SearchResultIndex].Id;
            Utils.DbAccess.DeleteFileTag(fileId, SelectedTagToUpdate.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleTag, SelectedTagToUpdate.Id, SelectedTagToUpdate.Name);
        }

        public bool TagSelected()
        {
            return SelectedTagToUpdate != null;
        }

        public void SetFileDescription()
        {
            if (SearchResultIndex != -1)
            {
                var selection = SearchResult.Files[SearchResultIndex];
                var fileId = selection.Id;
                var description = string.IsNullOrEmpty(NewFileDescription) ? null : NewFileDescription;

                try
                {
                    Utils.DbAccess.UpdateFileDescription(fileId, description);
                    selection.Description = description;
                    LoadFile(SearchResultIndex);
                }
                catch (DataValidationException e)
                {
                    Utils.ShowErrorDialog(e.Message);
                }
            }
        }

        public void UpdateFileFromMetaData()
        {
            if (SearchResultIndex != -1)
            {
                var selection = SearchResult.Files[SearchResultIndex];
                var fileId = selection.Id;
                Utils.DbAccess.UpdateFileFromMetaData(selection.Id, Utils.FilesystemAccess);

                var updatedFile = Utils.DbAccess.GetFileById(fileId);
                selection.Datetime = updatedFile.Datetime;
                selection.Position = updatedFile.Position;
                LoadFile(SearchResultIndex);
            }
        }

        public void CreatePerson()
        {
            var window = new AddPersonWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();

            ReloadPersons();
        }

        public void CreateLocation()
        {
            var window = new AddLocationWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();

            ReloadLocations();
        }

        public void CreateTag()
        {
            var window = new AddTagWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();

            ReloadTags();
        }

        private void ReloadPersons()
        {
            Persons.Clear();
            var persons = Utils.DbAccess.GetPersons().Select(p => new PersonToUpdate() { Id = p.Id, Name = p.Firstname + " " + p.Lastname }).ToList();
            persons.Sort(new PersonToUpdateSorter());
            foreach (var person in persons)
            {
                Persons.Add(person);
            }
        }

        private void ReloadLocations()
        {
            Locations.Clear();
            var locations = Utils.DbAccess.GetLocations().Select(l => new LocationToUpdate() { Id = l.Id, Name = l.Name }).ToList();
            locations.Sort(new LocationToUpdateSorter());
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }

        private void ReloadTags()
        {
            Tags.Clear();
            var tags = Utils.DbAccess.GetTags().Select(t => new TagToUpdate() { Id = t.Id, Name = t.Name }).ToList();
            tags.Sort(new TagToUpdateSorter());
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
        }

        private void ReloadFolders()
        {
            var root = new Folder(RootFolderName);

            Folders.Clear();
            Folders.Add(root);

            foreach (var file in Utils.DbAccess.GetFiles())
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
                UpdateHistoryItems.Remove(duplicatedItem);
            }

            UpdateHistoryItems.Insert(0, new UpdateHistoryItem(type, itemId, itemName));

            while (UpdateHistoryItems.Count > 12)
            {
                UpdateHistoryItems.Remove(UpdateHistoryItems.Last());
            }

            for (int i=0; i<UpdateHistoryItems.Count; i++)
            {
                var item = UpdateHistoryItems[i];
                var newShortcut = $"F{i + 1}";

                if (item.Shortcut != newShortcut)
                {
                    UpdateHistoryItems[i] = new UpdateHistoryItem(item.Type, item.ItemId, item.ItemName) { Shortcut = newShortcut };
                }
            }

            OnPropertyChanged(nameof(UpdateHistoryItems));
        }

        public ICommand FunctionKeyCommand => functionKeyCommand ??= new CommandHandler(FunctionKey);
        private ICommand functionKeyCommand;

        private void FunctionKey(object parameter)
        {
            if (!ReadWriteMode || !HasNonEmptySearchResult)
            {
                return;
            }

            var fileId = SearchResult.Files[SearchResultIndex].Id;

            var number = int.Parse((string)parameter);
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
                    if (Utils.DbAccess.GetPersonsFromFile(fileId).Any(x => x.Id == personId))
                    {
                        Utils.DbAccess.DeleteFilePerson(fileId, personId);
                    }
                    else
                    {
                        Utils.DbAccess.InsertFilePerson(fileId, personId);
                    }
                    break;

                case UpdateHistoryType.ToggleLocation:
                    var locationId = historyItem.ItemId;
                    if (Utils.DbAccess.GetLocationsFromFile(fileId).Any(x => x.Id == locationId))
                    {
                        Utils.DbAccess.DeleteFileLocation(fileId, locationId);
                    }
                    else
                    {
                        Utils.DbAccess.InsertFileLocation(fileId, locationId);
                    }
                    break;

                case UpdateHistoryType.ToggleTag:
                    var tagId = historyItem.ItemId;
                    if (Utils.DbAccess.GetTagsFromFile(fileId).Any(x => x.Id == tagId))
                    {
                        Utils.DbAccess.DeleteFileTag(fileId, tagId);
                    }
                    else
                    {
                        Utils.DbAccess.InsertFileTag(fileId, tagId);
                    }
                    break;
            }

            LoadFile(SearchResultIndex);
        }
    }
}
