using FileDBInterface.DatabaseAccess;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

public interface IFilesFilter
{
    public bool CanRun();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess);
}
