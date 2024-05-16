using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.Extensions;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels.Search.Filters;

namespace FileDBAvalonia.ViewModels.Search;

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

