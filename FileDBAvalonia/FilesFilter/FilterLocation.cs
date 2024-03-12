using FileDBAvalonia.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FilterLocation(LocationForSearch? location) : IFilesFilter
{
    public bool CanRun() => location is not null;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithLocations([location!.Id]);
    }
}
