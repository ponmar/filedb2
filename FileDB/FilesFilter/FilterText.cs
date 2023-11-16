using FileDB.Extensions;
using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterText(string searchPattern) : IFilesFilter
{
    public bool CanRun() => searchPattern.HasContent();

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.SearchFiles(searchPattern);
    }
}
