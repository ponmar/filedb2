using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class FileListViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private string fileListIds = string.Empty;

    [ObservableProperty]
    private bool negate;

    public IFilesFilter CreateFilter() =>
        Negate ? new ExceptFileListFilter(FileListIds) : new FileListFilter(FileListIds);
}
