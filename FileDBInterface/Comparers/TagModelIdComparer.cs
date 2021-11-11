using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBInterface.Comparers
{
    public class TagModelIdComparer : IEqualityComparer<TagModel>
    {
        public int GetHashCode(TagModel co)
        {
            if (co == null)
            {
                return 0;
            }
            return co.id.GetHashCode();
        }

        public bool Equals(TagModel x1, TagModel x2)
        {
            if (object.ReferenceEquals(x1, x2))
            {
                return true;
            }
            if (object.ReferenceEquals(x1, null) ||
                object.ReferenceEquals(x2, null))
            {
                return false;
            }
            return x1.id == x2.id;
        }
    }
}
