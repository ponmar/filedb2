using FileDBInterface.DatabaseAccess;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class DirectoryFilter(string? directory) : IFilesFilter
{
    public bool CanRun() => directory.HasContent();

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        return dbAccess.SearchFilesByPath(directory!);
    }
}
