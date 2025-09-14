using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class RandomViewModel : ObservableValidator, IFilterViewModel
{
    private const int DefaultNumRandomFiles = 10;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Range(1, 1000)]
    private int numRandomFiles = DefaultNumRandomFiles;

    public RandomViewModel()
    {
        ValidateAllProperties();
    }

    public IFilesFilter CreateFilter()
    {
        return new RandomFilter(NumRandomFiles);
    }
}
