using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterPersonSex(Sex personSex) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.SearchFilesBySex(personSex);
    }
}
