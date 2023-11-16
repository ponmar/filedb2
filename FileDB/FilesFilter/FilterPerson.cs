using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterPerson(PersonModel? person) : IFilesFilter
{
    public bool CanRun() => person is not null;

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.SearchFilesWithPersons(new List<int>() { person!.Id });
    }
}
