using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using FileDBInterface.DatabaseAccess;
using System.Collections.Generic;
using System;
using System.Linq;

namespace FileDB.ViewModels.Search.Filters;

public partial class SeasonViewModel : ObservableObject, IFilterViewModel
{
    public static IEnumerable<Season> Seasons { get; } = Enum.GetValues<Season>();

    [ObservableProperty]
    private Season selectedSeason = Seasons.First();

    public IFilesFilter CreateFilter() => new SeasonFilter(SelectedSeason);
}
