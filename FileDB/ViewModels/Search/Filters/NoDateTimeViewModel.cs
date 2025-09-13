using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class NoDateTimeViewModel : ObservableValidator, IFilterViewModel
{
    public NoDateTimeViewModel()
    {
        ValidateAllProperties();
    }

    public IFilesFilter CreateFilter() => new WithoutDateTimeFilter();
}
