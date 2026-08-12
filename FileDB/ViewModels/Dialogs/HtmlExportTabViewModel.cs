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
    IProcessUtils processUtils,
    IHtmlExporter htmlExporter) : DirectoryExportTabViewModel(dialogs, fileSystem, dataBuilder, processUtils)
{
    protected override string? DoExport(string destination, CancellationToken cancellationToken)
    {
        var richData = DataBuilder.BuildRich(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        htmlExporter.Export(richData, Path.Combine(destination, "Html"));
        return destination;
    }
}
