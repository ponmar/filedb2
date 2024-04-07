using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FilterText(string searchPattern, bool caseSensitive) : IFilesFilter
{
    public bool CanRun() => searchPattern.HasContent();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFiles(searchPattern, caseSensitive);
    }
}
