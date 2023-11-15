using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class Person(PersonModel person) : IFilesFilter
{
    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
    {
        return dbAccess.SearchFilesWithPersons(new List<int>() { person.Id });
    }
}
