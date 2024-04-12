using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class NoMetaDataViewModel : AbstractFilterViewModel
{
    public NoMetaDataViewModel() : base(FilterType.NoMetaData)
    {
    }

    protected override IFilesFilter DoCreate() => new FilterWithoutMetaData();
}
