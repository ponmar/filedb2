using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class NumPersonsViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private string numPersonsMin = "1";

    [ObservableProperty]
    private string numPersonsMax = "1";

    public IFilesFilter CreateFilter() => new NumberOfPersonsFilter(NumPersonsMin, NumPersonsMax);
}
