using FileDBInterface.Extensions;
using FileDBInterface.DatabaseAccess;
using FileDBInterface.FileFormats;
using FileDBInterface.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FileTypeFilter(FileType fileType) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDatabaseAccess dbAccess)
    {
        var fileExtensions = fileType.GetSupportedFileExtensions();

        var result = new List<FileModel>();
        foreach (var extension in fileExtensions)
        {
            result.AddRange(dbAccess.SearchFilesByExtension(extension));
        }

        return result;
    }
}
