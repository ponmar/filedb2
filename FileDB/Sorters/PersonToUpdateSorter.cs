using System.Collections.Generic;
using FileDB.ViewModel;

namespace FileDB.Sorters
{
    public class PersonToUpdateSorter : IComparer<PersonToUpdate>
    {
        public int Compare(PersonToUpdate x, PersonToUpdate y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
