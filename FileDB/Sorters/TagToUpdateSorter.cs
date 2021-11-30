using System.Collections.Generic;
using FileDB.ViewModel;

namespace FileDB.Sorters
{
    public class TagToUpdateSorter : IComparer<TagToUpdate>
    {
        public int Compare(TagToUpdate x, TagToUpdate y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
