using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Sorters;

public class FileModelByPathSorter : IComparer<FileModel>
{
    public int Compare(FileModel? x, FileModel? y)
    {
        return x!.Path.CompareTo(y!.Path);
    }
}
