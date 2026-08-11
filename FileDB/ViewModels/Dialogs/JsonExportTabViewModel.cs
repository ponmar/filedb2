using FileDB.Dialogs;
using FileDB.Export;
using FileDB.Export.SearchResult;
using System.Threading;

namespace FileDB.ViewModels.Dialogs;

public class JsonExportTabViewModel(
    IDialogs dialogs,
    ISearchResultExportDataBuilder dataBuilder,
    IJsonExporter jsonExporter) : FileExportTabViewModel(dialogs, dataBuilder)
{
    protected override string FileExtension => "json";
    protected override string FileTypeDescription => "JSON files";

    protected override void DoExport(CancellationToken cancellationToken)
    {
        var jsonData = DataBuilder.BuildForJson(SearchResult!.Files, ExportName, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
        jsonExporter.Export(jsonData, DestinationFile!);
    }
}
