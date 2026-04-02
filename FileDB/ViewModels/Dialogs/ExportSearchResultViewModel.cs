using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Lang;
using FileDB.Model;
using FileDB.ViewModels.Search;
using FileDBInterface.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace FileDB.ViewModels.Dialogs;

public partial class ExportSearchResultViewModel : ObservableObject
{
    public SearchResult? SearchResult { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExportEnabled))]
    public partial string? ExportFilesDestinationDirectory { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExportEnabled))]
    public partial string ExportName { get; set; } = "My Files";

    [ObservableProperty]
    public partial bool ExportIncludesFiles { get; set; } = false;

    public bool ExportEnabled => ExportName.HasContent() && ExportFilesDestinationDirectory.HasContent();

    partial void OnExportIncludesFilesChanged(bool value)
    {
        if (!value)
        {
            ExportIncludesM3u = false;
        }
    }

    [ObservableProperty]
    public partial bool ExportIncludesHtml { get; set; } = false;

    [ObservableProperty]
    public partial bool ExportIncludesM3u { get; set; } = false;

    partial void OnExportIncludesM3uChanged(bool value)
    {
        if (value)
        {
            ExportIncludesFiles = true;
        }
    }

    [ObservableProperty]
    public partial bool ExportIncludesJson { get; set; } = false;

    [ObservableProperty]
    public partial bool ExportIncludesPdf { get; set; } = false;

    private readonly IDialogs dialogs;
    private readonly IDatabaseAccessProvider dbAccessProvider;
    private readonly IFilesystemAccessProvider filesystemAccessProvider;
    private readonly IFileSystem fileSystem;

    public ExportSearchResultViewModel(IDialogs dialogs, IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IFileSystem fileSystem)
    {
        this.dialogs = dialogs;
        this.dbAccessProvider = dbAccessProvider;
        this.filesystemAccessProvider = filesystemAccessProvider;
        this.fileSystem = fileSystem;
    }

    [RelayCommand]
    private async Task BrowseDestinationDirectoryAsync()
    {
        var initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var selectedDir = await dialogs.ShowBrowseExistingDirectoryDialogAsync(initialPath, Strings.ExportSelectYourDestinationDirectory);
        ExportFilesDestinationDirectory = selectedDir ?? string.Empty;
    }

    [RelayCommand]
    private async Task ExportAsync()
    {
        if (!fileSystem.Directory.Exists(ExportFilesDestinationDirectory))
        {
            await dialogs.ShowErrorDialogAsync(Strings.ExportDestinationDirectoryDoesNotExist);
            return;
        }

        if (!IsDirectoryEmpty(ExportFilesDestinationDirectory))
        {
            await dialogs.ShowErrorDialogAsync(Strings.ExportDestinationDirectoryIsNotEmpty);
            return;
        }

        var selections = new List<SearchResultExportType>();
        if (ExportIncludesFiles)
        {
            selections.Add(SearchResultExportType.Files);
        }
        if (ExportIncludesHtml)
        {
            selections.Add(SearchResultExportType.Html);
        }
        if (ExportIncludesM3u)
        {
            selections.Add(SearchResultExportType.M3u);
        }
        if (ExportIncludesJson)
        {
            selections.Add(SearchResultExportType.Json);
        }
        if (ExportIncludesPdf)
        {
            selections.Add(SearchResultExportType.Pdf);
        }

        if (!selections.Any())
        {
            await dialogs.ShowErrorDialogAsync(Strings.ExportNothingToExport);
            return;
        }

        if (!await dialogs.ShowConfirmDialogAsync(string.Format(Strings.ExportSelectedData, SearchResult!.Count, ExportFilesDestinationDirectory)))
        {
            return;
        }

        await dialogs.ShowProgressDialogAsync(progress =>
        {
            progress.Report(Strings.ExportExporting);
            try
            {
                var exporter = new SearchResultExportHandler(dbAccessProvider, filesystemAccessProvider, fileSystem);
                exporter.Export(ExportFilesDestinationDirectory, ExportName, SearchResult.Files, selections);
            }
            catch
            {
                // TODO: log and throw?
            }
        });
    }

    private bool IsDirectoryEmpty(string dirPath)
    {
        var nonHiddenItemsInDir = fileSystem.DirectoryInfo.New(dirPath).EnumerateFileSystemInfos().Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden));
        return !nonHiddenItemsInDir.Any();
    }
}
