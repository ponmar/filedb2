﻿using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.FilesFilter;
using FileDBShared.Model;
using System;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class TimeViewModel : ObservableObject, IFilterViewModel
{
    [ObservableProperty]
    private TimeSpan startTime;

    [ObservableProperty]
    private TimeSpan endTime;

    public TimeViewModel(IFileSelector fileSelector)
    {
        var currentFileDateTime = DatabaseParsing.ParseFilesDatetime(fileSelector.SelectedFile?.Datetime);
        if (currentFileDateTime is not null)
        {
            StartTime = currentFileDateTime.Value.TimeOfDay;
            EndTime = currentFileDateTime.Value.TimeOfDay;
        }
    }

    public IFilesFilter CreateFilter()
    {
        return new FilterTime(TimeOnly.FromTimeSpan(StartTime), TimeOnly.FromTimeSpan(EndTime));
    }
}
