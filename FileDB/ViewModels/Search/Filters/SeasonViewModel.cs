using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class SeasonViewModel : ObservableValidator, IFilterViewModel
{
    public static IEnumerable<Season> Seasons { get; } = Enum.GetValues<Season>();

    [ObservableProperty]
    private Season selectedSeason = Seasons.First();

    public SeasonViewModel()
    {
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesBySeason(SelectedSeason);
    }
}
