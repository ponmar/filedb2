using FileDBShared.Extensions;
using FileDBInterface.DatabaseAccess;
using FileDBShared.FileFormats;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDBAvalonia.FilesFilter;

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
