using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class NumPersonsViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    private string numPersonsMin = "1";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    private string numPersonsMax = "1";

    public NumPersonsViewModel()
    {
        ValidateAllProperties();
    }

    public IFilesFilter CreateFilter() => new NumberOfPersonsFilter(NumPersonsMin, NumPersonsMax);
}
