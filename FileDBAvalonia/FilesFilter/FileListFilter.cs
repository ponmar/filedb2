using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FileListFilter(string fileIdsText) : IFilesFilter
{
    public bool CanRun() => fileIdsText.HasContent();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var fileIds = Utils.CreateFileIds(fileIdsText);
        return dbAccess.SearchFilesFromIds(fileIds);
    }
}
