using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Extensions;
using FileDB.FilesFilter;
using FileDB.Model;
using FileDBInterface.Model;

namespace FileDB.ViewModels.Search.Filters;

public partial class PersonSexViewModel : ObservableObject, IFilterViewModel
{
    public static IEnumerable<Sex> PersonSexValues { get; } = Enum.GetValues<Sex>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private Sex selectedPersonSex = PersonSexValues.First();

    public IFilesFilter CreateFilter() => new PersonSexFilter(SelectedPersonSex);
}
