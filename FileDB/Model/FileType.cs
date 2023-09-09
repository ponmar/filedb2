using FileDB.Resources;
using System;
using System.IO;
using System.Linq;

namespace FileDB.ViewModel;

public enum FileType
{
    Picture,
    Movie,
    Document,
    Audio,
}

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
            _ => throw new NotImplementedException(),
        };
    }

    public static string[] GetSupportedFileExtensions(this FileType fileType)
    {
        return fileType switch
        {
            FileType.Picture => new[] { ".jpg", ".png", ".bmp", ".gif" },
            FileType.Movie => new[] { ".mkv", ".avi", ".mpg", ".mov", ".mp4" },
            FileType.Document => new[] { ".doc", ".pdf", ".txt", ".md" },
            FileType.Audio => new[] { ".mp3", ".wav" },
            _ => throw new NotImplementedException(),
        };
    }
}

public static class FileTypeUtils
{
    public static FileType? GetFileType(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        var fileExtension = Path.GetExtension(path);
        if (fileExtension == null)
        {
            return null;
        }

        fileExtension = fileExtension.ToLower();
        var fileTypes = Enum.GetValues<FileType>().Where(x => x.GetSupportedFileExtensions().Contains(fileExtension)).ToList();
        return fileTypes.FirstOrDefault();
    }
}