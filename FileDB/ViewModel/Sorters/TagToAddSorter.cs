using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB.ViewModel.Sorters
{
    public class TagToAddSorter : IComparer<TagToUpdate>
    {
        public int Compare(TagToUpdate x, TagToUpdate y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
