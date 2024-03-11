using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FilterWithoutMetaData : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithMissingData();
    }
}
