using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Lang;
using System.Threading.Tasks;

namespace FileDB.ViewModels.Dialogs;

public abstract partial class FileExportTabViewModel(
    IDialogs dialogs,
    ISearchResultExportDataBuilder dataBuilder,
    IProcessUtils processUtils) : ExportTabViewModel(dialogs, dataBuilder, processUtils)
{
    protected abstract string FileExtension { get; }
    protected abstract string FileTypeDescription { get; }

    [RelayCommand]
    private async Task SelectAndExportAsync()
    {
        var suggestedName = $"{ExportName}.{FileExtension}";
        var selected = await Dialogs.ShowSaveFileDialogAsync(Strings.ExportSelectDestinationFile, suggestedName, FileExtension, FileTypeDescription);
        if (string.IsNullOrEmpty(selected))
            return;

        await ExportAsync(selected);
    }

    protected override void RevealInExplorer(string path)
    {
        if (ProcessUtils.IsSelectFileInExplorerSupported())
            ProcessUtils.SelectFileInExplorer(path);
    }
}
