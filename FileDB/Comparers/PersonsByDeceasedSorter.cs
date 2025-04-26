using System.Collections.Generic;
using FileDB.ViewModels;

namespace FileDB.Comparers;

public class PersonsByDeceasedSorter : IComparer<DeceasedPersonViewModel>
{
    public int Compare(DeceasedPersonViewModel? x, DeceasedPersonViewModel? y)
    {
        return x!.Deceased.CompareTo(y!.Deceased);
    }
}
