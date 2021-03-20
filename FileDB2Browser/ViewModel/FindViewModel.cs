using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
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

        public List<FilesModel> SearchResult
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
                }
            }
        }
        private List<FilesModel> searchResult = null;

        public int SearchNumberOfHits { get; private set; } = 0;

        public int TotalNumberOfFiles { get; }

        public void FindRandomFiles()
        {
            SearchResult = fileDB2Handle.SearchFilesRandom(10);
        }

        public FindViewModel(FileDB2Handle fileDB2Handle)
        {
            // TODO: set from db
            TotalNumberOfFiles = 10;

            this.fileDB2Handle = fileDB2Handle;
        }
    }
}
