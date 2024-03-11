using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FilterDateTime(DateTime start, DateTime end) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByDate(start.Date, end.Date);
    }
}
