using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FilterPersonSex(Sex personSex) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesBySex(personSex);
    }
}
