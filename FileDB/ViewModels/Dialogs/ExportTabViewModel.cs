using CommunityToolkit.Mvvm.ComponentModel;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Lang;
using FileDB.ViewModels.Search;
using System;
using System.Threading.Tasks;

namespace FileDB.ViewModels.Dialogs;

public abstract partial class ExportTabViewModel(IDialogs dialogs, ISearchResultExportDataBuilder dataBuilder, IProcessUtils processUtils) : ObservableObject
{
    public SearchResult? SearchResult { get; set; }

    [ObservableProperty]
    public partial string ExportName { get; set; } = "My Files";

    protected IDialogs Dialogs { get; } = dialogs;
    protected ISearchResultExportDataBuilder DataBuilder { get; } = dataBuilder;
    protected IProcessUtils ProcessUtils { get; } = processUtils;

    protected async Task ExportAsync(string destination)
    {
        Exception? exportError = null;
        string? exportedPath = null;
        await Dialogs.ShowProgressDialogAsync((progress, cancellationToken) =>
        {
            progress.Report(Strings.ExportExporting);
            try
            {
                exportedPath = DoExport(destination, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // user cancelled — exit silently
            }
            catch (Exception e)
            {
                exportError = e;
            }
        });

        if (exportError is not null)
        {
            await Dialogs.ShowErrorDialogAsync(exportError.Message);
        }
        else if (exportedPath is not null)
        {
            RevealInExplorer(exportedPath);
        }
    }

    protected abstract string? DoExport(string destination, System.Threading.CancellationToken cancellationToken);

    protected abstract void RevealInExplorer(string path);
}
