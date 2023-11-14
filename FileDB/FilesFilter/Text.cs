using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class Text : IFilesFilter
{
    public required string SearchPattern { get; init; }

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.SearchFiles(SearchPattern);
    }
}
