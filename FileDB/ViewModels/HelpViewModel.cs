using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Extensions;
using FileDB.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Abstractions;

namespace FileDB.ViewModels;

public partial class HelpViewModel : ObservableObject
{
    private const string ChangesFilePath = "CHANGES.txt";
    private readonly string LicensesJsonFilePath = Path.Combine(AppContext.BaseDirectory, "Resources", "licenses.json");

    public static string ProjectUrl => Utils.ApplicationProjectUrl;
    public static string DownloadLink => Utils.ApplicationDownloadUrl;

    public string Changes => fileSystem.File.Exists(ChangesFilePath) ? fileSystem.File.ReadAllText(ChangesFilePath) : "Not deployed";

    public static string Author => Utils.Author;
    public static string Version => Utils.GetVersionString();

    public bool IsOpenUrlSupported => processUtils.IsOpenUriInBrowserSupported();

    public ObservableCollection<LicenseFileFormatDto> Licenses { get; } = [];

    private readonly IFileSystem fileSystem;
    private readonly IProcessUtils processUtils;

    public HelpViewModel(IFileSystem fileSystem, IProcessUtils processUtils)
    {
        this.fileSystem = fileSystem;
        this.processUtils = processUtils;

        var licenses = LicensesJsonFilePath.FromJson<List<LicenseFileFormatDto>>(fileSystem);
        licenses!.ForEach(x => Licenses.Add(x));
    }

    [RelayCommand]
    private void OpenUrl(string url) => processUtils.OpenUriInBrowser(url);
}
