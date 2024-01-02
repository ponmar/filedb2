using FileDB.Extensions;
using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterExceptFileList(string exceptFileIdsText) : IFilesFilter
{
    public bool CanRun() => exceptFileIdsText.HasContent();

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        var fileIds = Utils.CreateFileIds(exceptFileIdsText);
        return dbAccess.SearchFilesExceptIds(fileIds);
    }
}
