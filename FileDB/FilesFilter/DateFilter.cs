﻿using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class DateFilter(DateTimeOffset date) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByDate(date.Date);
    }
}
