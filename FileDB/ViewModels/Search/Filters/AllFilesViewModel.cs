using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class AllFilesViewModel : ObservableValidator, IFilterViewModel
{
    public AllFilesViewModel()
    {
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.GetFiles();
    }
}
