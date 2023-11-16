using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.FilesFilter;

public class FilterWithoutDateTime : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.GetFiles().Where(x => x.Datetime is null);
    }
}
