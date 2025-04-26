using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.FilesFilter;
using FileDB.Model;
using FileDBInterface.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FileDB.ViewModels.Search.Filters;

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

    public bool CurrentFileHasDateTime => fileSelector.SelectedFile?.Datetime is not null;

    private readonly IFileSelector fileSelector;

    public AnnualDateViewModel(IFileSelector fileSelector)
    {
        this.fileSelector = fileSelector;
        this.RegisterForEvent<FileSelectionChanged>(x => OnPropertyChanged(nameof(CurrentFileHasDateTime)));

        if (CurrentFileHasDateTime)
        {
            SetStartDateFromCurrentFile();
            SetEndDateFromCurrentFile();
        }
        else
        {
            SetStartDateFromToday();
            SetEndDateFromToday();
        }
    }

    [RelayCommand]
    private void SetStartDateFromCurrentFile()
    {
        var date = DatabaseParsing.ParseFilesDatetime(fileSelector.SelectedFile?.Datetime);
        if (date is not null)
        {
            SelectedAnnualMonthStart = date.Value.Month;
            SelectedAnnualDayStart = date.Value.Day;
        }
    }

    [RelayCommand]
    private void SetEndDateFromCurrentFile()
    {
        var date = DatabaseParsing.ParseFilesDatetime(fileSelector.SelectedFile?.Datetime);
        if (date is not null)
        {
            SelectedAnnualMonthEnd = date.Value.Month;
            SelectedAnnualDayEnd = date.Value.Day;
        }
    }

    [RelayCommand]
    private void SetStartDateFromToday()
    {
        var today = DateTime.Now;
        SelectedAnnualMonthStart = today.Month;
        SelectedAnnualDayStart = today.Day;
    }

    [RelayCommand]
    private void SetEndDateFromToday()
    {
        var today = DateTime.Now;
        SelectedAnnualMonthEnd = today.Month;
        SelectedAnnualDayEnd = today.Day;
    }

    public IFilesFilter CreateFilter()
    {
        return AnnualDateIsRange ?
            new AnnualDateRangeFilter(SelectedAnnualMonthStart, SelectedAnnualDayStart, SelectedAnnualMonthEnd, SelectedAnnualDayEnd) :
            new AnnualDateFilter(SelectedAnnualMonthStart, SelectedAnnualDayStart);
    }
}
