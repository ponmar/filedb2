using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Cache;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
    public interface IImagePresenter
    {
        void ShowImage(BitmapImage image);
    }

    public class FindViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly FileDB2Handle fileDB2Handle;

        private readonly IImagePresenter imagePresenter;

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
            set
            {
                if (value != searchPattern)
                {
                    searchPattern = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SearchPattern)));
                }
            }
        }
        private string searchPattern;

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

                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SearchNumberOfHits)));
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(HasSearchResult)));
                }
            }
        }
        private List<FilesModel> searchResult = null;

        public int SearchResultIndex
        {
            get => searchResultIndex;
            private set
            {
                if (value != searchResultIndex)
                {
                    searchResultIndex = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SearchResultIndex)));
                }
            }
        }
        private int searchResultIndex = -1;

        public int SearchNumberOfHits { get; private set; } = 0;

        public int TotalNumberOfFiles { get; }

        public bool HasSearchResult => SearchResult != null;

        public string CurrentFileInternalPath
        {
            get => currentFileInternalPath;
            private set
            {
                if (value != currentFileInternalPath)
                {
                    currentFileInternalPath = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileInternalPath)));
                }
            }
        }
        private string currentFileInternalPath;

        public string CurrentFilePath
        {
            get => currentFilePath;
            private set
            {
                if (value != currentFilePath)
                {
                    currentFilePath = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFilePath)));
                }
            }
        }
        private string currentFilePath;

        public string CurrentFileDescription
        {
            get => currentFileDescription;
            private set
            {
                if (value != currentFileDescription)
                {
                    currentFileDescription = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileDescription)));
                }
            }
        }
        private string currentFileDescription;

        public string CurrentFileDateTime
        {
            get => currentFiledateTime;
            private set
            {
                if (value != currentFiledateTime)
                {
                    currentFiledateTime = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileDateTime)));
                }
            }
        }
        private string currentFiledateTime;

        public string CurrentFilePosition
        {
            get => currentFilePosition;
            private set
            {
                if (value != currentFilePosition)
                {
                    currentFilePosition = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFilePosition)));
                }
            }
        }
        private string currentFilePosition;

        public string CurrentFilePersons
        {
            get => currentFilePersons;
            private set
            {
                if (value != currentFilePersons)
                {
                    currentFilePersons = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFilePersons)));
                }
            }
        }
        private string currentFilePersons;

        public string CurrentFileLocations
        {
            get => currentFileLocations;
            private set
            {
                if (value != currentFileLocations)
                {
                    currentFileLocations = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileLocations)));
                }
            }
        }
        private string currentFileLocations;

        public string CurrentFileTags
        {
            get => currentFileTags;
            private set
            {
                if (value != currentFileTags)
                {
                    currentFileTags = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileTags)));
                }
            }
        }
        private string currentFileTags;

        public string CurrentFileLoadError
        {
            get => currentFileLoadError;
            private set
            {
                if (value != currentFileLoadError)
                {
                    currentFileLoadError = value;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentFileLoadError)));
                }
            }
        }
        private string currentFileLoadError;

        public FindViewModel(FileDB2Handle fileDB2Handle, IImagePresenter imagePresenter)
        {
            this.fileDB2Handle = fileDB2Handle;
            this.imagePresenter = imagePresenter;
            TotalNumberOfFiles = fileDB2Handle.GetFileCount();
        }

        public void PrevFile(object parameter)
        {
            LoadFile(SearchResultIndex - 1);
        }

        public void NextFile(object parameter)
        {
            LoadFile(SearchResultIndex + 1);
        }

        public void PrevDirectory(object parameter)
        {
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
            LoadFile(0);
        }

        public void LastFile(object parameter)
        {
            if (searchResult != null)
            {
                LoadFile(SearchResult.Count - 1);
            }
        }

        public void FindRandomFiles(object parameter)
        {
            SearchResult = fileDB2Handle.SearchFilesRandom(10);
        }

        public void FindAllFiles(object parameter)
        {
            SearchResult = fileDB2Handle.GetFiles();
        }

        public void FindFilesByText(object parameter)
        {
            if (!string.IsNullOrEmpty(SearchPattern))
            {
                SearchResult = fileDB2Handle.SearchFiles(SearchPattern);
            }
            else
            {
                SearchResult = null;
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
            int yearsAgo = Utils.GetYearsAgo(DateTime.Now, datetime.Value);
            if (yearsAgo == 0)
            {
                resultString = $"{resultString} (this year)";
            }
            else if (yearsAgo > 0)
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
    }
}
