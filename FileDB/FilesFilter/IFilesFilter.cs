using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public interface IFilesFilter
{
    public IEnumerable<FileModel> Run(IDbAccess dbAccess);
}
