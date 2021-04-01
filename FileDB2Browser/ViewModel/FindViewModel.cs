using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using FileDB2Browser.Config;
using FileDB2Browser.View;
using FileDB2Interface;
using FileDB2Interface.Model;
using TextCopy;

namespace FileDB2Browser.ViewModel
{
    public interface IImagePresenter
    {
        void ShowImage(BitmapImage image);
    }

    public class PersonToAddSorter : IComparer<PersonToAdd>
    {
        public int Compare(PersonToAdd x, PersonToAdd y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }

    public class LocationToAddSorter : IComparer<LocationToAdd>
    {
        public int Compare(LocationToAdd x, LocationToAdd y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }

    public class TagToAddSorter : IComparer<TagToAdd>
    {
        public int Compare(TagToAdd x, TagToAdd y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }

    public class PersonToAdd
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class LocationToAdd
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class TagToAdd
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    public class FindViewModel : ViewModelBase
    {
        private readonly FileDB2Handle fileDB2Handle;

        private readonly IImagePresenter imagePresenter;

        #region Browsing commands

        public ICommand PrevFileCommand
        {
            get
            {
                return prevFileCommand ??= new CommandHandler(PrevFile);
            }
        }
        private ICommand prevFileCommand;

        public ICommand NextFileCommand
        {
            get
            {
                return nextFileCommand ??= new CommandHandler(NextFile);
            }
        }
        private ICommand nextFileCommand;

        public ICommand PrevDirectoryCommand
        {
            get
            {
                return prevDirectoryCommand ??= new CommandHandler(PrevDirectory);
            }
        }
        private ICommand prevDirectoryCommand;

        public ICommand NextDirectoryCommand
        {
            get
            {
                return nextDirectoryCommand ??= new CommandHandler(NextDirectory);
            }
        }
        private ICommand nextDirectoryCommand;

        public ICommand FirstFileCommand
        {
            get
            {
                return firstFileCommand ??= new CommandHandler(FirstFile);
            }
        }
        private ICommand firstFileCommand;

        public ICommand LastFileCommand
        {
            get
            {
                return lastFileCommand ??= new CommandHandler(LastFile);
            }
        }
        private ICommand lastFileCommand;

        public ICommand ToggleSlideshowCommand
        {
            get
            {
                return toggleSlideshowCommand ??= new CommandHandler(ToggleSlideshow);
            }
        }
        private ICommand toggleSlideshowCommand;

        public bool SlideshowActive
        {
            get => slideshowActive;
            set { SetProperty(ref slideshowActive, value); }
        }
        private bool slideshowActive = false;

        #endregion

        #region Search commands and properties

        public ICommand FindRandomFilesCommand
        {
            get
            {
                return findRandomFilesCommand ??= new CommandHandler(FindRandomFiles);
            }
        }
        private ICommand findRandomFilesCommand;

        public ICommand FindAllFilesCommand
        {
            get
            {
                return findAllFilesCommand ??= new CommandHandler(FindAllFiles);
            }
        }
        private ICommand findAllFilesCommand;

        public ICommand FindFilesByTextCommand
        {
            get
            {
                return findFilesByTextCommand ??= new CommandHandler(FindFilesByText);
            }
        }
        private ICommand findFilesByTextCommand;

        public string SearchPattern
        {
            get => searchPattern;
            set { SetProperty(ref searchPattern, value); }
        }
        private string searchPattern;

        public ICommand FindFilesWithPersonCommand
        {
            get
            {
                return findFilesWithPersonCommand ??= new CommandHandler(FindFilesWithPerson);
            }
        }
        private ICommand findFilesWithPersonCommand;

        public ICommand FindFilesWithLocationCommand
        {
            get
            {
                return findFilesWithLocationCommand ??= new CommandHandler(FindFilesWithLocation);
            }
        }
        private ICommand findFilesWithLocationCommand;

        public ICommand FindFilesWithTagCommand
        {
            get
            {
                return findFilesWithTagCommand ??= new CommandHandler(FindFilesWithTag);
            }
        }
        private ICommand findFilesWithTagCommand;

        public ICommand FindFilesByPersonAgeCommand
        {
            get
            {
                return findFilesByPersonAgeCommand ??= new CommandHandler(FindFilesByPersonAge);
            }
        }
        private ICommand findFilesByPersonAgeCommand;

        public string SearchPersonAge
        {
            get => searchPersonAge;
            set { SetProperty(ref searchPersonAge, value); }
        }
        private string searchPersonAge;

        #endregion

        #region Meta-data change commands and properties

        public ICommand OpenFileLocationCommand
        {
            get
            {
                return openFileLocationCommand ??= new CommandHandler(OpenFileLocation);
            }
        }
        private ICommand openFileLocationCommand;

        public ICommand FindFilesFromListCommand
        {
            get
            {
                return findFilesFromListCommand ??= new CommandHandler(FindFilesFromList);
            }
        }
        private ICommand findFilesFromListCommand;

        public string FileListSearch
        {
            get => fileListSearch;
            set { SetProperty(ref fileListSearch, value); }
        }
        private string fileListSearch;

        public ICommand ExportFileListCommand
        {
            get
            {
                return exportFileListCommand ??= new CommandHandler(ExportFileList);
            }
        }
        private ICommand exportFileListCommand;

        public ICommand AddFilePersonCommand
        {
            get
            {
                return addFilePersonCommand ??= new CommandHandler(AddFilePerson);
            }
        }
        private ICommand addFilePersonCommand;

        public ICommand RemoveFilePersonCommand
        {
            get
            {
                return removeFilePersonCommand ??= new CommandHandler(RemoveFilePerson);
            }
        }
        private ICommand removeFilePersonCommand;

        public ICommand AddFileLocationCommand
        {
            get
            {
                return addFileLocationCommand ??= new CommandHandler(AddFileLocation);
            }
        }
        private ICommand addFileLocationCommand;

        public ICommand RemoveFileLocationCommand
        {
            get
            {
                return removeFileLocationCommand ??= new CommandHandler(RemoveFileLocation);
            }
        }
        private ICommand removeFileLocationCommand;

        public ICommand AddFileTagCommand
        {
            get
            {
                return addFileTagCommand ??= new CommandHandler(AddFileTag);
            }
        }
        private ICommand addFileTagCommand;

        public ICommand RemoveFileTagCommand
        {
            get
            {
                return removeFileTagCommand ??= new CommandHandler(RemoveFileTag);
            }
        }
        private ICommand removeFileTagCommand;

        public ICommand SetFileDescriptionCommand
        {
            get
            {
                return setFileDescriptionCommand ??= new CommandHandler(SetFileDescription);
            }
        }
        private ICommand setFileDescriptionCommand;

        public string NewFileDescription
        {
            get => newFileDescription;
            set { SetProperty(ref newFileDescription, value); }
        }
        private string newFileDescription;

        public ICommand CreatePersonCommand
        {
            get
            {
                return createPersonCommand ??= new CommandHandler(CreatePerson);
            }
        }
        private ICommand createPersonCommand;

        public ICommand CreateLocationCommand
        {
            get
            {
                return createLocationCommand ??= new CommandHandler(CreateLocation);
            }
        }
        private ICommand createLocationCommand;

        public ICommand CreateTagCommand
        {
            get
            {
                return createTagCommand ??= new CommandHandler(CreateTag);
            }
        }
        private ICommand createTagCommand;

        #endregion

        #region Search result

        private List<FilesModel> SearchResult
        {
            get => searchResult;
            set
            {
                if (searchResult != value)
                {
                    searchResult = value;
                    if (searchResult != null)
                    {
                        SearchNumberOfHits = searchResult.Count;
                        if (searchResult.Count > 0)
                        {
                            LoadFile(0);
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
        private List<FilesModel> searchResult = null;

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

        public ObservableCollection<PersonToAdd> Persons { get; } = new ObservableCollection<PersonToAdd>();

        public PersonToAdd SelectedPersonToAdd { get; set; }

        public PersonToAdd SelectedPersonSearch { get; set; }

        public ObservableCollection<LocationToAdd> Locations { get; } = new ObservableCollection<LocationToAdd>();

        public LocationToAdd SelectedLocationToAdd { get; set; }

        public LocationToAdd SelectedLocationSearch { get; set; }

        public ObservableCollection<TagToAdd> Tags { get; } = new ObservableCollection<TagToAdd>();

        public TagToAdd SelectedTagToAdd { get; set; }

        public TagToAdd SelectedTagSearch { get; set; }

        private DispatcherTimer slideshowTimer;

        public FindViewModel(FileDB2BrowserConfig config, FileDB2Handle fileDB2Handle, IImagePresenter imagePresenter)
        {
            this.fileDB2Handle = fileDB2Handle;
            this.imagePresenter = imagePresenter;

            TotalNumberOfFiles = fileDB2Handle.GetFileCount();

            ReloadPersons();
            ReloadLocations();
            ReloadTags();

            slideshowTimer = new DispatcherTimer();
            slideshowTimer.Tick += SlideshowTimer_Tick;
            slideshowTimer.Interval = config.SlideshowDelay;
        }

        public void PrevFile(object parameter)
        {
            StopSlideshow();
            LoadFile(SearchResultIndex - 1);
        }

        public void NextFile(object parameter)
        {
            StopSlideshow();
            NextFile();
        }

        private void NextFile()
        {
            LoadFile(SearchResultIndex + 1);
        }

        public void PrevDirectory(object parameter)
        {
            StopSlideshow();

            if (SearchResultIndex < 1)
                return;

            var currentDirectory = Path.GetDirectoryName(SearchResult[SearchResultIndex].path);

            for (int i = SearchResultIndex - 1; i < searchResult.Count; i--)
            {
                var directory = Path.GetDirectoryName(SearchResult[i].path);
                if (directory != currentDirectory)
                {
                    LoadFile(i);
                    return;
                }
            }
        }

        public void NextDirectory(object parameter)
        {
            StopSlideshow();

            if (SearchResultIndex == -1 || SearchResultIndex == searchResult.Count - 1)
                return;

            var currentDirectory = Path.GetDirectoryName(SearchResult[SearchResultIndex].path);

            for (int i=SearchResultIndex + 1; i<searchResult.Count; i++)
            {
                var directory = Path.GetDirectoryName(SearchResult[i].path);
                if (directory != currentDirectory)
                {
                    LoadFile(i);
                    return;
                }
            }
        }

        public void FirstFile(object parameter)
        {
            StopSlideshow();
            LoadFile(0);
        }

        public void LastFile(object parameter)
        {
            StopSlideshow();
            if (searchResult != null)
            {
                LoadFile(SearchResult.Count - 1);
            }
        }

        public void ToggleSlideshow(object parameter)
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
            NextFile();
            if (SearchResultIndex == SearchResult.Count - 1)
            {
                StopSlideshow();
            }
        }

        public void FindRandomFiles(object parameter)
        {
            StopSlideshow();
            SearchResult = fileDB2Handle.SearchFilesRandom(10);
        }

        public void FindAllFiles(object parameter)
        {
            StopSlideshow();
            SearchResult = fileDB2Handle.GetFiles();
        }

        public void FindFilesByText(object parameter)
        {
            StopSlideshow();
            if (!string.IsNullOrEmpty(SearchPattern))
            {
                SearchResult = fileDB2Handle.SearchFiles(SearchPattern);
            }
            else
            {
                SearchResult = null;
            }
        }

        public void FindFilesWithPerson(object parameter)
        {
            StopSlideshow();
            if (SelectedPersonSearch != null)
            {
                SearchResult = fileDB2Handle.GetFilesWithPersons(new List<int>() { SelectedPersonSearch.Id });
            }
        }

        public void FindFilesWithLocation(object parameter)
        {
            StopSlideshow();
            if (SelectedLocationSearch != null)
            {
                SearchResult = fileDB2Handle.GetFilesWithLocations(new List<int>() { SelectedLocationSearch.Id });
            }
        }

        public void FindFilesWithTag(object parameter)
        {
            StopSlideshow();
            if (SelectedTagSearch != null)
            {
                SearchResult = fileDB2Handle.GetFilesWithTags(new List<int>() { SelectedTagSearch.Id });
            }
        }

        public void FindFilesByPersonAge(object parameter)
        {
            StopSlideshow();
            if (!string.IsNullOrEmpty(SearchPersonAge))
            {
                // TODO

            }
        }

        public void FindFilesFromList(object parameter)
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
                SearchResult = fileIds.Select(f => fileDB2Handle.GetFileById(f)).ToList();
            }
        }

        public void OpenFileLocation(object parameter)
        {
            if (!string.IsNullOrEmpty(CurrentFilePath) &&
                File.Exists(CurrentFilePath))
            {
                var explorerPath = CurrentFilePath.Replace("/", @"\");
                Process.Start("explorer.exe", "/select, " + explorerPath);
            }
        }

        public void ExportFileList(object parameter)
        {
            if (HasNonEmptySearchResult)
            {
                var fileIdList = string.Join(';', SearchResult.Select(f => f.id));
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

                var selection = SearchResult[SearchResultIndex];

                CurrentFileInternalPath = selection.path;
                CurrentFilePath = fileDB2Handle.InternalPathToPath(selection.path);
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
            var persons = fileDB2Handle.GetPersonsFromFile(selection.id);
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
            var locations = fileDB2Handle.GetLocationsFromFile(fileId);
            var locationStrings = locations.Select(l => l.name);
            return string.Join("\n", locationStrings);
        }

        private string GetFileTagsString(int fileId)
        {
            var tags = fileDB2Handle.GetTagsFromFile(fileId);
            var tagStrings = tags.Select(t => t.name);
            return string.Join("\n", tagStrings);
        }

        public void AddFilePerson(object parameter)
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedPersonToAdd == null)
            {
                Utils.ShowErrorDialog("No person selected");
                return;
            }

            var fileId = searchResult[SearchResultIndex].id;
            if (!fileDB2Handle.GetPersonsFromFile(fileId).Any(p => p.id == SelectedPersonToAdd.Id))
            {
                fileDB2Handle.InsertFilePerson(fileId, SelectedPersonToAdd.Id);
                LoadFile(SearchResultIndex);
            }
            else
            {
                Utils.ShowErrorDialog("This person has already been added");
            }
        }

        public void RemoveFilePerson(object parameter)
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedPersonToAdd == null)
            {
                Utils.ShowErrorDialog("No person selected");
                return;
            }

            var fileId = searchResult[SearchResultIndex].id;
            fileDB2Handle.DeleteFilePerson(fileId, SelectedPersonToAdd.Id);
            LoadFile(SearchResultIndex);
        }

        public void AddFileLocation(object parameter)
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedLocationToAdd == null)
            {
                Utils.ShowErrorDialog("No location selected");
                return;
            }

