using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class NumPersonsViewModel : ObservableValidator, IFilterViewModel
{
    [ObservableProperty]
    private int numPersonsMin = 1;

    partial void OnNumPersonsMinChanged(int value)
    {
        if (value > NumPersonsMax)
        {
            NumPersonsMax = value;
        }
    }

    [ObservableProperty]
    private int numPersonsMax = 1;

    partial void OnNumPersonsMaxChanged(int value)
    {
        if (value < NumPersonsMin)
        {
            NumPersonsMin = value;
        }
    }

    public NumPersonsViewModel()
    {
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByNumPersons(new Range(NumPersonsMin, NumPersonsMax));
    }
}
