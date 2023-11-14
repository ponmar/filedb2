using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.FilesFilter;

public class WithoutDateTime : IFilesFilter
{
    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.GetFiles().Where(x => x.Datetime is null);
    }
}
