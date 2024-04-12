using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;
using FileDBInterface.DatabaseAccess;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class SeasonViewModel : AbstractFilterViewModel
{
    public static IEnumerable<Season> Seasons { get; } = Enum.GetValues<Season>();

    [ObservableProperty]
    private Season selectedSeason = Seasons.First();

    public SeasonViewModel() : base(FilterType.Season)
    {
    }

    protected override IFilesFilter DoCreate() => new FilterSeason(SelectedSeason);
}
