using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.FilesFilter;
using FileDB.Model;
using FileDBInterface.Model;
using System;

namespace FileDB.ViewModels.Search.Filters;

public partial class DateViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private bool dateIsRange = false;

    [ObservableProperty]
    private DateTimeOffset firstDateTime = DateTime.Now;

    [ObservableProperty]
    private DateTimeOffset secondDateTime = DateTime.Now;

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

    public IFilesFilter CreateFilter()
    {
        return DateIsRange ?
            new DateRangeFilter(FirstDateTime, SecondDateTime) :
            new DateFilter(FirstDateTime);
    }
}
