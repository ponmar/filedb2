using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class NoDateTimeViewModel : AbstractFilterViewModel
{
    public NoDateTimeViewModel() : base(FilterType.NoDateTime)
    {
    }

    protected override IFilesFilter DoCreate() => new FilterWithoutDateTime();
}
