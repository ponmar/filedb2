using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB2Browser.ViewModel.Comparers
{
    public class PersonToAddSorter : IComparer<PersonToAdd>
    {
        public int Compare(PersonToAdd x, PersonToAdd y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
