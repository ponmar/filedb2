using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class RandomViewModel : ObservableValidator, IFilterViewModel
{
    private const int DefaultNumRandomFiles = 10;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [Range(1, 1000, ErrorMessage = "Must be between 1 and 1000")]
    private int numRandomFiles = DefaultNumRandomFiles;

    public RandomViewModel()
    {
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesRandom(NumRandomFiles);
    }
}
