using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using System;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class DateViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private bool dateIsRange = false;

    [ObservableProperty]
    private DateTimeOffset firstDateTime = DateTime.Now;

    [ObservableProperty]
    private DateTimeOffset secondDateTime = DateTime.Now;

    public IFilesFilter CreateFilter()
    {
        return DateIsRange ?
            new DateRangeFilter(FirstDateTime, SecondDateTime) :
            new DateFilter(FirstDateTime);
    }
}
