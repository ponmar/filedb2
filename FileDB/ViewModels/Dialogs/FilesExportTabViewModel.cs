using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using System.IO.Abstractions;
using System.Threading;

namespace FileDB.ViewModels.Dialogs;

public class FilesExportTabViewModel(
    IDialogs dialogs,
    IFileSystem fileSystem,
    ISearchResultExportDataBuilder dataBuilder,
    IFilesExporter filesExporter) : DirectoryExportTabViewModel(dialogs, fileSystem, dataBuilder)
{
    protected override void DoExport(CancellationToken cancellationToken)
    {
        var files = DataBuilder.BuildFilesList(SearchResult!.Files, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        filesExporter.Export(files, DestinationDirectory!);
    }
}
