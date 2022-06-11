﻿using System.Collections.Generic;
using FileDB.ViewModel;
using FileDBInterface.Model;

namespace FileDB.Sorters
{
    public class LocationModelByNameSorter : IComparer<LocationModel>
    {
        public int Compare(LocationModel x, LocationModel y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}