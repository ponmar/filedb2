using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
    public class FileSearchResult
    {
        public string InternalPath { get; set; }
        public string Path { get; set; }
        public string Description { get; set; }
        public DateTime? Datetime { get; set; }
        public string Position { get; set; }
    }

    public class FindViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly FileDB2Handle fileDB2Handle;

        public ICommand PrevFileCommand
        {
            get
            {
                return prevFileCommand ??= new CommandHandler(() => PrevFile());
            }
        }
        private ICommand prevFileCommand;

        public ICommand NextFileCommand
        {
            get
            {
                return nextFileCommand ??= new CommandHandler(() => NextFile());
            }
        }
        private ICommand nextFileCommand;

        public ICommand FindRandomFilesCommand
        {
            get
            {
                return findRandomFilesCommand ??= new CommandHandler(() => FindRandomFiles());
            }
        }
        private ICommand findRandomFilesCommand;


        private List<FileSearchResult> SearchResult
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
        private List<FileSearchResult> searchResult = null;

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

        public void FindRandomFiles()
        {
            var files = fileDB2Handle.SearchFilesRandom(10);
            SearchResult = files.Select(fm => new FileSearchResult()
            {
                InternalPath = fm.path,
                Path = fileDB2Handle.InternalPathToPath(fm.path),
                Description = fm.description,
                Datetime = Utils.InternalDatetimeToDatetime(fm.datetime),
                Position = fm.position,
            }).ToList();
        }

        public FindViewModel(FileDB2Handle fileDB2Handle)
        {
            this.fileDB2Handle = fileDB2Handle;
            TotalNumberOfFiles = fileDB2Handle.GetFileCount();
        }

        public void PrevFile()
        {
            LoadFile(SearchResultIndex - 1);
        }

        public void NextFile()
        {
            LoadFile(SearchResultIndex + 1);
        }

        private void LoadFile(int index)
        {
            if (SearchResult != null &&
                index >= 0 && index < SearchResult.Count)
            {
                SearchResultIndex = index;
                CurrentFilePath = SearchResult[SearchResultIndex].Path;
            }
        }

        private void ResetFile()
        {
            SearchResultIndex = -1;
            CurrentFilePath = string.Empty;
        }
    }
}
