using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Extensions;
using FileDB.Model;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonSexViewModel : ObservableValidator, IFilterViewModel
{
    public static IEnumerable<Sex> PersonSexValues { get; } = Enum.GetValues<Sex>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    public partial Sex SelectedPersonSex { get; set; } = PersonSexValues.First();

    public PersonSexViewModel()
    {
        ValidateAllProperties();
    }

    public IEnumerable<FileModel> ApplyFilter(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesBySex(SelectedPersonSex);
    }
}
