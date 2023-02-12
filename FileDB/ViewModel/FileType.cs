using FileDB.Extensions;
using System;
using System.IO;
using System.Linq;

namespace FileDB.ViewModel;

public enum FileType
{
    [FileExtensions(new string[] { ".jpg", ".png", ".bmp", ".gif" })]
    Picture,

    [FileExtensions(new string[] { ".mkv", ".avi", ".mpg", ".mov", ".mp4" })]
    Movie,

    [FileExtensions(new string[] { ".doc", ".pdf", ".txt", ".md" })]
    Document,

    [FileExtensions(new string[] { ".mp3", ".wav" })]
    Audio,
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
        var fileTypes = Enum.GetValues<FileType>().Where(x => x.GetAttribute<FileExtensionsAttribute>().FileExtensions.Contains(fileExtension)).ToList();
        if (!fileTypes.Any())
        {
            return null;
        }

        return fileTypes[0];
    }
}