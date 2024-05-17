using FileDBAvalonia.Extensions;
using FileDBInterface.FileFormats;
using System.IO.Abstractions;

namespace FileDBAvalonia.Export.SearchResult;

public class JsonExporter(IFileSystem fileSystem) : ISearchResultExporter
{
    public void Export(SearchResultExport data, string filename)
    {
        var json = data.ToFormattedJson();
        fileSystem.File.WriteAllText(filename, json);
    }
}
