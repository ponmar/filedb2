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
        public string Path { get; set; }
        public string Description { get; set; }
        public DateTime? Datetime { get; set; }
        public string Position { get; set; }
    }

    public class FindViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly FileDB2Handle fileDB2Handle;

        private ICommand findRandomFilesCommand;
        public ICommand FindRandomFilesCommand
        {
            get
            {
                return findRandomFilesCommand ??= new CommandHandler(() => FindRandomFiles());
            }
        }

        public List<FileSearchResult> SearchResult
        {
            get => searchResult;
            private set
            {
                if (searchResult != value)
                {
                    searchResult = value;
                    if (searchResult != null)
                    {
                        SearchNumberOfHits = searchResult.Count;
                    }
                    else
                    {
                        SearchNumberOfHits = 0;
                    }

                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SearchNumberOfHits)));
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(SearchResult)));
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(HasSearchResult)));
                }
            }
        }
        private List<FileSearchResult> searchResult = null;

        public int SearchNumberOfHits { get; private set; } = 0;

        public int TotalNumberOfFiles { get; }

        public bool HasSearchResult => SearchResult != null;

        public void FindRandomFiles()
        {
            var files = fileDB2Handle.SearchFilesRandom(10);
            SearchResult = files.Select(fm => new FileSearchResult()
            {
                Path = fm.path,
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
    }
}
