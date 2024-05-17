using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class TimeFilter(TimeOnly start, TimeOnly end) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByTime(start, end);
    }
}
