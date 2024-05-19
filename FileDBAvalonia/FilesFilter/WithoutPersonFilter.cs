﻿using FileDBAvalonia.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class WithoutPersonFilter(PersonForSearch? person) : IFilesFilter
{
    public bool CanRun() => person is not null;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithoutPersons([person!.Id]);
    }
}