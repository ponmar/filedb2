using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class ExceptFileListFilter(string exceptFileIdsText) : IFilesFilter
{
    public bool CanRun() => exceptFileIdsText.HasContent();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var fileIds = Utils.CreateFileIds(exceptFileIdsText);
        return dbAccess.SearchFilesExceptIds(fileIds);
    }
}
