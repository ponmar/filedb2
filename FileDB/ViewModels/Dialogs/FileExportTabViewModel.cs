using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Lang;
using FileDBInterface.Extensions;
using System.Threading.Tasks;

namespace FileDB.ViewModels.Dialogs;

public abstract partial class FileExportTabViewModel(
    IDialogs dialogs,
    ISearchResultExportDataBuilder dataBuilder) : ExportTabViewModel(dialogs, dataBuilder)
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExportEnabled))]
    public partial string? DestinationFile { get; set; }

    public override bool ExportEnabled => ExportName.HasContent() && DestinationFile.HasContent();

    protected abstract string FileExtension { get; }
    protected abstract string FileTypeDescription { get; }

    [RelayCommand]
    private async Task BrowseFileAsync()
    {
        var suggestedName = $"{ExportName}.{FileExtension}";
        var selected = await Dialogs.ShowSaveFileDialogAsync(Strings.ExportSelectDestinationFile, suggestedName, FileExtension, FileTypeDescription);
        DestinationFile = selected ?? string.Empty;
    }

    [RelayCommand]
    private async Task ExportToFileAsync()
    {
        if (!await Dialogs.ShowConfirmDialogAsync(string.Format(Strings.ExportSelectedData, SearchResult!.Count, DestinationFile)))
        {
            return;
        }

        await ExportAsync();
    }
}
