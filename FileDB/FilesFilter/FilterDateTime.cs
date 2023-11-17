using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterDateTime(DateTime start, DateTime end) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.SearchFilesByDate(start.Date, end.Date);
    }
}
