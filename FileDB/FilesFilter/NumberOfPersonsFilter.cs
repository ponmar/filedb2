using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class NumberOfPersonsFilter(int minNumPersons, int maxNumPersons) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByNumPersons(new Range(minNumPersons, maxNumPersons));
    }
}
