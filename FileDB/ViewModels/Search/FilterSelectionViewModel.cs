using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Extensions;
using FileDB.Model;
using FileDB.ViewModels.Search.Filters;

namespace FileDB.ViewModels.Search;

public enum CombineMethod { And, Or, Xor }

public static class CombineMethodExtensions
{
    public static string ToFriendlyString(this CombineMethod combineMethod)
    {
        return combineMethod switch
        {
            CombineMethod.And => "AND",
            CombineMethod.Or => "OR",
            CombineMethod.Xor => "XOR",
            _ => throw new NotImplementedException(),
        };
    }
}

public partial class FilterSelectionViewModel : ObservableObject
{
    public static IEnumerable<FilterType> FilterTypes { get; } = Enum.GetValues<FilterType>().OrderBy(x => x.ToFriendlyString(), StringComparer.Ordinal);

    [ObservableProperty]
    private FilterType selectedFilterType;

    public static IEnumerable<CombineMethod> CombineMethods { get; } = Enum.GetValues<CombineMethod>();

    [ObservableProperty]
    private CombineMethod selectedCombineMethod;

    partial void OnSelectedFilterTypeChanged(FilterType value)
    {
        FilterViewModel = CreateFilterFromType(value);
    }

    [ObservableProperty]
    private bool isFirstFilter;

    [ObservableProperty]
    private IFilterViewModel filterViewModel;

    public FilterSelectionViewModel(IConfigProvider configProvider)
    {
        SelectedFilterType = configProvider.Config.InitialSearchFilterType;
        FilterViewModel = CreateFilterFromType(SelectedFilterType);
    }

    private static IFilterViewModel CreateFilterFromType(FilterType filterType)
    {
        var viewModelType = filterType.GetAttribute<FilterTypeViewModelAttribute>().ViewModelType;
        return (IFilterViewModel)ServiceLocator.Resolve(viewModelType);
    }
}

