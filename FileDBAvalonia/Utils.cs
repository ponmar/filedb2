using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using FileDBInterface.Extensions;
using FileDBInterface.Model;
using FileDBInterface.Utils;

namespace FileDBAvalonia;

public static class Utils
{
    public const string ApplicationName = "FileDB";
    public const string Author = "Pontus Markström";
    public const string ApplicationProjectUrl = "https://bitbucket.org/pontusmarkstrom/filedb2/";
    public const string ApplicationDownloadUrl = "https://drive.google.com/drive/folders/1GyZpdDcMdUOlvvtwtKUuylazoy7XaIcm?usp=sharing";

    public static void SetInvariantCulture()
    {
        SetCulture(CultureInfo.InvariantCulture);
    }

    public static void SetCulture(CultureInfo culture)
    {
        Thread.CurrentThread.CurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
    }

    public static void SetInvariantUICulture()
    {
        SetUICulture(CultureInfo.InvariantCulture);
    }

    public static void SetUICulture(string cultureName)
    {
        SetUICulture(new CultureInfo(cultureName));
    }

    public static void SetUICulture(CultureInfo culture)
    {
        Thread.CurrentThread.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    public static void OpenUriInBrowser(string uri)
    {
        Process.Start(new ProcessStartInfo(uri) { UseShellExecute = true });
    }

    public static void SelectFileInExplorer(string path)
    {
        var explorerPath = path.Replace("/", @"\");
        Process.Start("explorer.exe", "/select, " + explorerPath);
    }

    public static void OpenDirectoryInExplorer(string path)
    {
        var explorerPath = path.Replace("/", @"\");
        Process.Start("explorer.exe", "/open, " + explorerPath);
    }

    public static void OpenFileWithDefaultApp(string path)
    {
        var explorerPath = path.Replace("/", @"\");
        Process.Start("explorer", $"\"{explorerPath}\"");
    }

    public static string CreateFileList(IEnumerable<FileModel> files)
    {
        return CreateFileList(files.Select(x => x.Id));
    }

    public static string CreateFileList(IEnumerable<int> fileIds)
    {
        return string.Join(";", fileIds);
    }

    public static List<int> CreateFileIds(string fileList)
    {
        List<int> fileIds = [];

        if (fileList.HasContent())
        {
            var items = fileList.Split(';');
            foreach (var item in items)
            {
                if (int.TryParse(item, out var fileId))
                {
                    fileIds.Add(fileId);
                }
            }
        }

        return fileIds;
    }

    public static string GetPersonAgeInFileString(string? fileDatetimeStr, string? personDateOfBirthStr)
    {
        DateTime? fileDatetime = fileDatetimeStr is null ? null : DatabaseParsing.ParseFilesDatetime(fileDatetimeStr);
        if (fileDatetime is null)
        {
            return string.Empty;
        }

        DateTime? personDateOfBirth = personDateOfBirthStr is null ? null : DatabaseParsing.ParsePersonDateOfBirth(personDateOfBirthStr);
        if (personDateOfBirth is null)
        {
            return string.Empty;
        }

        var age = TimeUtils.GetAgeInYears(fileDatetime.Value, personDateOfBirth.Value);
        return $" ({age})";
    }

    public static string CreateShortFilePositionString(string filePosition)
    {
        (var lat, var lon) = DatabaseParsing.ParseFilesPosition(filePosition)!.Value;
        return $"{lat:0.000}... {lon:0.000}...";
    }

    public static string? CreatePositionLink(string? position, string? locationLinkConfig)
    {
        if (!position.HasContent() || !locationLinkConfig.HasContent())
        {
            return null;
        }

        var positionParts = position!.Split(" ");
        if (positionParts.Length != 2)
        {
            return null;
        }

        return locationLinkConfig!.Replace("LAT", positionParts[0]).Replace("LON", positionParts[1]);
    }

    public static string GetVersionString()
    {
        var version = Assembly.GetEntryAssembly()!.GetName().Version!;
        return $"{version.Major}.{version.Minor}";
    }

    public static string CreateShortText(string text, int maxLength)
    {
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}
