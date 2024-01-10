using FileDBInterface.Extensions;
using FileDBShared.Extensions;
using FileDBShared.FileFormats;
using System;
using System.IO;
using System.Linq;

namespace FileDB.Model;

public static class FileTypeUtils
{
    public static FileType GetFileType(string path)
    {
        if (!path.HasContent())
        {
            return FileType.Unknown;
        }

        var fileExtension = Path.GetExtension(path);
        if (fileExtension is null)
        {
            return FileType.Unknown;
        }

        fileExtension = fileExtension.ToLower();
        return Enum.GetValues<FileType>().FirstOrDefault(x => x.GetSupportedFileExtensions().Contains(fileExtension), FileType.Unknown);
    }
}
