using System;
using FileDBInterface.FileFormats;

namespace FileDBInterface.Extensions;

public static class FileTypeExtensions
{
    public static string[] GetSupportedFileExtensions(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Picture => [".jpg", ".png", ".bmp", ".gif"],
            FileType.Movie => [".mkv", ".avi", ".mpg", ".mov", ".mp4"],
            FileType.Document => [".doc", ".pdf", ".txt", ".md"],
            FileType.Audio => [".mp3", ".wav"],
            FileType.Unknown => [],
            _ => throw new NotImplementedException(),
        };
    }
}
