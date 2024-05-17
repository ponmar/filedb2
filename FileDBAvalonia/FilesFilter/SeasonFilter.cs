using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class SeasonFilter(Season? season) : IFilesFilter
{
    public bool CanRun() => season is not null;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesBySeason(season!.Value);
    }
}
