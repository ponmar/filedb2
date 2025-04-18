using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class RandomFilter(int numFiles) : IFilesFilter
{
    public bool CanRun() => numFiles > 0;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesRandom(numFiles);
    }
}
