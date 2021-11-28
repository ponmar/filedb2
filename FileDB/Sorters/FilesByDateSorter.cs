using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Sorters
{
    public class FilesByDateSorter : IComparer<FilesModel>
    {
        public int Compare(FilesModel x, FilesModel y)
        {
            if (x.Datetime == null && y.Datetime == null)
            {
                return 0;
            }
            else if (x.Datetime == null)
            {
                return 1;
            }
            else if (y.Datetime == null)
            {
                return -1;
            }

            return x.Datetime.CompareTo(y.Datetime);
        }
    }
}
