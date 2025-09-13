using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class NoMetaDataViewModel : ObservableValidator, IFilterViewModel
{
    public NoMetaDataViewModel()
    {
        ValidateAllProperties();
    }

    public IFilesFilter CreateFilter() => new WithoutMetaDataFilter();
}
