using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.ViewModel.Sorters
{
    public class FilesByPathSorter : IComparer<FilesModel>
    {
        public int Compare(FilesModel x, FilesModel y)
        {
            return x.path.CompareTo(y.path);
        }
    }
}
