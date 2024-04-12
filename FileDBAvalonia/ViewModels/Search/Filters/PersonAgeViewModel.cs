using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class PersonAgeViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private string personAgeFrom = string.Empty;

    [ObservableProperty]
    private string personAgeTo = string.Empty;

    public PersonAgeViewModel() : base(FilterType.PersonAge)
    {
    }

    protected override IFilesFilter DoCreate() => new FilterPersonAge(PersonAgeFrom, PersonAgeTo);
}
