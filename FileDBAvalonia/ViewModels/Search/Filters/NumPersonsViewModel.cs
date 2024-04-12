using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class NumPersonsViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private string numPersonsMin = "1";

    [ObservableProperty]
    private string numPersonsMax = "1";

    public NumPersonsViewModel() : base(FilterType.NumPersons)
    {
    }

    protected override IFilesFilter DoCreate() => new FilterNumberOfPersons(NumPersonsMin, NumPersonsMax);
}
