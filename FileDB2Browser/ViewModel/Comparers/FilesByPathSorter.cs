using System.Collections.Generic;
using FileDB2Interface.Model;

namespace FileDB2Browser.ViewModel.Comparers
{
    public class FilesByPathSorter : IComparer<FilesModel>
    {
        public int Compare(FilesModel x, FilesModel y)
        {
            return x.path.CompareTo(y.path);
        }
    }
}
