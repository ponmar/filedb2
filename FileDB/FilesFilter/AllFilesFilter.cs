using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class AllFilesFilter() : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.GetFiles();
    }
}
