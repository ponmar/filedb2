using FileDBAvalonia.ViewModels;
using FileDBInterface.DatabaseAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public class FilterPerson(PersonForSearch? person) : IFilesFilter
{
    public bool CanRun() => person is not null;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithPersons([person!.Id]);
    }
}
