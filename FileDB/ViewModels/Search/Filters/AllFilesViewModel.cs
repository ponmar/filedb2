using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.FilesFilter;
using System;

namespace FileDB.ViewModels.Search.Filters;

public partial class AllFilesViewModel : ObservableObject, IFilterViewModel
{
    public IFilesFilter CreateFilter()
    {
        return new AllFilesFilter();
    }
}
