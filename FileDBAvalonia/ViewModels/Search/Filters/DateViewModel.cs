using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;
using System;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class DateViewModel : AbstractFilterViewModel
{
    [ObservableProperty]
    private bool dateIsRange = false;

    [ObservableProperty]
    private DateTimeOffset firstDateTime = DateTime.Now;

    [ObservableProperty]
    private DateTimeOffset secondDateTime = DateTime.Now;

    public DateViewModel() : base(FilterType.Date)
    {
    }

    protected override IFilesFilter DoCreate()
    {
        return DateIsRange ?
            new FilterDateRange(FirstDateTime, SecondDateTime) :
            new FilterDate(FirstDateTime);
    }
}
