using FileDB.Lang;
using FileDBInterface.Model;
using System;

namespace FileDB.Extensions;

public static class FileTypeExtensions
{
    public static string ToFriendlyString(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Picture => Strings.FileTypePicture,
            FileType.Movie => Strings.FileTypeMovie,
            FileType.Document => Strings.FileTypeDocument,
            FileType.Audio => Strings.FileTypeAudio,
            FileType.Unknown => Strings.FileTypeUnknown,
            _ => throw new NotImplementedException(),
        };
    }

    public static string ToIcon(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Picture => "\xD83D\xDDBC", // Unicode character name "Frame with Picture"
            FileType.Movie => "\xD83C\xDFAC", // Unicode character name "Clapper Board"
            FileType.Document => "\xD83D\xDDCE", // Unicode character name "Document"
            FileType.Audio => "\xD83C\xDFB5", // Unicode character name "Musical Note"
            FileType.Unknown => "\x2370", // Unicode characeter name "APL Functional Symbol Quad Question"
            _ => throw new NotImplementedException(),
        };
    }

    public static string[] GetSupportedFileExtensions(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Picture => [".jpg", ".jpeg", ".png", ".bmp", ".gif"],
            FileType.Movie => [".mkv", ".avi", ".mpg", ".mov", ".mp4"],
            FileType.Document => [".doc", ".pdf", ".txt", ".md"],
            FileType.Audio => [".mp3", ".wav"],
            FileType.Unknown => [],
            _ => throw new NotImplementedException(),
        };
    }
}
