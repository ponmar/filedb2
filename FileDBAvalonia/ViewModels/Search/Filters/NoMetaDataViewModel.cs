using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class NoMetaDataViewModel : ObservableObject, IFilterViewModel
{
    public IFilesFilter CreateFilter() => new WithoutMetaDataFilter();
}
