using System.Collections.Generic;
using FileDB.ViewModels;

namespace FileDB.Comparers;

public class PersonsByDeceasedSorter : IComparer<DeceasedPerson>
{
    public int Compare(DeceasedPerson? x, DeceasedPerson? y)
    {
        return x!.Deceased.CompareTo(y!.Deceased);
    }
}
