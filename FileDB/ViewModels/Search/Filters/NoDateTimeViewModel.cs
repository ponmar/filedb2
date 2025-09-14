using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class NoDateTimeViewModel : ObservableValidator, IFilterViewModel
{
    public NoDateTimeViewModel()
    {
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithoutDate();
    }
}
