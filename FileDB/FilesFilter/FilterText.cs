using FileDB.Extensions;
using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterText(string searchPattern) : IFilesFilter
{
    public bool CanRun() => searchPattern.HasContent();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFiles(searchPattern);
    }
}
