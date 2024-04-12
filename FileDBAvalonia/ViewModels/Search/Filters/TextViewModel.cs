using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class TextViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private string textFilterSearchPattern = string.Empty;

    [ObservableProperty]
    private bool textFilterCaseSensitive = false;

    public TextViewModel() : base(FilterType.Text)
    {
    }

    protected override IFilesFilter DoCreate() => new FilterText(TextFilterSearchPattern, TextFilterCaseSensitive);
}
