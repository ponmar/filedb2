using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class NoMetaDataViewModel : ObservableValidator, IFilterViewModel
{
    public NoMetaDataViewModel()
    {
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesWithMissingData();
    }
}
