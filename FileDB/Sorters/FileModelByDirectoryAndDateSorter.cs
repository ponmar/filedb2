using System.Collections.Generic;
using System.IO;
using FileDBInterface.Model;

namespace FileDB.Sorters;

public class FileModelByDirectoryAndDateSorter : IComparer<FileModel>
{
    public int Compare(FileModel? x, FileModel? y)
    {
        var xDir = Path.GetDirectoryName(x!.Path) ?? string.Empty;
        var yDir = Path.GetDirectoryName(y!.Path) ?? string.Empty;

        var dirComparison = xDir.CompareTo(yDir);
        if (dirComparison != 0)
        {
            return dirComparison;
        }

        if (x.Datetime == y.Datetime)
        {
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
