using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBShared.Model;
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
    private int selectedAnnualDayStart = Days.First();

    [ObservableProperty]
    private int selectedAnnualMonthEnd = Months.First();

    [ObservableProperty]
    private int selectedAnnualDayEnd = Days.First();

    public AnnualDateViewModel(IFileSelector fileSelector)
    {
        var currentFileDateTime = DatabaseParsing.ParseFilesDatetime(fileSelector.SelectedFile?.Datetime);
        if (currentFileDateTime is not null)
        {
            selectedAnnualMonthStart = currentFileDateTime.Value.Month;
            selectedAnnualDayStart = currentFileDateTime.Value.Day;

            selectedAnnualMonthEnd = currentFileDateTime.Value.Month;
            selectedAnnualDayEnd = currentFileDateTime.Value.Day;
        }
    }

    public IFilesFilter CreateFilter()
    {
        return AnnualDateIsRange ?
            new AnnualDateRangeFilter(SelectedAnnualMonthStart, SelectedAnnualDayStart, SelectedAnnualMonthEnd, SelectedAnnualDayEnd) :
            new AnnualDateFilter(SelectedAnnualMonthStart, SelectedAnnualDayStart);
    }
}
