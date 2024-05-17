using System;
using FileDBInterface.FileFormats;

namespace FileDBInterface.Extensions;

public static class FileTypeExtensions
{
    public static string[] GetSupportedFileExtensions(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Picture => new[] { ".jpg", ".png", ".bmp", ".gif" },
            FileType.Movie => new[] { ".mkv", ".avi", ".mpg", ".mov", ".mp4" },
            FileType.Document => new[] { ".doc", ".pdf", ".txt", ".md" },
            FileType.Audio => new[] { ".mp3", ".wav" },
            FileType.Unknown => [],
            _ => throw new NotImplementedException(),
        };
    }
}
