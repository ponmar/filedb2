using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class RandomViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private string numRandomFiles = "10";

    public IFilesFilter CreateFilter()
    {
        _ = int.TryParse(NumRandomFiles, out var value);
        return new RandomFilter(value);
    }
}
