using FileDB.ViewModels.Search.Filters;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.FilesFilter;

public class PersonGroupFilter(IEnumerable<PersonForSearch> persons, bool allowOtherPersons) : IFilesFilter
{
    public bool CanRun() => persons.Any();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        // TODO: how did the previous implementation search?
        return dbAccess.SearchFilesWithPersons(persons.Select(x => x.Id), allowOtherPersons);
    }
}
