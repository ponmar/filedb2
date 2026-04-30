using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Sorters;

public class FileModelByPersonCountSorter : IComparer<FileModel>
{
    private readonly Dictionary<int, int> personCounts;

    public FileModelByPersonCountSorter(Dictionary<int, int> personCounts)
    {
        this.personCounts = personCounts;
    }

    public int Compare(FileModel? x, FileModel? y)
    {
        var xCount = personCounts.GetValueOrDefault(x!.Id, 0);
        var yCount = personCounts.GetValueOrDefault(y!.Id, 0);
        return xCount != yCount ? xCount.CompareTo(yCount) : x.Path.CompareTo(y.Path);
    }
}
