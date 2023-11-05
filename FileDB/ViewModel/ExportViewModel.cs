using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Export;
using FileDB.Extensions;
using FileDB.Model;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;

namespace FileDB.ViewModel
{
    public partial class ExportViewModel : ObservableObject
    {
        public SearchResult? SearchResult { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ExportEnabled))]
        private string? exportFilesDestinationDirectory;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ExportEnabled))]
        private string exportName = $"{Utils.ApplicationName} Export";

        [ObservableProperty]
        private bool exportIncludesFiles = true;

        public bool ExportEnabled => ExportName.HasContent() && ExportFilesDestinationDirectory.HasContent();

        partial void OnExportIncludesFilesChanged(bool value)
        {
            if (!value)
            {
                ExportIncludesM3u = false;
            }
        }

        [ObservableProperty]
        private bool exportIncludesHtml = true;

        [ObservableProperty]
        private bool exportIncludesM3u = true;

        partial void OnExportIncludesM3uChanged(bool value)
        {
            if (value)
            {
                ExportIncludesFiles = true;
            }
        }

        [ObservableProperty]
        private bool exportIncludesFilesWithMetaData = true;

        [ObservableProperty]
        private bool exportIncludesJson = true;

        [ObservableProperty]
        private bool exportIncludesPdf = true;

        private readonly IDialogs dialogs;
        private readonly IDbAccessRepository dbAccessRepository;
        private readonly IFilesystemAccessRepository filesystemAccessRepository;
        private readonly IFileSystem fileSystem;

        public ExportViewModel(IDialogs dialogs, IDbAccessRepository dbAccessRepository, IFilesystemAccessRepository filesystemAccessRepository, IFileSystem fileSystem)
        {
            this.dialogs = dialogs;
            this.dbAccessRepository = dbAccessRepository;
            this.filesystemAccessRepository = filesystemAccessRepository;
            this.fileSystem = fileSystem;
        }

        [RelayCommand]
        private void BrowseDestinationDirectory()
        {
            var initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ExportFilesDestinationDirectory = dialogs.BrowseExistingDirectory(initialPath, "Select your destination directory");
        }

        [RelayCommand]
        private void Export()
        {
            if (!fileSystem.Directory.Exists(ExportFilesDestinationDirectory))
            {
                dialogs.ShowErrorDialog("Destination directory does not exist");
                return;
            }

            if (!IsDirectoryEmpty(ExportFilesDestinationDirectory))
            {
                dialogs.ShowErrorDialog("Destination directory is not empty");
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
            if (ExportIncludesFilesWithMetaData)
            {
                selections.Add(SearchResultExportType.FilesWithMetaData);
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
                dialogs.ShowErrorDialog("Nothing to export");
                return;
            }

            if (!dialogs.ShowConfirmDialog($"Export selected data for {SearchResult!.Count} files to {ExportFilesDestinationDirectory}?"))
            {
                return;
            }

            dialogs.ShowProgressDialog(progress =>
            {
                progress.Report("Exporting...");
                try
                {
                    var exporter = new SearchResultExportHandler(dbAccessRepository, filesystemAccessRepository, fileSystem);
                    exporter.Export(ExportFilesDestinationDirectory, ExportName, SearchResult.Files, selections);
                    dialogs.ShowInfoDialog("Export finished successfully.");
                }
                catch (Exception e)
                {
                    dialogs.ShowErrorDialog("Export error: " + e.Message);
                }
            });
        }

        private bool IsDirectoryEmpty(string path)
        {
            return !fileSystem.Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
