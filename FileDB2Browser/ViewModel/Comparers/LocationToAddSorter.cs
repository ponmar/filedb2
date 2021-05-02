using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB2Browser.ViewModel.Comparers
{
    public class LocationToAddSorter : IComparer<LocationToAdd>
    {
        public int Compare(LocationToAdd x, LocationToAdd y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
