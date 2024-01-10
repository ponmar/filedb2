using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Export;
using FileDB.Model;
using FileDB.Resources;
using FileDBInterface.Extensions;
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
        private string exportName = "My Files";

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
        private readonly IDatabaseAccessProvider dbAccessProvider;
        private readonly IFilesystemAccessProvider filesystemAccessProvider;
        private readonly IFileSystem fileSystem;

        public ExportViewModel(IDialogs dialogs, IDatabaseAccessProvider dbAccessProvider, IFilesystemAccessProvider filesystemAccessProvider, IFileSystem fileSystem)
        {
            this.dialogs = dialogs;
            this.dbAccessProvider = dbAccessProvider;
            this.filesystemAccessProvider = filesystemAccessProvider;
            this.fileSystem = fileSystem;
        }

        [RelayCommand]
        private void BrowseDestinationDirectory()
        {
            var initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ExportFilesDestinationDirectory = dialogs.BrowseExistingDirectory(initialPath, Strings.ExportSelectYourDestinationDirectory);
        }

        [RelayCommand]
        private void Export()
        {
            if (!fileSystem.Directory.Exists(ExportFilesDestinationDirectory))
            {
                dialogs.ShowErrorDialog(Strings.ExportDestinationDirectoryDoesNotExist);
                return;
            }

            if (!IsDirectoryEmpty(ExportFilesDestinationDirectory))
            {
                dialogs.ShowErrorDialog(Strings.ExportDestinationDirectoryIsNotEmpty);
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
                selections.Add(SearchResultExportType.PicturesWithMetaData);
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
                dialogs.ShowErrorDialog(Strings.ExportNothingToExport);
                return;
            }

            if (!dialogs.ShowConfirmDialog(string.Format(Strings.ExportSelectedData, SearchResult!.Count, ExportFilesDestinationDirectory)))
            {
                return;
            }

            dialogs.ShowProgressDialog(progress =>
            {
                progress.Report(Strings.ExportExporting);
                try
                {
                    var exporter = new SearchResultExportHandler(dbAccessProvider, filesystemAccessProvider, fileSystem);
                    exporter.Export(ExportFilesDestinationDirectory, ExportName, SearchResult.Files, selections);
                    dialogs.ShowInfoDialog(Strings.ExportFinishedSuccessfully);
                }
                catch (Exception e)
                {
                    dialogs.ShowErrorDialog(string.Format(Strings.ExportError, e.Message));
                }
            });
        }

        private bool IsDirectoryEmpty(string path)
        {
            return !fileSystem.Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
