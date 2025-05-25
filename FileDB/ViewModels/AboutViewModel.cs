using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;

namespace FileDB.ViewModels;

public record LicenseFileFormatDto(string PackageName, string PackageVersion, string PackageUrl, string LicenseType);

public partial class AboutViewModel : ObservableObject
{
    private const string ChangesFilePath = "CHANGES.txt";
    private readonly string LicensesJsonFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "licenses.json");

    public static string ProjectUrl => Utils.ApplicationProjectUrl;
    public static string DownloadLink => Utils.ApplicationDownloadUrl;

    public string Changes => fileSystem.File.Exists(ChangesFilePath) ? fileSystem.File.ReadAllText(ChangesFilePath) : "Not deployed";

    public static string AppName => Utils.ApplicationName;
    public static string Author => Utils.Author;
    public static string Version => Utils.GetVersionString();

    public ObservableCollection<LicenseFileFormatDto> Licenses { get; } = [];

    private readonly IFileSystem fileSystem;

    public AboutViewModel(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;

        var licenses = LicensesJsonFilePath.FromJson<List<LicenseFileFormatDto>>(fileSystem);
        licenses!.ForEach(x => Licenses.Add(x));
    }

    [RelayCommand]
    private static void OpenUrl(string url) => Utils.OpenUriInBrowser(url);
}
