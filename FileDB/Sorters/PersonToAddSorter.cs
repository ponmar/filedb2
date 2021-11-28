using System.Collections.Generic;
using FileDB.ViewModel;

namespace FileDB.Sorters
{
    public class PersonToAddSorter : IComparer<PersonToUpdate>
    {
        public int Compare(PersonToUpdate x, PersonToUpdate y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
