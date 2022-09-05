using System.Collections.Generic;
using FileDB.ViewModel;

namespace FileDB.Comparers
{
    public class PersonsByDeceasedSorter : IComparer<DeceasedPerson>
    {
        public int Compare(DeceasedPerson? x, DeceasedPerson? y)
        {
            return x!.Deceased.CompareTo(y!.Deceased);
        }
    }
}
