using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FileDB2Interface;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel
{
    public class FindViewModel
    {
        private readonly FileDB2Handle fileDB2Handle;

        private ICommand findRandomFilesCommand;
        public ICommand FindRandomFilesCommand
        {
            get
            {
                return findRandomFilesCommand ??= new CommandHandler(() => FindRandomFiles());
            }
        }

        private List<FilesModel> searchResult = null;

        public int SearchNumberOfHits { get; private set; } = -1;

        public bool HasSearchResult => SearchNumberOfHits != -1;

        public int TotalNumberOfFiles { get; }

        public void FindRandomFiles()
        {
            // TODO: add SearchFilesRandom(10);
            searchResult = fileDB2Handle.SearchFiles("test");
            SearchNumberOfHits = searchResult.Count;
        }

        public FindViewModel(FileDB2Handle fileDB2Handle)
        {
            // TODO: set from db
            TotalNumberOfFiles = 10;

            this.fileDB2Handle = fileDB2Handle;
        }
    }
}
