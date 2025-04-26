using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.FilesFilter;
using FileDB.Model;
using FileDBInterface.Model;
using System;

namespace FileDB.ViewModels.Search.Filters;

public partial class TimeViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private TimeSpan startTime;

    [ObservableProperty]
    private TimeSpan endTime;

    public bool CurrentFileHasTime => fileSelector.SelectedFile?.Datetime is not null && fileSelector.SelectedFile?.Datetime.Contains('T') == true;

    private readonly IFileSelector fileSelector;

    public TimeViewModel(IFileSelector fileSelector)
    {
        this.fileSelector = fileSelector;
        this.RegisterForEvent<FileSelectionChanged>(x => OnPropertyChanged(nameof(CurrentFileHasTime)));

        if (CurrentFileHasTime)
        {
            SetStartTimeFromCurrentFile();
            SetEndTimeFromCurrentFile();
        }
        else
        {
            SetStartTimeFromNow();
            SetEndTimeFromNow();
        }
    }

    [RelayCommand]
    private void SetStartTimeFromNow()
    {
        StartTime = DateTime.Now.TimeOfDay;
    }

    [RelayCommand]
    private void SetEndTimeFromNow()
    {
        EndTime = DateTime.Now.TimeOfDay;
    }

    [RelayCommand]
    private void SetStartTimeFromCurrentFile()
    {
        var time = DatabaseParsing.ParseFileTime(fileSelector.SelectedFile?.Datetime);
        if (time is not null)
        {
            StartTime = time.Value.ToTimeSpan();
        }
    }

    [RelayCommand]
    private void SetEndTimeFromCurrentFile()
    {
        var time = DatabaseParsing.ParseFileTime(fileSelector.SelectedFile?.Datetime);
        if (time is not null)
        {
            EndTime = time.Value.ToTimeSpan();
        }
    }

    public IFilesFilter CreateFilter()
    {
        return new TimeFilter(TimeOnly.FromTimeSpan(StartTime), TimeOnly.FromTimeSpan(EndTime));
    }
}
