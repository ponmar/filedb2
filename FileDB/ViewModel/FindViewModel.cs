﻿using System;
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
using FileDB.View;
using FileDB.ViewModel.Sorters;
using FileDBInterface.Comparers;
using FileDBInterface.Model;
using TextCopy;

namespace FileDB.ViewModel
{
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
        public string Name => $"{Count}";

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

        public ICommand FindFilesWithPersonCommand => findFilesWithPersonCommand ??= new CommandHandler(FindFilesWithPerson);
        private ICommand findFilesWithPersonCommand;

        public ICommand FindFilesWithLocationCommand => findFilesWithLocationCommand ??= new CommandHandler(FindFilesWithLocation);
        private ICommand findFilesWithLocationCommand;

        public ICommand FindFilesWithTagCommand => findFilesWithTagCommand ??= new CommandHandler(FindFilesWithTag);
        private ICommand findFilesWithTagCommand;

        public ICommand FindFilesByPersonAgeCommand => findFilesByPersonAgeCommand ??= new CommandHandler(FindFilesByPersonAge);
        private ICommand findFilesByPersonAgeCommand;

        public string SearchPersonAge
        {
            get => searchPersonAge;
            set => SetProperty(ref searchPersonAge, value);
        }
        private string searchPersonAge;

        public ICommand FindFilesFromUnionCommand => findFilesFromUnionCommand ??= new CommandHandler(FindFilesFromUnion, FindFilesFromHistoryEnabled);
        private ICommand findFilesFromUnionCommand;

        public ICommand FindFilesFromIntersectionCommand => findFilesFromIntersectionCommand ??= new CommandHandler(FindFilesFromIntersection, FindFilesFromHistoryEnabled);
        private ICommand findFilesFromIntersectionCommand;

        public ICommand FindFilesFromDifferenceCommand => findFilesFromDifferenceCommand ??= new CommandHandler(FindFilesFromDifference, FindFilesFromHistoryEnabled);
        private ICommand findFilesFromDifferenceCommand;

        #endregion

        #region Meta-data change commands and properties

        public ICommand OpenFileLocationCommand => openFileLocationCommand ??= new CommandHandler(OpenFileLocation, () => HasNonEmptySearchResult);
        private ICommand openFileLocationCommand;

        public ICommand FindFilesFromMissingCategorizationCommand => findFilesFromMissingCategorizationCommand ??= new CommandHandler(FindFilesFromMissingCategorization);
        private ICommand findFilesFromMissingCategorizationCommand;

