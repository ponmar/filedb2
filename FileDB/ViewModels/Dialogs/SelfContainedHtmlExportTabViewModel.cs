using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using System.Threading;

namespace FileDB.ViewModels.Dialogs;

public class SelfContainedHtmlExportTabViewModel(
    IDialogs dialogs,
    ISearchResultExportDataBuilder dataBuilder,
    ISelfContainedHtmlExporter selfContainedHtmlExporter) : FileExportTabViewModel(dialogs, dataBuilder)
{
    protected override string FileExtension => "html";
    protected override string FileTypeDescription => "HTML files";

    protected override void DoExport(CancellationToken cancellationToken)
    {
        var richData = DataBuilder.BuildRich(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        selfContainedHtmlExporter.Export(richData, DestinationFile!);
    }
}
