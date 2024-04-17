using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FilterTime(TimeOnly start, TimeOnly end) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByTime(start, end);
    }
}
