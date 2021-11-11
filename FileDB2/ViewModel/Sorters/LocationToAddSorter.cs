using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB2Browser.ViewModel.Sorters
{
    public class LocationToAddSorter : IComparer<LocationToUpdate>
    {
        public int Compare(LocationToUpdate x, LocationToUpdate y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
