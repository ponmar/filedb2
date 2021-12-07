using System.Collections.Generic;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB.Sorters
{
    public class FilesByDateSorter : IComparer<FilesModel>
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

            // TODO: returns null here for "YYYY-MM-DD" format. Must try the other formats here.
            var xDatetime = DatabaseParsing.ParseFilesDatetime(x.Datetime).Value;
            // TODO: returns null here for "YYYY-MM-DD" format. Must try the other formats here.
            var yDatetime = DatabaseParsing.ParseFilesDatetime(y.Datetime).Value;

            return xDatetime.CompareTo(yDatetime);
        }
    }
}
