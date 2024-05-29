using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class NoDateTimeViewModel : ObservableObject, IFilterViewModel
{
    public IFilesFilter CreateFilter() => new WithoutDateTimeFilter();
}
