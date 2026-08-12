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
    IProcessUtils processUtils,
    IFilesExporter filesExporter,
    IM3uExporter m3uExporter) : DirectoryExportTabViewModel(dialogs, fileSystem, dataBuilder, processUtils)
{
    protected override string? DoExport(string destination, CancellationToken cancellationToken)
    {
        var m3uData = DataBuilder.BuildForM3u(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        filesExporter.Export(m3uData.Files, destination);
        m3uExporter.Export(m3uData, Path.Combine(destination, "Export.m3u"));
        return destination;
    }
}
