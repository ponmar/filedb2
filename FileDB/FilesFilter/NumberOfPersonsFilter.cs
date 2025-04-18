using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class NumberOfPersonsFilter(string minText, string maxText) : IFilesFilter
{
    public bool CanRun() => 
        int.TryParse(minText, out var min) &&
        int.TryParse(maxText, out var max) &&
        min >= 0 && min <= max;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var min = int.Parse(minText);
        var max = int.Parse(maxText);
        return dbAccess.SearchFilesByNumPersons(new Range(min, max));
    }
}
