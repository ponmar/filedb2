using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class TextViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private string textFilterSearchPattern = string.Empty;

    [ObservableProperty]
    private bool textFilterCaseSensitive = false;

    [ObservableProperty]
    private bool textFilterPersons;

    [ObservableProperty]
    private bool textFilterLocations;

    [ObservableProperty]
    private bool textFilterTags;

    public IFilesFilter CreateFilter() => new TextFilter(TextFilterSearchPattern, TextFilterCaseSensitive, TextFilterPersons, TextFilterLocations, TextFilterTags);
}
