using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB.ViewModel.Sorters
{
    public class PersonToAddSorter : IComparer<PersonToUpdate>
    {
        public int Compare(PersonToUpdate x, PersonToUpdate y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
