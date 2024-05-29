using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public interface IFilesFilter
{
    public bool CanRun();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess);
}
