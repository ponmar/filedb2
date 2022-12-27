using System.Collections.Generic;
using FileDBShared.Model;

namespace FileDB.Sorters;

public class FilesModelByPathSorter : IComparer<FilesModel>
{
    public int Compare(FilesModel? x, FilesModel? y)
    {
        return x!.Path.CompareTo(y!.Path);
    }
}
