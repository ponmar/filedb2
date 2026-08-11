using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using System.IO;
using System.IO.Abstractions;
using System.Threading;

namespace FileDB.ViewModels.Dialogs;

public class M3uExportTabViewModel(
    IDialogs dialogs,
    IFileSystem fileSystem,
    ISearchResultExportDataBuilder dataBuilder,
    IFilesExporter filesExporter,
    IM3uExporter m3uExporter) : DirectoryExportTabViewModel(dialogs, fileSystem, dataBuilder)
{
    protected override void DoExport(CancellationToken cancellationToken)
    {
        var m3uData = DataBuilder.BuildForM3u(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        filesExporter.Export(m3uData.Files, DestinationDirectory!);
        m3uExporter.Export(m3uData, Path.Combine(DestinationDirectory!, "Export.m3u"));
    }
}
