using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonAgeViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private string personAgeFrom = string.Empty;

    [ObservableProperty]
    private string personAgeTo = string.Empty;

    public IFilesFilter CreateFilter() => new PersonAgeFilter(PersonAgeFrom, PersonAgeTo);
}
