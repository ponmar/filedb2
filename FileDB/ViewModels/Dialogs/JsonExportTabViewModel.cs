using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using System.Threading;

namespace FileDB.ViewModels.Dialogs;

public class JsonExportTabViewModel(
    IDialogs dialogs,
    ISearchResultExportDataBuilder dataBuilder,
    IProcessUtils processUtils,
    IJsonExporter jsonExporter) : FileExportTabViewModel(dialogs, dataBuilder, processUtils)
{
    protected override string FileExtension => "json";
    protected override string FileTypeDescription => "JSON files";

    protected override string? DoExport(string destination, CancellationToken cancellationToken)
    {
        var jsonData = DataBuilder.BuildForJson(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        jsonExporter.Export(jsonData, destination);
        return destination;
    }
}
