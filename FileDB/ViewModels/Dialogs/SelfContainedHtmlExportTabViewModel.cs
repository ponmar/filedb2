using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using System.Threading;

namespace FileDB.ViewModels.Dialogs;

public class SelfContainedHtmlExportTabViewModel(
    IDialogs dialogs,
    ISearchResultExportDataBuilder dataBuilder,
    IProcessUtils processUtils,
    ISelfContainedHtmlExporter selfContainedHtmlExporter) : FileExportTabViewModel(dialogs, dataBuilder, processUtils)
{
    protected override string FileExtension => "html";
    protected override string FileTypeDescription => "HTML files";

    protected override string? DoExport(string destination, CancellationToken cancellationToken)
    {
        var richData = DataBuilder.BuildRich(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        selfContainedHtmlExporter.Export(richData, destination);
        return destination;
    }
}
