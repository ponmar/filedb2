using FileDB.Model;
using FileDBInterface.DbAccess;
using FileDBShared.Model;
using System.Collections.Generic;

namespace FileDB.FilesFilter;

public class FilterFileType(Model.FileType fileType) : IFilesFilter
{
    public bool CanRun() => true;

    public IEnumerable<FileModel> Run(IDbAccess dbAccess)
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
