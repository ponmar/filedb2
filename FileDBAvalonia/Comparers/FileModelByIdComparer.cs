using System.Collections.Generic;
using FileDBInterface.Model;

namespace FileDBAvalonia.Comparers;

public class FileModelByIdComparer : IEqualityComparer<FileModel>
{
    public int GetHashCode(FileModel co)
    {
        if (co is null)
        {
            return 0;
        }
        return co.Id.GetHashCode();
    }

    public bool Equals(FileModel? x1, FileModel? x2)
    {
        if (ReferenceEquals(x1, x2))
        {
            return true;
        }
        if (ReferenceEquals(x1, null) ||
            ReferenceEquals(x2, null))
        {
            return false;
        }
        return x1.Id == x2.Id;
    }
}
