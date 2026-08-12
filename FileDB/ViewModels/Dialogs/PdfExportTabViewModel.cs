using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using System.Threading;

namespace FileDB.ViewModels.Dialogs;

public class PdfExportTabViewModel(
    IDialogs dialogs,
    ISearchResultExportDataBuilder dataBuilder,
    IProcessUtils processUtils,
    IPdfExporter pdfExporter) : FileExportTabViewModel(dialogs, dataBuilder, processUtils)
{
    protected override string FileExtension => "pdf";
    protected override string FileTypeDescription => "PDF files";

    protected override string? DoExport(string destination, CancellationToken cancellationToken)
    {
        var richData = DataBuilder.BuildRich(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        pdfExporter.Export(richData, destination);
        return destination;
    }
}
