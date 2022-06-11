using System.Collections.Generic;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.Sorters
{
    public class FilesModelByDateSorter : IComparer<FilesModel>
    {
        public int Compare(FilesModel x, FilesModel y)
        {
            if (x.Datetime == y.Datetime)
            {
                // Note: covers same datetime and when both are null
                return x.Path.CompareTo(y.Path);
            }
            else if (x.Datetime == null)
            {
                return 1;
            }
            else if (y.Datetime == null)
            {
                return -1;
            }

            var xDatetime = DatabaseParsing.ParseFilesDatetime(x.Datetime).Value;
            var yDatetime = DatabaseParsing.ParseFilesDatetime(y.Datetime).Value;

            return xDatetime.CompareTo(yDatetime);
        }
    }
}
