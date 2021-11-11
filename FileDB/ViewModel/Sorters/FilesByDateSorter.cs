using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FileDBInterface.Model;

namespace FileDB.ViewModel.Sorters
{
    public class FilesByDateSorter : IComparer<FilesModel>
    {
        public int Compare(FilesModel x, FilesModel y)
        {
            if (x.datetime == null && y.datetime == null)
            {
                return 0;
            }
            else if (x.datetime == null)
            {
                return 1;
            }
            else if (y.datetime == null)
            {
                return -1;
            }

            return x.datetime.CompareTo(y.datetime);
        }
    }
}
