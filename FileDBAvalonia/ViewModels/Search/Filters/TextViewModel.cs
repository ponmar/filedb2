using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class TextViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private string textFilterSearchPattern = string.Empty;

    [ObservableProperty]
    private bool textFilterCaseSensitive = false;

    public IFilesFilter CreateFilter() => new FilterText(TextFilterSearchPattern, TextFilterCaseSensitive);
}
