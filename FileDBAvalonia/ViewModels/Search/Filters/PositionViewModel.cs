using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class PositionViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private string positionText = string.Empty;

    [ObservableProperty]
    private string radiusText = "500";

    public PositionViewModel() : base(FilterType.Position)
    {
    }

    protected override IFilesFilter DoCreate() => new FilterPosition(PositionText, RadiusText);
}
