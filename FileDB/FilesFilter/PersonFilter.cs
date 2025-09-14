using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class PersonFilter(PersonForSearch? person) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithPersons([person!.Id]);
    }
}
