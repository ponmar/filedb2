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
    IProcessUtils processUtils,
    IFilesExporter filesExporter) : DirectoryExportTabViewModel(dialogs, fileSystem, dataBuilder, processUtils)
{
    protected override string? DoExport(string destination, CancellationToken cancellationToken)
    {
        var files = DataBuilder.BuildFilesList(SearchResult!.Files, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        filesExporter.Export(files, destination);
        return destination;
    }
}
