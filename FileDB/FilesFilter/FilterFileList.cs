﻿using FileDB.Extensions;
using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterFileList(string fileIdsText) : IFilesFilter
{
    public bool CanRun() => fileIdsText.HasContent();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var fileIds = Utils.CreateFileIds(fileIdsText);
        return dbAccess.SearchFilesFromIds(fileIds);
    }
}
