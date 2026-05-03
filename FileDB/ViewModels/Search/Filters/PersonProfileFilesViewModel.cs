using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonProfileFilesViewModel : ObservableValidator, IFilterViewModel
{
    private readonly IPersonsRepository personsRepository;

    public PersonProfileFilesViewModel(IPersonsRepository personsRepository)
    {
        this.personsRepository = personsRepository;
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        var personProfileFileIds = personsRepository.Persons.Where(x => x.ProfileFileId is not null).Select(x => x.ProfileFileId!.Value);
        return dbAccess.SearchFilesFromIds(personProfileFileIds);
    }
}
