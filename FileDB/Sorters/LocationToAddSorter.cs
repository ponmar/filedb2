using System.Collections.Generic;
using FileDB.ViewModel;

namespace FileDB.Sorters
{
    public class LocationToAddSorter : IComparer<LocationToUpdate>
    {
        public int Compare(LocationToUpdate x, LocationToUpdate y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
