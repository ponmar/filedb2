﻿using FileDB.ViewModels.Search;
using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.FilesFilter;

public class TagsFilter(IEnumerable<TagForSearch> tags, bool allowOtherTags) : IFilesFilter
{
    public bool CanRun() => tags.Any();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithTags(tags.Select(x => x.Id), allowOtherTags);
    }
}
