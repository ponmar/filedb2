using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using System.Collections.Generic;
using System.Linq;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class AnnualDateViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private bool annualDateIsRange = false;

    public static IEnumerable<int> Months { get; } = Enumerable.Range(1, 12);

    public static IEnumerable<int> Days { get; } = Enumerable.Range(1, 31);

    [ObservableProperty]
    private int selectedAnnualMonthStart = Months.First();

    [ObservableProperty]
    private int selectedAnnualMonthEnd = Months.First();

    [ObservableProperty]
    private int selectedAnnualDayStart = Days.First();

    [ObservableProperty]
    private int selectedAnnualDayEnd = Days.First();

    public IFilesFilter CreateFilter()
    {
        return AnnualDateIsRange ?
            new FilterAnnualDateRange(SelectedAnnualMonthStart, SelectedAnnualDayStart, SelectedAnnualMonthEnd, SelectedAnnualDayEnd) :
            new FilterAnnualDate(SelectedAnnualMonthStart, SelectedAnnualDayStart);
    }
}
