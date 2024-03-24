﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDBAvalonia.Dialogs;
using FileDBAvalonia.Export;
using FileDBAvalonia.Lang;
using FileDBAvalonia.Model;
using FileDBAvalonia.ViewModels.Search;
using FileDBInterface.Extensions;
using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace FileDBAvalonia.ViewModels.Dialogs
{
    public partial class ExportSearchResultViewModel : ObservableObject
    {
        public SearchResult? SearchResult { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ExportEnabled))]
        private string? exportFilesDestinationDirectory;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(ExportEnabled))]
        private string exportName = "My Files";

        [ObservableProperty]
        private bool exportIncludesFiles = false;

        public bool ExportEnabled => ExportName.HasContent() && ExportFilesDestinationDirectory.HasContent();

        partial void OnExportIncludesFilesChanged(bool value)
        {
            if (!value)
            {
                ExportIncludesM3u = false;
            }
        }

        [ObservableProperty]
        private bool exportIncludesHtml = false;

        [ObservableProperty]
        private bool exportIncludesM3u = false;

        partial void OnExportIncludesM3uChanged(bool value)
        {
            if (value)
            {
                ExportIncludesFiles = true;
            }
        }

        [ObservableProperty]
        private bool exportIncludesFilesWithMetaData = false;

        [ObservableProperty]
        private bool exportFilesWithMetaDataJpeg = true;

        [ObservableProperty]
        private bool exportIncludesJson = false;

        [ObservableProperty]
        private bool exportIncludesPdf = false;

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
            if (ExportIncludesFilesWithMetaData)
            {
                selections.Add(ExportFilesWithMetaDataJpeg ? SearchResultExportType.JpegPicturesWithMetaData : SearchResultExportType.PngPicturesWithMetaData);
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

            dialogs.ShowProgressDialog(async progress =>
            {
                progress.Report(Strings.ExportExporting);
                try
                {
                    var exporter = new SearchResultExportHandler(dbAccessProvider, filesystemAccessProvider, fileSystem);
                    exporter.Export(ExportFilesDestinationDirectory, ExportName, SearchResult.Files, selections);
                    await dialogs.ShowInfoDialogAsync(Strings.ExportFinishedSuccessfully);
                }
                catch (Exception e)
                {
                    await dialogs.ShowErrorDialogAsync(string.Format(Strings.ExportError, e.Message));
                }
            });
        }

        private bool IsDirectoryEmpty(string path)
        {
            return !fileSystem.Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
