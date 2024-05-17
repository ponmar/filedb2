﻿using FileDBAvalonia.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class TagFilter(TagForSearch? tag) : IFilesFilter
{
    public bool CanRun() => tag is not null;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithTags([tag!.Id]);
    }
}
