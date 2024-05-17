﻿using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBAvalonia.Sorters;

public class TagModelByNameSorter : IComparer<TagModel>
{
    public int Compare(TagModel? x, TagModel? y)
    {
        return x!.Name.CompareTo(y!.Name);
    }
}
