using System.Collections.Generic;
using FileDBShared.Model;

namespace FileDB.Sorters;

public class FileModelByDateSorter : IComparer<FileModel>
{
    public int Compare(FileModel? x, FileModel? y)
    {
        if (x!.Datetime == y!.Datetime)
        {
            // Note: covers same datetime and when both are null
            return x.Path.CompareTo(y.Path);
        }
        else if (x.Datetime is null)
        {
            return 1;
        }
        else if (y.Datetime is null)
        {
            return -1;
        }

        var xDatetime = DatabaseParsing.ParseFilesDatetime(x.Datetime)!.Value;
        var yDatetime = DatabaseParsing.ParseFilesDatetime(y.Datetime)!.Value;

        return xDatetime.CompareTo(yDatetime);
    }
}
