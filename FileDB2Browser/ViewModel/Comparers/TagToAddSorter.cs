using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileDB2Browser.ViewModel.Comparers
{
    public class TagToAddSorter : IComparer<TagToAdd>
    {
        public int Compare(TagToAdd x, TagToAdd y)
        {
            return x.Name.CompareTo(y.Name);
        }
    }
}