            var fileId = searchResult[SearchResultIndex].id;
            if (!fileDB2Handle.GetLocationsFromFile(fileId).Any(l => l.id == SelectedLocationToAdd.Id))
            {
                fileDB2Handle.InsertFileLocation(fileId, SelectedLocationToAdd.Id);
                LoadFile(SearchResultIndex);
            }
            else
            {
                Utils.ShowErrorDialog("This location has already been added");
            }
        }

        public void RemoveFileLocation(object parameter)
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedLocationToAdd == null)
            {
                Utils.ShowErrorDialog("No location selected");
                return;
            }

            var fileId = searchResult[SearchResultIndex].id;
            fileDB2Handle.DeleteFileLocation(fileId, SelectedLocationToAdd.Id);
            LoadFile(SearchResultIndex);
        }

        public void AddFileTag(object parameter)
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedTagToAdd == null)
            {
                Utils.ShowErrorDialog("No tag selected");
                return;
            }

            var fileId = searchResult[SearchResultIndex].id;
            if (!fileDB2Handle.GetTagsFromFile(fileId).Any(t => t.id == SelectedTagToAdd.Id))
            {
                fileDB2Handle.InsertFileTag(fileId, SelectedTagToAdd.Id);
                LoadFile(SearchResultIndex);
            }
            else
            {
                Utils.ShowErrorDialog("This tag has already been added");
            }
        }

        public void RemoveFileTag(object parameter)
        {
            if (SearchResultIndex == -1)
            {
                Utils.ShowErrorDialog("No file selected");
                return;
            }

            if (SelectedTagToAdd == null)
            {
                Utils.ShowErrorDialog("No tag selected");
                return;
            }

            var fileId = searchResult[SearchResultIndex].id;
            fileDB2Handle.DeleteFileTag(fileId, SelectedTagToAdd.Id);
            LoadFile(SearchResultIndex);
        }

        public void SetFileDescription(object parameter)
        {
            if (SearchResultIndex != -1)
            {
                var selection = SearchResult[SearchResultIndex];
                var fileId = selection.id;
                var description = string.IsNullOrEmpty(NewFileDescription) ? null : NewFileDescription;
                fileDB2Handle.UpdateFileDescription(fileId, description);

                selection.description = description;
                LoadFile(SearchResultIndex);
            }
        }

        public void CreatePerson(object parameter)
        {
            var window = new AddPersonWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();

            ReloadPersons();
        }

        public void CreateLocation(object parameter)
        {
            var window = new AddLocationWindow
            {
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();

            ReloadLocations();
        }

        public void CreateTag(object parameter)
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
            var persons = fileDB2Handle.GetPersons().Select(p => new PersonToAdd() { Id = p.id, Name = p.firstname + " " + p.lastname }).ToList();
            persons.Sort(new PersonToAddSorter());
            foreach (var person in persons)
            {
                Persons.Add(person);
            }
        }

        private void ReloadLocations()
        {
            Locations.Clear();
            var locations = fileDB2Handle.GetLocations().Select(l => new LocationToAdd() { Id = l.id, Name = l.name }).ToList();
            locations.Sort(new LocationToAddSorter());
            foreach (var location in locations)
            {
                Locations.Add(location);
            }
        }

        private void ReloadTags()
        {
            Tags.Clear();
            var tags = fileDB2Handle.GetTags().Select(t => new TagToAdd() { Id = t.id, Name = t.name }).ToList();
            tags.Sort(new TagToAddSorter());
            foreach (var tag in tags)
            {
                Tags.Add(tag);
            }
        }
    }
}
