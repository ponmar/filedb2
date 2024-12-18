﻿using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Sorters;

public class PersonModelByNameSorter : IComparer<PersonModel>
{
    public int Compare(PersonModel? x, PersonModel? y)
    {
        var xName = $"{x!.Firstname} {x.Lastname}";
        var yName = $"{y!.Firstname} {y.Lastname}";

        return xName.CompareTo(yName);
    }
}
