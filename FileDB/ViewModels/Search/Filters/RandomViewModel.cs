using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class RandomViewModel : ObservableValidator, IFilterViewModel
{
    private const int DefaultNumRandomFiles = 10;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    private string numRandomFiles = DefaultNumRandomFiles.ToString();

    public RandomViewModel()
    {
        ValidateAllProperties();
    }

    public IFilesFilter CreateFilter()
    {
        _ = int.TryParse(NumRandomFiles, out var value);
        return new RandomFilter(value);
    }
}
