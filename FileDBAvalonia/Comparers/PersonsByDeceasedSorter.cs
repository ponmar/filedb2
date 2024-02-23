using System.Collections.Generic;
using FileDBAvalonia.ViewModels;

namespace FileDBAvalonia.Comparers;

public class PersonsByDeceasedSorter : IComparer<DeceasedPerson>
{
    public int Compare(DeceasedPerson? x, DeceasedPerson? y)
    {
        return x!.Deceased.CompareTo(y!.Deceased);
    }
}
