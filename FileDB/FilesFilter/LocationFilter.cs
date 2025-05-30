﻿using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class LocationFilter(LocationForSearch? location) : IFilesFilter
{
    public bool CanRun() => location is not null;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithLocations([location!.Id]);
    }
}