        public ICommand FindFilesFromListCommand => findFilesFromListCommand ??= new CommandHandler(FindFilesFromList);
        private ICommand findFilesFromListCommand;

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
        private bool readWriteMode = !Utils.BrowserConfig.ReadOnly;

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
            if (SearchResultHistory.Count == Utils.BrowserConfig.SearchHistorySize)
            {
                SearchResultHistory.RemoveAt(0);
            }
            SearchResultHistory.Add(searchResult);
        }

        // TODO: set empty search to avoid null checks everywhere?
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

        public string CurrentFileInternalPath
        {
            get => currentFileInternalPath;
            private set { SetProperty(ref currentFileInternalPath, value); }
        }
        private string currentFileInternalPath;

        public string CurrentFilePath
        {
            get => currentFilePath;
            private set { SetProperty(ref currentFilePath, value); }
        }
        private string currentFilePath;

        public string CurrentFileDescription
        {
            get => currentFileDescription;
            private set { SetProperty(ref currentFileDescription, value); }
        }
        private string currentFileDescription;

        public string CurrentFileDateTime
        {
            get => currentFiledateTime;
            private set { SetProperty(ref currentFiledateTime, value); }
        }
        private string currentFiledateTime;

        public string CurrentFilePosition
        {
            get => currentFilePosition;
            private set { SetProperty(ref currentFilePosition, value); }
        }
        private string currentFilePosition;

        public string CurrentFilePersons
        {
            get => currentFilePersons;
            private set { SetProperty(ref currentFilePersons, value); }
        }
        private string currentFilePersons;

        public string CurrentFileLocations
        {
            get => currentFileLocations;
            private set { SetProperty(ref currentFileLocations, value); }
        }
        private string currentFileLocations;

        public string CurrentFileTags
        {
            get => currentFileTags;
            private set { SetProperty(ref currentFileTags, value); }
        }
        private string currentFileTags;

        public string CurrentFileLoadError
        {
            get => currentFileLoadError;
            private set { SetProperty(ref currentFileLoadError, value); }
        }
        private string currentFileLoadError;

        #endregion

        public ObservableCollection<PersonToUpdate> Persons { get; } = new ObservableCollection<PersonToUpdate>();

        public PersonToUpdate SelectedPersonToUpdate { get; set; }

        public PersonToUpdate SelectedPersonSearch { get; set; }

        public ObservableCollection<LocationToUpdate> Locations { get; } = new ObservableCollection<LocationToUpdate>();

        public LocationToUpdate SelectedLocationToUpdate { get; set; }

        public LocationToUpdate SelectedLocationSearch { get; set; }

        public ObservableCollection<TagToUpdate> Tags { get; } = new ObservableCollection<TagToUpdate>();

        public TagToUpdate SelectedTagToUpdate { get; set; }

        public TagToUpdate SelectedTagSearch { get; set; }

        public ObservableCollection<UpdateHistoryItem> UpdateHistoryItems { get; } = new();

        private readonly DispatcherTimer slideshowTimer = new();

        public FindViewModel(IImagePresenter imagePresenter)
        {
            this.imagePresenter = imagePresenter;

            TotalNumberOfFiles = Utils.FileDBHandle.GetFileCount();

            ReloadPersons();
            ReloadLocations();
            ReloadTags();

            slideshowTimer.Tick += SlideshowTimer_Tick;
            slideshowTimer.Interval = Utils.BrowserConfig.SlideshowDelay;
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

            var currentDirectory = Path.GetDirectoryName(SearchResult.Files[SearchResultIndex].path);

            for (int i = SearchResultIndex - 1; i < searchResult.Count; i--)
            {
                var directory = Path.GetDirectoryName(SearchResult.Files[i].path);
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

            var currentDirectory = Path.GetDirectoryName(SearchResult.Files[SearchResultIndex].path);

            for (int i=SearchResultIndex + 1; i<searchResult.Count; i++)
            {
                var directory = Path.GetDirectoryName(SearchResult.Files[i].path);
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
                SearchResult = new SearchResult(Utils.FileDBHandle.SearchFilesRandom(value));
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

            var path = SearchResult.Files[SearchResultIndex].path;
            var dir = Path.GetDirectoryName(path).Replace('\\', '/');
            SearchResult = new SearchResult(Utils.FileDBHandle.SearchFilesByPath(dir));
        }

        public void FindAllFiles()
        {
            StopSlideshow();
            SearchResult = new SearchResult(Utils.FileDBHandle.GetFiles());
        }

        public void FindFilesByText()
        {
            StopSlideshow();
            if (!string.IsNullOrEmpty(SearchPattern))
            {
                SearchResult = new SearchResult(Utils.FileDBHandle.SearchFiles(SearchPattern));
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
            SearchResult = new SearchResult(Utils.FileDBHandle.SearchFilesBySex(sex));
        }

        public void FindFilesByDate()
        {
            StopSlideshow();
            SearchResult = new SearchResult(Utils.FileDBHandle.GetFileByDate(SearchStartDate.Date, SearchEndDate.Date));
        }

        public void FindFilesWithPerson()
        {
            StopSlideshow();
            if (SelectedPersonSearch != null)
            {
                SearchResult = new SearchResult(Utils.FileDBHandle.GetFilesWithPersons(new List<int>() { SelectedPersonSearch.Id }));
            }
        }

        public void FindFilesWithLocation()
        {
            StopSlideshow();
            if (SelectedLocationSearch != null)
            {
                SearchResult = new SearchResult(Utils.FileDBHandle.GetFilesWithLocations(new List<int>() { SelectedLocationSearch.Id }));
            }
        }

        public void FindFilesWithTag()
        {
            StopSlideshow();
            if (SelectedTagSearch != null)
            {
                SearchResult = new SearchResult(Utils.FileDBHandle.GetFilesWithTags(new List<int>() { SelectedTagSearch.Id }));
            }
        }

        public void FindFilesByPersonAge()
        {
            StopSlideshow();
            if (!string.IsNullOrEmpty(SearchPersonAge))
            {
                if (!int.TryParse(SearchPersonAge, out var age))
                {
                    Utils.ShowErrorDialog("Invalid age format");
                    return;
                }

                var result = new List<FilesModel>();
                var personsWithAge = Utils.FileDBHandle.GetPersons().Where(p => p.dateofbirth != null);

                foreach (var person in personsWithAge)
                {
                    var dateOfBirth = Utils.FileDBHandle.ParseDateOfBirth(person.dateofbirth);
                    foreach (var file in Utils.FileDBHandle.GetFilesWithPersons(new List<int>() { person.id }))
                    {
                        if (Utils.InternalDatetimeToDatetime(file.datetime, out var fileDatetime))
                        {
                            Utils.InternalDatetimeToDatetime(file.datetime);
                            int personAgeInFile = Utils.GetYearsAgo(fileDatetime.Value, dateOfBirth);
                            if (personAgeInFile == age)
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
            SearchResult = new SearchResult(Utils.FileDBHandle.GetFilesWithMissingData());
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
                SearchResult = new SearchResult(fileIds.Select(f => Utils.FileDBHandle.GetFileById(f)).ToList());
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
                var fileIdList = string.Join(';', SearchResult.Files.Select(f => f.id));
                ClipboardService.SetText(fileIdList);
            }
            else
            {
                ClipboardService.SetText("");
            }
        }

        private void LoadFile(int index)
        {
            if (SearchResult != null &&
                index >= 0 && index < SearchResult.Count)
            {
                SearchResultIndex = index;

                var selection = SearchResult.Files[SearchResultIndex];

                CurrentFileInternalPath = selection.path;
                CurrentFilePath = Utils.FileDBHandle.InternalPathToPath(selection.path);
                CurrentFileDescription = selection.description ?? string.Empty;
                CurrentFileDateTime = GetFileDateTimeString(selection.datetime);
                CurrentFilePosition = selection.position ?? string.Empty;
                CurrentFilePersons = GetFilePersonsString(selection);
                CurrentFileLocations = GetFileLocationsString(selection.id);
                CurrentFileTags = GetFileTagsString(selection.id);

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

            CurrentFileInternalPath = string.Empty;
            CurrentFilePath = string.Empty;
            CurrentFileDescription = string.Empty;
            CurrentFileDateTime = string.Empty;
            CurrentFilePosition = string.Empty;
            CurrentFilePersons = string.Empty;
            CurrentFileLocations = string.Empty;
            CurrentFileTags = string.Empty;

            CurrentFileLoadError = "No match";
            imagePresenter.ShowImage(null);
        }

        private string GetFileDateTimeString(string datetimeString)
        {
            var datetime = Utils.InternalDatetimeToDatetime(datetimeString);
            if (datetime == null)
            {
                return string.Empty;
            }

            var resultString = datetime.Value.ToString("yyyy-MM-dd HH:mm");
            var now = DateTime.Now;
            int yearsAgo = Utils.GetYearsAgo(now, datetime.Value);
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
            var persons = Utils.FileDBHandle.GetPersonsFromFile(selection.id);
            var personStrings = persons.Select(p => $"{p.firstname} {p.lastname}{GetPersonAgeInFileString(selection.datetime, p.dateofbirth)}");
            return string.Join("\n", personStrings);
        }

        private string GetPersonAgeInFileString(string fileDatetimeStr, string personDateOfBirthStr)
        {
            var fileDatetime = Utils.InternalDatetimeToDatetime(fileDatetimeStr);
            if (fileDatetime == null)
                return string.Empty;

            var personDateOfBirth = Utils.InternalDatetimeToDatetime(personDateOfBirthStr);
            if (personDateOfBirth == null)
                return string.Empty;

            var age = Utils.GetBornYearsAgo(fileDatetime.Value, personDateOfBirth.Value);
            if (age != string.Empty)
            {
                age = $" ({age})";
            }
            return age;
        }

        private string GetFileLocationsString(int fileId)
        {
            var locations = Utils.FileDBHandle.GetLocationsFromFile(fileId);
            var locationStrings = locations.Select(l => l.name);
            return string.Join("\n", locationStrings);
        }

        private string GetFileTagsString(int fileId)
        {
            var tags = Utils.FileDBHandle.GetTagsFromFile(fileId);
            var tagStrings = tags.Select(t => t.name);
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

            var fileId = searchResult.Files[SearchResultIndex].id;
            if (!Utils.FileDBHandle.GetPersonsFromFile(fileId).Any(p => p.id == SelectedPersonToUpdate.Id))
            {
                Utils.FileDBHandle.InsertFilePerson(fileId, SelectedPersonToUpdate.Id);
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

            var fileId = searchResult.Files[SearchResultIndex].id;
            Utils.FileDBHandle.DeleteFilePerson(fileId, SelectedPersonToUpdate.Id);
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

            var fileId = searchResult.Files[SearchResultIndex].id;
            if (!Utils.FileDBHandle.GetLocationsFromFile(fileId).Any(l => l.id == SelectedLocationToUpdate.Id))
            {
                Utils.FileDBHandle.InsertFileLocation(fileId, SelectedLocationToUpdate.Id);
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

            var fileId = searchResult.Files[SearchResultIndex].id;
            Utils.FileDBHandle.DeleteFileLocation(fileId, SelectedLocationToUpdate.Id);
            LoadFile(SearchResultIndex);
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

            var fileId = searchResult.Files[SearchResultIndex].id;
            if (!Utils.FileDBHandle.GetTagsFromFile(fileId).Any(t => t.id == SelectedTagToUpdate.Id))
            {
                Utils.FileDBHandle.InsertFileTag(fileId, SelectedTagToUpdate.Id);
                LoadFile(SearchResultIndex);
                AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, SelectedTagToUpdate.Id, SelectedTagToUpdate.Name);
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

            var fileId = searchResult.Files[SearchResultIndex].id;
            Utils.FileDBHandle.DeleteFileTag(fileId, SelectedTagToUpdate.Id);
            LoadFile(SearchResultIndex);
            AddUpdateHistoryItem(UpdateHistoryType.ToggleLocation, SelectedTagToUpdate.Id, SelectedTagToUpdate.Name);
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
                var fileId = selection.id;
                var description = string.IsNullOrEmpty(NewFileDescription) ? null : NewFileDescription;
                Utils.FileDBHandle.UpdateFileDescription(fileId, description);

                selection.description = description;
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
            var persons = Utils.FileDBHandle.GetPersons().Select(p => new PersonToUpdate() { Id = p.id, Name = p.firstname + " " + p.lastname }).ToList();
            persons.Sort(new PersonToAddSorter());
            foreach (var person in persons)
            {
                Persons.Add(person);
            }
        }

        private void ReloadLocations()
        {
            Locations.Clear();
            var locations = Utils.FileDBHandle.GetLocations().Select(l => new LocationToUpdate() { Id = l.id, Name = l.name }).ToList();
            locations.Sort(new LocationToAddSorter());
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }

        private void ReloadTags()
        {
            Tags.Clear();
            var tags = Utils.FileDBHandle.GetTags().Select(t => new TagToUpdate() { Id = t.id, Name = t.name }).ToList();
            tags.Sort(new TagToAddSorter());
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
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
        }

        public ICommand FunctionKeyCommand => functionKeyCommand ??= new CommandHandler(FunctionKey);
        private ICommand functionKeyCommand;

        private void FunctionKey(object parameter)
        {
            if (!ReadWriteMode || !HasNonEmptySearchResult)
            {
                return;
            }

            var fileId = SearchResult.Files[SearchResultIndex].id;

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
                    if (Utils.FileDBHandle.GetPersonsFromFile(fileId).Any(x => x.id == personId))
                    {
                        Utils.FileDBHandle.DeleteFilePerson(fileId, personId);
                    }
                    else
                    {
                        Utils.FileDBHandle.InsertFilePerson(fileId, personId);
                    }
                    break;

                case UpdateHistoryType.ToggleLocation:
                    var locationId = historyItem.ItemId;
                    if (Utils.FileDBHandle.GetLocationsFromFile(fileId).Any(x => x.id == locationId))
                    {
                        Utils.FileDBHandle.DeleteFileLocation(fileId, locationId);
                    }
                    else
                    {
                        Utils.FileDBHandle.InsertFileLocation(fileId, locationId);
                    }
                    break;

                case UpdateHistoryType.ToggleTag:
                    var tagId = historyItem.ItemId;
                    if (Utils.FileDBHandle.GetTagsFromFile(fileId).Any(x => x.id == tagId))
                    {
                        Utils.FileDBHandle.DeleteFileTag(fileId, tagId);
                    }
                    else
                    {
                        Utils.FileDBHandle.InsertFileTag(fileId, tagId);
                    }
                    break;
            }

            LoadFile(SearchResultIndex);
        }
    }
}