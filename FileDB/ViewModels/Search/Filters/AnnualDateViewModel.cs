using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Infrastructure;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class AnnualDateViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    public partial bool AnnualDateIsRange { get; set; } = false;
    public static IEnumerable<int> Months { get; } = Enumerable.Range(1, 12);

    public static IEnumerable<int> Days { get; } = Enumerable.Range(1, 31);

    [ObservableProperty]
    [Required(ErrorMessage = "Required")]
    public partial int SelectedAnnualMonthStart { get; set; } = Months.First();

    partial void OnSelectedAnnualMonthStartChanged(int value)
    {
        if (value == SelectedAnnualMonthEnd)
        {
            if (SelectedAnnualDayStart > SelectedAnnualDayEnd)
            {
                SelectedAnnualDayEnd = SelectedAnnualDayStart;
            }
        }
        else if (value > SelectedAnnualMonthEnd)
        {
            SelectedAnnualDayEnd = SelectedAnnualDayStart;
            SelectedAnnualMonthEnd = value;
        }
    }

    [ObservableProperty]
    [Required(ErrorMessage = "Required")]
    public partial int SelectedAnnualDayStart { get; set; } = Days.First();

    partial void OnSelectedAnnualDayStartChanged(int value)
    {
        if (SelectedAnnualMonthStart == SelectedAnnualMonthEnd &&
            value > SelectedAnnualDayEnd)
        {
            SelectedAnnualDayEnd = value;
        }
    }

    [ObservableProperty]
    [Required(ErrorMessage = "Required")]
    public partial int SelectedAnnualMonthEnd { get; set; } = Months.First();

    partial void OnSelectedAnnualMonthEndChanged(int value)
    {
        if (value == SelectedAnnualMonthStart)
        {
            if (SelectedAnnualDayStart > SelectedAnnualDayEnd)
            {
                SelectedAnnualDayStart = SelectedAnnualDayEnd;
            }
        }
        else if (value < SelectedAnnualMonthStart)
        {
            SelectedAnnualDayStart = SelectedAnnualDayEnd;
            SelectedAnnualMonthStart = value;
        }
    }

    [ObservableProperty]
    [Required(ErrorMessage = "Required")]
    public partial int SelectedAnnualDayEnd { get; set; } = Days.First();

    partial void OnSelectedAnnualDayEndChanged(int value)
    {
        if (SelectedAnnualMonthStart == SelectedAnnualMonthEnd &&
            value < SelectedAnnualDayStart)
        {
            SelectedAnnualDayStart = value;
        }
    }

    public static IReadOnlyList<FixedHoliday> SwedishFixedHolidays => SwedishHolidays.All;

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

        ValidateAllProperties();
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

    [RelayCommand]
    private void SetStartDateFromHoliday(FixedHoliday holiday)
    {
        SelectedAnnualMonthStart = holiday.Month;
        SelectedAnnualDayStart = holiday.Day;
    }

    [RelayCommand]
    private void SetEndDateFromHoliday(FixedHoliday holiday)
    {
        SelectedAnnualMonthEnd = holiday.Month;
        SelectedAnnualDayEnd = holiday.Day;
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        return AnnualDateIsRange ?
            dbAccess.SearchFilesByAnnualDate(SelectedAnnualMonthStart, SelectedAnnualDayStart, SelectedAnnualMonthEnd, SelectedAnnualDayEnd) :
            dbAccess.SearchFilesByAnnualDate(SelectedAnnualMonthStart, SelectedAnnualDayStart);
    }
}
