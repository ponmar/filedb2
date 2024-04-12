﻿using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using FileDBAvalonia.Extensions;
using FileDBAvalonia.FilesFilter;
using FileDBAvalonia.Model;
using FileDBShared.Model;

namespace FileDBAvalonia.ViewModels.Search.Filters;

public partial class PersonSexViewModel : AbstractFilterViewModel
{
    public static IEnumerable<Sex> PersonSexValues { get; } = Enum.GetValues<Sex>().OrderBy(x => x.ToFriendlyString());

    [ObservableProperty]
    private Sex selectedPersonSex = PersonSexValues.First();

    public PersonSexViewModel() : base(FilterType.PersonSex)
    {
    }

    protected override IFilesFilter DoCreate() => new FilterPersonSex(SelectedPersonSex);
}
