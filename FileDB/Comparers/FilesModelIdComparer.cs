using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDB.Comparers
{
    public class FilesModelIdComparer : IEqualityComparer<FilesModel>
    {
        public int GetHashCode(FilesModel co)
        {
            if (co == null)
            {
                return 0;
            }
            return co.id.GetHashCode();
        }

        public bool Equals(FilesModel x1, FilesModel x2)
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
