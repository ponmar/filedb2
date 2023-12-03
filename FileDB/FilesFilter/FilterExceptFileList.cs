using FileDB.Extensions;
using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.FilesFilter;

public class FilterExceptFileList(string exceptFileIdsText) : IFilesFilter
{
    public bool CanRun() => exceptFileIdsText.HasContent();

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        var fileIds = Utils.CreateFileIds(exceptFileIdsText);
        var allFiles = dbAccess.GetFiles();
        return allFiles.Where(x => !fileIds.Contains(x.Id));
    }
}
