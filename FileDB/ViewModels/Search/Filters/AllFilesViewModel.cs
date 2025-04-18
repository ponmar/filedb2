using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class AllFilesViewModel : ObservableObject, IFilterViewModel
{
    public IFilesFilter CreateFilter()
    {
        return new AllFilesFilter();
    }
}
