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
    public partial FilterType SelectedFilterType { get; set; }
    public static IEnumerable<CombineMethod> CombineMethods { get; } = Enum.GetValues<CombineMethod>();

    [ObservableProperty]
    public partial CombineMethod SelectedCombineMethod { get; set; }

    partial void OnSelectedFilterTypeChanged(FilterType value)
    {
        FilterViewModel = CreateFilterFromType(value);
        FilterViewModel.ErrorsChanged += (s, e) => Messenger.Send<FilterErrorsUpdated>();
        Messenger.Send<FilterErrorsUpdated>();
    }

    [ObservableProperty]
    public partial bool IsFirstFilter { get; set; }

    [ObservableProperty]
    public partial IFilterViewModel FilterViewModel { get; set; }

    public FilterSelectionViewModel(IConfigProvider configProvider)
    {
        SelectedFilterType = configProvider.Config.InitialSearchFilterType;
        FilterViewModel = CreateFilterFromType(SelectedFilterType);
        FilterViewModel.ErrorsChanged += (s, e) => Messenger.Send<FilterErrorsUpdated>();
    }

    private static IFilterViewModel CreateFilterFromType(FilterType filterType)
    {
        var viewModelType = filterType.GetAttribute<FilterTypeViewModelAttribute>().ViewModelType;
        return (IFilterViewModel)ServiceLocator.Resolve(viewModelType);
    }
}

