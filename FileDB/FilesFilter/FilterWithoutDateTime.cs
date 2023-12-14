using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterWithoutDateTime : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.SearchFilesWithoutDate();
    }
}
