using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class DateViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    public partial bool DateIsRange { get; set; } = false;

    [ObservableProperty]
    public partial DateTimeOffset FirstDateTime { get; set; } = DateTime.Now;

    [ObservableProperty]
    public partial DateTimeOffset SecondDateTime { get; set; } = DateTime.Now;

    public static IReadOnlyList<FixedHoliday> SwedishFixedHolidays => SwedishHolidays.All;

    public bool CurrentFileHasDateTime => fileSelector.SelectedFile?.Datetime is not null;

    private readonly IFileSelector fileSelector;

    public DateViewModel(IFileSelector fileSelector)
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
        if (fileSelector.SelectedFile?.Datetime is not null)
        {
            FirstDateTime = DatabaseParsing.ParseFilesDatetime(fileSelector.SelectedFile.Datetime)!.Value;
        }
    }

    [RelayCommand]
    private void SetEndDateFromCurrentFile()
    {
        if (fileSelector.SelectedFile?.Datetime is not null)
        {
            SecondDateTime = DatabaseParsing.ParseFilesDatetime(fileSelector.SelectedFile.Datetime)!.Value;
        }
    }

    [RelayCommand]
    private void SetStartDateFromToday()
    {
        FirstDateTime = DateTime.Now;
    }

    [RelayCommand]
    private void SetEndDateFromToday()
    {
        SecondDateTime = DateTime.Now;
    }

    [RelayCommand]
    private void SetStartDateFromHoliday(FixedHoliday holiday)
    {
        FirstDateTime = new DateTime(FirstDateTime.Year, holiday.Month, holiday.Day);
    }

    [RelayCommand]
    private void SetEndDateFromHoliday(FixedHoliday holiday)
    {
        SecondDateTime = new DateTime(SecondDateTime.Year, holiday.Month, holiday.Day);
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        if (DateIsRange)
        {
            var start = FirstDateTime.Date < SecondDateTime.Date ? FirstDateTime.Date : SecondDateTime.Date;
            var end = FirstDateTime.Date < SecondDateTime.Date ? SecondDateTime.Date : FirstDateTime.Date;
            return dbAccess.SearchFilesByDate(start.Date, end.Date);
        }
        else
        {
            return dbAccess.SearchFilesByDate(FirstDateTime.Date);
        }
    }
}
