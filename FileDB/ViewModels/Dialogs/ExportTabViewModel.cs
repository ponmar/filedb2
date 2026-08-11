using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Lang;
using FileDB.ViewModels.Search;
using System;
using System.Threading.Tasks;

namespace FileDB.ViewModels.Dialogs;

public abstract partial class ExportTabViewModel(IDialogs dialogs, ISearchResultExportDataBuilder dataBuilder) : ObservableObject
{
    public SearchResult? SearchResult { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ExportEnabled))]
    public partial string ExportName { get; set; } = "My Files";

    public abstract bool ExportEnabled { get; }

    protected IDialogs Dialogs { get; } = dialogs;
    protected ISearchResultExportDataBuilder DataBuilder { get; } = dataBuilder;

    [RelayCommand]
    public async Task ExportAsync()
    {
        Exception? exportError = null;
        await Dialogs.ShowProgressDialogAsync((progress, cancellationToken) =>
        {
            progress.Report(Strings.ExportExporting);
            try
            {
                DoExport(cancellationToken);
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
    }

    protected abstract void DoExport(System.Threading.CancellationToken cancellationToken);
}
