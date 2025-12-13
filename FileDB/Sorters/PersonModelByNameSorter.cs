using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Sorters;

public class PersonModelByNameSorter : IComparer<PersonModel>
{
    public int Compare(PersonModel? first, PersonModel? second)
    {
        return first!.FullName.CompareTo(second!.FullName);
    }
}
