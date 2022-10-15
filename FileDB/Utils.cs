using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using FileDB.Configuration;
using FileDB.Extensions;
using FileDB.Sorters;
using FileDBInterface.DbAccess;
using FileDBInterface.Model;

namespace FileDB;

public class WindowModeDescription
{
    public string Name => Mode.GetDescription();

    public WindowMode Mode { get; }

    public WindowModeDescription(WindowMode mode)
    {
        Mode = mode;
    }
}

public static class Utils
{
    public const string ApplicationName = "FileDB";

    public static void SetInvariantCulture()
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
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

    public static string CreateFileList(List<FilesModel> files)
    {
        return CreateFileList(files.Select(x => x.Id));
    }

    public static string CreateFileList(IEnumerable<int> fileIds)
    {
        return string.Join(";", fileIds);
    }

    public static List<int> CreateFileIds(string fileList)
    {
        List<int> fileIds = new();

        if (!string.IsNullOrEmpty(fileList))
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

    public static List<SortMethodDescription> GetSortMethods()
    {
        List<SortMethodDescription> sortMethods = new();
        foreach (var value in Enum.GetValues<SortMethod>())
        {
            sortMethods.Add(new SortMethodDescription(value));
        }
        return sortMethods;
    }

    public static List<WindowModeDescription> GetWindowModes()
    {
        List<WindowModeDescription> modes = new();
        foreach (var mode in Enum.GetValues<WindowMode>())
        {
            modes.Add(new WindowModeDescription(mode));
        }
        return modes;
    }

    public static string GetPersonAgeInFileString(string? fileDatetimeStr, string? personDateOfBirthStr)
    {
        DateTime? fileDatetime = fileDatetimeStr == null ? null : DatabaseParsing.ParseFilesDatetime(fileDatetimeStr);
        if (fileDatetime == null)
        {
            return string.Empty;
        }

        DateTime? personDateOfBirth = personDateOfBirthStr == null ? null : DatabaseParsing.ParsePersonsDateOfBirth(personDateOfBirthStr);
        if (personDateOfBirth == null)
        {
            return string.Empty;
        }

        var age = DatabaseUtils.GetYearsAgo(fileDatetime.Value, personDateOfBirth.Value);
        return $" ({age})";
    }

    public static string CreateShortFilePositionString(string filePosition)
    {
        (var lat, var lon) = DatabaseParsing.ParseFilesPosition(filePosition)!.Value;
        return $"{lat:0.000}... {lon:0.000}...";
    }

    public static string? CreatePositionLink(string? position, string? locationLinkConfig)
    {
        if (string.IsNullOrEmpty(position) || string.IsNullOrEmpty(locationLinkConfig))
        {
            return null;
        }

        var positionParts = position.Split(" ");
        if (positionParts.Length != 2)
        {
            return null;
        }

        return locationLinkConfig.Replace("LAT", positionParts[0]).Replace("LON", positionParts[1]);
    }

    public static Uri? CreatePositionUri(string position, string locationLinkConfig)
    {
        var link = CreatePositionLink(position, locationLinkConfig);
        return link != null ? new Uri(link) : null;
    }

    public static string GetVersionString()
    {
        var version = Assembly.GetEntryAssembly()!.GetName().Version!;
        return $"{version.Major}.{version.Minor}";
    }

    public const string ApplicationDownloadUrl = "https://drive.google.com/drive/folders/1GyZpdDcMdUOlvvtwtKUuylazoy7XaIcm?usp=sharing";
}
