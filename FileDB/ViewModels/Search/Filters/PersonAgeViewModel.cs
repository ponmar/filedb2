using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonAgeViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [Range(0, 200, ErrorMessage = "Must be between 0 and 200")]
    private int personAgeFrom = 1;

    partial void OnPersonAgeFromChanged(int value)
    {
        if (value > personAgeTo)
        {
            PersonAgeTo = value;
        }
    }

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [Range(0, 200, ErrorMessage = "Must be between 0 and 200")]
    private int personAgeTo = 100;

    partial void OnPersonAgeToChanged(int value)
    {
        if (value < personAgeFrom)
        {
            PersonAgeFrom = value;
        }
    }

    public PersonAgeViewModel()
    {
        ValidateAllProperties();
    }

    public IFilesFilter CreateFilter() => new PersonAgeFilter(PersonAgeFrom, PersonAgeTo);
}
