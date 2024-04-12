using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public interface IFilterViewModel
{
    IEnumerable<FilterType> FilterTypes { get; }

    FilterType SelectedFilterType { get; }

    IFilesFilter Create();
}

public abstract partial class AbstractFilterViewModel : ObservableObject, IFilterViewModel
{
    private readonly FilterType filterType;

    public IEnumerable<FilterType> FilterTypes { get; } = Enum.GetValues<FilterType>().OrderBy(x => x.ToFriendlyString(), StringComparer.Ordinal);

    [ObservableProperty]
    private FilterType selectedFilterType;

    partial void OnSelectedFilterTypeChanged(FilterType value)
    {
        if (value != filterType)
        {
            Messenger.Send(new SearchFilterSelectionChanged(this, value));
        }
    }

    public IFilesFilter Create() => DoCreate();

    protected abstract IFilesFilter DoCreate();

    protected AbstractFilterViewModel(FilterType filterType)
    {
        this.filterType = filterType;
        SelectedFilterType = filterType;
    }
}
