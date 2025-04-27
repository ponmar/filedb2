using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class TimeFilter(TimeOnly start, TimeOnly end) : IFilesFilter
{
    public bool CanRun() => start <= end;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByTime(start, end);
    }
}
