using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class DateRangeFilter(DateTimeOffset first, DateTimeOffset second) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var start = first < second ? first : second;
        var end = first < second ? second : first;
        return dbAccess.SearchFilesByDate(start.Date, end.Date);
    }
}
