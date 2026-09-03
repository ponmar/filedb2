using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Lang;
using System;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace FileDB.ViewModels.Dialogs;

public abstract partial class DirectoryExportTabViewModel(
    IDialogs dialogs,
    IFileSystem fileSystem,
    ISearchResultExportDataBuilder dataBuilder,
    IProcessUtils processUtils) : ExportTabViewModel(dialogs, dataBuilder, processUtils)
{
    [RelayCommand]
    private async Task SelectAndExportAsync()
    {
        var initialPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var selected = await Dialogs.ShowBrowseExistingDirectoryDialogAsync(initialPath, Strings.ExportSelectYourDestinationDirectory);
        if (string.IsNullOrEmpty(selected))
            return;

        if (!fileSystem.Directory.Exists(selected))
        {
            await Dialogs.ShowErrorDialogAsync(Strings.ExportDestinationDirectoryDoesNotExist);
            return;
        }

        if (!IsDirectoryEmpty(selected))
        {
            await Dialogs.ShowErrorDialogAsync(Strings.ExportDestinationDirectoryIsNotEmpty);
            return;
        }

        await ExportAsync(selected);
    }

    protected override void RevealInExplorer(string path)
    {
        if (ProcessUtils.IsOpenDirectoryInExplorerSupported())
            ProcessUtils.OpenDirectoryInExplorer(path);
    }

    private bool IsDirectoryEmpty(string dirPath)
    {
        var nonHiddenItems = fileSystem.DirectoryInfo.New(dirPath).EnumerateFileSystemInfos()
            .Where(f => !f.Attributes.HasFlag(System.IO.FileAttributes.Hidden));
        return !nonHiddenItems.Any();
    }
}
