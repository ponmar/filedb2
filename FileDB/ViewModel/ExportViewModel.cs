using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Export;
using System;
using System.Collections.Generic;
using System.IO;
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
        private string exportFilesHeader = $"{Utils.ApplicationName} Export";

        [ObservableProperty]
        private bool exportIncludesFiles = true;

        public bool ExportEnabled => !string.IsNullOrEmpty(ExportFilesDestinationDirectory);

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


        public ExportViewModel()
        {
        }

        [RelayCommand]
        private void BrowseDestinationDirectory()
        {
            var initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            ExportFilesDestinationDirectory = Dialogs.Default.BrowseExistingDirectory(initialPath, "Select your destination directory");
        }

        [RelayCommand]
        private void Export()
        {
            if (string.IsNullOrEmpty(ExportFilesHeader))
            {
                Dialogs.Default.ShowErrorDialog("No header specified");
                return;
            }

            if (string.IsNullOrEmpty(ExportFilesDestinationDirectory))
            {
                Dialogs.Default.ShowErrorDialog("No destination directory specified");
                return;
            }

            if (!Directory.Exists(ExportFilesDestinationDirectory))
            {
                Dialogs.Default.ShowErrorDialog("Destination directory does not exist");
                return;
            }

            if (!IsDirectoryEmpty(ExportFilesDestinationDirectory))
            {
                Dialogs.Default.ShowErrorDialog("Destination directory is not empty");
                return;
            }

            var selection = new List<bool>() { ExportIncludesFiles, ExportIncludesHtml, ExportIncludesM3u, ExportIncludesFilesWithMetaData, ExportIncludesJson };
            if (!selection.Any(x => x))
            {
                Dialogs.Default.ShowErrorDialog("Nothing to export");
                return;
            }

            if (!Dialogs.Default.ShowConfirmDialog($"Export selected data for {SearchResult!.Count} files to {ExportFilesDestinationDirectory}?"))
            {
                return;
            }

            Dialogs.Default.ShowProgressDialog(progress =>
            {
                progress.Report("Exporting...");
                try
                {
                    new SearchResultExporter().Export(ExportFilesDestinationDirectory, ExportFilesHeader, SearchResult.Files,
                    ExportIncludesFiles, ExportIncludesHtml, ExportIncludesM3u, ExportIncludesFilesWithMetaData, ExportIncludesJson);
                    Dialogs.Default.ShowInfoDialog("Export finished successfully.");
                }
                catch (IOException e)
                {
                    Dialogs.Default.ShowErrorDialog("Export error: " + e.Message);
                }
            });
        }

        private static bool IsDirectoryEmpty(string path)
        {
            return !Directory.EnumerateFileSystemEntries(path).Any();
        }
    }
}
