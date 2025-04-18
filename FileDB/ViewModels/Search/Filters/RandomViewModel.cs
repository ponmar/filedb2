using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class RandomViewModel : ObservableObject, IFilterViewModel
{
    private const int DefaultNumRandomFiles = 10;

    [ObservableProperty]
    private string numRandomFiles = DefaultNumRandomFiles.ToString();

    public IFilesFilter CreateFilter()
    {
        _ = int.TryParse(NumRandomFiles, out var value);
        return new RandomFilter(value);
    }
}
