using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;

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

    public IFilesFilter CreateFilter() => new NumberOfPersonsFilter(NumPersonsMin, NumPersonsMax);
}
