﻿using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class Text(string searchPattern) : IFilesFilter
{
    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.SearchFiles(searchPattern);
    }
}
