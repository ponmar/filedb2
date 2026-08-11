using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using System.Threading;

namespace FileDB.ViewModels.Dialogs;

public class PdfExportTabViewModel(
    IDialogs dialogs,
    ISearchResultExportDataBuilder dataBuilder,
    IPdfExporter pdfExporter) : FileExportTabViewModel(dialogs, dataBuilder)
{
    protected override string FileExtension => "pdf";
    protected override string FileTypeDescription => "PDF files";

    protected override void DoExport(CancellationToken cancellationToken)
    {
        var richData = DataBuilder.BuildRich(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        pdfExporter.Export(richData, DestinationFile!);
    }
}
