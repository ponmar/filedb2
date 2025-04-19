using System;
using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Sorters;

public class FileModelByRandomSorter : IComparer<FileModel>
{
    private static readonly Random random = new();

    public int Compare(FileModel? x, FileModel? y)
    {
        return random.Next(2) == 0 ? -1 : 1;
    }
}
