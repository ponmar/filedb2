using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Lang;
using FileDBInterface.Extensions;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace FileDB.ViewModels.Dialogs;

public abstract partial class DirectoryExportTabViewModel(
    IDialogs dialogs,
    IFileSystem fileSystem,
    ISearchResultExportDataBuilder dataBuilder) : ExportTabViewModel(dialogs, dataBuilder)
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExportEnabled))]
    public partial string? DestinationDirectory { get; set; }

    public override bool ExportEnabled => ExportName.HasContent() && DestinationDirectory.HasContent();

    [RelayCommand]
    private async Task BrowseDirectoryAsync()
    {
        var initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var selected = await Dialogs.ShowBrowseExistingDirectoryDialogAsync(initialPath, Strings.ExportSelectYourDestinationDirectory);
        DestinationDirectory = selected ?? string.Empty;
    }

    [RelayCommand]
    private async Task ExportToDirectoryAsync()
    {
        if (!fileSystem.Directory.Exists(DestinationDirectory))
        {
            await Dialogs.ShowErrorDialogAsync(Strings.ExportDestinationDirectoryDoesNotExist);
            return;
        }

        if (!IsDirectoryEmpty(DestinationDirectory))
        {
            await Dialogs.ShowErrorDialogAsync(Strings.ExportDestinationDirectoryIsNotEmpty);
            return;
        }

        if (!await Dialogs.ShowConfirmDialogAsync(string.Format(Strings.ExportSelectedData, SearchResult!.Count, DestinationDirectory)))
        {
            return;
        }

        await ExportAsync();
    }

    private bool IsDirectoryEmpty(string dirPath)
    {
        var nonHiddenItems = fileSystem.DirectoryInfo.New(dirPath).EnumerateFileSystemInfos()
            .Where(f => !f.Attributes.HasFlag(System.IO.FileAttributes.Hidden));
        return !nonHiddenItems.Any();
    }
}
