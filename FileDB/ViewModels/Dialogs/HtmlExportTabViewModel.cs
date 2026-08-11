using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using System.IO;
using System.IO.Abstractions;
using System.Threading;

namespace FileDB.ViewModels.Dialogs;

public class HtmlExportTabViewModel(
    IDialogs dialogs,
    IFileSystem fileSystem,
    ISearchResultExportDataBuilder dataBuilder,
    IHtmlExporter htmlExporter) : DirectoryExportTabViewModel(dialogs, fileSystem, dataBuilder)
{
    protected override void DoExport(CancellationToken cancellationToken)
    {
        var richData = DataBuilder.BuildRich(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        htmlExporter.Export(richData, Path.Combine(DestinationDirectory!, "Html"));
    }
}
