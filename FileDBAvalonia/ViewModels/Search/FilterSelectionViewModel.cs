using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.Extensions;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels.Search.Filters;

namespace FileDBAvalonia.ViewModels.Search;

public partial class FilterSelectionViewModel : ObservableObject
{
    public static IEnumerable<FilterType> FilterTypes { get; } = Enum.GetValues<FilterType>().OrderBy(x => x.ToFriendlyString(), StringComparer.Ordinal);

    [ObservableProperty]
    private FilterType selectedFilterType;

    partial void OnSelectedFilterTypeChanged(FilterType value)
    {
        FilterViewModel = CreateFilterFromType(value);
    }

    [ObservableProperty]
    private IFilterViewModel filterViewModel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public FilterSelectionViewModel()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
        SelectedFilterType = FilterType.Text;
    }

    private static IFilterViewModel CreateFilterFromType(FilterType filterType)
    {
        var viewModelType = filterType.GetAttribute<FilterTypeViewModelAttribute>().ViewModelType;
        return (IFilterViewModel)ServiceLocator.Resolve(viewModelType);
    }
}

