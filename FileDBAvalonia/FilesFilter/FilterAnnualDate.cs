using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FilterAnnualDate(int startMonth, int startDay, int endMonth, int endDay) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByAnnualDate(startMonth, startDay, endMonth, endDay);
    }
}
