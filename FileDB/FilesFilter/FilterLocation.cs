﻿using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterLocation(LocationModel? location) : IFilesFilter
{
    public bool CanRun() => location is not null;

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.SearchFilesWithLocations(new List<int>() { location!.Id });
    }
}