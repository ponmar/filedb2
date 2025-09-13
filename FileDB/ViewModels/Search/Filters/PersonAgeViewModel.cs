using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonAgeViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    private string personAgeFrom = "1";

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    private string personAgeTo = "100";

    public PersonAgeViewModel()
    {
        ValidateAllProperties();
    }

    public IFilesFilter CreateFilter() => new PersonAgeFilter(PersonAgeFrom, PersonAgeTo);
}
